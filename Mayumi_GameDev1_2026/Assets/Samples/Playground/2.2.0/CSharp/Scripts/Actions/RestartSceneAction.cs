using UnityEngine;
// SceneManager, which loads scenes, comes from here.
using UnityEngine.SceneManagement;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Restart Scene Action
    // ============================================
    // WHAT IT DOES: Loads the open scene again, from the start, when Restart is called.
    // HOW TO USE:   Add it to any GameObject. Call Restart from an event.
    // NEEDS:        The scene in the scene list of File > Build Profiles.
    // ============================================
    public class RestartSceneAction : MonoBehaviour
    {
        // ===== Actions =====
        // Loads the open scene again.
        [ContextMenu("Restart")]
        public void Restart()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            string sceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(sceneName);
        }
    }
}
