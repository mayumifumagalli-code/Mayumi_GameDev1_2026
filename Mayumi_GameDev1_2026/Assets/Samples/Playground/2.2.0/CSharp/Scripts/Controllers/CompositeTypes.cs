using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Composite Types
    // ============================================
    // WHAT IT DOES: Declares two variables of composite value types. Each value is made of several values.
    // ============================================
    public class CompositeTypes : MonoBehaviour
    {
        // One Vector3 made of three floats: x, y and z.
        public Vector3 size = new Vector3(2f, 0.5f, 1f);

        // One Color made of four floats: red, green, blue and alpha, each from 0 to 1.
        public Color tint = new Color(1f, 0.5f, 0f, 1f);
    }
}
