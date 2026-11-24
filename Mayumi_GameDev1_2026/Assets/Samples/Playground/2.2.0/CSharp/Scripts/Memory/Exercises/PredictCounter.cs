// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Predict Counter · EXERCISE · PREDICT
    // ============================================
    // GOAL:    Before pressing Play, say the values of Current Count and Message after three calls
    //          to Add Two, and which block runs on each call.
    // TASK:    Add Two is called three times. Current Count starts at 1.
    // ============================================
    public class PredictCounter : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The number the count is compared with")]
        public int limit = 5;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("The count now")]
        [SerializeField, ReadOnly] private int currentCount = 1;

        [Tooltip("The message the last call wrote")]
        [SerializeField, ReadOnly] private string message = "";

        // ===== Actions =====
        // Adds two to the count, then compares the count with the limit.
        [ContextMenu("Add Two")]
        public void AddTwo()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            // += adds 2 to currentCount.
            currentCount += 2;

            if (currentCount > limit)
            {
                message = "Over the limit";
            }
            else
            {
                message = "Under the limit";
            }

            Debug.Log(currentCount + ": " + message);
        }
    }
}
