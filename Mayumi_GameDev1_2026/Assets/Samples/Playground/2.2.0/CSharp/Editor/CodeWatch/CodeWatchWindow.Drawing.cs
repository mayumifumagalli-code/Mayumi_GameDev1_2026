using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // Code Watch, part two: drawing the code (left) and the side panel (right).
    public partial class CodeWatchWindow
    {
        static readonly Color GlowColor = new Color(1f, 0.78f, 0.2f);
        static readonly Color ChangeColor = new Color(0.35f, 0.8f, 1f);
        static readonly Color EventColor = new Color(0.75f, 0.5f, 1f);
        static readonly Color MarkColor = new Color(0.3f, 0.6f, 1f);
        static readonly Color RightColor = new Color(0.3f, 0.8f, 0.4f);
        static readonly Color MissedColor = new Color(0.95f, 0.35f, 0.3f);
        static readonly Color UnexpectedColor = new Color(1f, 0.6f, 0.15f);

        const float DimAlpha = 0.38f;

        GUIStyle codeStyle, tokenStyle, gutterStyle, countStyle, valueStyle, sideStyle, noteStyle, wrapStyle;
        float charWidth = 7f;
        float lineHeight = 18f;

        // The coloured pieces of each line, made once per script.
        struct Piece
        {
            public string text;
            public int column;
            public CodeTokenKind kind;
        }

        List<Piece>[] pieces;
        int maxColumns;

        void BuildStyles()
        {
            if (codeStyle != null) return;
            Font mono = Font.CreateDynamicFontFromOSFont(new[] { "Consolas", "Menlo", "Courier New" }, fontSize);
            codeStyle = new GUIStyle(EditorStyles.label) { font = mono, fontSize = fontSize, richText = false, wordWrap = false, alignment = TextAnchor.MiddleLeft };
            codeStyle.padding = new RectOffset(0, 0, 0, 0);
            codeStyle.margin = new RectOffset(0, 0, 0, 0);
            tokenStyle = new GUIStyle(codeStyle);
            gutterStyle = new GUIStyle(codeStyle) { alignment = TextAnchor.MiddleRight };
            gutterStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            countStyle = new GUIStyle(codeStyle) { alignment = TextAnchor.MiddleRight, fontStyle = FontStyle.Bold };
            countStyle.normal.textColor = EditorGUIUtility.isProSkin ? GlowColor : new Color(0.7f, 0.45f, 0f);
            valueStyle = new GUIStyle(codeStyle);
            valueStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.55f, 0.8f, 0.95f) : new Color(0.1f, 0.35f, 0.6f);
            sideStyle = new GUIStyle(codeStyle) { fontSize = Mathf.Max(10, fontSize - 1) };
            sideStyle.padding = new RectOffset(2, 2, 1, 1);
            noteStyle = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
            wrapStyle = new GUIStyle(EditorStyles.label) { wordWrap = true };

            float ten = codeStyle.CalcSize(new GUIContent("0000000000")).x;
            float twenty = codeStyle.CalcSize(new GUIContent("00000000000000000000")).x;
            charWidth = Mathf.Max(4f, (twenty - ten) / 10f);
            lineHeight = Mathf.Ceil(codeStyle.lineHeight) + 3f;
        }

        void EnsurePieces()
        {
            if (pieces != null && pieces.Length == lines.Length) return;
            pieces = new List<Piece>[lines.Length];
            maxColumns = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                var list = new List<Piece>();
                string line = lines[i];
                maxColumns = Mathf.Max(maxColumns, line.Length);
                if (i < tokens.Length && tokens[i] != null)
                {
                    foreach (CodeToken t in tokens[i])
                    {
                        if (t.start < 0 || t.length <= 0 || t.start + t.length > line.Length) continue;
                        string text = line.Substring(t.start, t.length);
                        if (text.Trim().Length == 0) continue;
                        list.Add(new Piece { text = text, column = t.start, kind = t.kind });
                    }
                }
                pieces[i] = list;
            }
        }

        static Color TokenColor(CodeTokenKind kind)
        {
            bool dark = EditorGUIUtility.isProSkin;
            switch (kind)
            {
                case CodeTokenKind.Keyword: return dark ? new Color(0.40f, 0.62f, 0.95f) : new Color(0.0f, 0.2f, 0.8f);
                case CodeTokenKind.Type: return dark ? new Color(0.31f, 0.80f, 0.69f) : new Color(0.1f, 0.45f, 0.55f);
                case CodeTokenKind.Text: return dark ? new Color(0.85f, 0.60f, 0.45f) : new Color(0.65f, 0.1f, 0.1f);
                case CodeTokenKind.Number: return dark ? new Color(0.72f, 0.85f, 0.60f) : new Color(0.05f, 0.5f, 0.3f);
                case CodeTokenKind.Comment: return dark ? new Color(0.45f, 0.65f, 0.40f) : new Color(0.25f, 0.5f, 0.2f);
                default: return dark ? new Color(0.85f, 0.85f, 0.85f) : new Color(0.1f, 0.1f, 0.1f);
            }
        }

        // A faint colour per section; the five usual sections each have their own.
        static Color SectionColor(string name)
        {
            string n = name.ToLowerInvariant();
            if (n.Contains("variable")) return new Color(0.3f, 0.55f, 1f);
            if (n.Contains("event")) return new Color(0.75f, 0.45f, 1f);
            if (n.Contains("lifecycle") || n.Contains("unity")) return new Color(0.3f, 0.85f, 0.45f);
            if (n.Contains("action")) return new Color(1f, 0.6f, 0.2f);
            if (n.Contains("helper")) return new Color(0.6f, 0.6f, 0.6f);
            Color[] others = { new Color(0.9f, 0.4f, 0.6f), new Color(0.4f, 0.8f, 0.9f), new Color(0.85f, 0.8f, 0.3f) };
            int sum = 0;
            foreach (char c in n) sum += c;
            return others[sum % others.Length];
        }

        static Color WithAlpha(Color c, float a)
        {
            c.a = a;
            return c;
        }

        // ---------------------------------------------------------------- code

        void DrawCode()
        {
            EnsurePieces();
            float cw = charWidth;
            float h = lineHeight;
            float numberWidth = cw * Mathf.Max(3, lines.Length.ToString().Length) + 6f;
            const float markWidth = 16f;
            float countWidth = cw * 6f + 4f;
            float gutterWidth = numberWidth + markWidth + countWidth + 8f;
            float contentWidth = gutterWidth + (maxColumns + 40) * cw;

            bool hide = PredictHidesRuns;
            bool showReveal = predictOn && revealed;
            bool playing = EditorApplication.isPlaying;
            Event e = Event.current;

            codeScroll = EditorGUILayout.BeginScrollView(codeScroll);
            float viewTop = codeScroll.y;
            float viewBottom = codeScroll.y + position.height;

            for (int i = 0; i < lines.Length; i++)
            {
                int line = i + 1;
                Rect row = GUILayoutUtility.GetRect(contentWidth, 100000f, h, h);
                var marginRect = new Rect(row.x, row.y, numberWidth + markWidth, row.height);

                // Predict: a click in the margin marks or unmarks a line.
                if (hide && IsExecutable(line))
                {
                    EditorGUIUtility.AddCursorRect(marginRect, MouseCursor.Link);
                    if (e.type == EventType.MouseDown && e.button == 0 && marginRect.Contains(e.mousePosition))
                    {
                        ToggleMarked(line);
                        e.Use();
                        Repaint();
                    }
                }

                if (e.type != EventType.Repaint) continue;
                if (row.yMax < viewTop || row.y > viewBottom) continue;

                var markRect = new Rect(row.x + numberWidth + 4f, row.y + h * 0.5f - 4f, 8f, 8f);
                var countRect = new Rect(row.x + numberWidth + markWidth, row.y, countWidth, row.height);
                float textX = row.x + gutterWidth;

                // The section band.
                string section = i < sectionOfLine.Length ? sectionOfLine[i] : null;
                if (section != null)
                {
                    Color sc = SectionColor(section);
                    EditorGUI.DrawRect(row, WithAlpha(sc, EditorGUIUtility.isProSkin ? 0.06f : 0.08f));
                    EditorGUI.DrawRect(new Rect(row.x, row.y, 3f, row.height), WithAlpha(sc, 0.6f));
                }

                bool dim = false;
                string count = null;

                if (hide)
                {
                    if (IsMarked(line))
                    {
                        EditorGUI.DrawRect(row, WithAlpha(MarkColor, 0.18f));
                        EditorGUI.DrawRect(markRect, MarkColor);
                    }
                    else if (IsExecutable(line))
                    {
                        DrawOutline(markRect, WithAlpha(MarkColor, 0.35f));
                    }
                }
                else if (showReveal)
                {
                    bool marked = IsMarked(line);
                    int ranCount = RevealCount(line);
                    Color c = Color.clear;
                    if (marked && ranCount > 0) c = RightColor;
                    else if (marked) c = MissedColor;
                    else if (ranCount > 0) c = UnexpectedColor;
                    if (c != Color.clear)
                    {
                        EditorGUI.DrawRect(row, WithAlpha(c, 0.2f));
                        if (marked) EditorGUI.DrawRect(markRect, c);
                        else DrawOutline(markRect, c);
                    }
                    if (ranCount > 0) count = "×" + ranCount;
                    dim = IsExecutable(line) && ranCount == 0 && !marked;
                }
                else
                {
                    float glow = Glow(line);
                    if (glow > 0f)
                    {
                        EditorGUI.DrawRect(row, WithAlpha(GlowColor, 0.08f + 0.32f * glow));
                        EditorGUI.DrawRect(markRect, GlowColor);
                        uint delta;
                        if (lastDeltaByLine.TryGetValue(line, out delta)) count = "×" + delta;
                    }
                    else if (ranThisSession.Contains(line) && IsExecutable(line))
                    {
                        // Ran earlier in this session: a small mark, the line itself normal.
                        EditorGUI.DrawRect(new Rect(markRect.x + 1f, markRect.y + 1f, 6f, 6f), WithAlpha(GlowColor, 0.55f));
                    }
                    dim = playing && Ready && IsExecutable(line) && !ranThisSession.Contains(line);
                }

                GUI.Label(new Rect(row.x, row.y, numberWidth, row.height), line.ToString(), gutterStyle);
                if (count != null) GUI.Label(countRect, count, countStyle);

                foreach (Piece p in pieces[i])
                {
                    Color c = TokenColor(p.kind);
                    if (dim) c.a = DimAlpha;
                    tokenStyle.normal.textColor = c;
                    GUI.Label(new Rect(textX + p.column * cw, row.y, p.text.Length * cw + cw, row.height), p.text, tokenStyle);
                }

                FieldInfo field;
                if (!hide && fieldByLine.TryGetValue(line, out field))
                    DrawLineValue(field, new Rect(textX + (lines[i].TrimEnd().Length + 3) * cw, row.y, 600f, row.height));
            }
            EditorGUILayout.EndScrollView();
        }

        static void DrawOutline(Rect r, Color c)
        {
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 1f), c);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1f, r.width, 1f), c);
            EditorGUI.DrawRect(new Rect(r.x, r.y, 1f, r.height), c);
            EditorGUI.DrawRect(new Rect(r.xMax - 1f, r.y, 1f, r.height), c);
        }

        // The value at the end of the line that declares a variable (or an event's listeners).
        void DrawLineValue(FieldInfo field, Rect rect)
        {
            string text;
            float highlight = 0f;
            Color highlightColor = ChangeColor;

            if (typeof(UnityEngine.Events.UnityEventBase).IsAssignableFrom(field.FieldType))
            {
                List<string> listeners = Listeners(EventValue(field));
                if (listeners.Count == 0) text = "no listeners";
                else if (listeners.Count == 1) text = listeners[0];
                else text = listeners.Count + " listeners";
                EventHook hook = HookFor(field.Name);
                if (hook != null)
                {
                    highlight = Mathf.Clamp01(1f - (float)(EditorApplication.timeSinceStartup - hook.firedAt) / GlowSeconds);
                    if (highlight > 0f) text = "fired · " + text;
                }
                highlightColor = EventColor;
            }
            else
            {
                string value;
                if (!valueByField.TryGetValue(field.Name, out value)) return;
                highlight = Changed(field.Name);
                string before;
                if (highlight > 0f && previousByField.TryGetValue(field.Name, out before)) text = before + " → " + value;
                else text = "= " + value;
            }

            float width = valueStyle.CalcSize(new GUIContent(text)).x + 8f;
            var r = new Rect(rect.x - 4f, rect.y + 1f, width, rect.height - 2f);
            if (highlight > 0f) EditorGUI.DrawRect(r, WithAlpha(highlightColor, 0.35f * highlight));
            GUI.Label(new Rect(rect.x, rect.y, width, rect.height), text, valueStyle);
        }

        // ---------------------------------------------------------------- side panel

        void DrawSide()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(SidePanelWidth)))
            {
                sideScroll = EditorGUILayout.BeginScrollView(sideScroll);
                bool hide = PredictHidesRuns;
                bool showReveal = predictOn && revealed;
                float rowHeight = Mathf.Ceil(sideStyle.lineHeight) + 4f;

                GUILayout.Label("Variables", EditorStyles.boldLabel);
                if (fields.Length == 0) GUILayout.Label("No variables.", noteStyle);
                foreach (FieldInfo f in fields)
                {
                    string access = f.IsPublic ? "public " : "";
                    string declaration = access + CodeLines.TypeName(f.FieldType) + " " + f.Name;
                    string value;
                    valueByField.TryGetValue(f.Name, out value);

                    if (hide)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Label(declaration, sideStyle, GUILayout.Height(rowHeight));
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("expect", EditorStyles.miniLabel, GUILayout.Width(40f));
                            string typed = ExpectedFor(f.Name);
                            string now = EditorGUILayout.TextField(typed, GUILayout.Width(64f));
                            if (now != typed) SetExpected(f.Name, now);
                        }
                        continue;
                    }

                    string expected = ExpectedFor(f.Name);
                    if (showReveal && expected.Trim().Length > 0)
                    {
                        string actual = RevealActualFor(f.Name);
                        bool right = CodeLines.ValueMatches(expected, actual);
                        Color c = WithAlpha(right ? RightColor : MissedColor, 0.25f);
                        Rect first = GUILayoutUtility.GetRect(100f, SidePanelWidth, rowHeight, rowHeight);
                        EditorGUI.DrawRect(first, c);
                        GUI.Label(first, declaration + " = " + actual, sideStyle);
                        Rect second = GUILayoutUtility.GetRect(100f, SidePanelWidth, rowHeight, rowHeight);
                        EditorGUI.DrawRect(second, c);
                        GUI.Label(second, "    expected " + expected.Trim() + (right ? " · right" : " · not right"), sideStyle);
                        continue;
                    }

                    float k = Changed(f.Name);
                    string before;
                    string shownValue = (k > 0f && previousByField.TryGetValue(f.Name, out before)) ? before + " → " + value : value;
                    Rect row = GUILayoutUtility.GetRect(100f, SidePanelWidth, rowHeight, rowHeight);
                    if (k > 0f) EditorGUI.DrawRect(row, WithAlpha(ChangeColor, 0.35f * k));
                    GUI.Label(row, declaration + " = " + shownValue, sideStyle);
                }

                if (eventFields.Length > 0)
                {
                    GUILayout.Space(10f);
                    GUILayout.Label("Events", EditorStyles.boldLabel);
                    foreach (FieldInfo f in eventFields)
                    {
                        EventHook hook = hide ? null : HookFor(f.Name);
                        Rect row = GUILayoutUtility.GetRect(100f, SidePanelWidth, rowHeight, rowHeight);
                        if (hook != null)
                        {
                            float k = Mathf.Clamp01(1f - (float)(EditorApplication.timeSinceStartup - hook.firedAt) / GlowSeconds);
                            if (k > 0f) EditorGUI.DrawRect(row, WithAlpha(EventColor, 0.4f * k));
                        }
                        string fired = hook != null ? "   fired ×" + hook.fired : "";
                        GUI.Label(row, f.Name + fired, sideStyle);

                        List<string> listeners = Listeners(EventValue(f));
                        if (listeners.Count == 0) GUILayout.Label("    No listeners set in the Inspector.", noteStyle);
                        foreach (string listener in listeners) GUILayout.Label("    " + listener, noteStyle);
                    }
                }

                GUILayout.Space(10f);
                GUILayout.Label("Methods", EditorStyles.boldLabel);
                if (methods.Length == 0) GUILayout.Label("No methods.", noteStyle);
                foreach (MethodInfo m in methods) DrawMethod(m, hide || showReveal);

                EditorGUILayout.EndScrollView();
            }
        }

        void DrawMethod(MethodInfo m, bool hideCalls)
        {
            ParameterInfo[] parameters = m.GetParameters();
            string description;
            bool unity = CodeLines.UnityMessages.TryGetValue(m.Name, out description);

            using (new EditorGUILayout.HorizontalScope())
            {
                uint calls;
                callsByMethod.TryGetValue(m, out calls);
                GUILayout.Label(m.Name + "(" + ParameterTypes(parameters) + ")", sideStyle, GUILayout.Width(150f));
                GUILayout.Label(hideCalls ? "" : "×" + calls, sideStyle, GUILayout.Width(52f));

                if (unity)
                {
                    GUILayout.FlexibleSpace();
                }
                else if (m.IsPublic && !m.IsStatic && parameters.Length == 0)
                {
                    using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
                        if (GUILayout.Button("Call", GUILayout.Width(52f))) CallMethod(m, null);
                }
                else if (m.IsPublic && !m.IsStatic && parameters.Length == 1 && IsSimpleType(parameters[0].ParameterType))
                {
                    object argument = ArgumentField(m, parameters[0].ParameterType);
                    using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
                        if (GUILayout.Button("Call", GUILayout.Width(44f))) CallMethod(m, new[] { argument });
                }
                else
                {
                    GUILayout.Label(m.IsPublic ? "needs values" : "private", EditorStyles.miniLabel);
                }
            }
            if (unity) GUILayout.Label("Unity calls it " + description + ".", noteStyle);
        }

        static string ParameterTypes(ParameterInfo[] parameters)
        {
            var names = new List<string>();
            foreach (ParameterInfo p in parameters) names.Add(CodeLines.TypeName(p.ParameterType));
            return string.Join(", ", names.ToArray());
        }

        static bool IsSimpleType(System.Type t)
        {
            return t == typeof(float) || t == typeof(int) || t == typeof(bool) || t == typeof(string);
        }

        // The small field beside a Call button; returns the value it holds.
        object ArgumentField(MethodInfo m, System.Type t)
        {
            object value;
            if (!argumentByMethod.TryGetValue(m, out value))
            {
                if (t == typeof(float)) value = 1f;
                else if (t == typeof(int)) value = 1;
                else if (t == typeof(bool)) value = true;
                else value = "";
            }
            if (t == typeof(float)) value = EditorGUILayout.FloatField((float)value, GUILayout.Width(48f));
            else if (t == typeof(int)) value = EditorGUILayout.IntField((int)value, GUILayout.Width(48f));
            else if (t == typeof(bool)) value = EditorGUILayout.Toggle((bool)value, GUILayout.Width(18f));
            else value = EditorGUILayout.TextField((string)value, GUILayout.Width(70f));
            argumentByMethod[m] = value;
            return value;
        }

        void CallMethod(MethodInfo m, object[] arguments)
        {
            try
            {
                m.Invoke(target, arguments);
            }
            catch (TargetInvocationException ex)
            {
                // The method itself failed: show its error in the Console, as if the game had called it.
                Debug.LogException(ex.InnerException != null ? ex.InnerException : ex, target);
            }
        }
    }
}
