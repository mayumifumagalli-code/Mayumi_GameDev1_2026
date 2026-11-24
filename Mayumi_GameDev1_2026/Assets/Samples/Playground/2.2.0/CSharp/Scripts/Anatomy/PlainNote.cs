using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Plain Note
    // ============================================
    // WHAT IT DOES: Holds a text and a number, and writes them to the Console when Write is called.
    //               It is not a MonoBehaviour: Unity cannot add it to a GameObject.
    // ============================================
    public class PlainNote : MonoBehaviour
    {
        public string text = "Hello";
        public int number = 3;

        public void Write()
        {
            Debug.Log(text + " " + number);
        }
    }
}
