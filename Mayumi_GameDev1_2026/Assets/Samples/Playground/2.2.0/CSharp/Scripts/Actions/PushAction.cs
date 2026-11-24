using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Push Action
    // ============================================
    // WHAT IT DOES: Gives a Rigidbody one push when Push is called. Push() pushes with Strength;
    //               Push(float amount) pushes with the number the caller hands over.
    // HOW TO USE:   Add it to any GameObject. Drag the Rigidbody to push into Body To Push (leave it
    //               empty for this object's own). Call Push from an event, a button or a Signal.
    //               The Toolbox's Force Action is the course's ready-made push, with more settings.
    // NEEDS:        A Rigidbody with Is Kinematic not ticked.
    // ============================================
    public class PushAction : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The Rigidbody to push. Leave it empty for this object's own Rigidbody.")]
        public Rigidbody bodyToPush;

        [Tooltip("Which way the push goes, in the world's axes. (0, 1, 0) is up.")]
        public Vector3 direction = Vector3.up;

        [Tooltip("How hard Push() pushes. Heavier bodies move less.")]
        [Min(0f)]
        public float strength = 5f;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object is created (before Start).
        private void Awake()
        {
            if (bodyToPush == null) bodyToPush = GetComponent<Rigidbody>();
        }

        // ===== Actions =====
        // Pushes with the variable strength.
        [ContextMenu("Push")]
        public void Push()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;
            if (bodyToPush == null) return;

            bodyToPush.AddForce(direction * strength, ForceMode.Impulse);
        }

        // Pushes with the number the caller hands over.
        // amount is a parameter: a variable that lives only for this one call.
        public void Push(float amount)
        {
            if (!Application.isPlaying) return;
            if (bodyToPush == null) return;

            bodyToPush.AddForce(direction * amount, ForceMode.Impulse);
        }
    }
}
