// Teaching support: hides and shows the column of screen buttons. Not a script for students to read.
using TMPro;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // On the Canvas "Canvas - Buttons" (made by StationKit.ScreenCanvas). The small button at the top of the screen calls Toggle,
    // in Play mode too, so the buttons can be put away while watching the scene.
    [AddComponentMenu("Mavrodiev/Teaching Support/Screen Buttons Toggle")]
    public class ScreenButtonsToggle : MonoBehaviour
    {
        public GameObject column;
        public TMP_Text label;

        public void Toggle()
        {
            if (column == null) return;
            column.SetActive(!column.activeSelf);
            if (label != null)
            {
                if (column.activeSelf) label.text = "Hide buttons";
                else label.text = "Show buttons";
            }
        }
    }
}
