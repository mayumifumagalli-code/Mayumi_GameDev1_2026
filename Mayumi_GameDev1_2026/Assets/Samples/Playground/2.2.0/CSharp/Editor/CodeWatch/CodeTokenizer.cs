using System.Collections.Generic;

namespace Mavrodiev.Playground.CSharp
{
    public enum CodeTokenKind { Plain, Keyword, Type, Text, Number, Comment }

    // One coloured run of a line: where it starts (column), how long it is, and what it is.
    public struct CodeToken
    {
        public int start;
        public int length;
        public CodeTokenKind kind;

        public CodeToken(int start, int length, CodeTokenKind kind)
        {
            this.start = start;
            this.length = length;
            this.kind = kind;
        }
    }

    // A small hand-written C# tokenizer for Code Watch's colours. It works one line at a time;
    // `state` carries a block comment or a verbatim string from one line to the next.
    // It never throws: any character it does not know becomes plain text.
    public static class CodeTokenizer
    {
        public const int StateNormal = 0;
        public const int StateBlockComment = 1;
        public const int StateVerbatimText = 2;

        static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "abstract", "as", "base", "break", "case", "catch", "class", "const", "continue", "default",
            "delegate", "do", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed",
            "for", "foreach", "get", "goto", "if", "implicit", "in", "interface", "internal", "is", "lock",
            "namespace", "new", "null", "operator", "out", "override", "params", "private", "protected",
            "public", "readonly", "ref", "return", "sealed", "set", "sizeof", "static", "struct", "switch",
            "this", "throw", "true", "try", "typeof", "unchecked", "unsafe", "using", "virtual", "volatile",
            "while", "yield", "async", "await", "partial", "where", "value"
        };

        static readonly HashSet<string> BuiltInTypes = new HashSet<string>
        {
            "bool", "byte", "char", "decimal", "double", "float", "int", "long", "object", "sbyte",
            "short", "string", "uint", "ulong", "ushort", "void", "var"
        };

        public static List<CodeToken> TokenizeLine(string line, ref int state)
        {
            var tokens = new List<CodeToken>();
            if (line == null) return tokens;
            int n = line.Length;
            int i = 0;
            int plainStart = -1;

            while (i < n)
            {
                if (state == StateBlockComment)
                {
                    int close = line.IndexOf("*/", i, System.StringComparison.Ordinal);
                    int end = close < 0 ? n : close + 2;
                    FlushPlain(tokens, ref plainStart, i);
                    tokens.Add(new CodeToken(i, end - i, CodeTokenKind.Comment));
                    if (close >= 0) state = StateNormal;
                    i = end;
                    continue;
                }
                if (state == StateVerbatimText)
                {
                    int end = EndOfVerbatim(line, i, ref state);
                    FlushPlain(tokens, ref plainStart, i);
                    tokens.Add(new CodeToken(i, end - i, CodeTokenKind.Text));
                    i = end;
                    continue;
                }

                char c = line[i];
                char next = i + 1 < n ? line[i + 1] : '\0';

                if (c == '/' && next == '/')
                {
                    FlushPlain(tokens, ref plainStart, i);
                    tokens.Add(new CodeToken(i, n - i, CodeTokenKind.Comment));
                    i = n;
                }
                else if (c == '/' && next == '*')
                {
                    FlushPlain(tokens, ref plainStart, i);
                    int close = line.IndexOf("*/", i + 2, System.StringComparison.Ordinal);
                    int end = close < 0 ? n : close + 2;
                    if (close < 0) state = StateBlockComment;
                    tokens.Add(new CodeToken(i, end - i, CodeTokenKind.Comment));
                    i = end;
                }
                else if (IsVerbatimStart(line, i))
                {
                    FlushPlain(tokens, ref plainStart, i);
                    int quote = line.IndexOf('"', i);
                    state = StateVerbatimText;
                    int end = EndOfVerbatim(line, quote + 1, ref state);
                    tokens.Add(new CodeToken(i, end - i, CodeTokenKind.Text));
                    i = end;
                }
                else if (c == '"' || (c == '$' && next == '"'))
                {
                    FlushPlain(tokens, ref plainStart, i);
                    int end = EndOfQuoted(line, c == '$' ? i + 2 : i + 1, '"');
                    tokens.Add(new CodeToken(i, end - i, CodeTokenKind.Text));
                    i = end;
                }
                else if (c == '\'')
                {
                    FlushPlain(tokens, ref plainStart, i);
                    int end = EndOfQuoted(line, i + 1, '\'');
                    tokens.Add(new CodeToken(i, end - i, CodeTokenKind.Text));
                    i = end;
                }
                else if (char.IsDigit(c) || (c == '.' && char.IsDigit(next) && !PreviousIsWordChar(line, i)))
                {
                    if (PreviousIsWordChar(line, i))
                    {
                        // A digit inside a name, like the 3 in Vector3: part of the word, handled below.
                        if (plainStart < 0) plainStart = i;
                        i++;
                        continue;
                    }
                    FlushPlain(tokens, ref plainStart, i);
                    int end = i + 1;
                    while (end < n)
                    {
                        char d = line[end];
                        if (char.IsLetterOrDigit(d) || d == '_') end++;
                        else if (d == '.' && end + 1 < n && char.IsDigit(line[end + 1])) end++;
                        else break;
                    }
                    tokens.Add(new CodeToken(i, end - i, CodeTokenKind.Number));
                    i = end;
                }
                else if (char.IsLetter(c) || c == '_' || (c == '@' && (char.IsLetter(next) || next == '_')))
                {
                    int end = i + 1;
                    while (end < n && (char.IsLetterOrDigit(line[end]) || line[end] == '_')) end++;
                    string word = line.Substring(i, end - i);
                    CodeTokenKind kind = ClassifyWord(line, i, end, word);
                    if (kind == CodeTokenKind.Plain)
                    {
                        if (plainStart < 0) plainStart = i;
                    }
                    else
                    {
                        FlushPlain(tokens, ref plainStart, i);
                        tokens.Add(new CodeToken(i, end - i, kind));
                    }
                    i = end;
                }
                else
                {
                    if (plainStart < 0) plainStart = i;
                    i++;
                }
            }
            FlushPlain(tokens, ref plainStart, n);
            return tokens;
        }

        // Tokenizes a whole script, line by line.
        public static List<CodeToken>[] TokenizeAll(string[] lines)
        {
            if (lines == null) return new List<CodeToken>[0];
            var all = new List<CodeToken>[lines.Length];
            int state = StateNormal;
            for (int i = 0; i < lines.Length; i++) all[i] = TokenizeLine(lines[i], ref state);
            return all;
        }

        static void FlushPlain(List<CodeToken> tokens, ref int plainStart, int end)
        {
            if (plainStart >= 0 && end > plainStart) tokens.Add(new CodeToken(plainStart, end - plainStart, CodeTokenKind.Plain));
            plainStart = -1;
        }

        static bool IsVerbatimStart(string line, int i)
        {
            int n = line.Length;
            if (line[i] == '@' && i + 1 < n && line[i + 1] == '"') return true;
            if (line[i] == '$' && i + 2 < n && line[i + 1] == '@' && line[i + 2] == '"') return true;
            if (line[i] == '@' && i + 2 < n && line[i + 1] == '$' && line[i + 2] == '"') return true;
            return false;
        }

        // From `from`, the end (exclusive) of a quoted run closed by `quote`, honouring backslashes.
        static int EndOfQuoted(string line, int from, char quote)
        {
            int i = from;
            while (i < line.Length)
            {
                if (line[i] == '\\') { i += 2; continue; }
                if (line[i] == quote) return i + 1;
                i++;
            }
            return line.Length;
        }

        // A verbatim string ends at a single quote mark; two quote marks are one quote inside the text.
        static int EndOfVerbatim(string line, int from, ref int state)
        {
            int i = Clamp(from, 0, line.Length);
            while (i < line.Length)
            {
                if (line[i] == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { i += 2; continue; }
                    state = StateNormal;
                    return i + 1;
                }
                i++;
            }
            return line.Length;
        }

        static bool PreviousIsWordChar(string line, int i)
        {
            return i > 0 && (char.IsLetterOrDigit(line[i - 1]) || line[i - 1] == '_');
        }

        static CodeTokenKind ClassifyWord(string line, int start, int end, string word)
        {
            if (BuiltInTypes.Contains(word)) return CodeTokenKind.Type;
            if (Keywords.Contains(word)) return CodeTokenKind.Keyword;
            if (!char.IsUpper(word[0])) return CodeTokenKind.Plain;

            // A capitalised name is taken as a type name unless it is a member (after a dot)
            // or a method call (followed by an opening bracket, except after `new` or inside an attribute).
            char before = PreviousNonSpace(line, start);
            char after = NextNonSpace(line, end);
            if (before == '.') return CodeTokenKind.Plain;
            if (after == '<' && GenericCallFollows(line, end))
                after = '(';   // GetComponent<Rigidbody>() is a method call, like GetComponent()
            if (after == '(')
            {
                if (before == '[' || PreviousWord(line, start) == "new") return CodeTokenKind.Type;
                return CodeTokenKind.Plain;
            }
            return CodeTokenKind.Type;
        }

        // True when the text from `from` is "<...>" followed by an opening bracket.
        static bool GenericCallFollows(string line, int from)
        {
            int open = line.IndexOf('<', from);
            if (open < 0) return false;
            int depth = 0;
            for (int k = open; k < line.Length; k++)
            {
                if (line[k] == '<') depth++;
                else if (line[k] == '>')
                {
                    depth--;
                    if (depth == 0) return NextNonSpace(line, k + 1) == '(';
                }
                else if (line[k] == ';' || line[k] == '(' || line[k] == '=') return false;
            }
            return false;
        }

        static char PreviousNonSpace(string line, int i)
        {
            for (int k = i - 1; k >= 0; k--) if (!char.IsWhiteSpace(line[k])) return line[k];
            return '\0';
        }

        static char NextNonSpace(string line, int i)
        {
            for (int k = i; k < line.Length; k++) if (!char.IsWhiteSpace(line[k])) return line[k];
            return '\0';
        }

        static string PreviousWord(string line, int i)
        {
            int k = i - 1;
            while (k >= 0 && char.IsWhiteSpace(line[k])) k--;
            int end = k + 1;
            while (k >= 0 && (char.IsLetterOrDigit(line[k]) || line[k] == '_')) k--;
            return end > k + 1 ? line.Substring(k + 1, end - k - 1) : "";
        }

        static int Clamp(int v, int min, int max)
        {
            return v < min ? min : (v > max ? max : v);
        }
    }
}
