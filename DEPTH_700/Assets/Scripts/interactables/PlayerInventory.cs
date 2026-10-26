using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    private const int SlotCount = 4;

    // Assign each item prefab here. Each prefab needs a unique Item Id.
    [SerializeField] private WorldPickup[] itemPrefabs;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private Transform holdPoint;

    private readonly NetworkList<FixedString64Bytes> slots = new();
    private readonly NetworkVariable<NetworkObjectReference> heldObject = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkList<FixedString64Bytes> Slots => slots;

    private void Awake()
    {
        if (holdPoint == null)
            holdPoint = transform.Find("HoldPoint");
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        while (slots.Count < SlotCount)
            slots.Add(default);
    }

    public void RequestSelectSlot(int slotIndex)
    {
        if (IsOwner && IsSpawned && slotIndex >= 0 && slotIndex < SlotCount)
            SelectSlotServerRpc(slotIndex);
    }

    public void RequestDropHeldItem(Vector3 viewDirection)
    {
        if (IsOwner && IsSpawned)
            DropHeldItemServerRpc(viewDirection);
    }

    public void TryPickUpToHandsOnServer(WorldPickup pickup)
    {
        if (!IsServer || pickup == null || !pickup.IsSpawned || pickup.IsHeld)
            return;

        if (TryGetHeldPickup(out _))
        {
            Debug.Log("Your hands are full. Store or drop the held item first.");
            return;
        }

        if (holdPoint == null)
        {
            Debug.LogError("PlayerInventory could not find HoldPoint on the Player prefab.");
            return;
        }

        if (!pickup.NetworkObject.TrySetParent(NetworkObject, false))
        {
            Debug.LogError($"Could not attach {pickup.ItemId} to the player over the network.");
            return;
        }

        pickup.transform.localPosition = holdPoint.localPosition;
        pickup.transform.localRotation = holdPoint.localRotation;
        pickup.SetHeldOnServer(true);
        heldObject.Value = new NetworkObjectReference(pickup.NetworkObject);

        Debug.Log($"Picked up {pickup.ItemId}. Press 1–4 to store it in a free slot.");
    }

    [ServerRpc]
    private void SelectSlotServerRpc(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount || slots.Count != SlotCount)
            return;

        bool slotIsEmpty = slots[slotIndex].Length == 0;
        bool hasHeldItem = TryGetHeldPickup(out WorldPickup currentlyHeld);

        if (hasHeldItem)
        {
            if (!slotIsEmpty)
            {
                Debug.Log($"Slot {slotIndex + 1} is occupied. The held item stays in your hands.");
                return;
            }

            // The slot stores the item ID; despawning the held object frees the hands.
            slots[slotIndex] = new FixedString64Bytes(currentlyHeld.ItemId);
            heldObject.Value = default;
            currentlyHeld.NetworkObject.Despawn(true);
            Debug.Log($"Stored {currentlyHeld.ItemId} in slot {slotIndex + 1}.");
            return;
        }

        if (slotIsEmpty)
        {
            Debug.Log($"Slot {slotIndex + 1} is empty.");
            return;
        }

        if (holdPoint == null)
        {
            Debug.LogError("PlayerInventory could not find HoldPoint on the Player prefab.");
            return;
        }

        string itemId = slots[slotIndex].ToString();
        WorldPickup prefab = FindPrefab(itemId);
        if (prefab == null)
        {
            Debug.LogError($"No item prefab assigned for Item Id '{itemId}'.");
            return;
        }

        WorldPickup item = Instantiate(prefab, holdPoint.position, holdPoint.rotation);
        item.NetworkObject.Spawn();

        if (!item.NetworkObject.TrySetParent(NetworkObject, false))
        {
            item.NetworkObject.Despawn(true);
            Debug.LogError($"Could not attach {itemId} to the player over the network.");
            return;
        }

        item.transform.localPosition = holdPoint.localPosition;
        item.transform.localRotation = holdPoint.localRotation;
        item.SetHeldOnServer(true);

        heldObject.Value = new NetworkObjectReference(item.NetworkObject);
        slots[slotIndex] = default;
        Debug.Log($"Took {itemId} from slot {slotIndex + 1}.");
    }

    [ServerRpc]
    private void DropHeldItemServerRpc(Vector3 requestedViewDirection)
    {
        if (!TryGetHeldPickup(out WorldPickup item))
            return;

        float directionLengthSquared = requestedViewDirection.sqrMagnitude;
        if (float.IsNaN(directionLengthSquared) || float.IsInfinity(directionLengthSquared) ||
            directionLengthSquared < 0.25f || directionLengthSquared > 2.25f)
            return;

        if (!item.NetworkObject.TryRemoveParent())
        {
            Debug.LogError($"Could not detach {item.ItemId} from the player over the network.");
            return;
        }

        Vector3 forward = new Vector3(requestedViewDirection.x, 0f, requestedViewDirection.z);
        if (forward.sqrMagnitude < 0.01f)
            forward = transform.forward;
        forward.Normalize();

        Vector3 basePosition = dropPoint != null
            ? dropPoint.position
            : transform.position + Vector3.up * 0.5f;
        Vector3 position = basePosition + forward * 1.2f;
        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

        item.transform.SetPositionAndRotation(position, rotation);
        item.SetHeldOnServer(false);
        heldObject.Value = default;

        Debug.Log($"Dropped {item.ItemId}.");
    }

    private bool TryGetHeldPickup(out WorldPickup pickup)
    {
        pickup = null;
        if (!heldObject.Value.TryGet(out NetworkObject networkObject))
            return false;

        pickup = networkObject.GetComponent<WorldPickup>();
        return pickup != null && pickup.IsSpawned;
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
