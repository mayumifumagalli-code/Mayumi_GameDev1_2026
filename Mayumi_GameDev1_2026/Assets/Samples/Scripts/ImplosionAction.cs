// ============================================
// Implosion Action
// ============================================
// PURPOSE: Pulls all nearby Rigidbodies toward a point (opposite of explosion)
// USAGE: Attach to any GameObject, call Implode() to trigger
// EXAMPLES: Black hole burst, tractor beam, gravity bomb, vacuum effect
// ============================================

using UnityEngine;

public class ImplosionAction : MonoBehaviour
{
    // ===== Settings =====
    [Header("Settings")]
    [Tooltip("How strongly objects are pulled inward")]
    public float force = 500f;

    [Tooltip("Radius of the implosion effect")]
    public float radius = 5f;

    [Tooltip("How the force is applied")]
    public ForceMode forceMode = ForceMode.Impulse;

    [Tooltip("Which layers are affected")]
    public LayerMask affectedLayers = ~0;

    // ===== Advanced =====
    [Header("Advanced")]
    [Tooltip("If true, force decreases with distance from center")]
    public bool useDistanceFalloff = true;

    [Tooltip("Minimum force multiplier at the edge (0-1)")]
    [Range(0f, 1f)]
    public float edgeForceMultiplier = 0.2f;

    [Tooltip("If true, objects are also pulled slightly downward")]
    public float downwardModifier = 0f;

    // ===== Actions =====

    [ContextMenu("Implode")]
    public void Implode()
    {
        // Find all colliders in radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, affectedLayers);

        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                ApplyImplosionForce(rb);
            }
        }
    }

    // Overload: Implode with custom force
    public void Implode(float customForce)
    {
        float originalForce = force;
        force = customForce;
        Implode();
        force = originalForce;
    }

    // Overload: Implode at specific position
    public void ImplodeAt(Vector3 position)
    {
        Vector3 originalPosition = transform.position;
        transform.position = position;
        Implode();
        transform.position = originalPosition;
    }

    // ===== Private Methods =====

    private void ApplyImplosionForce(Rigidbody rb)
    {
        // Calculate direction TOWARD the implosion center
        Vector3 direction = transform.position - rb.position;
        float distance = direction.magnitude;

        // Skip if at center
        if (distance < 0.01f) return;

        // Normalize direction
        direction = direction.normalized;

        // Calculate force magnitude
        float forceMagnitude = force;

        if (useDistanceFalloff)
        {
            // Force is stronger near center, weaker at edges
            // Lerp from full force at center to edgeForceMultiplier at radius
            float distanceRatio = distance / radius;
            float falloffMultiplier = Mathf.Lerp(1f, edgeForceMultiplier, distanceRatio);
            forceMagnitude *= falloffMultiplier;
        }

        // Apply downward modifier
        if (downwardModifier != 0f)
        {
            direction += Vector3.down * downwardModifier;
            direction = direction.normalized;
        }

        // Apply the force
        rb.AddForce(direction * forceMagnitude, forceMode);
    }

    // ===== Gizmos =====

    private void OnDrawGizmosSelected()
    {
        // Draw implosion radius
        Gizmos.color = new Color(0.5f, 0f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, radius);

        // Draw wire sphere
        Gizmos.color = new Color(0.5f, 0f, 1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, radius);

        // Draw inward arrows
        DrawInwardArrows();
    }

    private void DrawInwardArrows()
    {
        Gizmos.color = new Color(1f, 0f, 1f, 0.8f);

        int arrowCount = 8;
        for (int i = 0; i < arrowCount; i++)
        {
            float angle = (360f / arrowCount) * i * Mathf.Deg2Rad;

            // Arrow on XZ plane
            Vector3 outer = transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Vector3 inner = transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius * 0.4f;
            Gizmos.DrawLine(outer, inner);

            // Arrow head
            Vector3 dir = (inner - outer).normalized;
            Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;
            Gizmos.DrawLine(inner, inner - dir * 0.3f + right * 0.15f);
            Gizmos.DrawLine(inner, inner - dir * 0.3f - right * 0.15f);
        }

        // Draw center point
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position, radius * 0.08f);
    }
}

// ============================================
// IMPLEMENTATION STEPS
// ============================================
// 1. Attach to any GameObject (empty or with visual)
// 2. Set force strength and radius
// 3. Choose forceMode:
//    - Impulse: instant strong pull (gravity bomb)
//    - Force: gentler pull (tractor beam)
// 4. Set affectedLayers to control what gets pulled
// 5. Call Implode() from triggers, buttons, or other scripts
// ============================================

// ============================================
// EXAMPLE SETUPS
// ============================================
//
// GRAVITY BOMB:
//   - force: 800
//   - radius: 8
//   - forceMode: Impulse
//   - useDistanceFalloff: true
//
// TRACTOR BEAM PULSE:
//   - force: 200
//   - radius: 15
//   - forceMode: Impulse
//   - useDistanceFalloff: false
//
// BLACK HOLE BURST:
//   - force: 1500
//   - radius: 20
//   - forceMode: Impulse
//   - downwardModifier: 0.3
//
// ============================================
