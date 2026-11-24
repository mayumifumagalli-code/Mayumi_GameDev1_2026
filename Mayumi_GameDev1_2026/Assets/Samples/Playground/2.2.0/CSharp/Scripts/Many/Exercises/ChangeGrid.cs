using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Change Grid · EXERCISE · CHANGE
    // ============================================
    // GOAL: Build a grid of cubes: several rows, one behind the other.
    // TASK: Add a variable rows. Put the for loop inside a second for loop that counts the rows,
    //       and move each row back by spacing.
    // ============================================
    public class ChangeGrid : MonoBehaviour
    {
        [Tooltip("How many cubes each row has")]
        public int count = 4;

        [Tooltip("The distance from one cube to the next, in metres")]
        public float spacing = 0.6f;

        // Builds one row of cubes.
        [ContextMenu("Build")]
        public void Build()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            // i++ adds one to i after each pass.
            for (int i = 0; i < count; i++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = transform.position + new Vector3(spacing * i, 0.25f, 0f);
                cube.transform.localScale = Vector3.one * 0.5f;
            }
        }
    }
}
