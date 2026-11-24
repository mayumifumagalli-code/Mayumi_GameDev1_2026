using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Value Types
    // ============================================
    // WHAT IT DOES: Declares four variables of the basic value types. Each public variable appears in the Inspector.
    // ============================================
    public class ValueTypes : MonoBehaviour
    {
        // A number with a fraction. The f at the end marks it as a float.
        public float height = 1.5f;

        // A whole number.
        public int count = 3;

        // A text, written between double quotes.
        public string label = "Box 1";

        // true or false.
        public bool isOn = true;
    }
}
