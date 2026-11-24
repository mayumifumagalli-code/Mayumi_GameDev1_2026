// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Alarm Meter
    // ============================================
    // WHAT IT DOES: Keeps an alarm level from 0 to Max Alarm. It rises while it is filling.
    //               Other scripts read it with Get Alarm.
    // HOW TO USE:   Add it to any GameObject. Call Start Filling and Stop Filling from events,
    //               or Raise Alarm. Drag a Transform into Bar to show the alarm.
    // NEEDS:        Nothing.
    // ============================================
    public class AlarmMeter : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The highest the alarm can go")]
        [Min(1f)]
        public float maxAlarm = 100f;

        [Tooltip("How much the alarm rises in one second while it is filling")]
        public float fillPerSecond = 20f;

        [Tooltip("A Transform that is scaled in y to show the alarm. Leave it empty for no bar.")]
        public Transform bar;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("The alarm now, from 0 to Max Alarm")]
        [SerializeField, ReadOnly] private float currentAlarm;

        [Tooltip("True while the alarm rises")]
        [SerializeField, ReadOnly] private bool isFilling;

        // ===== Unity Lifecycle =====
        // Unity calls this once every frame.
        private void Update()
        {
            // 1. Rise while filling. += adds to the value.
            if (isFilling) currentAlarm += fillPerSecond * Time.deltaTime;

            // 2. Never above Max Alarm.
            if (currentAlarm > maxAlarm) currentAlarm = maxAlarm;

            // 3. Show it on the bar.
            if (bar != null) bar.localScale = new Vector3(1f, currentAlarm / maxAlarm, 1f);
        }

        // ===== Actions =====
        // Makes the alarm rise every frame.
        [ContextMenu("Start Filling")]
        public void StartFilling()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            isFilling = true;
        }

        // Stops the alarm from rising.
        [ContextMenu("Stop Filling")]
        public void StopFilling()
        {
            if (!Application.isPlaying) return;

            isFilling = false;
        }

        // Adds amount to the alarm at once.
        public void RaiseAlarm(float amount)
        {
            if (!Application.isPlaying) return;

            currentAlarm += amount;
        }

        // Answers with the alarm now. A method that returns an answer: float before its name is
        // the type of the answer, and return sends the value back to the script that asked.
        public float GetAlarm()
        {
            return currentAlarm;
        }
    }
}
