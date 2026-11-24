// Teaching support: draws the wires and readouts in the Scene view and holds the Show Wires switch. Not a script for students to read.
using System;
using UnityEditor;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // The switch Tools > Mavrodiev > C# Playground > Show Wires (remembered in EditorPrefs, off by default) for the whole editor,
    // and the Scene view drawing of SceneWires and ValueReadout (the Game view drawing is their own OnGUI, in Play mode).
    [InitializeOnLoad]
    public static class PlaygroundSceneViews
    {
        const string MenuPath = "Tools/Mavrodiev/C# Playground/Show Wires";
        const string PrefKey = "Mavrodiev.CSharpPlayground.ShowWires";
        const double PlayRepaintSeconds = 0.1;

        static double nextRepaint;
        static readonly Func<Vector3, Vector3> sceneProject = ProjectInSceneView;

        static PlaygroundSceneViews()
        {
            SceneWires.Show = EditorPrefs.GetBool(PrefKey, false);
            SceneView.duringSceneGui += OnSceneGui;
            EditorApplication.update += OnUpdate;
        }

        [MenuItem(MenuPath, false, 200)]
        static void Toggle()
        {
            bool show = !EditorPrefs.GetBool(PrefKey, false);
            EditorPrefs.SetBool(PrefKey, show);
            SceneWires.Show = show;
            Menu.SetChecked(MenuPath, show);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        [MenuItem(MenuPath, true)]
        static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(PrefKey, false));
            return true;
        }

        static void OnSceneGui(SceneView view)
        {
            if (!SceneWires.Show) return;
            if (Event.current == null || Event.current.type != EventType.Repaint) return;
            if (SceneWires.All.Count == 0 && ValueReadout.All.Count == 0) return;

            Handles.BeginGUI();
            try
            {
                DebugViewPainter.BeginLabels(view, new Rect(0f, 0f, view.position.width, view.position.height));
                for (int i = 0; i < SceneWires.All.Count; i++)
                {
                    SceneWires wires = SceneWires.All[i];
                    if (wires == null || !wires.isActiveAndEnabled) continue;
                    wires.RefreshInEditMode();
                    wires.Draw(sceneProject, 0.85f);
                }
                for (int i = 0; i < ValueReadout.All.Count; i++)
                {
                    ValueReadout readout = ValueReadout.All[i];
                    if (readout == null || !readout.isActiveAndEnabled) continue;
                    readout.Draw(sceneProject, 0.85f);
                }
            }
            catch (Exception)
            {
                // A debug view never gets in the way of the Scene view.
            }
            finally
            {
                Handles.EndGUI();
            }
        }

        static Vector3 ProjectInSceneView(Vector3 world)
        {
            return HandleUtility.WorldToGUIPointWithDepth(world);
        }

        // In Play mode the Scene view repaints only when something moves in it: repaint it ten times a second so readouts and glows stay live.
        static void OnUpdate()
        {
            if (!SceneWires.Show || !EditorApplication.isPlaying || EditorApplication.isPaused) return;
            if (SceneWires.All.Count == 0 && ValueReadout.All.Count == 0) return;
            double now = EditorApplication.timeSinceStartup;
            if (now < nextRepaint) return;
            nextRepaint = now + PlayRepaintSeconds;
            SceneView.RepaintAll();
        }
    }
}
