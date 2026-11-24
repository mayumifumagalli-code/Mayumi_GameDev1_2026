using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Base Action
    // ============================================
    // WHAT IT DOES: The base class: the variable and the action that ShrinkAction and GrowAction share.
    // ============================================
    public class BaseAction : MonoBehaviour
    {
        [Tooltip("The object to make smaller or bigger")]
        public Transform objectToResize;

        // Puts the object back to its normal size, 1 on every axis.
        [ContextMenu("Reset Size")]
        public void ResetSize()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;
            if (objectToResize == null) return;

            objectToResize.localScale = Vector3.one;
        }
    }
}
