// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;
using UnityEngine.Events;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Fix Damage Zone · EXERCISE · FIX
    // ============================================
    // GOAL:    Each time the player comes into the zone, Damage is added to Total Damage
    //          and On Damage fires. Other objects do not count.
    // PROBLEM: The visitor walks through the zone, and Total Damage stays 0.
    // ============================================
    [RequireComponent(typeof(Collider))]
    public class FixDamageZone : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("Only objects with this tag take damage. Leave it empty for any object.")]
        public string requiredTag = "Player";

        [Tooltip("How much damage each entry adds")]
        [Min(0)]
        public int damage = 10;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("All the damage added so far")]
        [SerializeField, ReadOnly] private int totalDamage;

        // ===== Events =====
        [Header("Events")]
        public UnityEvent onDamage;

        // ===== Unity Lifecycle =====
        // Unity calls this when another collider enters this trigger collider.
        private void OnTriggerEnter(Collider other)
        {
            // Unity sends this even to a switched-off script, so it stops here.
            if (!enabled) return;
            // Stop if a tag is set and the object does not have it.
            if (requiredTag != "" && !gameObject.CompareTag(requiredTag)) return;

            // += adds the value on the right to the variable on the left.
            totalDamage += damage;
            Debug.Log(other.name + " takes " + damage + " damage. Total: " + totalDamage);
            onDamage.Invoke();
        }
    }
}
