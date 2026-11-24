using UnityEngine;
using UnityEngine.Events;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Trigger Template
    // ============================================
    // WHAT IT DOES: Say in one or two sentences what this trigger notices, and which event it fires.
    // HOW TO USE:   Say what to add it to, what to fill, and where to choose what happens.
    // NEEDS:        Say what must be in the scene (a collider, a Rigidbody), or "Nothing."
    //
    // TO MAKE YOUR OWN:
    //   1. Copy this file. Give the copy a new name, for example FallTrigger.cs.
    //   2. Rename the class to the same name as the file.
    //   3. Rename onSomething to say what happened (onFall), keep the Unity method you need,
    //      and write its condition. Then rewrite this card.
    // ============================================
    public class TriggerTemplate : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("Only objects with this tag make it fire. Leave it empty for any object.")]
        public string requiredTag = "Player";

        // ===== Events =====
        [Header("Events")]
        public UnityEvent onSomething;

        // ===== Unity Lifecycle =====
        // Unity calls this when another collider enters this trigger collider.
        // Use the Unity method your trigger needs: OnTriggerEnter, OnTriggerExit, OnCollisionEnter, Update.
        private void OnTriggerEnter(Collider other)
        {
            // Unity sends this even to a switched-off script, so it stops here.
            if (!enabled) return;
            // Stop if a tag is set and the object does not have it.
            if (requiredTag != "" && !other.CompareTag(requiredTag)) return;

            onSomething.Invoke();
        }
    }
}
