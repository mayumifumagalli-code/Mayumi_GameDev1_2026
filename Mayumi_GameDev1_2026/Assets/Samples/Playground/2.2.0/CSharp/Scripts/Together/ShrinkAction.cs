using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Shrink Action
    // ============================================
    // WHAT IT DOES: Built on BaseAction: it has objectToResize and ResetSize, and adds Shrink.
    // ============================================
    // ": BaseAction" means this class is built on BaseAction, and gets what BaseAction has.
    public class ShrinkAction : BaseAction
    {
        // Makes the object smaller: 0.8 times its size.
        [ContextMenu("Shrink")]
        public void Shrink()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;
            if (objectToResize == null) return;

            objectToResize.localScale = objectToResize.localScale * 0.8f;
        }
    }
}
