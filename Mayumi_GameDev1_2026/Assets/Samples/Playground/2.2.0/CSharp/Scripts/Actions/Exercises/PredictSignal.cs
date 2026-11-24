// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Predict Signal · EXERCISE · PREDICT
    // ============================================
    // GOAL: Say how many times each line runs, before pressing Play.
    // TASK: The Timeline beside the stand sends its Signal twice. The Signal calls StepUp.
    //       Write the number beside each line, then press Play and check it in Code Watch.
    // ============================================
    public class PredictSignal : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How far the block moves at each step, in metres")]
        public float stepHeight = 0.5f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("The steps up minus the steps down")]
        [SerializeField, ReadOnly] private int stepCount;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object first appears.
        private void Start()
        {
            stepCount = 0;
        }

        // ===== Actions =====
        // Moves the block one step up.
        [ContextMenu("Step Up")]
        public void StepUp()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            // ++ adds one.
            stepCount++;
            // += adds to what is there: the same as position = position + ...
            transform.position += Vector3.up * stepHeight;
        }

        // Moves the block one step down.
        [ContextMenu("Step Down")]
        public void StepDown()
        {
            if (!Application.isPlaying) return;

            // -- takes one away, and -= takes away from what is there.
            stepCount--;
            transform.position -= Vector3.up * stepHeight;
        }
    }
}
