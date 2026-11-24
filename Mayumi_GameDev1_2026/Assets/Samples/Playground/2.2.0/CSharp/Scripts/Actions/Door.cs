// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Door
    // ============================================
    // WHAT IT DOES: Raises the door panel when Open is called, and lowers it when Close is called.
    //               Unity never calls Open or Close: something else must call them.
    // HOW TO USE:   Add it to the door panel. Call Open and Close from events, buttons or Signals.
    // NEEDS:        Nothing.
    // ============================================
    public class Door : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How high the panel rises when the door opens, in metres")]
        public float openHeight = 1.6f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("True while the door is open")]
        [SerializeField, ReadOnly] private bool isOpen;

        [Tooltip("Where the panel is when the door is closed")]
        [SerializeField, ReadOnly] private Vector3 closedPosition;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object first appears.
        private void Start()
        {
            closedPosition = transform.position;
        }

        // ===== Actions =====
        // Raises the panel.
        [ContextMenu("Open")]
        public void Open()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;
            // Already open: nothing to do.
            if (isOpen) return;

            isOpen = true;
            transform.position = closedPosition + Vector3.up * openHeight;
        }

        // Lowers the panel back to where it was at the start.
        [ContextMenu("Close")]
        public void Close()
        {
            if (!Application.isPlaying) return;
            // Already closed: nothing to do. The ! means "not".
            if (!isOpen) return;

            isOpen = false;
            transform.position = closedPosition;
        }
    }
}
