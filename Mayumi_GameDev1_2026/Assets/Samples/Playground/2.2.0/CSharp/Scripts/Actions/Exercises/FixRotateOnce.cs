// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Fix Rotate Once · EXERCISE · FIX
    // ============================================
    // GOAL:    The cube turns by Angle each time Turn is called.
    // PROBLEM: The cube turns once, when it first appears, and Turn does nothing.
    // ============================================
    public class FixRotateOnce : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How far the cube turns each time Turn is called, in degrees")]
        public float angle = 45f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("How many times Turn has been called")]
        [SerializeField, ReadOnly] private int turnCount;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object first appears.
        private void Start()
        {
            transform.Rotate(0f, angle, 0f);
        }

        // ===== Actions =====
        // Turns the cube by Angle around its vertical axis.
        [ContextMenu("Turn")]
        public void Turn()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            // ++ adds one.
            turnCount++;
        }
    }
}
