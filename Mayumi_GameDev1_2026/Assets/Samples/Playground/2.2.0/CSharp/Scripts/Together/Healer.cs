using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Healer
    // ============================================
    // WHAT IT DOES: Gives health back to a Health, a little every frame.
    // HOW TO USE:   Add it to any GameObject. Drag a Health into Target Health. Set Heal Per Second.
    // NEEDS:        A Health.
    // ============================================
    public class Healer : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The Health to heal")]
        public Health targetHealth;

        [Tooltip("How much health it gives back in one second")]
        [Min(0f)]
        public float healPerSecond = 8f;

        // ===== Unity Lifecycle =====
        // Unity calls this once every frame.
        private void Update()
        {
            if (targetHealth == null) return;

            // Heal Per Second, spread over the frames of one second.
            targetHealth.Heal(healPerSecond * Time.deltaTime);
        }
    }
}
