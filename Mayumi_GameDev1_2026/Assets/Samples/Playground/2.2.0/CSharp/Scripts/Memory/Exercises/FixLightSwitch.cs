// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Fix Light Switch · EXERCISE · FIX
    // ============================================
    // GOAL:    Each call to Toggle turns the light over: off becomes on, on becomes off.
    // PROBLEM: The first call turns the light on. After that it never turns off.
    // ============================================
    public class FixLightSwitch : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The light to switch on and off")]
        public Light lamp;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("True while the light is on")]
        [SerializeField, ReadOnly] private bool isOn;

        // ===== Actions =====
        // Switches the light to the opposite of what it is now.
        [ContextMenu("Toggle")]
        public void Toggle()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;
            // Stop here if the slot is empty.
            if (lamp == null) return;

            if (isOn)
            {
                isOn = false;
            }

            if (!isOn)
            {
                isOn = true;
            }

            lamp.enabled = isOn;
        }
    }
}
