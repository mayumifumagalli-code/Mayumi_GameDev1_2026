using UnityEngine;

namespace Mavrodiev.Playground.Physics
{
    // Kicks away whatever hits it: an impulse along the contact, or always along this object's up.
    public class Bumper : MonoBehaviour
    {
        public float kick = 8f;
        [Tooltip("On: kick along this object's up (a trampoline). Off: kick straight away from the hit (a pinball bumper).")]
        public bool alongUp = true;

        void OnCollisionEnter(Collision collision)
        {
            var body = collision.rigidbody;
            if (body == null || body.isKinematic) return;
            Vector3 away = alongUp ? transform.up : -collision.GetContact(0).normal;
            body.AddForce(away * kick, ForceMode.VelocityChange);
        }
    }
}
