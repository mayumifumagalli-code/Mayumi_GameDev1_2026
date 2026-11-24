// Teaching support: draws the wires and readouts as flat screen marks (IMGUI). Not a script for students to read.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // The drawing shared by SceneWires and ValueReadout, in the Scene view (called from the editor) and in the Game view (OnGUI, Play mode).
    // Everything is drawn in GUI coordinates: a "project" function turns a world point into (x, y, depth), depth <= 0 meaning behind the camera.
    // Drawn on top of the picture, so a wire is never hidden inside an object, and a label always faces the viewer at a fixed pixel size.
    public static class DebugViewPainter
    {
        public static readonly Color WireColor = new Color(1.00f, 0.78f, 0.20f, 0.95f);
        public static readonly Color PulseColor = new Color(1.00f, 1.00f, 0.75f, 1.00f);
        public static readonly Color ReadoutColor = new Color(0.35f, 0.85f, 1.00f, 1.00f);
        static readonly Color LabelBack = new Color(0.07f, 0.08f, 0.10f, 0.86f);

        // How many labels have been drawn since the editor started (read by a check, never by the views).
        public static int labelsDrawn;

        static GUIStyle style;
        static readonly GUIContent content = new GUIContent();
        static readonly List<Rect> placed = new List<Rect>();
        static readonly Camera[] cameraBuffer = new Camera[16];

        // ------------------------------------------------------------------ the Game view
        static Camera projectionCamera;
        static Func<Vector3, Vector3> gameProject;

        // The camera the Game view shows: the enabled camera with the highest depth that draws to the screen.
        // On a tie, a camera that is not the Main Camera wins (a station camera switched on by hand).
        public static Camera GameCamera()
        {
            Camera best = null;
            int count = Camera.allCamerasCount;
            Camera[] cameras = cameraBuffer;
            if (count > cameraBuffer.Length) cameras = Camera.allCameras;
            else Camera.GetAllCameras(cameraBuffer);
            if (count > cameras.Length) count = cameras.Length;
            for (int i = 0; i < count; i++)
            {
                Camera c = cameras[i];
                if (c == null || c.targetTexture != null || c.cameraType != CameraType.Game) continue;
                if (best == null || c.depth > best.depth) { best = c; continue; }
                if (c.depth == best.depth && best.CompareTag("MainCamera") && !c.CompareTag("MainCamera")) best = c;
            }
            for (int i = 0; i < cameraBuffer.Length; i++) cameraBuffer[i] = null;
            return best;
        }

        // A project function for the Game view, through the given camera.
        public static Func<Vector3, Vector3> GameProjection(Camera camera)
        {
            projectionCamera = camera;
            if (gameProject == null) gameProject = ProjectThroughCamera;
            return gameProject;
        }

        static Vector3 ProjectThroughCamera(Vector3 world)
        {
            if (projectionCamera == null) return new Vector3(0f, 0f, -1f);
            Vector3 s = projectionCamera.WorldToScreenPoint(world);
            return new Vector3(s.x, Screen.height - s.y, s.z);
        }

        public static Rect GameViewRect()
        {
            return new Rect(0f, 0f, Screen.width, Screen.height);
        }

        // Sizes in the Game view grow with its height (1 at 1080 pixels).
        public static float GameScale()
        {
            return Mathf.Max(0.6f, Screen.height / 1080f);
        }

        // ------------------------------------------------------------------ a frame of drawing
        // Labels placed in one view in one frame avoid each other. Each drawer calls this first with its view
        // (GameView for the Game view, the SceneView object for a Scene view) and the view's size in GUI units:
        // the list of placed labels is emptied only for a new view or a new frame.
        public static readonly object GameView = new object();
        static object labelView;
        static int labelFrame = -1;

        // The size of the view being drawn, in GUI units: labels are kept inside it.
        static Rect viewRect = new Rect(0f, 0f, 100000f, 100000f);

        public static void BeginLabels(object view, Rect viewArea)
        {
            viewRect = viewArea;
            if (view == labelView && Time.frameCount == labelFrame) return;
            labelView = view;
            labelFrame = Time.frameCount;
            placed.Clear();
        }

        // ------------------------------------------------------------------ lines
        public static void Segment(Vector2 a, Vector2 b, float width, Color color)
        {
            Vector2 d = b - a;
            float length = d.magnitude;
            if (length < 0.5f || float.IsNaN(length) || length > 20000f) return;
            float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            Matrix4x4 savedMatrix = GUI.matrix;
            Color savedColor = GUI.color;
            GUIUtility.RotateAroundPivot(angle, a);
            GUI.color = color;
            // A little longer at both ends, so the joints of a polyline have no gaps.
            GUI.DrawTexture(new Rect(a.x - width * 0.5f, a.y - width * 0.5f, length + width, width), Texture2D.whiteTexture);
            GUI.matrix = savedMatrix;
            GUI.color = savedColor;
        }

        public static void Arrowhead(Vector2 tip, Vector2 from, float size, float width, Color color)
        {
            Vector2 d = tip - from;
            if (d.sqrMagnitude < 0.01f) return;
            d.Normalize();
            Vector2 side = new Vector2(-d.y, d.x);
            Segment(tip, tip - d * size + side * size * 0.6f, width, color);
            Segment(tip, tip - d * size - side * size * 0.6f, width, color);
        }

        public static void Dot(Vector2 center, float size, Color color)
        {
            Color saved = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(center.x - size * 0.5f, center.y - size * 0.5f, size, size), Texture2D.whiteTexture);
            GUI.color = saved;
        }

        // ------------------------------------------------------------------ labels
        // A label in a dark box. above = true: the box sits on top of the point; otherwise it is centred on it.
        // A label that would cover one placed earlier moves up until it is free.
        public static void Label(Vector2 point, string text, Color accent, float scale, bool above, float glow)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (!viewRect.Contains(point)) return;   // what it labels is out of the picture
            GUIStyle s = Style(Mathf.RoundToInt(20f * scale));
            content.text = text;
            Vector2 size = s.CalcSize(content);
            size.x = size.x + 4f * scale;   // room for the coloured bar on the left
            Rect r;
            if (above) r = new Rect(point.x - size.x * 0.5f, point.y - size.y, size.x, size.y);
            else r = new Rect(point.x - size.x * 0.5f, point.y - size.y * 0.5f, size.x, size.y);

            // Kept inside the view, so a label near an edge is not cut.
            r.x = Mathf.Clamp(r.x, viewRect.xMin + 2f, Mathf.Max(viewRect.xMin + 2f, viewRect.xMax - r.width - 2f));
            r.y = Mathf.Clamp(r.y, viewRect.yMin + 2f, Mathf.Max(viewRect.yMin + 2f, viewRect.yMax - r.height - 2f));
            for (int tries = 0; tries < 10; tries++)
            {
                Rect hit;
                if (!Overlaps(r, out hit)) break;
                r.y = hit.y - r.height - 2f * scale;
            }
            placed.Add(r);
            labelsDrawn = labelsDrawn + 1;

            Color saved = GUI.color;
            GUI.color = Color.Lerp(LabelBack, new Color(0.35f, 0.25f, 0.02f, 0.95f), glow);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = accent;
            GUI.DrawTexture(new Rect(r.x, r.y, 4f * scale, r.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            s.normal.textColor = Color.Lerp(new Color(0.96f, 0.96f, 0.96f), Color.white, glow);
            GUI.Label(new Rect(r.x + 4f * scale, r.y, r.width - 4f * scale, r.height), content, s);
            GUI.color = saved;
        }

        static bool Overlaps(Rect r, out Rect hit)
        {
            for (int i = 0; i < placed.Count; i++)
            {
                if (placed[i].Overlaps(r)) { hit = placed[i]; return true; }
            }
            hit = new Rect();
            return false;
        }

        static GUIStyle Style(int fontSize)
        {
            if (style == null)
            {
                style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.fontStyle = FontStyle.Bold;
                style.richText = false;
                style.wordWrap = false;
                style.clipping = TextClipping.Overflow;
            }
            style.fontSize = Mathf.Max(10, fontSize);
            int padX = Mathf.Max(4, Mathf.RoundToInt(fontSize * 0.45f));
            int padY = Mathf.Max(2, Mathf.RoundToInt(fontSize * 0.2f));
            style.padding = new RectOffset(padX, padX, padY, padY);
            return style;
        }

        // ------------------------------------------------------------------ where an object is
        // The centre of what is drawn of the object (its renderer, else its collider, else its position).
        public static Vector3 Center(Transform t)
        {
            Renderer r = t.GetComponent<Renderer>();
            if (r != null) return r.bounds.center;
            Collider c = t.GetComponent<Collider>();
            if (c != null) return c.bounds.center;
            return t.position;
        }

        // The top centre of what is drawn of the object (its renderer, else its collider, else its position).
        public static Vector3 Top(Transform t)
        {
            Renderer r = t.GetComponent<Renderer>();
            if (r != null) return new Vector3(r.bounds.center.x, r.bounds.max.y, r.bounds.center.z);
            Collider c = t.GetComponent<Collider>();
            if (c != null) return new Vector3(c.bounds.center.x, c.bounds.max.y, c.bounds.center.z);
            return t.position;
        }
    }
}
