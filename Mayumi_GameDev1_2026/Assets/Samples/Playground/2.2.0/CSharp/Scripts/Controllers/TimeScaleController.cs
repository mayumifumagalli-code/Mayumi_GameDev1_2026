using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Time Scale Controller
    // ============================================
    // WHAT IT DOES: Writes one number, the speed of game time, into the engine every frame.
    //               When it is switched off, time goes back to normal speed.
    // HOW TO USE:   Add it to any GameObject. Set Time Scale while the game runs, or key it on a
    //               Timeline. On a Timeline, set the director's Update Method to Unscaled Game Time,
    //               or never key 0: at 0 a normal Timeline stops itself, and time never comes back.
    // NEEDS:        Nothing.
    // ============================================
    public class TimeScaleController : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The speed of game time: 1 is normal, 0.5 is half speed, 0 stops it")]
        [Range(0f, 2f)]
        public float timeScale = 1f;

        // ===== Unity Lifecycle =====
        // Unity calls this once every frame.
        private void Update()
        {
            Time.timeScale = timeScale;
        }

        // Unity calls this when the component is switched off, and when the game stops.
        private void OnDisable()
        {
            // Back to normal speed, so the change does not outlast this controller.
            Time.timeScale = 1f;
        }
    }
}
