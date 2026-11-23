using UnityEngine;

namespace Mavrodiev.Playground.Physics
{
    // Gives this Rigidbody one push, after a delay or when a key is pressed. The first domino.
    [RequireComponent(typeof(Rigidbody))]
    public class Kick : MonoBehaviour
    {
        public Vector3 push = new Vector3(0f, 0f, 3f);
        public float delay = 1f;
        public bool waitForSpace = false;

        float timer;
        bool done;

        void Update()
        {
            if (done) return;
            timer += Time.deltaTime;
            bool space = UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame;
            if (waitForSpace ? space : timer >= delay)
            {
                GetComponent<Rigidbody>().AddForce(push, ForceMode.VelocityChange);
                done = true;
            }
        }
    }
}
