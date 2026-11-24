// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Predict Zone · EXERCISE · PREDICT
    // ============================================
    // GOAL: Say which lines run each time something comes in or leaves, before pressing Play.
    // TASK: Ball E1 falls through the zone first. Then Visitor E1 walks in.
    // ============================================
    public class PredictZone : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("Only objects with this tag are counted. Leave it empty for any object.")]
        public string requiredTag = "Player";

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("How many times an object with the required tag has come in")]
        [SerializeField, ReadOnly] private int playerCount;

        // ===== Unity Lifecycle =====
        // Unity calls this when another collider enters this trigger collider.
        private void OnTriggerEnter(Collider other)
        {
            // Unity sends this even to a switched-off script, so it stops here.
            if (!enabled) return;

            Debug.Log(other.name + " entered.");

            // Stop if a tag is set and the object does not have it.
            if (requiredTag != "" && !other.CompareTag(requiredTag)) return;

            // ++ adds one.
            playerCount++;
            Debug.Log("Player count: " + playerCount);
        }

        // Unity calls this when another collider leaves this trigger collider.
        private void OnTriggerExit(Collider other)
        {
            if (!enabled) return;

            Debug.Log(other.name + " left.");
        }
    }
}
