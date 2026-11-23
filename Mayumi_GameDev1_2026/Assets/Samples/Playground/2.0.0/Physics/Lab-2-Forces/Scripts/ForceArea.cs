using UnityEngine;

namespace Mavrodiev.Playground.Physics
{
    // A trigger volume that pushes every Rigidbody inside it along a direction (local to this object).
    [RequireComponent(typeof(BoxCollider))]
    public class ForceArea : MonoBehaviour
    {
        public Vector3 direction = Vector3.forward;
        public float strength = 10f;
        public ForceMode forceMode = ForceMode.Acceleration;

        void Reset() { GetComponent<BoxCollider>().isTrigger = true; }

        void OnTriggerStay(Collider other)
        {
            var body = other.attachedRigidbody;
            if (body == null || body.isKinematic) return;
            body.AddForce(transform.TransformDirection(direction.normalized) * strength, forceMode);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.5f, 0.9f, 1f, 0.8f);
            Vector3 d = transform.TransformDirection(direction.normalized);
            Gizmos.DrawLine(transform.position, transform.position + d * 2f);
            Gizmos.DrawWireSphere(transform.position + d * 2f, 0.1f);
        }
    }
}
