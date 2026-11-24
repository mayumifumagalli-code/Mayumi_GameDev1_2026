using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Fog Controller
    // ============================================
    // WHAT IT DOES: Writes one number, the fog's density, into the scene's fog every frame.
    //               The fog stays as it was last set until the scene ends.
    // HOW TO USE:   Add it to any GameObject. Set Density while the game runs, or key it on a
    //               Timeline (you see the change in Play mode).
    // NEEDS:        The fog switched on (Lighting › Environment › Fog), in Exponential mode.
    // ============================================
    public class FogController : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        // Range makes a slider in the Inspector.
        [Tooltip("How thick the fog is: 0 is no fog, 0.1 is thick fog")]
        [Range(0f, 0.2f)]
        public float density = 0.01f;

        // ===== Unity Lifecycle =====
        // Unity calls this once every frame.
        private void Update()
        {
            RenderSettings.fogDensity = density;
        }
    }
}
