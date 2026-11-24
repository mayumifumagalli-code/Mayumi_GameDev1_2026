// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Key Door
    // ============================================
    // WHAT IT DOES: Raises the door panel while the visitor is near AND the key is held,
    //               and lowers it again when either one stops being true.
    // HOW TO USE:   Add it to the door panel. Drag the visitor (or the player) into Visitor, and set
    //               the Range. Call Give Key and Take Key from events, or from the right-click menu.
    // NEEDS:        Nothing.
    // ============================================
    public class KeyDoor : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The object that must come near: the visitor, or the player")]
        public Transform visitor;

        [Tooltip("How near the visitor must come for the door to open, in metres")]
        [Min(0f)]
        public float range = 2f;

        [Tooltip("How high the panel rises when the door opens, in metres")]
        public float openHeight = 1.6f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("True while the key is held. Give Key and Take Key change it.")]
        [SerializeField, ReadOnly] private bool hasKey;

        [Tooltip("True while the visitor is nearer than the Range")]
        [SerializeField, ReadOnly] private bool isNear;

        [Tooltip("Where the panel is when the door is closed")]
        [SerializeField, ReadOnly] private Vector3 closedPosition;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object first appears.
        private void Start()
        {
            closedPosition = transform.position;
        }

        // Unity calls this once every frame.
        private void Update()
        {
            // Stop here if the slot is empty.
            if (visitor == null) return;

            // 1. A comparison gives a bool: true when the visitor is nearer than range.
            isNear = Vector3.Distance(closedPosition, visitor.position) < range;

            // 2. && means "and": the door opens only when both are true.
            if (isNear && hasKey)
            {
                transform.position = closedPosition + Vector3.up * openHeight;
            }
            else
            {
                transform.position = closedPosition;
            }
        }

        // ===== Actions =====
        // Gives the key to the visitor.
        [ContextMenu("Give Key")]
        public void GiveKey()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            hasKey = true;
        }

        // Takes the key away again.
        [ContextMenu("Take Key")]
        public void TakeKey()
        {
            if (!Application.isPlaying) return;

            hasKey = false;
        }
    }
}
