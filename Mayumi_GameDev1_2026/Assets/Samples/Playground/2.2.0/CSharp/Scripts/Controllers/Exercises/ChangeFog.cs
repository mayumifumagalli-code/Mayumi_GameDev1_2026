using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Change Fog · EXERCISE · CHANGE
    // ============================================
    // GOAL: The controller also sets the fog's color.
    // TASK: Add a public Color variable, fogColor, with a tooltip, and write it into
    //       RenderSettings.fogColor in Update.
    // ============================================
    public class ChangeFog : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
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
