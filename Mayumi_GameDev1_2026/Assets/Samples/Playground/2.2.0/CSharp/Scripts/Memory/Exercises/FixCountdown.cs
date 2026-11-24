// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;
using UnityEngine.Events;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Fix Countdown · EXERCISE · FIX
    // ============================================
    // GOAL:    Current Time counts down from the Duration to 0, in seconds. At 0 it stops,
    //          Is Running turns off, and On Finished fires (Lamp E5 lights up).
    // PROBLEM: Current Time goes below 0 and keeps going. On Finished never fires.
    // ============================================
    public class FixCountdown : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How long the countdown runs, in seconds")]
        [Min(0.01f)]
        public float duration = 5f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("The time left, in seconds")]
        [SerializeField, ReadOnly] private float currentTime;

        [Tooltip("True while the countdown is counting")]
        [SerializeField, ReadOnly] private bool isRunning;

        // ===== Events =====
        [Header("Events")]
        public UnityEvent onFinished;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object first appears.
        private void Start()
        {
            currentTime = duration;
            isRunning = true;
        }

        // Unity calls this once every frame.
        private void Update()
        {
            if (!isRunning) return;

            // 1. Move the time one frame down. -= takes the seconds the last frame took away from currentTime.
            currentTime -= Time.deltaTime;

            // 2. At 0 the time is up: stop counting, and fire the event.
            if (currentTime == 0f)
            {
                currentTime = 0f;
                isRunning = false;
                onFinished.Invoke();
            }
        }
    }
}
