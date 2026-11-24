using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Slow Motion Action
    // ============================================
    // WHAT IT DOES: Slows the whole game down when SlowDown is called, and brings it back to normal
    //               speed when NormalSpeed is called. When it is switched off, time goes back to normal.
    // HOW TO USE:   Add it to any GameObject. Set Slow Speed, then call SlowDown and NormalSpeed from events.
    // NEEDS:        Nothing.
    // ============================================
    public class SlowMotionAction : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The speed of game time during slow motion: 1 is normal, 0.5 is half speed")]
        [Range(0.05f, 1f)]
        public float slowSpeed = 0.2f;

        // ===== Unity Lifecycle =====
        // Unity calls this when the component is switched off, when its object is removed, and when the game stops.
        private void OnDisable()
        {
            // Back to normal speed, so the change does not outlast this action (a restart of the scene, for example).
            Time.timeScale = 1f;
        }

        // ===== Actions =====
        // The game runs at Slow Speed.
        [ContextMenu("Slow Down")]
        public void SlowDown()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            Time.timeScale = slowSpeed;
        }

        // The game runs at normal speed.
        [ContextMenu("Normal Speed")]
        public void NormalSpeed()
        {
            if (!Application.isPlaying) return;

            Time.timeScale = 1f;
        }
    }
}
