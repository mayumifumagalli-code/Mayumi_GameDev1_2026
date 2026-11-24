// "using" lets this script use code from elsewhere: here, [ReadOnly] from the Toolbox.
using Mavrodiev.Toolbox;
using UnityEngine;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Explode Action
    // ============================================
    // WHAT IT DOES: Pushes away every Rigidbody inside the Radius when Explode is called.
    //               Explode Tagged pushes only those with the Required Tag. Put Back returns the crates.
    // HOW TO USE:   Add it to any GameObject. Drag the crates around it into Crates, so that Put Back
    //               can return them. Call Explode from an event, a button or the right-click menu.
    // NEEDS:        Objects with a Rigidbody, closer than the Radius.
    // ============================================
    public class ExplodeAction : MonoBehaviour
    {
        // ===== Variables (public) =====
        [Header("Public")]
        [Tooltip("How far the explosion reaches, in metres")]
        [Min(0f)]
        public float radius = 2f;

        [Tooltip("How hard it pushes. 10 throws a crate about one metre.")]
        public float force = 10f;

        [Tooltip("Explode Tagged pushes only the objects with this tag. Explode pushes them all.")]
        public string requiredTag = "Player";

        [Tooltip("The crates that Put Back returns to where they stood at the start of the scene")]
        public Rigidbody[] crates;

        // ===== Variables (private) =====
        [Header("Private (read-only)")]
        [Tooltip("Where each crate stood at the start of the scene, in the same order as Crates")]
        [SerializeField, ReadOnly] private Vector3[] startPositions;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object first appears.
        private void Start()
        {
            // new Vector3[...] makes an array with one slot for each crate. ++ adds one.
            startPositions = new Vector3[crates.Length];
            for (int i = 0; i < crates.Length; i++)
            {
                if (crates[i] != null) startPositions[i] = crates[i].transform.position;
            }
        }

        // ===== Actions =====
        // Pushes every Rigidbody inside the radius away from this object.
        [ContextMenu("Explode")]
        public void Explode()
        {
            // These actions only work while the game runs.
            if (!Application.isPlaying) return;

            // OverlapSphere returns every collider inside the sphere, in an array.
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider hit in hits)
            {
                Rigidbody body = hit.GetComponent<Rigidbody>();
                if (body != null)
                {
                    body.AddExplosionForce(force, transform.position, radius, 0.5f, ForceMode.Impulse);
                }
            }
        }

        // Pushes away only the Rigidbodies inside the radius that have the required tag.
        [ContextMenu("Explode Tagged")]
        public void ExplodeTagged()
        {
            if (!Application.isPlaying) return;

            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider hit in hits)
            {
                Rigidbody body = hit.GetComponent<Rigidbody>();
                // && means "and": both must be true.
                if (body != null && hit.CompareTag(requiredTag))
                {
                    body.AddExplosionForce(force, transform.position, radius, 0.5f, ForceMode.Impulse);
                }
            }
        }

        // Puts each crate back where it stood at the start of the scene, and stops it.
        [ContextMenu("Put Back")]
        public void PutBack()
        {
            if (!Application.isPlaying) return;

            for (int i = 0; i < startPositions.Length; i++)
            {
                if (crates[i] != null)
                {
                    crates[i].linearVelocity = Vector3.zero;
                    crates[i].angularVelocity = Vector3.zero;
                    crates[i].transform.position = startPositions[i];
                    crates[i].transform.rotation = Quaternion.identity;
                }
            }
        }
    }
}
