// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Predict Parts · EXERCISE · PREDICT
    // ============================================
    // GOAL: Name the parts of this script, from the top down.
    // TASK: Find the using lines, the namespace, the card, the class and its sections.
    //       Name the variables or the methods in each section. Can it be a component?
    // ============================================
    public class PredictParts : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The size of the block when it first appears, in metres")]
        public float size = 0.8f;

        [Tooltip("How far the block rises at each jump, in metres")]
        public float jumpHeight = 0.5f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("How many times the block has jumped")]
        [SerializeField, ReadOnly] private int jumps;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object first appears.
        private void Start()
        {
            transform.localScale = Vector3.one * size;
        }

        // ===== Actions =====
        // Raises the block by Jump Height.
        [ContextMenu("Jump")]
        public void Jump()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            // ++ adds one.
            jumps++;
            transform.position = transform.position + Vector3.up * jumpHeight;
        }
    }
}
