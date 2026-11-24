// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Predict Inspector · EXERCISE · PREDICT
    // ============================================
    // GOAL: Say what the Inspector shows for each variable, before you look.
    // TASK: For each of the six variables, write the name the Inspector shows, its kind of
    //       field and its group, or "nothing".
    // ============================================
    public class PredictInspector : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How many lives the player starts with")]
        public int lives = 3;

        [Tooltip("Whether the player can jump")]
        public bool canJump;

        [Tooltip("Where the player starts, in metres")]
        public Vector3 startPosition;

        [Tooltip("The object the camera follows")]
        public Transform objectToFollow;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("The seconds since the object first appeared")]
        [SerializeField, ReadOnly] private float timeAlive;

        private float jumpHeight = 2f;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object first appears.
        private void Start()
        {
            Debug.Log("Jump height: " + jumpHeight);
        }

        // Unity calls this once every frame.
        private void Update()
        {
            // += adds to what is already there.
            timeAlive += Time.deltaTime;
        }
    }
}
