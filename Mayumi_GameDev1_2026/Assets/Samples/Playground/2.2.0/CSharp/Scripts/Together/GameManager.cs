// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;
using UnityEngine.Events;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Game Manager
    // ============================================
    // WHAT IT DOES: Asks an AlarmMeter for its alarm every frame. When the alarm reaches Lose At,
    //               fires On Lose, once.
    // HOW TO USE:   Add it to any GameObject. Drag the AlarmMeter into Alarm Meter, set Lose At,
    //               then choose what happens in On Lose.
    // NEEDS:        An AlarmMeter.
    // ============================================
    public class GameManager : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        // A variable whose type is another script: it holds one AlarmMeter.
        [Tooltip("The AlarmMeter to watch")]
        public AlarmMeter alarmMeter;

        [Tooltip("The alarm at which the game is lost")]
        public float loseAt = 100f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("True after On Lose has fired")]
        [SerializeField, ReadOnly] private bool hasLost;

        // ===== Events =====
        [Header("Events")]
        public UnityEvent onLose;

        // ===== Unity Lifecycle =====
        // Unity calls this once every frame.
        private void Update()
        {
            if (alarmMeter == null) return;

            // 1. Ask the other script for its value: GetAlarm returns the alarm now.
            float alarm = alarmMeter.GetAlarm();

            // 2. Fire On Lose once, the first time the alarm reaches Lose At.
            if (alarm >= loseAt && !hasLost)
            {
                hasLost = true;
                onLose.Invoke();
            }
        }
    }
}
