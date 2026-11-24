// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Sun Controller
    // ============================================
    // WHAT IT DOES: One number, the time of day, sets the sun's angle, its intensity and the fog's
    //               color, every frame. They stay as they were last set until the scene ends.
    // HOW TO USE:   Add it to any GameObject. Drag the scene's Directional Light into Sun.
    //               Set Time Of Day while the game runs, or key it on a Timeline (you see the
    //               change in Play mode).
    // NEEDS:        A Directional Light. The fog switched on, to see its color change.
    // ============================================
    public class SunController : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The scene's Directional Light")]
        public Light sun;

        [Tooltip("The hour, from 0 to 24: 6 is sunrise, 12 is noon, 18 is sunset")]
        [Range(0f, 24f)]
        public float timeOfDay = 10f;

        [Tooltip("The direction the sun faces, in degrees around the vertical axis")]
        public float sunDirection = 150f;

        [Tooltip("The sun's brightness at noon, the same number as the Light's Intensity")]
        public float noonIntensity = 2.4f;

        [Tooltip("The fog's color at noon")]
        public Color dayFogColor = new Color(0.85f, 0.88f, 0.94f);

        [Tooltip("The fog's color at night")]
        public Color nightFogColor = new Color(0.12f, 0.14f, 0.22f);

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("How high the sun is: 1 at noon, 0 at 6, at 18 and at night")]
        [SerializeField, ReadOnly] private float sunHeight;

        // ===== Unity Lifecycle =====
        // Unity calls this once every frame.
        private void Update()
        {
            if (sun == null) return;

            // 1. The angle: the sun turns 15 degrees each hour. At 6 it is on the horizon, at 12 straight above.
            //    Quaternion.Euler makes a rotation from three angles, as in the Transform's Rotation.
            float angle = (timeOfDay - 6f) * 15f;
            sun.transform.rotation = Quaternion.Euler(angle, sunDirection, 0f);

            // 2. The height: 1 at noon, 0 at 6 and at 18.
            //    Mathf.Abs removes the minus sign (the distance from noon, in hours).
            //    Clamp01 keeps it between 0 and 1: at night it stays 0, never below.
            sunHeight = Mathf.Clamp01(1f - Mathf.Abs(timeOfDay - 12f) / 6f);

            // 3. The light and the fog follow the height.
            sun.intensity = noonIntensity * sunHeight;
            // Lerp mixes two colors: 0 gives the first, 1 gives the second.
            RenderSettings.fogColor = Color.Lerp(nightFogColor, dayFogColor, sunHeight);
        }
    }
}
