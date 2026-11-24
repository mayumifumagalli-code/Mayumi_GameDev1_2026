// Teaching support: draws a wire from each event to the method it calls. Not a script for students to read.
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;

namespace Mavrodiev.Playground.CSharp
{
    // One per scene, on its own object "Wires" (made by StationKit.Wires()). It reads the listeners set in the Inspector:
    // every UnityEvent field of every script in its scene, and the reactions of every Signal Receiver.
    // It draws a wire from the object that holds the event to the object whose method is called, labelled "EnterEvent → Door.Open".
    // A Signal Receiver holds its reactions itself, so its wire usually starts and ends on one object: it is drawn as a loop
    // above that object, labelled with the signal's name ("Actions-Open-Door → Door.Open"). Events on UI objects (screen buttons) are left out.
    // In Play mode it adds a listener of its own to each event (never saved), so a wire glows for a second when its event fires.
    // It runs nothing and changes nothing. Off (Show = false, always in a player build) it costs one check per frame.
    [ExecuteAlways]
    [AddComponentMenu("Mavrodiev/Teaching Support/Scene Wires")]
    public class SceneWires : MonoBehaviour
    {
        // The switch: Tools > Mavrodiev > C# Playground > Show Wires sets it. It stays false in a player build.
        public static bool Show;

        // Every enabled SceneWires, for the Scene view drawing.
        public static readonly List<SceneWires> All = new List<SceneWires>();

        public const float PulseSeconds = 1f;

        public class Pulse
        {
            public float lastFired = -100f;
            public void Fire() { lastFired = Time.unscaledTime; }
            public void Fire1<A>(A a) { Fire(); }
            public void Fire2<A, B>(A a, B b) { Fire(); }
            public void Fire3<A, B, C>(A a, B b, C c) { Fire(); }
            public void Fire4<A, B, C, D>(A a, B b, C c, D d) { Fire(); }

            public float Amount()
            {
                float t = (Time.unscaledTime - lastFired) / PulseSeconds;
                if (t < 0f || t > 1f) return 0f;
                return 1f - t;
            }
        }

        public class Wire
        {
            public Transform from;
            public Transform to;
            public string label;
            public Pulse pulse;
            public int pairIndex;   // the how-manieth wire between the same two objects
        }

        [NonSerialized] public readonly List<Wire> wires = new List<Wire>();
        bool listening;
        double nextEditScan;

        static readonly Dictionary<Type, FieldInfo[]> fieldCache = new Dictionary<Type, FieldInfo[]>();
        static readonly List<MonoBehaviour> behaviours = new List<MonoBehaviour>();
        static readonly List<Vector2> points = new List<Vector2>();

        void OnEnable()
        {
            if (!All.Contains(this)) All.Add(this);
            listening = false;
            nextEditScan = 0;
        }

        void OnDisable()
        {
            All.Remove(this);
        }

        void Update()
        {
            if (!Show || listening || !Application.isPlaying) return;
            // First frame of Play with the switch on: read the wires once and listen to their events.
            listening = true;
            Scan(true);
        }

        void OnGUI()
        {
            if (!Show || !Application.isPlaying) return;
            if (Event.current.type != EventType.Repaint) return;
            Camera cam = DebugViewPainter.GameCamera();
            if (cam == null) return;
            try
            {
                DebugViewPainter.BeginLabels(DebugViewPainter.GameView, DebugViewPainter.GameViewRect());
                Draw(DebugViewPainter.GameProjection(cam), DebugViewPainter.GameScale());
            }
            catch (Exception)
            {
                // A debug view never stops the game.
            }
        }

        // ------------------------------------------------------------------ reading the wires
        // In Edit mode, read the wires again at most once a second (listeners change in the Inspector).
        public void RefreshInEditMode()
        {
            if (Application.isPlaying) return;
            double now = Time.realtimeSinceStartupAsDouble;
            if (now < nextEditScan) return;
            nextEditScan = now + 1.0;
            Scan(false);
        }

        void Scan(bool listen)
        {
            wires.Clear();
            try
            {
                CollectInScene();
                foreach (MonoBehaviour mb in behaviours)
                {
                    if (mb == null || mb == this || mb is ValueReadout) continue;
                    if (IsUi(mb.transform)) continue;
                    SignalReceiver receiver = mb as SignalReceiver;
                    if (receiver != null)
                    {
                        AddReceiver(receiver, listen);
                        continue;
                    }
                    FieldInfo[] fields = EventFields(mb.GetType());
                    for (int f = 0; f < fields.Length; f++)
                    {
                        UnityEventBase ev = fields[f].GetValue(mb) as UnityEventBase;
                        if (ev != null) AddEvent(mb.transform, NiceName(fields[f].Name), ev, listen);
                    }
                }
                NumberPairs();
            }
            catch (Exception)
            {
                // Whatever could not be read is not drawn.
            }
            behaviours.Clear();
        }

        void CollectInScene()
        {
            behaviours.Clear();
            if (!gameObject.scene.IsValid() || !gameObject.scene.isLoaded) return;
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                // Active objects only: an object switched off is not seen, nor are its wires.
                if (!roots[i].activeInHierarchy) continue;
                behaviours.AddRange(roots[i].GetComponentsInChildren<MonoBehaviour>(false));
            }
        }

        void AddReceiver(SignalReceiver receiver, bool listen)
        {
            int count = receiver.Count();
            for (int i = 0; i < count; i++)
            {
                SignalAsset signal = receiver.GetSignalAssetAtIndex(i);
                UnityEvent reaction = receiver.GetReactionAtIndex(i);
                if (reaction == null) continue;
                string name = "(no signal)";
                if (signal != null) name = signal.name;
                AddEvent(receiver.transform, name, reaction, listen);
            }
        }

        void AddEvent(Transform from, string eventName, UnityEventBase ev, bool listen)
        {
            Pulse pulse = null;
            int count = ev.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (ev.GetPersistentListenerState(i) == UnityEventCallState.Off) continue;
                UnityEngine.Object target = ev.GetPersistentTarget(i);
                string method = ev.GetPersistentMethodName(i);
                if (target == null || string.IsNullOrEmpty(method)) continue;   // an empty slot in the Inspector
                Transform to = TransformOf(target);
                if (to == null || IsUi(to)) continue;
                if (pulse == null)
                {
                    pulse = new Pulse();
                    if (listen) Listen(ev, pulse);
                }
                Wire wire = new Wire();
                wire.from = from;
                wire.to = to;
                wire.label = eventName + " → " + target.GetType().Name + "." + method;
                wire.pulse = pulse;
                wires.Add(wire);
            }
        }

        void NumberPairs()
        {
            for (int i = 0; i < wires.Count; i++)
            {
                int index = 0;
                for (int j = 0; j < i; j++)
                {
                    bool same = (wires[j].from == wires[i].from && wires[j].to == wires[i].to)
                             || (wires[j].from == wires[i].to && wires[j].to == wires[i].from);
                    if (same) index = index + 1;
                }
                wires[i].pairIndex = index;
            }
        }

        // A listener of our own, added at run time only (a runtime listener is never saved into the scene).
        static void Listen(UnityEventBase ev, Pulse pulse)
        {
            try
            {
                UnityEvent plain = ev as UnityEvent;
                if (plain != null)
                {
                    plain.AddListener(pulse.Fire);
                    return;
                }
                Type t = ev.GetType();
                while (t != null && !(t.IsGenericType && t.Namespace == "UnityEngine.Events" && t.Name.StartsWith("UnityEvent`"))) t = t.BaseType;
                if (t == null) return;
                Type[] args = t.GetGenericArguments();
                if (args.Length < 1 || args.Length > 4) return;
                MethodInfo add = t.GetMethod("AddListener");
                MethodInfo fire = typeof(Pulse).GetMethod("Fire" + args.Length).MakeGenericMethod(args);
                Delegate call = Delegate.CreateDelegate(add.GetParameters()[0].ParameterType, pulse, fire);
                add.Invoke(ev, new object[] { call });
            }
            catch (Exception)
            {
                // No glow for this event; the wire is still drawn.
            }
        }

        static FieldInfo[] EventFields(Type type)
        {
            FieldInfo[] cached;
            if (fieldCache.TryGetValue(type, out cached)) return cached;
            List<FieldInfo> found = new List<FieldInfo>();
            Type t = type;
            while (t != null && t != typeof(MonoBehaviour))
            {
                FieldInfo[] all = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                for (int i = 0; i < all.Length; i++)
                {
                    if (!typeof(UnityEventBase).IsAssignableFrom(all[i].FieldType)) continue;
                    if (!all[i].IsPublic && !Attribute.IsDefined(all[i], typeof(SerializeField))) continue;
                    found.Add(all[i]);
                }
                t = t.BaseType;
            }
            cached = found.ToArray();
            fieldCache[type] = cached;
            return cached;
        }

        static string NiceName(string fieldName)
        {
            if (fieldName.StartsWith("m_") && fieldName.Length > 2) return char.ToUpperInvariant(fieldName[2]) + fieldName.Substring(3);
            return fieldName;
        }

        static Transform TransformOf(UnityEngine.Object target)
        {
            Component component = target as Component;
            if (component != null) return InScene(component.transform);
            GameObject go = target as GameObject;
            if (go != null) return InScene(go.transform);
            return null;
        }

        static Transform InScene(Transform t)
        {
            if (!t.gameObject.scene.IsValid()) return null;   // a prefab asset, not an object of the scene
            return t;
        }

        static bool IsUi(Transform t)
        {
            return t is RectTransform || t.GetComponentInParent<Canvas>(true) != null;
        }

        // ------------------------------------------------------------------ drawing
        // Draws every wire. project turns a world point into GUI (x, y, depth); scale 1 = sizes for a 1080-pixel view.
        // The caller calls DebugViewPainter.BeginLabels first.
        public void Draw(Func<Vector3, Vector3> project, float scale)
        {
            for (int i = 0; i < wires.Count; i++)
            {
                Wire w = wires[i];
                if (w == null || w.from == null || w.to == null) continue;   // destroyed since the scan
                float glow = w.pulse.Amount();
                Color color = Color.Lerp(DebugViewPainter.WireColor, DebugViewPainter.PulseColor, glow);
                float width = (3f + 5f * glow) * scale;
                if (w.from == w.to) DrawLoop(w, project, scale, color, width, glow);
                else DrawArc(w, project, scale, color, width, glow);
            }
        }

        static void DrawArc(Wire w, Func<Vector3, Vector3> project, float scale, Color color, float width, float glow)
        {
            Vector3 a = DebugViewPainter.Top(w.from) + Vector3.up * 0.1f;
            Vector3 b = DebugViewPainter.Top(w.to) + Vector3.up * 0.1f;
            float distance = Vector3.Distance(a, b);
            float lift = 0.5f + 0.15f * Mathf.Min(distance, 10f) + 0.45f * w.pairIndex;
            Vector3 control = (a + b) * 0.5f + Vector3.up * lift * 2f;   // the arc's top is half the control point's lift

            points.Clear();
            bool visible = true;
            const int steps = 20;
            for (int s = 0; s <= steps; s++)
            {
                float t = s / (float)steps;
                Vector3 world = (1f - t) * (1f - t) * a + 2f * (1f - t) * t * control + t * t * b;
                Vector3 p = project(world);
                if (p.z <= 0.05f) { visible = false; break; }
                points.Add(new Vector2(p.x, p.y));
            }
            if (!visible || points.Count < 2) return;

            Color shadow = new Color(0f, 0f, 0f, 0.45f);
            for (int s = 0; s + 1 < points.Count; s++) DebugViewPainter.Segment(points[s], points[s + 1], width + 2f * scale, shadow);
            for (int s = 0; s + 1 < points.Count; s++) DebugViewPainter.Segment(points[s], points[s + 1], width, color);
            DebugViewPainter.Dot(points[0], width + 6f * scale, color);
            DebugViewPainter.Arrowhead(points[points.Count - 1], points[points.Count - 3], 14f * scale, width, color);
            DebugViewPainter.Label(points[steps / 2], w.label, color, scale, true, glow);
        }

        // A wire from an object to itself: a loop drawn over the middle of the object (the middle stays in the picture
        // when the object moves, as a door that rises does), with the label above it.
        static void DrawLoop(Wire w, Func<Vector3, Vector3> project, float scale, Color color, float width, float glow)
        {
            Vector3 p = project(DebugViewPainter.Center(w.to));
            if (p.z <= 0.05f) return;
            float radius = (24f + 9f * w.pairIndex) * (1f + 0.3f * glow) * scale;   // it swells while it glows
            Vector2 center = new Vector2(p.x, p.y);
            Color shadow = new Color(0f, 0f, 0f, 0.45f);
            const int steps = 24;
            for (int pass = 0; pass < 2; pass++)
            {
                Vector2 previous = center + new Vector2(0f, radius);
                for (int s = 1; s <= steps; s++)
                {
                    float angle = s / (float)steps * Mathf.PI * 2f;
                    Vector2 next = center + new Vector2(Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius);
                    if (pass == 0) DebugViewPainter.Segment(previous, next, width + 2f * scale, shadow);
                    else DebugViewPainter.Segment(previous, next, width, color);
                    previous = next;
                }
            }
            // An arrowhead on the loop, going round: the event comes back to the same object.
            Vector2 tip = center + new Vector2(radius, 0f);
            DebugViewPainter.Arrowhead(tip, tip + new Vector2(0f, radius), 8f * scale, width, color);
            DebugViewPainter.Label(new Vector2(center.x, center.y - radius - 4f * scale), w.label, color, scale, true, glow);
        }
    }
}
