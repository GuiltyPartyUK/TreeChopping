using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TreeCollider : MonoBehaviour
{
    private Terrain terrain;
    private Vector3 worldPosition;
    private int prototypeIndex;

    [SerializeField] private float currHealth = 3f;
    [SerializeField] private float minHealth = 3f;
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] InventoryItem logItem;
    [SerializeField] InventoryItem branchItem;
    public GameObject fallenTreeePrefab;

    [Tooltip("Chance to spawn a stick once the tree is chopped. Smaller number = smaller chance.")]
    [Range(0, 100)]
    public int chanceToSpawnBranch;

    [Tooltip("Chance to get wood once the tree is chopped. Smaller number = smaller chance.")]
    [Range(0, 100)]
    public int chanceToGetWood;

    public void Initialize(Terrain t, Vector3 pos, int protoIndex)
    {
        terrain = t;
        worldPosition = pos;
        prototypeIndex = protoIndex;
        currHealth = Random.Range(minHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        currHealth -= amount;
        SoundManager.Instance.PlaySound(
            SoundManager.Instance.woodHitClips[Random.Range(0, SoundManager.Instance.woodHitClips.Length)],
            transform.position,
            1f
        );

        int logSpawnChance = Random.Range(0, 100);
        if (logSpawnChance < chanceToGetWood + Player.instance.baseResourceGainChance)
        {
            bool added = InventoryUI.Instance.TryAddItem(logItem);
            if (added)
            {
                PickupNotifierUI.Instance.ShowPickupMessage($"Got: {logItem.itemName}", logItem.icon);
            }
            else
            {
                PickupNotifierUI.Instance.ShowPickupMessage($"{logItem.name} not added. Weight exceeded.", logItem.icon);
            }
        }

        int spawnNumber = Random.Range(0, 100);
        if (spawnNumber <= chanceToSpawnBranch)
        {
            InventoryUI.Instance.TryAddItem(branchItem);
            PickupNotifierUI.Instance.ShowPickupMessage($"Got: {branchItem.itemName}", branchItem.icon);
        }

        if (currHealth <= 0f)
        {
            SpawnFelledTreePrefab();
            RemoveTreeFromTerrain();
            Destroy(gameObject);
            PlayerXP.Instance.GainXP(3);
            SoundManager.Instance.PlaySound(
                SoundManager.Instance.treeFallClips[Random.Range(0, SoundManager.Instance.treeFallClips.Length)],
                transform.position
            );
        }
    }

    void SpawnFelledTreePrefab()
    {
        TreePrototype[] prototypes = terrain.terrainData.treePrototypes;
        if (prototypeIndex >= 0 && prototypeIndex < prototypes.Length)
        {
            GameObject treePrefab = prototypes[prototypeIndex].prefab;
            if (treePrefab != null)
            {
                //GameObject go = Instantiate(treePrefab, worldPosition, Quaternion.identity);
                GameObject go = Instantiate(fallenTreeePrefab, worldPosition, Quaternion.identity);
                Collider col = go.AddComponent<CapsuleCollider>();
                Destroy(go, 10f);

                //go.AddComponent<Rigidbody>();

                //award the last log.
                InventoryUI.Instance.TryAddItem(logItem);
                PickupNotifierUI.Instance.ShowPickupMessage($"Got: {logItem.itemName}", logItem.icon);
            }
        }
    }

    void RemoveTreeFromTerrain()
    {
        TreeInstance[] trees = terrain.terrainData.treeInstances;
        List<TreeInstance> treeList = new(trees);

        int index = -1;
        float closest = float.MaxValue;
        for (int i = 0; i < treeList.Count; i++)
        {
            Vector3 treePos = Vector3.Scale(treeList[i].position, terrain.terrainData.size) + terrain.transform.position;
            float dist = Vector3.Distance(treePos, worldPosition);
            

            if (dist < 2f && dist < closest)
            {
                index = i;
                closest = dist;
            }
        }

        if (index != -1)
        {
            treeList.RemoveAt(index);
            terrain.terrainData.treeInstances = treeList.ToArray();
            terrain.Flush();
        }
    }

     
}
