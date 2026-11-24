// "using" lets this script use code from elsewhere: here, List from C#, and [ReadOnly] from the Toolbox.
using System.Collections.Generic;
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Recorder
    // ============================================
    // WHAT IT DOES: Stores where a Rigidbody is at each physics step while it records, then moves it
    //               back along the same positions, the last one first.
    // HOW TO USE:   Add it to any GameObject. Drag an object with a Rigidbody into Body To Record.
    //               Call Record, let the object move, then call Play Backwards.
    // NEEDS:        A Rigidbody.
    // ============================================
    public class Recorder : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("The Rigidbody to record")]
        public Rigidbody bodyToRecord;

        [Tooltip("The most positions it stores. At 50 physics steps a second, 200 positions last 4 seconds.")]
        [Min(1)]
        public int maxPositions = 200;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("True while it stores positions")]
        [SerializeField, ReadOnly] private bool isRecording;

        [Tooltip("True while it moves the body back")]
        [SerializeField, ReadOnly] private bool isPlayingBack;

        // A List starts empty and grows: Add puts one more value at its end.
        [Tooltip("The stored positions, the first one first")]
        [SerializeField, ReadOnly] private List<Vector3> positions = new List<Vector3>();

        [Tooltip("The same positions, the last one first")]
        [SerializeField, ReadOnly] private List<Vector3> backwards = new List<Vector3>();

        [Tooltip("The number of the position in Backwards that the body goes to next")]
        [SerializeField, ReadOnly] private int step;

        // ===== Unity Lifecycle =====
        // Unity calls this at each physics step: 50 times a second, unless the project sets otherwise.
        private void FixedUpdate()
        {
            // 1. While recording: store where the body is now, and stop when the list is full.
            if (isRecording)
            {
                positions.Add(bodyToRecord.position);
                if (positions.Count >= maxPositions) isRecording = false;
            }

            // 2. While playing back: move the body to the next position. ++ adds one.
            if (isPlayingBack)
            {
                bodyToRecord.MovePosition(backwards[step]);
                step++;
                if (step >= backwards.Count) isPlayingBack = false;
            }
        }

        // ===== Actions =====
        // Lets the body go, and starts storing its positions.
        [ContextMenu("Record")]
        public void Record()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;
            if (bodyToRecord == null) return;

            positions.Clear();
            isPlayingBack = false;
            isRecording = true;
            // Physics moves the body while it records.
            bodyToRecord.isKinematic = false;
        }

        // Moves the body back along the stored positions, the last one first.
        [ContextMenu("Play Backwards")]
        public void PlayBackwards()
        {
            if (!Application.isPlaying) return;
            if (bodyToRecord == null) return;
            if (positions.Count == 0) return;

            // 1. Physics stops moving the body: the list moves it now.
            isRecording = false;
            bodyToRecord.isKinematic = true;

            // 2. Copy the positions into Backwards, the last one first.
            //    i starts at the last index and counts down: -- takes one away after each pass.
            backwards.Clear();
            for (int i = positions.Count - 1; i >= 0; i--)
            {
                backwards.Add(positions[i]);
            }

            // 3. Start from the first value of Backwards.
            step = 0;
            isPlayingBack = true;
        }
    }
}
