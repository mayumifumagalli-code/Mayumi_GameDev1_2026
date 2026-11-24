// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;
using UnityEngine.Events;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Meter
    // ============================================
    // WHAT IT DOES: Keeps a level between 0 and 1. Fill raises it, and it leaks away a little every frame.
    //               Fires On Full when the level rises to the Threshold, and On Low when it falls below.
    // HOW TO USE:   Add it to any GameObject. Set the Threshold and the leak, call Fill from an event,
    //               then choose what happens in On Full and On Low. Drag a bar into Object To Scale to see the level.
    // NEEDS:        Nothing.
    // ============================================
    public class Meter : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The level when the scene starts, from 0 to 1")]
        [Range(0f, 1f)]
        public float startLevel;

        [Tooltip("How much one call to Fill adds to the level")]
        [Range(0f, 1f)]
        public float fillAmount = 0.25f;

        [Tooltip("How much the level falls in one second")]
        [Min(0f)]
        public float leakPerSecond = 0.1f;

        [Tooltip("The level that fires On Full when the level rises to it, and On Low when it falls below it")]
        [Range(0f, 1f)]
        public float threshold = 0.7f;

        [Tooltip("The bar that shows the level: its height is scaled from 0 to 1. Leave it empty for no bar.")]
        public Transform objectToScale;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("The level now, from 0 to 1")]
        [SerializeField, ReadOnly] private float level;

        [Tooltip("True after On Full, false after On Low")]
        [SerializeField, ReadOnly] private bool isFull;

        // ===== Events =====
        [Header("Events")]
        public UnityEvent onFull;
        public UnityEvent onLow;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object first appears.
        private void Start()
        {
            level = startLevel;
        }

        // Unity calls this once every frame.
        private void Update()
        {
            // 1. Leak a little. -= takes the leak of this frame away from level.
            level -= leakPerSecond * Time.deltaTime;

            // 2. Defensive: the level never goes below 0.
            if (level < 0f)
            {
                level = 0f;
            }

            // 3. Fire an event when the level crosses the threshold, once each way.
            if (level >= threshold && !isFull)
            {
                isFull = true;
                onFull.Invoke();
            }
            else if (level < threshold && isFull)
            {
                isFull = false;
                onLow.Invoke();
            }

            // 4. Show the level on the bar, if there is one.
            if (objectToScale != null)
            {
                objectToScale.localScale = new Vector3(1f, level, 1f);
            }
        }

        // ===== Actions =====
        // Adds Fill Amount to the level.
        [ContextMenu("Fill")]
        public void Fill()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            // += adds fillAmount to level.
            level += fillAmount;

            // Defensive: the level never goes above 1.
            if (level > 1f)
            {
                level = 1f;
            }
        }
    }
}
