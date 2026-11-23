using UnityEngine;

namespace Mavrodiev.Playground.Physics
{
    // Pulls every Rigidbody within the radius toward this point. Negative strength pushes away (a repulsor).
    public class Attractor : MonoBehaviour
    {
        public float radius = 5f;
        public float strength = 10f;
        [Tooltip("Weaker toward the edge of the radius.")]
        public bool falloff = true;
        public ForceMode forceMode = ForceMode.Acceleration;

        void FixedUpdate()
        {
            foreach (var hit in UnityEngine.Physics.OverlapSphere(transform.position, radius))
            {
                var body = hit.attachedRigidbody;
                if (body == null || body.isKinematic || body.transform == transform) continue;
                Vector3 toCenter = transform.position - body.worldCenterOfMass;
                float distance = toCenter.magnitude;
                if (distance < 0.01f) continue;
                float amount = falloff ? strength * (1f - distance / radius) : strength;
                body.AddForce(toCenter / distance * amount, forceMode);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = strength >= 0 ? new Color(0.3f, 0.6f, 1f, 0.5f) : new Color(1f, 0.4f, 0.3f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
