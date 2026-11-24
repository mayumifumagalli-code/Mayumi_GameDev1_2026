using UnityEngine;
using UnityEngine.Events;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Damager
    // ============================================
    // WHAT IT DOES: Takes Damage away from a Health each time a collider comes into the trigger,
    //               and fires On Hit.
    // HOW TO USE:   Add it to a GameObject whose collider has Is Trigger ticked, or to an object with a
    //               Rigidbody that walks into a trigger. Drag a Health into Target Health, or leave it
    //               empty and choose what happens in On Hit.
    // NEEDS:        A trigger collider, and a Rigidbody on one of the two objects.
    // ============================================
    public class Damager : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        // A variable whose type is another script: it holds one Health.
        [Tooltip("The Health to damage. Leave it empty to use only On Hit.")]
        public Health targetHealth;

        [Tooltip("How much health one hit takes away")]
        [Min(0f)]
        public float damage = 10f;

        // ===== Events =====
        [Header("Events")]
        public UnityEvent onHit;

        // ===== Unity Lifecycle =====
        // Unity calls this when a collider comes into the trigger, on both objects.
        private void OnTriggerEnter(Collider other)
        {
            // Unity sends this even to a switched-off script, so it stops here.
            if (!enabled) return;

            // 1. Called directly: this line names a method of Health.
            if (targetHealth != null) targetHealth.TakeDamage(damage);

            // 2. Called through the event: the Inspector holds the list of calls.
            onHit.Invoke();
        }
    }
}
