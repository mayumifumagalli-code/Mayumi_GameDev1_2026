// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Fix Respawn · EXERCISE · FIX
    // ============================================
    // GOAL:    When the ball falls into the pit, the pit's Area Trigger calls Respawn, and the ball
    //          goes back to the respawn point and rolls down the ramp again.
    // PROBLEM: The ball falls into the pit and stays there. The Console shows a red error.
    // ============================================
    public class FixRespawn : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The object to put back, for example the ball")]
        public Transform objectToRespawn;

        [Tooltip("Where the object goes back to")]
        public Transform respawnPoint;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("How many times the object has respawned")]
        [SerializeField, ReadOnly] private int respawnCount;

        // ===== Actions =====
        // Puts the object back at the respawn point.
        [ContextMenu("Respawn")]
        public void Respawn()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;
            if (objectToRespawn == null) return;

            // 1. Move the object. SyncTransforms tells the physics engine at once that it moved.
            objectToRespawn.position = respawnPoint.position;
            Physics.SyncTransforms();

            // 2. A Rigidbody would keep the speed it had, so its speed is set to zero.
            Rigidbody body = objectToRespawn.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            // ++ adds one.
            respawnCount++;
        }
    }
}
