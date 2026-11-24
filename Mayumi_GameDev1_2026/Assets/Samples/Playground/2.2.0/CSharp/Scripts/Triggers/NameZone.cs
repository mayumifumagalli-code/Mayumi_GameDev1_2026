using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Name Zone
    // ============================================
    // WHAT IT DOES: Writes the name of the object that comes into this object's trigger collider.
    // ============================================
    public class NameZone : MonoBehaviour
    {
        // Unity calls this when another collider enters this trigger collider.
        // other is that collider.
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.name + " entered the zone.");
        }
    }
}
