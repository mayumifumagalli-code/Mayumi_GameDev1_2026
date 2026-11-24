// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Flash Action
    // ============================================
    // WHAT IT DOES: Turns an object to a color when Flash is called. The color then fades back to
    //               what it was at the start of the scene.
    // HOW TO USE:   Add it to any GameObject. Drag a Renderer into Target Renderer (leave it empty for
    //               this object's own) and choose Target Color. Call Flash from an event.
    //               FlashWith(Color color) is for other scripts: an event cannot hand over a Color.
    // NEEDS:        A Renderer.
    // ============================================
    public class FlashAction : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The Renderer whose color flashes. Leave it empty for this object's own Renderer.")]
        public Renderer targetRenderer;

        // An event cannot hand over a Color, so the action keeps its own.
        [Tooltip("The color Flash() turns the object to")]
        public Color targetColor = Color.red;

        [Tooltip("How fast the color fades back: higher is faster. 0 never fades.")]
        [Min(0f)]
        public float fadeSpeed = 2f;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("The color at the start of the scene, which the flash fades back to")]
        [SerializeField, ReadOnly] private Color startColor;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object is created (before Start).
        private void Awake()
        {
            if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
            if (targetRenderer == null) return;

            startColor = targetRenderer.material.color;
        }

        // Unity calls this once every frame.
        private void Update()
        {
            if (targetRenderer == null) return;

            // Every frame, the color moves a little back towards the start color.
            targetRenderer.material.color = Color.Lerp(targetRenderer.material.color, startColor, fadeSpeed * Time.deltaTime);
        }

        // ===== Actions =====
        // Flashes with Target Color.
        [ContextMenu("Flash")]
        public void Flash()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;
            if (targetRenderer == null) return;

            targetRenderer.material.color = targetColor;
        }

        // Flashes with a color handed over by another script.
        public void FlashWith(Color color)
        {
            if (!Application.isPlaying) return;
            if (targetRenderer == null) return;

            targetRenderer.material.color = color;
        }
    }
}
