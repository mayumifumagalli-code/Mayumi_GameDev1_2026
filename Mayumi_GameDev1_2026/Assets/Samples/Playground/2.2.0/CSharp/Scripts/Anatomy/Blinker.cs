// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;
using UnityEngine.Events;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Blinker
    // ============================================
    // WHAT IT DOES: Switches a lamp on and off, again and again, every Interval seconds.
    // HOW TO USE:   Add it to any GameObject. Drag the lamp object (a bulb, a light) into Lamp.
    //               Call Stop Blinking and Start Blinking from events, buttons or Signals.
    // NEEDS:        Nothing.
    // ============================================
    public class Blinker : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The object that is switched on and off, for example a bulb")]
        public GameObject lamp;

        [Tooltip("The time between two switches, in seconds")]
        [Min(0.05f)]
        public float interval = 0.5f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("True while the lamp is blinking")]
        [SerializeField, ReadOnly] private bool isBlinking = true;

        [Tooltip("The seconds since the last switch")]
        [SerializeField, ReadOnly] private float timer;

        // ===== Events =====
        [Header("Events")]
        public UnityEvent onSwitched;

        // ===== Unity Lifecycle =====
        // Unity calls this once every frame.
        private void Update()
        {
            if (lamp == null) return;
            if (!isBlinking) return;

            // 1. Add up the seconds since the last switch. += adds to what is already there.
            timer += Time.deltaTime;

            // 2. Not time to switch yet: stop here until the next frame.
            if (timer < interval) return;

            // 3. Switch the lamp, start counting again, and tell others.
            //    The ! turns true into false, and false into true.
            timer = 0f;
            lamp.SetActive(!lamp.activeSelf);
            onSwitched.Invoke();
        }

        // ===== Actions =====
        // Starts the blinking again.
        [ContextMenu("Start Blinking")]
        public void StartBlinking()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            isBlinking = true;
        }

        // Stops the blinking and switches the lamp off.
        [ContextMenu("Stop Blinking")]
        public void StopBlinking()
        {
            if (!Application.isPlaying) return;

            isBlinking = false;
            if (lamp == null) return;
            lamp.SetActive(false);
        }
    }
}
