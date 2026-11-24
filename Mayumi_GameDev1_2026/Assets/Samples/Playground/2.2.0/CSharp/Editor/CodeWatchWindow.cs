using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;

namespace Mavrodiev.Playground.CSharp
{
    // Code Watch: shows the source of a script on the selected object while the game runs.
    // A line is bright while it runs (with how many times it ran in that frame), keeps a mark in
    // the margin once it has run, and stays dim if it never ran. The script's variables are shown
    // with their current values, its methods with call counts and a Call button, its events with
    // their listeners. Predict mode hides all of that until the student has marked a guess.
    // It reads Unity's code coverage data (UnityEngine.TestTools.Coverage), so the scripts
    // themselves need no extra code. Editor only; needs code coverage turned on.
    //
    // The class is split in three files: this one (data and toolbar),
    // CodeWatch/CodeWatchWindow.Drawing.cs (the code and the side panel),
    // CodeWatch/CodeWatchWindow.Predict.cs (Predict mode).
    public partial class CodeWatchWindow : EditorWindow
    {
        const float GlowSeconds = 1.5f;
        const float ChangeSeconds = 2.5f;
        const float SidePanelWidth = 310f;
        const string FontSizeKey = "Mavrodiev.CodeWatch.FontSize";
        const int DefaultFontSize = 13;
        const int MinFontSize = 10;
        const int MaxFontSize = 24;

        [SerializeField] MonoBehaviour target;
        [SerializeField] bool pinned;
        [SerializeField] string preferredScript = "";

        MonoBehaviour shown;
        string[] lines = new string[0];
        List<CodeToken>[] tokens = new List<CodeToken>[0];
        string[] sectionOfLine = new string[0];
        bool[] executable = new bool[1];                 // by line number (index 0 unused)
        MethodInfo[] methods = new MethodInfo[0];
        FieldInfo[] fields = new FieldInfo[0];           // variables, events left out
        FieldInfo[] eventFields = new FieldInfo[0];
        readonly Dictionary<int, FieldInfo> fieldByLine = new Dictionary<int, FieldInfo>();

        // Per source line (1-based): hits so far, hits in the last frame it ran, when it last ran.
        readonly Dictionary<int, uint> totalByLine = new Dictionary<int, uint>();
        readonly Dictionary<int, uint> lastDeltaByLine = new Dictionary<int, uint>();
        readonly Dictionary<int, double> lastHitTimeByLine = new Dictionary<int, double>();
        readonly HashSet<int> hitInLastSample = new HashSet<int>();
        readonly HashSet<int> ranThisSession = new HashSet<int>();
        readonly Dictionary<MethodInfo, uint> callsByMethod = new Dictionary<MethodInfo, uint>();

        // Variables: the value now, the value before the last change, when it changed.
        readonly Dictionary<string, string> valueByField = new Dictionary<string, string>();
        readonly Dictionary<string, string> previousByField = new Dictionary<string, string>();
        readonly Dictionary<string, double> changeTimeByField = new Dictionary<string, double>();
        double lastValueRead;

        // The value typed beside a Call button, per method.
        readonly Dictionary<MethodInfo, object> argumentByMethod = new Dictionary<MethodInfo, object>();

        // Events: a listener added in Play mode to see when the event fires.
        readonly List<EventHook> hooks = new List<EventHook>();

        // How many objects in the open scenes carry the shown script (the counts are per script).
        int objectsWithScript;
        double lastObjectCount = -100.0;

        int lastSampledFrame = -1;
        double lastRepaint;
        double lastActivity;
        Vector2 codeScroll;
        Vector2 sideScroll;
        int fontSize;

        class EventHook
        {
            public string field;
            public UnityEventBase unityEvent;
            public Delegate valueListener;      // for an event that sends a value (UnityEvent<T>)
            public MethodInfo removeListener;   // its RemoveListener method
            public double firedAt = -100.0;
            public int fired;

            public void OnFired()
            {
                firedAt = EditorApplication.timeSinceStartup;
                fired++;
            }

            public void OnFiredWith<T>(T value)
            {
                OnFired();
            }
        }

        [MenuItem("Tools/Mavrodiev/Code Watch")]
        static void Open()
        {
            GetWindow<CodeWatchWindow>("Code Watch").minSize = new Vector2(700f, 320f);
        }

        void OnEnable()
        {
            fontSize = Mathf.Clamp(EditorPrefs.GetInt(FontSizeKey, DefaultFontSize), MinFontSize, MaxFontSize);
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            PickFromSelection();
        }

        void OnDisable()
        {
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            RemoveHooks();
        }

        void OnSelectionChange()
        {
            PickFromSelection();
            Repaint();
        }

        void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode) RemoveHooks();
            if (change == PlayModeStateChange.ExitingEditMode || change == PlayModeStateChange.ExitingPlayMode) Remember();
            if (change == PlayModeStateChange.EnteredEditMode || change == PlayModeStateChange.EnteredPlayMode)
                restoreTicks = 20;
            PredictOnPlayModeChanged(change);
        }

        // The editor can lose the selection when Play starts or stops. The shown script is
        // remembered by its object's place in the hierarchy, and found again once after the change.
        [SerializeField] string rememberedPath = "";
        [SerializeField] string rememberedScript = "";
        [SerializeField] bool restorePending;
        int restoreTicks;   // set when Play has started or stopped; the remembered script wins for that many updates

        void Remember()
        {
            restorePending = target != null;
            restoreTicks = 0;
            if (target == null) return;
            rememberedScript = target.GetType().Name;
            string path = target.gameObject.name;
            for (Transform t = target.transform.parent; t != null; t = t.parent) path = t.name + "/" + path;
            rememberedPath = path;
        }

        bool Restore()
        {
            if (rememberedPath.Length == 0) return false;
            string[] parts = rememberedPath.Split(new[] { '/' }, 2);
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.name != parts[0]) continue;
                    Transform found = parts.Length > 1 ? root.transform.Find(parts[1]) : root.transform;
                    if (found == null) continue;
                    foreach (MonoBehaviour mb in ScriptsOn(found.gameObject))
                        if (mb.GetType().Name == rememberedScript) { target = mb; restorePending = false; return true; }
                }
            }
            return false;
        }

        void PickFromSelection()
        {
            if (pinned && target != null) return;
            // Just after Play has started or stopped, the remembered script comes first.
            bool settled = EditorApplication.isPlayingOrWillChangePlaymode == EditorApplication.isPlaying;
            if (restorePending && settled && Restore()) return;
            GameObject go = Selection.activeGameObject;
            if (go == null) return;
            if (target != null && target.gameObject == go) return;   // same object: keep the tab
            List<MonoBehaviour> scripts = ScriptsOn(go);
            if (scripts.Count == 0) return;
            target = scripts[0];
            foreach (MonoBehaviour mb in scripts)
                if (mb.GetType().Name == preferredScript) { target = mb; break; }
        }

        static List<MonoBehaviour> ScriptsOn(GameObject go)
        {
            var list = new List<MonoBehaviour>();
            if (go == null) return list;
            foreach (MonoBehaviour mb in go.GetComponents<MonoBehaviour>())
                if (mb != null && IsProjectScript(mb)) list.Add(mb);
            return list;
        }

        static bool IsProjectScript(MonoBehaviour mb)
        {
            MonoScript script = MonoScript.FromMonoBehaviour(mb);
            if (script == null) return false;
            // The project's own scripts, and the course packages' (the Toolbox's triggers and actions).
            string path = AssetDatabase.GetAssetPath(script);
            return path.StartsWith("Assets/") || path.StartsWith("Packages/com.mavrodiev.");
        }

        static bool Ready
        {
            get { return Coverage.enabled; }
        }

        static string ScriptPath(MonoBehaviour mb)
        {
            if (mb == null) return "";
            MonoScript script = MonoScript.FromMonoBehaviour(mb);
            return script == null ? "" : AssetDatabase.GetAssetPath(script);
        }

        // ---------------------------------------------------------------- data

        void Load()
        {
            RemoveHooks();
            shown = target;
            totalByLine.Clear();
            lastDeltaByLine.Clear();
            lastHitTimeByLine.Clear();
            hitInLastSample.Clear();
            ranThisSession.Clear();
            callsByMethod.Clear();
            valueByField.Clear();
            previousByField.Clear();
            changeTimeByField.Clear();
            argumentByMethod.Clear();
            fieldByLine.Clear();
            lastSampledFrame = -1;
            lastObjectCount = -100.0;
            objectsWithScript = 0;
            lines = new string[0];
            tokens = new List<CodeToken>[0];
            sectionOfLine = new string[0];
            executable = new bool[1];
            methods = new MethodInfo[0];
            fields = new FieldInfo[0];
            eventFields = new FieldInfo[0];
            pieces = null;
            PredictOnLoad();
            if (target == null) return;

            MonoScript script = MonoScript.FromMonoBehaviour(target);
            if (script != null) lines = script.text.Replace("\r\n", "\n").Replace("\t", "    ").Split('\n');
            tokens = CodeTokenizer.TokenizeAll(lines);
            sectionOfLine = CodeLines.Sections(lines, tokens);

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            Type type = target.GetType();
            var methodList = new List<MethodInfo>();
            foreach (MethodInfo m in type.GetMethods(flags))
                if (!m.IsSpecialName) methodList.Add(m);
            methodList.Sort((a, b) => a.MetadataToken.CompareTo(b.MetadataToken));
            methods = methodList.ToArray();

            var fieldList = new List<FieldInfo>();
            var eventList = new List<FieldInfo>();
            foreach (FieldInfo f in type.GetFields(flags))
            {
                if (f.Name.Contains("<")) continue;
                if (typeof(UnityEventBase).IsAssignableFrom(f.FieldType)) eventList.Add(f);
                else fieldList.Add(f);
            }
            fieldList.Sort((a, b) => a.MetadataToken.CompareTo(b.MetadataToken));
            eventList.Sort((a, b) => a.MetadataToken.CompareTo(b.MetadataToken));
            fields = fieldList.ToArray();
            eventFields = eventList.ToArray();
            foreach (FieldInfo f in fields) AddDeclarationLine(f);
            foreach (FieldInfo f in eventFields) AddDeclarationLine(f);

            executable = FindExecutableLines();
            ReadValues(true);

            // Lines that already ran in this Play session (Start, for example, before the object
            // was selected) count as "ran earlier", not as "running now".
            if (EditorApplication.isPlaying && Ready)
            {
                foreach (KeyValuePair<int, uint> pair in ReadCounts(callsByMethod))
                {
                    totalByLine[pair.Key] = pair.Value;
                    if (pair.Value > 0) ranThisSession.Add(pair.Key);
                }
                lastSampledFrame = Time.frameCount;
            }
            PredictAfterLoad();
        }

        void AddDeclarationLine(FieldInfo f)
        {
            int line = CodeLines.DeclarationLine(lines, tokens, f.Name);
            if (line > 0 && !fieldByLine.ContainsKey(line)) fieldByLine[line] = f;
        }

        // A line is executable when the compiler put a sequence point on it, and it is more than a brace.
        bool[] FindExecutableLines()
        {
            var result = new bool[lines.Length + 1];
            if (!Ready) return result;
            foreach (MethodInfo m in methods)
            {
                foreach (CoveredSequencePoint p in Coverage.GetSequencePointsFor(m))
                {
                    int line = (int)p.line;
                    if (line < 1 || line > lines.Length) continue;
                    string code = CodeLines.CodeOnly(lines[line - 1], tokens[line - 1]);
                    if (code.Length == 0 || CodeLines.IsBracesOnly(code)) continue;
                    result[line] = true;
                }
            }
            return result;
        }

        bool IsExecutable(int line)
        {
            return line > 0 && line < executable.Length && executable[line];
        }

        // The hit count of every line (the highest of its sequence points); also each method's calls.
        Dictionary<int, uint> ReadCounts(Dictionary<MethodInfo, uint> calls)
        {
            var current = new Dictionary<int, uint>();
            if (!Ready) return current;
            foreach (MethodInfo m in methods)
            {
                CoveredSequencePoint[] points = Coverage.GetSequencePointsFor(m);
                uint firstOffset = uint.MaxValue, callCount = 0;
                foreach (CoveredSequencePoint p in points)
                {
                    int line = (int)p.line;
                    uint known;
                    if (!current.TryGetValue(line, out known) || p.hitCount > known) current[line] = p.hitCount;
                    if (p.ilOffset < firstOffset) { firstOffset = p.ilOffset; callCount = p.hitCount; }
                }
                if (calls != null) calls[m] = callCount;
            }
            return current;
        }

        void Tick()
        {
            // Entering Play mode recreates the scene's objects: pick the selected one again.
            if (target == null) PickFromSelection();
            if (restorePending && restoreTicks > 0 && --restoreTicks == 0) restorePending = false;
            if (!ReferenceEquals(target, shown)) { Load(); Repaint(); }
            if (target == null) return;

            double now = EditorApplication.timeSinceStartup;
            bool playing = EditorApplication.isPlaying;
            if (playing) AddHooks();
            if (now - lastObjectCount > 1.0)
            {
                lastObjectCount = now;
                int count = CountObjectsWithScript(target.GetType());
                if (count != objectsWithScript) { objectsWithScript = count; Repaint(); }
            }

            if (playing && Ready && Time.frameCount != lastSampledFrame)
            {
                int frames = lastSampledFrame < 0 ? 1 : Mathf.Max(1, Time.frameCount - lastSampledFrame);
                lastSampledFrame = Time.frameCount;
                Sample(frames);
                ReadValues(false);
                RepaintNow();
            }
            else if (!playing && now - lastValueRead > 0.25)
            {
                // In Edit mode the Inspector can still change a value.
                ReadValues(false);
            }

            // Keep fading marks moving while the game is paused or stopped.
            if (now - lastActivity < ChangeSeconds + 0.1 && now - lastRepaint > 1.0 / 30.0) RepaintNow();
        }

        // The number of GameObjects in the open scenes that have a component of this type, inactive ones included.
        public static int CountObjectsWithScript(Type type)
        {
            if (type == null) return 0;
            var objects = new HashSet<GameObject>();
            foreach (UnityEngine.Object o in Resources.FindObjectsOfTypeAll(type))
            {
                var component = o as Component;
                if (component == null) continue;
                GameObject go = component.gameObject;
                if (EditorUtility.IsPersistent(go)) continue;                  // a prefab asset, not in a scene
                if ((go.hideFlags & HideFlags.HideAndDontSave) != 0) continue;
                if (!go.scene.IsValid() || !go.scene.isLoaded) continue;
                objects.Add(go);
            }
            return objects.Count;
        }

        void RepaintNow()
        {
            lastRepaint = EditorApplication.timeSinceStartup;
            Repaint();
        }

        void Sample(int frames)
        {
            double now = EditorApplication.timeSinceStartup;
            Dictionary<int, uint> current = ReadCounts(callsByMethod);

            hitInLastSample.Clear();
            foreach (KeyValuePair<int, uint> pair in current)
            {
                uint before;
                totalByLine.TryGetValue(pair.Key, out before);
                if (pair.Value < before) before = 0;   // the counters restarted
                uint delta = pair.Value - before;
                totalByLine[pair.Key] = pair.Value;
                if (delta == 0) continue;
                lastDeltaByLine[pair.Key] = (uint)Mathf.CeilToInt(delta / (float)frames);
                lastHitTimeByLine[pair.Key] = now;
                hitInLastSample.Add(pair.Key);
                ranThisSession.Add(pair.Key);
                lastActivity = now;
            }
        }

        void ReadValues(bool first)
        {
            if (target == null) return;
            double now = EditorApplication.timeSinceStartup;
            lastValueRead = now;
            foreach (FieldInfo f in fields)
            {
                string value;
                try { value = CodeLines.Format(f.IsStatic ? f.GetValue(null) : f.GetValue(target)); }
                catch (Exception) { value = "?"; }
                string last;
                if (!first && valueByField.TryGetValue(f.Name, out last) && last != value)
                {
                    previousByField[f.Name] = last;
                    changeTimeByField[f.Name] = now;
                    lastActivity = now;
                }
                valueByField[f.Name] = value;
            }
        }

        // 0 when the line is not running; up to 1 just after it ran.
        float Glow(int line)
        {
            if (hitInLastSample.Contains(line)) return 1f;
            double t;
            if (!lastHitTimeByLine.TryGetValue(line, out t)) return 0f;
            return Mathf.Clamp01(1f - (float)(EditorApplication.timeSinceStartup - t) / GlowSeconds);
        }

        // 0 when the variable has not changed lately; up to 1 just after it changed.
        float Changed(string field)
        {
            double t;
            if (!changeTimeByField.TryGetValue(field, out t)) return 0f;
            return Mathf.Clamp01(1f - (float)(EditorApplication.timeSinceStartup - t) / ChangeSeconds);
        }

        // ---------------------------------------------------------------- events

        void AddHooks()
        {
            if (hooks.Count > 0 || target == null) return;
            foreach (FieldInfo f in eventFields)
            {
                UnityEventBase eventBase;
                try { eventBase = (f.IsStatic ? f.GetValue(null) : f.GetValue(target)) as UnityEventBase; }
                catch (Exception) { eventBase = null; }
                if (eventBase == null) continue;
                var hook = new EventHook { field = f.Name, unityEvent = eventBase };
                UnityEvent plain = eventBase as UnityEvent;
                if (plain != null)
                {
                    plain.AddListener(hook.OnFired);
                    hooks.Add(hook);
                    continue;
                }
                // An event that sends one value, such as UnityEvent<int>: listen with OnFiredWith<int>.
                Type eventType = eventBase.GetType();
                while (eventType != null && !(eventType.IsGenericType && eventType.GetGenericTypeDefinition() == typeof(UnityEvent<>)))
                    eventType = eventType.BaseType;
                if (eventType == null) continue;
                try
                {
                    Type valueType = eventType.GetGenericArguments()[0];
                    MethodInfo handler = typeof(EventHook).GetMethod("OnFiredWith").MakeGenericMethod(valueType);
                    hook.valueListener = Delegate.CreateDelegate(typeof(UnityAction<>).MakeGenericType(valueType), hook, handler);
                    eventType.GetMethod("AddListener").Invoke(eventBase, new object[] { hook.valueListener });
                    hook.removeListener = eventType.GetMethod("RemoveListener");
                    hooks.Add(hook);
                }
                catch (Exception) { }
            }
            // An empty marker so that a script without plain UnityEvents is not searched every frame.
            if (hooks.Count == 0) hooks.Add(new EventHook { field = "" });
        }

        void RemoveHooks()
        {
            foreach (EventHook hook in hooks)
            {
                if (hook.unityEvent is UnityEvent) ((UnityEvent)hook.unityEvent).RemoveListener(hook.OnFired);
                else if (hook.valueListener != null && hook.removeListener != null)
                {
                    try { hook.removeListener.Invoke(hook.unityEvent, new object[] { hook.valueListener }); }
                    catch (Exception) { }
                }
            }
            hooks.Clear();
        }

        EventHook HookFor(string field)
        {
            foreach (EventHook hook in hooks)
                if (hook.field == field && hook.unityEvent != null) return hook;
            return null;
        }

        // The listeners set in the Inspector, as "object → method".
        static List<string> Listeners(UnityEventBase unityEvent)
        {
            var list = new List<string>();
            if (unityEvent == null) return list;
            int count = unityEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                UnityEngine.Object listener = unityEvent.GetPersistentTarget(i);
                string method = unityEvent.GetPersistentMethodName(i);
                string who;
                if (listener == null) who = "None";
                else if (listener is Component) who = ((Component)listener).gameObject.name + " → " + listener.GetType().Name + ".";
                else who = listener.name + " → ";
                string text = who + (string.IsNullOrEmpty(method) ? "(no method)" : method + "()");
                if (unityEvent.GetPersistentListenerState(i) == UnityEventCallState.Off) text += " (off)";
                list.Add(text);
            }
            return list;
        }

        UnityEventBase EventValue(FieldInfo f)
        {
            try { return (f.IsStatic ? f.GetValue(null) : f.GetValue(target)) as UnityEventBase; }
            catch (Exception) { return null; }
        }

        // ---------------------------------------------------------------- toolbar

        void OnGUI()
        {
            BuildStyles();
            if (!ReferenceEquals(target, shown)) Load();

            DrawToolbar();

            if (!Ready)
            {
                EditorGUILayout.HelpBox("Code Watch needs Unity's code coverage turned on. Turning it on reloads the scripts once.", MessageType.Info);
                if (GUILayout.Button("Turn on Code Watch", GUILayout.Height(28f)))
                {
                    Coverage.enabled = true;
                    EditorUtility.RequestScriptReload();
                }
            }

            if (target == null)
            {
                EditorGUILayout.HelpBox("Select an object that has one of your scripts on it.", MessageType.None);
                return;
            }

            DrawTabs();
            if (objectsWithScript > 1)
                GUILayout.Label(objectsWithScript + " objects use this script. A line lights up when any of them runs it.", EditorStyles.helpBox);
            DrawPredictBar();

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawCode();
                DrawSide();
            }
        }

        void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();
                var picked = (MonoBehaviour)EditorGUILayout.ObjectField(target, typeof(MonoBehaviour), true, GUILayout.Width(220f));
                if (EditorGUI.EndChangeCheck())
                {
                    target = picked;
                    if (picked != null) preferredScript = picked.GetType().Name;
                }
                pinned = GUILayout.Toggle(pinned, "Pin", EditorStyles.toolbarButton, GUILayout.Width(36f));
                GUILayout.Space(8f);

                bool paused = GUILayout.Toggle(EditorApplication.isPaused, "Pause", EditorStyles.toolbarButton, GUILayout.Width(48f));
                if (paused != EditorApplication.isPaused) EditorApplication.isPaused = paused;
                using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
                    if (GUILayout.Button("Step", EditorStyles.toolbarButton, GUILayout.Width(40f))) EditorApplication.Step();
                GUILayout.Space(8f);

                DrawPredictToolbar();

                GUILayout.FlexibleSpace();
                GUILayout.Label("Text", EditorStyles.miniLabel);
                using (new EditorGUI.DisabledScope(fontSize <= MinFontSize))
                    if (GUILayout.Button("A-", EditorStyles.toolbarButton, GUILayout.Width(26f))) SetFontSize(fontSize - 1);
                using (new EditorGUI.DisabledScope(fontSize >= MaxFontSize))
                    if (GUILayout.Button("A+", EditorStyles.toolbarButton, GUILayout.Width(26f))) SetFontSize(fontSize + 1);
            }
        }

        void SetFontSize(int size)
        {
            fontSize = Mathf.Clamp(size, MinFontSize, MaxFontSize);
            EditorPrefs.SetInt(FontSizeKey, fontSize);
            codeStyle = null;   // rebuilt at the next repaint
        }

        void DrawTabs()
        {
            List<MonoBehaviour> scripts = ScriptsOn(target.gameObject);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (scripts.Count > 1)
                {
                    foreach (MonoBehaviour mb in scripts)
                    {
                        bool on = ReferenceEquals(mb, target);
                        bool now = GUILayout.Toggle(on, mb.GetType().Name, EditorStyles.toolbarButton);
                        if (now && !on)
                        {
                            target = mb;
                            preferredScript = mb.GetType().Name;
                            GUIUtility.ExitGUI();
                        }
                    }
                }
                else
                {
                    GUILayout.Label(target.gameObject.name + " · " + target.GetType().Name, EditorStyles.miniLabel);
                }
                GUILayout.FlexibleSpace();
                string status = EditorApplication.isPlaying
                    ? (EditorApplication.isPaused ? "Paused: press Step to run one frame" : "Running")
                    : "Press Play";
                GUILayout.Label(status, EditorStyles.miniLabel);
            }
        }
    }
}
