// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;
using UnityEngine.Events;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Chance
    // ============================================
    // WHAT IT DOES: Each time Roll is called, draws a random number and fires one of two events:
    //               On Success as often as the Probability says, On Fail the other times.
    // HOW TO USE:   Add it to any GameObject. Set the Probability, call Roll from an event,
    //               then choose what happens in On Success and On Fail.
    // NEEDS:        Something that calls Roll.
    // ============================================
    public class Chance : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How often On Success fires, from 0 (never) to 1 (always): 0.25 is one time in four")]
        [Range(0f, 1f)]
        public float probability = 0.5f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("The last random number drawn, from 0 to 1")]
        [SerializeField, ReadOnly] private float lastRoll;

        // ===== Events =====
        [Header("Events")]
        public UnityEvent onSuccess;
        public UnityEvent onFail;

        // ===== Actions =====
        // Draws a random number, compares it with the probability, and fires one of the two events.
        [ContextMenu("Roll")]
        public void Roll()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            // Random.value is a random number between 0 and 1.
            lastRoll = Random.value;

            if (lastRoll < probability)
            {
                onSuccess.Invoke();
            }
            else
            {
                onFail.Invoke();
            }
        }
    }
}
