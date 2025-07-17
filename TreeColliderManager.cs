using System.Collections.Generic;
using UnityEngine;

public class TreeColliderManager : MonoBehaviour
{
    public GameObject treeColliderPrefab;
    public float spawnRadius = 50f;
    public float despawnRadius = 60f;
    public float updateInterval = 1f;

    public Transform player;
    private Dictionary<Vector3, GameObject> activeColliders = new();

    void Start()
    {
        InvokeRepeating(nameof(UpdateTreeColliders), 0f, updateInterval);
    }

    void UpdateTreeColliders()
    {
        if (player == null) return;

        foreach (Terrain terrain in Terrain.activeTerrains)
        {
            TerrainData data = terrain.terrainData;
            TreeInstance[] trees = data.treeInstances;

            foreach (TreeInstance tree in trees)
            {
                Vector3 worldPos = Vector3.Scale(tree.position, data.size) + terrain.transform.position;
                float dist = Vector3.Distance(worldPos, player.position);

                if (dist < spawnRadius)
                {
                    if (!activeColliders.ContainsKey(worldPos))
                    {
                        GameObject colliderObj = Instantiate(treeColliderPrefab, worldPos, Quaternion.identity);
                        colliderObj.GetComponent<TreeCollider>().Initialize(terrain, worldPos, tree.prototypeIndex);
                        activeColliders[worldPos] = colliderObj;
                    }
                }
            }
        }

        List<Vector3> toRemove = new();
        foreach (var kvp in activeColliders)
        {
            float dist = Vector3.Distance(kvp.Key, player.position);
            if (dist > despawnRadius)
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
        {
            activeColliders.Remove(key);
        }
    }
}
