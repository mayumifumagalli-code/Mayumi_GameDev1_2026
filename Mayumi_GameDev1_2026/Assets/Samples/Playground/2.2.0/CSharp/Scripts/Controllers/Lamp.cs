using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Lamp
    // ============================================
    // WHAT IT DOES: Writes Intensity into a Light every frame. The variable lamp points at that Light.
    // ============================================
    public class Lamp : MonoBehaviour
    {
        // A reference type: it points at a Light in the scene. Fill the slot by dragging a Light into it.
        public Light lamp;

        // A value type: the brightness written into the Light.
        public float intensity = 3f;

        // Unity calls this once every frame.
        private void Update()
        {
            lamp.intensity = intensity;
        }
    }
}
