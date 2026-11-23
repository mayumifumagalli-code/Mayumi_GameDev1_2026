// ============================================
// Explosion Action
// ============================================
// PURPOSE: Adds explosion force to all Rigidbodies in radius
// USAGE: Attach to any GameObject, configure explosion settings
// ACTIONS:
//   - Explode() - trigger explosion
// ============================================

using UnityEngine;

public class ExplosionAction : MonoBehaviour
{
    // ===== Settings =====
    [Header("Settings")]
    public float force = 500f;
    public float radius = 5f;
    public float upwardsModifier = 1f;
    public ForceMode forceMode = ForceMode.Impulse;
    public LayerMask affectedLayers = ~0;

    // ===== Actions =====
    [ContextMenu("Explode")]
    public void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, affectedLayers);

        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(force, transform.position, radius, upwardsModifier, forceMode);
            }
        }
    }

    // ===== Gizmos =====
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

// ============================================
// IMPLEMENTATION STEPS
// ============================================
// 1. Attach to explosion origin GameObject
// 2. Set force strength and radius
// 3. Adjust upwardsModifier for more vertical lift
// 4. Set affectedLayers to limit what gets pushed
// 5. Call Explode() from triggers
// ============================================
