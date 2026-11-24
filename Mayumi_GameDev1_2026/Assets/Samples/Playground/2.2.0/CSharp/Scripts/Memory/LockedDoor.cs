// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Locked Door
    // ============================================
    // WHAT IT DOES: A door in one of three states: Closed, Open or Locked. It starts Locked.
    //               What Open does depends on the state: a locked door stays shut.
    // HOW TO USE:   Add it to the door panel. Call Open, Close and Unlock from events, buttons or Signals.
    // NEEDS:        Nothing.
    // ============================================
    public class LockedDoor : MonoBehaviour
    {
        // An enum is a type with a fixed list of named values.
        public enum State { Closed, Open, Locked }

        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How high the panel rises when the door opens, in metres")]
        public float openHeight = 1.6f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("The state the door is in now: Closed, Open or Locked")]
        [SerializeField, ReadOnly] private State state = State.Locked;

        [Tooltip("Where the panel is when the door is closed")]
        [SerializeField, ReadOnly] private Vector3 closedPosition;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object first appears.
        private void Start()
        {
            closedPosition = transform.position;
        }

        // ===== Actions =====
        // Opens a closed door. A locked door, or one already open, only writes a line in the Console.
        [ContextMenu("Open")]
        public void Open()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            if (state == State.Locked)
            {
                Debug.Log(name + " is locked.");
            }
            else if (state == State.Closed)
            {
                state = State.Open;
                transform.position = closedPosition + Vector3.up * openHeight;
            }
            else
            {
                Debug.Log(name + " is already open.");
            }
        }

        // Closes an open door.
        [ContextMenu("Close")]
        public void Close()
        {
            if (!Application.isPlaying) return;
            if (state != State.Open) return;

            state = State.Closed;
            transform.position = closedPosition;
        }

        // Unlocks a locked door: it becomes Closed, and Open can open it.
        [ContextMenu("Unlock")]
        public void Unlock()
        {
            if (!Application.isPlaying) return;
            if (state != State.Locked) return;

            state = State.Closed;
        }
    }
}
