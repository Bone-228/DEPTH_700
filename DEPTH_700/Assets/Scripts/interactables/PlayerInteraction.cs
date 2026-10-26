using Unity.Netcode;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour
{
    [SerializeField] private Camera interactionCamera;
    [SerializeField, Min(0.1f)] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionLayers = ~0;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode dropKey = KeyCode.G;

    private PlayerInventory inventory;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        // The project camera is a separate scene object tagged MainCamera.
        if (interactionCamera == null)
            interactionCamera = Camera.main;

        if (interactionCamera == null)
            Debug.LogError("PlayerInteraction: could not find the Main Camera.", this);

        if (inventory == null)
            Debug.LogError("PlayerInteraction needs PlayerInventory on the Player.", this);
    }

    private void Update()
    {
        if (!IsSpawned || !IsOwner)
            return;

        if (Input.GetKeyDown(interactKey))
            TryInteract();

        if (inventory == null)
            return;

        if (Input.GetKeyDown(dropKey))
        {
            Vector3 viewDirection = interactionCamera != null
                ? interactionCamera.transform.forward
                : transform.forward;
            inventory.RequestDropHeldItem(viewDirection);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.RequestSelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.RequestSelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) inventory.RequestSelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) inventory.RequestSelectSlot(3);
    }

    private void TryInteract()
    {
        if (interactionCamera == null)
            return;

        Ray ray = new Ray(
            interactionCamera.transform.position,
            interactionCamera.transform.forward
        );

        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayers))
            return;

        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
        if (interactable == null)
            return;

        Debug.Log($"Interacted with: {interactable.InteractionName}", hit.collider.gameObject);
        interactable.Interact(this);
    }

    public void RequestPickup(WorldPickup pickup)
    {
        if (!IsOwner || !IsSpawned || pickup == null || !pickup.IsSpawned)
            return;

        RequestPickupServerRpc(new NetworkObjectReference(pickup.NetworkObject));
    }

    [ServerRpc]
    private void RequestPickupServerRpc(NetworkObjectReference pickupReference)
    {
        if (inventory == null || !pickupReference.TryGet(out NetworkObject pickupObject))
            return;

        WorldPickup pickup = pickupObject.GetComponent<WorldPickup>();
        if (pickup == null || !pickup.IsSpawned || pickup.IsHeld)
            return;

        // Validate distance on the server so clients can't pick up remote items.
        if (Vector3.Distance(transform.position, pickup.transform.position) >
            interactionDistance + 0.5f)
            return;

        inventory.TryPickUpToHandsOnServer(pickup);
    }
}
