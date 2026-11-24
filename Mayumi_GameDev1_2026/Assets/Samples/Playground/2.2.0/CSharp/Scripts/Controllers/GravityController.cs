// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Gravity Controller
    // ============================================
    // WHAT IT DOES: Writes one number, the strength of gravity, into the physics engine every frame.
    //               When it is switched off, the project's own gravity comes back.
    // HOW TO USE:   Add it to any GameObject. Set Strength while the game runs, or key it on a Timeline.
    // NEEDS:        Nothing.
    // ============================================
    public class GravityController : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How strongly things fall, in metres per second every second: 9.81 on Earth, 1.62 on the Moon")]
        [Range(0f, 30f)]
        public float strength = 9.81f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("The project's own gravity, put back when this controller is switched off")]
        [SerializeField, ReadOnly] private Vector3 startGravity;

        // ===== Unity Lifecycle =====
        // Unity calls this when the component is switched on, and when it first appears.
        private void OnEnable()
        {
            startGravity = Physics.gravity;
        }

        // Unity calls this once every frame.
        private void Update()
        {
            // Gravity pulls down, so y is minus the strength.
            Physics.gravity = new Vector3(0f, -strength, 0f);
        }

        // Unity calls this when the component is switched off, and when the game stops.
        private void OnDisable()
        {
            Physics.gravity = startGravity;
        }
    }
}
