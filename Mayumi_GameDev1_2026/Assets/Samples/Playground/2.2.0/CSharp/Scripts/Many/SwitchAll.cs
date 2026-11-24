using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Switch All
    // ============================================
    // WHAT IT DOES: Holds several lights in one array, and switches the first one, or every one.
    // ============================================
    public class SwitchAll : MonoBehaviour
    {
        // [] makes an array: one variable that holds many values of one type.
        [Tooltip("The lights to switch. Drag them into the slots, one light in each.")]
        public Light[] lights;

        // Switches the first light on or off.
        [ContextMenu("Switch First")]
        public void SwitchFirst()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;
            if (lights.Length == 0) return;
            // An array counts its slots from 0, so lights[0] is the first light.
            if (lights[0] == null) return;

            // The ! turns true into false, and false into true.
            lights[0].enabled = !lights[0].enabled;
        }

        // Switches every light off.
        [ContextMenu("Switch Off")]
        public void SwitchOff()
        {
            if (!Application.isPlaying) return;

            // foreach runs its body once for each light in the array, and calls it light each time.
            foreach (Light light in lights)
            {
                if (light != null) light.enabled = false;
            }
        }

        // Switches every light on.
        [ContextMenu("Switch On")]
        public void SwitchOn()
        {
            if (!Application.isPlaying) return;

            foreach (Light light in lights)
            {
                if (light != null) light.enabled = true;
            }
        }
    }
}
