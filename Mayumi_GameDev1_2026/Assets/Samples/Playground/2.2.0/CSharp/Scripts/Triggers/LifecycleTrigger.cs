using UnityEngine;
using UnityEngine.Events;

namespace Mavrodiev.Playground.CSharp
{
    // ============================================
    // Lifecycle Trigger
    // ============================================
    // WHAT IT DOES: Fires an event at each of Unity's own moments for this object: once when it wakes,
    //               each time it is switched on, once before its first frame, every frame while it is on,
    //               and each time it is switched off.
    // HOW TO USE:   Add it to any GameObject. Choose what happens in the events you need.
    //               Switch the object off and on to see On Enable and On Disable fire.
    // NEEDS:        Nothing.
    // ============================================
    public class LifecycleTrigger : MonoBehaviour
    {
        // ===== Events =====
        [Header("Events")]
        public UnityEvent onAwake;
        public UnityEvent onEnable;
        public UnityEvent onStart;
        public UnityEvent onUpdate;
        public UnityEvent onDisable;

        // ===== Unity Lifecycle =====
        // Unity calls this once, when the object first becomes active.
        private void Awake()
        {
            onAwake.Invoke();
        }

        // Unity calls this each time the object is switched on, the first time too.
        private void OnEnable()
        {
            onEnable.Invoke();
        }

        // Unity calls this once, before the object's first Update.
        private void Start()
        {
            onStart.Invoke();
        }

        // Unity calls this once every frame, while the object is on.
        private void Update()
        {
            onUpdate.Invoke();
        }

        // Unity calls this each time the object is switched off, and when it is destroyed.
        private void OnDisable()
        {
            onDisable.Invoke();
        }
    }
}
