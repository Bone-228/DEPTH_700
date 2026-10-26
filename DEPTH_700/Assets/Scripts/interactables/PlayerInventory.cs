using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    [SerializeField] private int capacity = 10;

    // Assign the matching pickup prefabs here in the Player prefab.
    [SerializeField] private WorldPickup[] itemPrefabs;

    // Optional. If empty, items drop in front of the player.
    [SerializeField] private Transform dropPoint;

    private readonly NetworkList<FixedString64Bytes> items = new();

    public NetworkList<FixedString64Bytes> Items => items;

    public void RequestDrop()
    {
        if (IsOwner && IsSpawned)
            DropLastItemServerRpc();
    }

    public void TryStoreOnServer(WorldPickup pickup)
    {
        if (!IsServer || pickup == null)
            return;

        if (items.Count >= capacity)
        {
            Debug.Log("Inventory is full.");
            return;
        }

        items.Add(new FixedString64Bytes(pickup.ItemId));
        Debug.Log($"Picked up {pickup.ItemId}. Inventory: {items.Count}/{capacity}");

        pickup.NetworkObject.Despawn();
    }

    [ServerRpc]
    private void DropLastItemServerRpc()
    {
        if (items.Count == 0)
            return;

        string itemId = items[items.Count - 1].ToString();
        WorldPickup prefab = FindPrefab(itemId);

        if (prefab == null)
        {
            Debug.LogError($"Assign a pickup prefab for item ID '{itemId}' in PlayerInventory.");
            return;
        }

        Vector3 position;
        Quaternion rotation;

        if (dropPoint != null)
        {
            position = dropPoint.position + dropPoint.forward * 1.5f;
            rotation = dropPoint.rotation;
        }
        else
        {
            position = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;
            rotation = transform.rotation;
        }

        WorldPickup droppedItem = Instantiate(prefab, position, rotation);
        NetworkObject networkObject = droppedItem.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Destroy(droppedItem.gameObject);
            Debug.LogError($"Pickup prefab '{prefab.name}' needs a NetworkObject.");
            return;
        }

        networkObject.Spawn();
        items.RemoveAt(items.Count - 1);

        Debug.Log($"Dropped {itemId}. Inventory: {items.Count}/{capacity}");
    }

    private WorldPickup FindPrefab(string itemId)
    {
        if (itemPrefabs == null)
            return null;

        foreach (WorldPickup prefab in itemPrefabs)
        {
            if (prefab != null && prefab.ItemId == itemId)
                return prefab;
        }

        return null;
    }
}