// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Predict Row · EXERCISE · PREDICT
    // ============================================
    // GOAL: Say how many times the body of the loop runs, and what i is on the last pass.
    // TASK: Count is 4. Write both answers down before you click E1 · Place blocks.
    // ============================================
    public class PredictRow : MonoBehaviour
    {
        [Tooltip("The number the loop compares i with")]
        public int count = 4;

        [Tooltip("The distance from one block to the next, in metres")]
        public float spacing = 0.6f;

        [Tooltip("The value of i on the last pass of the loop")]
        [SerializeField, ReadOnly] private int lastIndex;

        // Places a row of blocks.
        [ContextMenu("Place Blocks")]
        public void PlaceBlocks()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            // i++ adds one to i after each pass.
            for (int i = 1; i < count; i++)
            {
                GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.transform.position = transform.position + new Vector3(spacing * i, 0.25f, 0f);
                block.transform.localScale = Vector3.one * 0.5f;
                lastIndex = i;
            }
        }
    }
}
