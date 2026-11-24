using UnityEngine;
using UnityEngine.Events;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Change Zone · EXERCISE · CHANGE
    // ============================================
    // GOAL: The door opens when the player comes into the zone, and closes when the player leaves.
    // TASK: Add an event onExit, and an OnTriggerExit that fires it, as Area Trigger does.
    // ============================================
    [RequireComponent(typeof(Collider))]
    public class ChangeZone : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("Only objects with this tag make it fire. Leave it empty for any object.")]
        public string requiredTag = "Player";

        // ===== Events =====
        [Header("Events")]
        public UnityEvent onEnter;

        // ===== Unity Lifecycle =====
        // Unity calls this when another collider enters this trigger collider.
        private void OnTriggerEnter(Collider other)
        {
            // Unity sends this even to a switched-off script, so it stops here.
            if (!enabled) return;
            // Stop if a tag is set and the object does not have it.
            if (requiredTag != "" && !other.CompareTag(requiredTag)) return;

            onEnter.Invoke();
        }
    }
}
