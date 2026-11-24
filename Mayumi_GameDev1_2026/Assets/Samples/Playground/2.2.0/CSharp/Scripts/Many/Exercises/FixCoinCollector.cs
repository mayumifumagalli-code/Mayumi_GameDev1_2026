// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Fix Coin Collector · EXERCISE · FIX
    // ============================================
    // GOAL:    Collect All hides every coin, then writes "Collected 5 coins" in the Console.
    // PROBLEM: The coins disappear, but the Console shows a red error, and the message never appears.
    // ============================================
    public class FixCoinCollector : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The coins to collect, one in each slot")]
        public GameObject[] coins;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("How many coins the last Collect All counted")]
        [SerializeField, ReadOnly] private int collected;

        // ===== Actions =====
        // Hides every coin, counts them, and writes the count in the Console.
        [ContextMenu("Collect All")]
        public void CollectAll()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            // 1. Hide each coin and count it. ++ adds one.
            collected = 0;
            for (int i = 0; i <= coins.Length; i++)
            {
                if (coins[i] != null) coins[i].SetActive(false);
                collected++;
            }

            // 2. Write the count in the Console.
            Debug.Log("Collected " + collected + " coins");
        }

        // Shows every coin again.
        [ContextMenu("Put Back")]
        public void PutBack()
        {
            if (!Application.isPlaying) return;

            for (int i = 0; i < coins.Length; i++)
            {
                if (coins[i] != null) coins[i].SetActive(true);
            }
        }
    }
}
