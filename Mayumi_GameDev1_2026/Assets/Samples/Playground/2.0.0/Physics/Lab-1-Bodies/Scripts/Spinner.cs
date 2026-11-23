using UnityEngine;

namespace Mavrodiev.Playground.Physics
{
    // Turns a kinematic Rigidbody at a constant speed. Physics pushes others with it; nothing pushes it back.
    [RequireComponent(typeof(Rigidbody))]
    public class Spinner : MonoBehaviour
    {
        public Vector3 axis = Vector3.up;
        public float degreesPerSecond = 120f;

        Rigidbody body;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
        }

        void FixedUpdate()
        {
            var step = Quaternion.AngleAxis(degreesPerSecond * Time.fixedDeltaTime, axis);
            body.MoveRotation(step * body.rotation);
        }
    }
}
