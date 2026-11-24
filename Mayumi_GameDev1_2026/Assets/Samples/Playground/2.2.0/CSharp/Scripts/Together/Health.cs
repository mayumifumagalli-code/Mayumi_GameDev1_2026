// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Health
    // ============================================
    // WHAT IT DOES: Keeps a health value between 0 and Max Health. It starts full.
    //               Other scripts change it through its two actions, Take Damage and Heal.
    // HOW TO USE:   Add it to any GameObject. Set Max Health. Call Take Damage and Heal from
    //               another script or from an event. Drag a Transform into Bar to show the health.
    // NEEDS:        Nothing.
    // ============================================
    public class Health : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The most health it can have. It starts with this much.")]
        [Min(1f)]
        public float maxHealth = 100f;

        [Tooltip("A Transform that is scaled in x to show the health that is left. Leave it empty for no bar.")]
        public Transform bar;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("The health now, from 0 to Max Health")]
        [SerializeField, ReadOnly] private float currentHealth;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object is created (before Start).
        private void Awake()
        {
            currentHealth = maxHealth;
        }

        // Unity calls this once every frame.
        private void Update()
        {
            // The bar is as wide as the part of the health that is left.
            if (bar != null) bar.localScale = new Vector3(currentHealth / maxHealth, 1f, 1f);
        }

        // ===== Actions =====
        // Takes amount away from the health. It never goes below 0.
        public void TakeDamage(float amount)
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            // -= takes the amount away from the value.
            currentHealth -= amount;
            if (currentHealth < 0f) currentHealth = 0f;
        }

        // Adds amount to the health. It never goes above Max Health.
        public void Heal(float amount)
        {
            if (!Application.isPlaying) return;

            // += adds the amount to the value.
            currentHealth += amount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;
        }
    }
}
