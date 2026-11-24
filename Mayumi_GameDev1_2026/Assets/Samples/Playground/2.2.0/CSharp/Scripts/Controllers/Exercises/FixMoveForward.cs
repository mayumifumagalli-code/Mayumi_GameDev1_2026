// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Fix Move Forward · EXERCISE · FIX
    // ============================================
    // GOAL:    The cube moves forward by Speed metres every second.
    // PROBLEM: The cube stays where it is.
    // ============================================
    public class FixMoveForward : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How fast the cube moves forward, in metres per second")]
        public float speed = 0.5f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("How far the cube should move in one second, and in which direction, in metres")]
        [SerializeField, ReadOnly] private Vector3 velocity;

        // ===== Unity Lifecycle =====
        // Unity calls this once every frame.
        private void Update()
        {
            velocity = transform.forward * speed;
        }

        // ===== Gizmos =====
        // Draws the velocity as a yellow line in the Scene view: where the cube should be in one second.
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + velocity);
        }
    }
}
