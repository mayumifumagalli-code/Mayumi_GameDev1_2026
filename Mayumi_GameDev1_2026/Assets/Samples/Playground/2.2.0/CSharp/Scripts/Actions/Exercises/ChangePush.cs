using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Change Push · EXERCISE · CHANGE
    // ============================================
    // GOAL: The crate can also be pushed with a strength chosen by the caller.
    // TASK: Add a second Push method that asks for a float, and pushes with that number.
    // ============================================
    public class ChangePush : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The Rigidbody to push. Leave it empty for this object's own Rigidbody.")]
        public Rigidbody bodyToPush;

        [Tooltip("How hard Push() pushes, straight up. Heavier bodies move less.")]
        [Min(0f)]
        public float strength = 5f;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object is created (before Start).
        private void Awake()
        {
            if (bodyToPush == null) bodyToPush = GetComponent<Rigidbody>();
        }

        // ===== Actions =====
        // Pushes up with the variable strength.
        [ContextMenu("Push")]
        public void Push()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;
            if (bodyToPush == null) return;

            bodyToPush.AddForce(Vector3.up * strength, ForceMode.Impulse);
        }
    }
}
