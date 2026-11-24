// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Change Key Door · EXERCISE · CHANGE
    // ============================================
    // GOAL:    The door opens only after three keys have been given.
    // TASK:    Now one key is enough. Replace the bool hasKey with an int keyCount, add one to it
    //          in Give Key, and let Open open the door only when keyCount is 3 or more.
    // ============================================
    public class ChangeKeyDoor : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How high the panel rises when the door opens, in metres")]
        public float openHeight = 1.6f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("True after Give Key has been called")]
        [SerializeField, ReadOnly] private bool hasKey;

        [Tooltip("Where the panel is when the door is closed")]
        [SerializeField, ReadOnly] private Vector3 closedPosition;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object first appears.
        private void Start()
        {
            closedPosition = transform.position;
        }

        // ===== Actions =====
        // Gives a key.
        [ContextMenu("Give Key")]
        public void GiveKey()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            hasKey = true;
        }

        // Opens the door if the key has been given. Otherwise it writes a line in the Console.
        [ContextMenu("Open")]
        public void Open()
        {
            if (!Application.isPlaying) return;

            if (hasKey)
            {
                transform.position = closedPosition + Vector3.up * openHeight;
            }
            else
            {
                Debug.Log(name + " needs a key.");
            }
        }
    }
}
