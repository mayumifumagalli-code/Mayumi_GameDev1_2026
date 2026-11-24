// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;
using UnityEngine.Events;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Gate
    // ============================================
    // WHAT IT DOES: Remembers whether it is open, and turns that over each time Flip is called.
    //               Fires On Opened when it opens, and On Closed when it closes.
    // HOW TO USE:   Add it to any GameObject. Call Flip from an event, a button or a Signal,
    //               then choose what happens in On Opened and On Closed.
    // NEEDS:        Something that calls Flip.
    // ============================================
    public class Gate : MonoBehaviour
    {
        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("True while the gate is open")]
        [SerializeField, ReadOnly] private bool isOpen;

        // ===== Events =====
        [Header("Events")]
        public UnityEvent onOpened;
        public UnityEvent onClosed;

        // ===== Actions =====
        // Opens a closed gate, and closes an open one.
        [ContextMenu("Flip")]
        public void Flip()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            // ! means "not": true becomes false, false becomes true.
            isOpen = !isOpen;

            if (isOpen)
            {
                onOpened.Invoke();
            }
            else
            {
                onClosed.Invoke();
            }
        }
    }
}
