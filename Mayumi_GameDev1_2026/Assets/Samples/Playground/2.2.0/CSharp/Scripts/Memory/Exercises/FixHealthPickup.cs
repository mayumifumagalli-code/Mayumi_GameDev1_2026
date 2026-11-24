// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Fix Health Pickup · EXERCISE · FIX
    // ============================================
    // GOAL:    Picking up adds Heal Amount to Health, then the Console writes the new health.
    // PROBLEM: The Console writes the health from before the pickup.
    // ============================================
    public class FixHealthPickup : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How much health one pickup adds")]
        public int healAmount = 25;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("The health now")]
        [SerializeField, ReadOnly] private int health = 50;

        // ===== Actions =====
        // Adds the heal amount to the health, and writes the new health in the Console.
        [ContextMenu("Pick Up")]
        public void PickUp()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            Debug.Log("Health is now " + health);
            // += adds healAmount to health.
            health += healAmount;
        }
    }
}
