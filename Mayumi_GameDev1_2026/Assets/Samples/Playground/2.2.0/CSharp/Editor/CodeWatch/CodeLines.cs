using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // What Code Watch reads from the text of a script: the sections, the line that declares
    // each variable, which lines are only braces, and how values are written and compared.
    // Plain functions with no editor state, so they can be tested on their own.
    public static class CodeLines
    {
        static readonly Regex SectionLine = new Regex(@"^//\s*=+\s*([A-Za-z][A-Za-z0-9 \-\(\)]*?)\s*=+\s*$");
        static readonly Regex RuleLine = new Regex(@"^//\s*=+\s*$");
        static readonly HashSet<string> NotATypeWord = new HashSet<string>
        {
            "return", "else", "new", "throw", "case", "goto", "yield", "await", "var", "using", "if", "for", "foreach", "while"
        };

        // When Unity calls each of its methods, said plainly. Shown as "Unity calls it " + the text.
        public static readonly Dictionary<string, string> UnityMessages = new Dictionary<string, string>
        {
            { "Awake", "once, when the object is loaded, before Start" },
            { "OnEnable", "each time the script or its object is switched on" },
            { "Start", "once, when the object first appears, before its first Update" },
            { "FixedUpdate", "at every physics step (by default 50 times a second)" },
            { "Update", "every frame" },
            { "LateUpdate", "every frame, after every Update has run" },
            { "OnDisable", "each time the script or its object is switched off" },
            { "OnDestroy", "once, when the object is destroyed" },
            { "OnTriggerEnter", "when something enters the trigger" },
            { "OnTriggerStay", "at every physics step while something is inside the trigger" },
            { "OnTriggerExit", "when something leaves the trigger" },
            { "OnCollisionEnter", "when something starts touching this object" },
            { "OnCollisionStay", "at every physics step while something touches this object" },
            { "OnCollisionExit", "when something stops touching this object" },
            { "OnTriggerEnter2D", "when something enters the 2D trigger" },
            { "OnTriggerStay2D", "at every physics step while something is inside the 2D trigger" },
            { "OnTriggerExit2D", "when something leaves the 2D trigger" },
            { "OnCollisionEnter2D", "when something starts touching this 2D object" },
            { "OnCollisionStay2D", "at every physics step while something touches this 2D object" },
            { "OnCollisionExit2D", "when something stops touching this 2D object" },
            { "OnMouseDown", "when the mouse button is pressed over this object" },
            { "OnMouseUp", "when the mouse button is released, after a press over this object" },
            { "OnMouseEnter", "when the mouse pointer moves onto this object" },
            { "OnMouseExit", "when the mouse pointer moves off this object" },
            { "OnBecameVisible", "when a camera starts to see this object" },
            { "OnBecameInvisible", "when no camera sees this object any more" },
            { "OnDrawGizmos", "while it draws the Scene view, in the editor only, to show what the script does" },
            { "OnDrawGizmosSelected", "while it draws the Scene view, when the object is selected, in the editor only" }
        };

        // The code of a line without its comments, trimmed.
        public static string CodeOnly(string line, List<CodeToken> tokens)
        {
            if (string.IsNullOrEmpty(line) || tokens == null) return "";
            var sb = new StringBuilder();
            foreach (CodeToken t in tokens)
            {
                if (t.start < 0 || t.start + t.length > line.Length) continue;
                if (t.kind == CodeTokenKind.Comment) sb.Append(' ');
                else sb.Append(line, t.start, t.length);
            }
            return sb.ToString().Trim();
        }

        // True for a line whose code is only braces or semicolons, like "{" or "}".
        public static bool IsBracesOnly(string code)
        {
            if (string.IsNullOrEmpty(code)) return false;
            foreach (char c in code)
                if (c != '{' && c != '}' && c != ';' && !char.IsWhiteSpace(c)) return false;
            return true;
        }

        // Brace depth before each line (index = line - 1), counting only braces in code.
        public static int[] DepthBefore(string[] lines, List<CodeToken>[] tokens)
        {
            int n = lines == null ? 0 : lines.Length;
            var depth = new int[n + 1];
            int d = 0;
            for (int i = 0; i < n; i++)
            {
                depth[i] = d;
                if (tokens == null || i >= tokens.Length || tokens[i] == null) continue;
                foreach (CodeToken t in tokens[i])
                {
                    if (t.kind != CodeTokenKind.Plain) continue;
                    for (int k = t.start; k < t.start + t.length && k < lines[i].Length; k++)
                    {
                        if (lines[i][k] == '{') d++;
                        else if (lines[i][k] == '}') d--;
                    }
                }
            }
            depth[n] = d;
            return depth;
        }

        // The section name of each line (index = line - 1), or null. A section starts at a line
        // like "// ===== Variables =====" and ends at the next one, at a plain "// =====" rule,
        // or before the brace that closes the class.
        public static string[] Sections(string[] lines, List<CodeToken>[] tokens)
        {
            int n = lines == null ? 0 : lines.Length;
            var result = new string[n];
            int[] depth = DepthBefore(lines, tokens);
            string current = null;
            for (int i = 0; i < n; i++)
            {
                string trimmed = (lines[i] ?? "").Trim();
                Match m = SectionLine.Match(trimmed);
                if (m.Success)
                {
                    current = m.Groups[1].Value.Trim();
                    result[i] = current;
                    continue;
                }
                if (RuleLine.IsMatch(trimmed)) { current = null; continue; }
                if (current != null && depth[i] >= 1 && depth[i + 1] <= 0) { current = null; continue; }
                result[i] = current;
            }
            return result;
        }

        // The line (1-based) that declares the variable `name`, or 0 if none is found.
        public static int DeclarationLine(string[] lines, List<CodeToken>[] tokens, string name)
        {
            if (lines == null || string.IsNullOrEmpty(name)) return 0;
            var pattern = new Regex(@"^(\[[^\]]*\]\s*)*((public|private|protected|internal|static|readonly|const|volatile)\s+)*"
                + @"(?<type>[A-Za-z_][\w\.]*(\s*<[^;=()]*>)?(\s*\[\s*\])?\??)\s+" + Regex.Escape(name) + @"\s*(=|;|,)");
            for (int i = 0; i < lines.Length; i++)
            {
                string code = CodeOnly(lines[i], tokens != null && i < tokens.Length ? tokens[i] : null);
                Match m = pattern.Match(code);
                if (!m.Success) continue;
                if (NotATypeWord.Contains(m.Groups["type"].Value)) continue;
                return i + 1;
            }
            return 0;
        }

        // A value as the window shows it.
        public static string Format(object value)
        {
            if (value == null) return "null";
            var unityObject = value as UnityEngine.Object;
            if (!ReferenceEquals(unityObject, null)) return unityObject == null ? "None" : unityObject.name;
            if (value is float) return ((float)value).ToString("0.00", CultureInfo.InvariantCulture);
            if (value is double) return ((double)value).ToString("0.00", CultureInfo.InvariantCulture);
            if (value is bool) return (bool)value ? "true" : "false";
            if (value is string) return "\"" + value + "\"";
            if (value is Vector3) { Vector3 v = (Vector3)value; return "(" + F1(v.x) + ", " + F1(v.y) + ", " + F1(v.z) + ")"; }
            if (value is Vector2) { Vector2 v = (Vector2)value; return "(" + F1(v.x) + ", " + F1(v.y) + ")"; }
            if (value is Color) { Color c = (Color)value; return "(" + F1(c.r) + ", " + F1(c.g) + ", " + F1(c.b) + ", " + F1(c.a) + ")"; }
            if (value is IFormattable) return ((IFormattable)value).ToString(null, CultureInfo.InvariantCulture);
            // An array or a List: how many values it holds, not its type name.
            var collection = value as System.Collections.ICollection;
            if (collection != null) return collection.Count == 1 ? "1 item" : collection.Count + " items";
            return value.ToString();
        }

        static string F1(float f)
        {
            return f.ToString("0.0", CultureInfo.InvariantCulture);
        }

        public static string TypeName(Type t)
        {
            if (t == typeof(float)) return "float";
            if (t == typeof(int)) return "int";
            if (t == typeof(bool)) return "bool";
            if (t == typeof(string)) return "string";
            if (t == typeof(double)) return "double";
            if (t == typeof(void)) return "void";
            if (t.IsArray) return TypeName(t.GetElementType()) + "[]";
            if (t.IsGenericType)
            {
                // List`1 is written as List<Vector3>.
                int tick = t.Name.IndexOf('`');
                string name = tick > 0 ? t.Name.Substring(0, tick) : t.Name;
                var arguments = new List<string>();
                foreach (Type a in t.GetGenericArguments()) arguments.Add(TypeName(a));
                return name + "<" + string.Join(", ", arguments.ToArray()) + ">";
            }
            return t.Name;
        }

        // Does the value a student typed match the actual value? Numbers are compared as numbers
        // (2 matches 2.00; a small difference is allowed), several numbers one by one (a Vector3 or
        // a Color), anything else as text, ignoring capitals, spaces and quote marks.
        public static bool ValueMatches(string expected, string actual)
        {
            if (expected == null || actual == null) return false;
            List<float> e = Numbers(expected);
            List<float> a = Numbers(actual);
            if (e.Count > 0 && e.Count == a.Count && LooksNumeric(expected) && LooksNumeric(actual))
            {
                for (int i = 0; i < e.Count; i++)
                    if (Mathf.Abs(e[i] - a[i]) > 0.051f) return false;
                return true;
            }
            return Normal(expected) == Normal(actual);
        }

        static bool LooksNumeric(string s)
        {
            foreach (char c in s)
                if (!(char.IsDigit(c) || c == '.' || c == '-' || c == ',' || c == '(' || c == ')' || c == 'f' || char.IsWhiteSpace(c))) return false;
            return true;
        }

        static List<float> Numbers(string s)
        {
            var list = new List<float>();
            foreach (Match m in Regex.Matches(s, @"-?\d+(\.\d+)?|-?\.\d+"))
            {
                float f;
                if (float.TryParse(m.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out f)) list.Add(f);
            }
            return list;
        }

        static string Normal(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s)
                if (!char.IsWhiteSpace(c) && c != '"' && c != '\'') sb.Append(char.ToLowerInvariant(c));
            return sb.ToString();
        }
    }
}
