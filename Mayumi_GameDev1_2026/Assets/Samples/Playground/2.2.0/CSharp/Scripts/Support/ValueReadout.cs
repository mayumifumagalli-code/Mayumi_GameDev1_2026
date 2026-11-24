// Teaching support: shows the live value of one variable above an object. Not a script for students to read.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // On its own child object "Readout - <name>" (made by StationKit.Readout), never on the object with the lesson script:
    // Code Watch shows a tab for every script on the selected object.
    // Shows "count = 3" at this object's position: in the Scene view (the value saved in the scene, while editing; the live value in Play)
    // and in the Game view during Play, every frame. It reads the value and changes nothing.
    [ExecuteAlways]
    [AddComponentMenu("Mavrodiev/Teaching Support/Value Readout")]
    public class ValueReadout : MonoBehaviour
    {
        // Every enabled ValueReadout, for the Scene view drawing.
        public static readonly List<ValueReadout> All = new List<ValueReadout>();

        public Component target;
        public string variableName = "";

        FieldInfo field;
        PropertyInfo property;
        Type resolvedType;
        string resolvedName;

        void OnEnable()
        {
            if (!All.Contains(this)) All.Add(this);
        }

        void OnDisable()
        {
            All.Remove(this);
        }

        void OnGUI()
        {
            if (!SceneWires.Show || !Application.isPlaying) return;
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

        // Draws the label above this object's position (the caller calls DebugViewPainter.BeginLabels first).
        public void Draw(Func<Vector3, Vector3> project, float scale)
        {
            Vector3 p = project(transform.position);
            if (p.z <= 0.05f) return;
            DebugViewPainter.Label(new Vector2(p.x, p.y), ReadText(), DebugViewPainter.ReadoutColor, scale, true, 0f);
        }

        // True when the target has a public variable (or readable property) with this name.
        public bool HasVariable()
        {
            Resolve();
            return field != null || property != null;
        }

        public string ReadText()
        {
            if (string.IsNullOrEmpty(variableName)) return "(no variable)";
            if (target == null) return variableName + " = (no object)";
            Resolve();
            object value;
            try
            {
                if (field != null) value = field.GetValue(target);
                else if (property != null) value = property.GetValue(target, null);
                else return variableName + " = ?";
            }
            catch (Exception)
            {
                return variableName + " = ?";
            }
            return variableName + " = " + Format(value);
        }

        void Resolve()
        {
            if (target == null) { field = null; property = null; resolvedType = null; return; }
            Type type = target.GetType();
            if (type == resolvedType && variableName == resolvedName) return;
            resolvedType = type;
            resolvedName = variableName;
            field = null;
            property = null;
            if (string.IsNullOrEmpty(variableName)) return;
            field = type.GetField(variableName, BindingFlags.Instance | BindingFlags.Public);
            if (field == null)
            {
                PropertyInfo p = type.GetProperty(variableName, BindingFlags.Instance | BindingFlags.Public);
                if (p != null && p.CanRead && p.GetIndexParameters().Length == 0) property = p;
            }
        }

        // floats with two decimals · true / false · a Unity object by its name ("none" when empty) · text in quotes.
        public static string Format(object value)
        {
            if (value == null) return "none";
            if (value is UnityEngine.Object)
            {
                UnityEngine.Object o = (UnityEngine.Object)value;
                if (o == null) return "none";
                return o.name;
            }
            if (value is bool) return ((bool)value) ? "true" : "false";
            if (value is float) return ((float)value).ToString("0.00", CultureInfo.InvariantCulture);
            if (value is double) return ((double)value).ToString("0.00", CultureInfo.InvariantCulture);
            if (value is string) return "\"" + (string)value + "\"";
            if (value is Vector2 || value is Vector3 || value is Vector4 || value is Quaternion || value is Color)
                return ((IFormattable)value).ToString("0.00", CultureInfo.InvariantCulture);
            IFormattable formattable = value as IFormattable;
            if (formattable != null) return formattable.ToString(null, CultureInfo.InvariantCulture);
            return value.ToString();
        }
    }
}
