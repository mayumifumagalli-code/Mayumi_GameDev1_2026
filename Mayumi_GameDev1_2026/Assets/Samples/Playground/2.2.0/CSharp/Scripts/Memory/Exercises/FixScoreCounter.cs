// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Fix Score Counter · EXERCISE · FIX
    // ============================================
    // GOAL:    Each call to Add Points adds Points to Score.
    // PROBLEM: The Console writes "Score: 10" every time, and Score in the Inspector stays 0.
    // ============================================
    public class FixScoreCounter : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How many points one call adds")]
        public int points = 10;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("The score now")]
        [SerializeField, ReadOnly] private int score;

        // ===== Actions =====
        // Adds the points to the score, and writes the score in the Console.
        [ContextMenu("Add Points")]
        public void AddPoints()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            int score = 0;
            // += adds points to score.
            score += points;
            Debug.Log("Score: " + score);
        }
    }
}
