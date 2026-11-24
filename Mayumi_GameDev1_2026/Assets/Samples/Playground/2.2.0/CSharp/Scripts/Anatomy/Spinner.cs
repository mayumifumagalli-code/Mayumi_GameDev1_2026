using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Spinner
    // ============================================
    // WHAT IT DOES: Turns the object. The variable angle changes every frame.
    // ============================================
    public class Spinner : MonoBehaviour
    {
        public float degreesPerSecond = 90f;
        public float angle;

        // Unity calls this once every frame.
        private void Update()
        {
            angle = angle + degreesPerSecond * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}
