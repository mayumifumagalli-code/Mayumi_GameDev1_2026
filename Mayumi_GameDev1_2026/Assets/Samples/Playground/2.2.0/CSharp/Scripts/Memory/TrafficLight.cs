// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Traffic Light
    // ============================================
    // WHAT IT DOES: Goes from red to green, to yellow, and back to red, staying Seconds Per State in each.
    //               Its state is one of three named values, and only the light of that state is on.
    // HOW TO USE:   Add it to the traffic light. Drag its three Light components into Red, Yellow and Green.
    // NEEDS:        Three Light components.
    // ============================================
    public class TrafficLight : MonoBehaviour
    {
        // An enum is a type with a fixed list of named values.
        public enum State { Red, Yellow, Green }

        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How long each light stays on, in seconds")]
        [Min(0.1f)]
        public float secondsPerState = 2f;

        [Tooltip("The red light")]
        public Light red;

        [Tooltip("The yellow light")]
        public Light yellow;

        [Tooltip("The green light")]
        public Light green;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("The state now: the light that is on")]
        [SerializeField, ReadOnly] private State state = State.Red;

        [Tooltip("How long the light of this state has been on, in seconds")]
        [SerializeField, ReadOnly] private float currentTime;

        // ===== Unity Lifecycle =====
        // Unity calls this once every frame.
        private void Update()
        {
            // Stop here if a slot is empty.
            if (red == null) return;
            if (yellow == null) return;
            if (green == null) return;

            // 1. Time adds up. += adds the seconds the last frame took to currentTime.
            currentTime += Time.deltaTime;

            // 2. The time of this state is up: go to the next state, and count again from 0.
            if (currentTime >= secondsPerState)
            {
                currentTime = 0f;
                Next();
            }

            // 3. Switch on the light of the state, and the other two off. A comparison gives true or false.
            red.enabled = state == State.Red;
            yellow.enabled = state == State.Yellow;
            green.enabled = state == State.Green;
        }

        // ===== Actions =====
        // Goes to the next state: red to green, green to yellow, yellow to red.
        [ContextMenu("Next")]
        public void Next()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            if (state == State.Red)
            {
                state = State.Green;
            }
            else if (state == State.Green)
            {
                state = State.Yellow;
            }
            else
            {
                state = State.Red;
            }
        }
    }
}
