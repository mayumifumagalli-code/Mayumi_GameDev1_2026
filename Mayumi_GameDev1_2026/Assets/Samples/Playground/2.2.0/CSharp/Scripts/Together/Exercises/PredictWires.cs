// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;
using UnityEngine.Events;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Predict Wires · EXERCISE · PREDICT
    // ============================================
    // GOAL: Say in which order the three cubes light up, before pressing Play.
    // TASK: Read the On Pass event of each cube in the Inspector. The cube with Starts First ticked
    //       lights up first.
    // ============================================
    public class PredictWires : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("Ticked: this cube lights up when the scene starts")]
        public bool startsFirst;

        [Tooltip("How long the cube stays lit before it passes on, in seconds")]
        public float waitTime = 1.5f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("How many times this cube has lit up")]
        [SerializeField, ReadOnly] private int passes;

        [Tooltip("True while the cube is lit")]
        [SerializeField, ReadOnly] private bool isLit;

        [Tooltip("The seconds left before the cube passes on")]
        [SerializeField, ReadOnly] private float timeLeft;

        // ===== Events =====
        [Header("Events")]
        public UnityEvent onPass;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object first appears.
        private void Start()
        {
            if (startsFirst) Pass();
        }

        // Unity calls this once every frame.
        private void Update()
        {
            if (!isLit) return;

            // 1. Count the time down. -= takes away.
            timeLeft -= Time.deltaTime;
            if (timeLeft > 0f) return;

            // 2. Time is over: go grey, and pass on to whoever On Pass calls.
            isLit = false;
            GetComponent<Renderer>().material.color = Color.gray;
            onPass.Invoke();
        }

        // ===== Actions =====
        // Lights the cube. It passes on when Wait Time is over.
        [ContextMenu("Pass")]
        public void Pass()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            // ++ adds one.
            passes++;
            isLit = true;
            timeLeft = waitTime;
            GetComponent<Renderer>().material.color = Color.yellow;
        }
    }
}
