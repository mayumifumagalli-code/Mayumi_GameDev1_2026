using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Grow Action
    // ============================================
    // WHAT IT DOES: Built on BaseAction: it has objectToResize and ResetSize, and adds Grow.
    // ============================================
    // ": BaseAction" means this class is built on BaseAction, and gets what BaseAction has.
    public class GrowAction : BaseAction
    {
        // Makes the object bigger: 1.25 times its size.
        [ContextMenu("Grow")]
        public void Grow()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;
            if (objectToResize == null) return;

            objectToResize.localScale = objectToResize.localScale * 1.25f;
        }
    }
}
