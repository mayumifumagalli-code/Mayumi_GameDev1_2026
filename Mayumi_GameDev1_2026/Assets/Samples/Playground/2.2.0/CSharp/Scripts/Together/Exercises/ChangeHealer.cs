using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Change Healer · EXERCISE · CHANGE
    // ============================================
    // GOAL: When the visitor comes into the spring, its Health gets Amount back.
    // TASK: Add a variable that holds a Health, named targetHealth, and call its Heal method here.
    //       Damager.cs does the same with TakeDamage.
    // ============================================
    public class ChangeHealer : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How much health one visit to the spring gives back")]
        [Min(0f)]
        public float amount = 30f;

        // ===== Unity Lifecycle =====
        // Unity calls this when a collider comes into the trigger.
        private void OnTriggerEnter(Collider other)
        {
            // Unity sends this even to a switched-off script, so it stops here.
            if (!enabled) return;

            Debug.Log(other.name + " came into the spring.");
            // Heal the Health here.
        }
    }
}
