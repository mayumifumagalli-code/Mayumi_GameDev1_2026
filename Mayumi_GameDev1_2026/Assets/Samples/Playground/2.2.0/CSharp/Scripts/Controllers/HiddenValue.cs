using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Hidden Value
    // ============================================
    // WHAT IT DOES: Turns the object. speed is public and appears in the Inspector; angle is private and does not.
    // ============================================
    public class HiddenValue : MonoBehaviour
    {
        // public: shown in the Inspector, where it can be changed. In degrees per second.
        public float speed = 90f;

        // private: only this script can use it.
        private float angle;

        // Unity calls this once every frame.
        private void Update()
        {
            angle = angle + speed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}
