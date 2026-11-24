using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Patrol
    // ============================================
    // WHAT IT DOES: Moves the object to the right and back, again and again, so that it walks
    //               through the triggers of a stand while nobody plays.
    // HOW TO USE:   Add it to the visitor. Set the Distance and the Speed.
    // NEEDS:        Nothing.
    // ============================================
    public class Patrol : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How far it walks to the right before it turns back, in metres")]
        public float distance = 6f;

        [Tooltip("How fast it walks, in metres per second")]
        public float speed = 2f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("Where the object was when the scene started")]
        [SerializeField, ReadOnly] private Vector3 startPosition;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object first appears.
        private void Start()
        {
            startPosition = transform.position;
        }

        // Unity calls this once every frame.
        private void Update()
        {
            // PingPong counts up to distance, then back down to 0, then up again.
            float offset = Mathf.PingPong(Time.time * speed, distance);
            transform.position = startPosition + Vector3.right * offset;
        }
    }
}
