using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Player Zone
    // ============================================
    // WHAT IT DOES: Writes a line only when the object that comes in has the tag Player.
    // ============================================
    public class PlayerZone : MonoBehaviour
    {
        // Unity calls this when another collider enters this trigger collider.
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("The player entered the zone.");
            }
        }
    }
}
