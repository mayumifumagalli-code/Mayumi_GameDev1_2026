using UnityEngine;

namespace Mavrodiev.Playground.Physics
{
    // A surface that carries what lies on it, at a set speed along a direction (local to this object).
    public class Conveyor : MonoBehaviour
    {
        public Vector3 direction = Vector3.forward;
        public float speed = 2f;

        void OnCollisionStay(Collision collision)
        {
            var body = collision.rigidbody;
            if (body == null || body.isKinematic) return;
            Vector3 belt = transform.TransformDirection(direction.normalized) * speed;
            Vector3 along = Vector3.Project(body.linearVelocity, belt.normalized);
            body.AddForce(belt - along, ForceMode.VelocityChange);
        }
    }
}
