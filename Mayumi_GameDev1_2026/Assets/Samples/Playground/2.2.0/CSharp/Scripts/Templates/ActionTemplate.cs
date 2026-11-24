using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Action Template
    // ============================================
    // WHAT IT DOES: Say in one or two sentences what this action does when it is called.
    // HOW TO USE:   Say what to add it to, what to drag into its slots, and what to call.
    // NEEDS:        Say what must be in the scene, or "Nothing."
    //
    // TO MAKE YOUR OWN:
    //   1. Copy this file. Give the copy a new name, for example SetNightAction.cs.
    //   2. Rename the class to the same name as the file.
    //   3. Rename DoSomething to say what happens (SetNight, Push, Open) and write its lines.
    //      Then rewrite this card. Test it with the component's right-click menu in Play mode.
    // ============================================
    public class ActionTemplate : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The object this action works on")]
        public GameObject objectToChange;

        // ===== Actions =====
        // Say what this method does.
        [ContextMenu("Do Something")]
        public void DoSomething()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;
            // Stop here if the slot is empty.
            if (objectToChange == null) return;

            // Write what the action does here.
        }
    }
}
