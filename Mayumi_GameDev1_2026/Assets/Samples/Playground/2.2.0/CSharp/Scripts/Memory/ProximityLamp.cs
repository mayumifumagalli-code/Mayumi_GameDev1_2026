// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Proximity Lamp
    // ============================================
    // WHAT IT DOES: Every frame, measures the distance to the visitor. Switches the light on when the
    //               visitor is nearer than the Range, and off when it is further away.
    // HOW TO USE:   Add it to the lamp. Drag its Light into Lamp, and the visitor (or the player) into Visitor.
    // NEEDS:        A Light component.
    // ============================================
    public class ProximityLamp : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The light to switch on and off")]
        public Light lamp;

        [Tooltip("The object whose distance is measured: the visitor, or the player")]
        public Transform visitor;

        [Tooltip("How near the visitor must come for the light to be on, in metres")]
        [Min(0f)]
        public float range = 2f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("The distance to the visitor now, in metres")]
        [SerializeField, ReadOnly] private float distance;

        // ===== Unity Lifecycle =====
        // Unity calls this once every frame.
        private void Update()
        {
            // Stop here if a slot is empty.
            if (lamp == null) return;
            if (visitor == null) return;

            // 1. Measure the distance from this object to the visitor.
            distance = Vector3.Distance(transform.position, visitor.position);

            // 2. Compare it with range: one of the two blocks runs.
            if (distance < range)
            {
                lamp.enabled = true;
            }
            else
            {
                lamp.enabled = false;
            }
        }
    }
}
