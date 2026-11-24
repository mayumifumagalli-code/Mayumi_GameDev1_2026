using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Zone
    // ============================================
    // WHAT IT DOES: Writes a line in the Console when something comes into this object's trigger collider.
    // ============================================
    public class Zone : MonoBehaviour
    {
        // Unity calls this when another collider enters this trigger collider.
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Something entered the zone.");
        }
    }
}
