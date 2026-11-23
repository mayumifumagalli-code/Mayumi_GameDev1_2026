// ============================================
// Spawn At Action
// ============================================
// PURPOSE: Spawns a prefab at a specific transform position
// USAGE: Attach to any GameObject, set prefab and spawn point
// ACTIONS:
//   - Spawn() - spawn prefab
// ============================================

using UnityEngine;

public class SpawnAtAction : MonoBehaviour
{
    // ===== Target =====
    [Header("Prefab")]
    public GameObject prefab;

    // ===== Settings =====
    [Header("Settings")]
    public Transform spawnPoint;
    public Transform parent;
    public bool useSpawnRotation = true;

    // ===== Actions =====
    [ContextMenu("Spawn")]
    public void Spawn()
    {
        if (prefab == null) return;

        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rotation = useSpawnRotation && spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        Instantiate(prefab, position, rotation, parent);
    }

    // ===== Gizmos =====
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Gizmos.DrawWireCube(pos, Vector3.one * 0.3f);
    }
}

// ============================================
// IMPLEMENTATION STEPS
// ============================================
// 1. Attach to any GameObject
// 2. Set prefab to spawn
// 3. Set spawnPoint (or uses this transform)
// 4. Optionally set parent for spawned object
// 5. Call Spawn() from triggers
// ============================================
