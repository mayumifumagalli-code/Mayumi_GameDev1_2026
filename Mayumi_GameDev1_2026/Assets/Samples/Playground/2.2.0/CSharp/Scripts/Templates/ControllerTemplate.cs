using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Controller Template
    // ============================================
    // WHAT IT DOES: Say which value this controller writes into the engine every frame.
    // HOW TO USE:   Say what to add it to. Set the value while the game runs, or key it on a
    //               Timeline (you see the change in Play mode).
    // NEEDS:        Say what must be in the scene, or "Nothing."
    //
    // TO MAKE YOUR OWN:
    //   1. Copy this file. Give the copy a new name, for example AmbientController.cs.
    //   2. Rename the class to the same name as the file.
    //   3. Rename value, give it the type it needs, and write it into the engine in Update.
    //      Then rewrite this card.
    // ============================================
    public class ControllerTemplate : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The value this controller writes. Change its type, its name and this text.")]
        public float value = 1f;

        // ===== Unity Lifecycle =====
        // Unity calls this once every frame.
        private void Update()
        {
            // Write the value into the engine here, for example:
            // RenderSettings.fogDensity = value;
        }
    }
}
