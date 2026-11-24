using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Cube Row
    // ============================================
    // WHAT IT DOES: Builds a row of cubes when Build is called. A for loop places one cube on each pass.
    // ============================================
    public class CubeRow : MonoBehaviour
    {
        [Tooltip("How many cubes the row has")]
        public int count = 5;

        [Tooltip("The distance from one cube to the next, in metres")]
        public float spacing = 0.6f;

        // Removes the row of the last build, then builds a new one.
        [ContextMenu("Build")]
        public void Build()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            // 1. Remove the cubes of the last build: they are the children of this object.
            //    A for loop: i starts at 0, the body runs while i is less than childCount,
            //    and i++ adds one to i after each pass.
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            // 2. Place count cubes. i is 0 on the first pass, so the first cube stands on this object.
            for (int i = 0; i < count; i++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.parent = transform;
                cube.transform.position = transform.position + new Vector3(spacing * i, 0.25f, 0f);
                cube.transform.localScale = Vector3.one * 0.5f;
            }
        }
    }
}
