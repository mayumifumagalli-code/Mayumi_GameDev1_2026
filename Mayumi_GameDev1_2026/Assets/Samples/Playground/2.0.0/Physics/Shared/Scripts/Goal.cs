using UnityEngine;

namespace Mavrodiev.Playground.Physics
{
    // A trigger that lights up when the right object enters it.
    [RequireComponent(typeof(Collider))]
    public class Goal : MonoBehaviour
    {
        public string targetName = "Ball";
        public Renderer lamp;
        public Color reachedColor = new Color(0.2f, 0.9f, 0.3f);

        void Reset() { GetComponent<Collider>().isTrigger = true; }

        void OnTriggerEnter(Collider other)
        {
            if (!other.name.StartsWith(targetName)) return;
            if (lamp != null) lamp.material.color = reachedColor;
            Debug.Log("Goal reached by " + other.name);
        }
    }
}
