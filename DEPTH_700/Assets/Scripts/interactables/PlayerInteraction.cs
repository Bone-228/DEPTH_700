using Unity.Netcode;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour
{
    [SerializeField] private Camera interactionCamera;
    [SerializeField] private float interactionDistance = 3f;
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

        // Your project has a separate scene camera tagged MainCamera.
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

        if (Input.GetKeyDown(dropKey) && inventory != null)
            inventory.RequestDrop();
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

        IInteractable interactable =
            hit.collider.GetComponentInParent<IInteractable>();

        if (interactable == null)
            return;

        Debug.Log($"Interacted with: {interactable.InteractionName}", hit.collider.gameObject);
        interactable.Interact(this);
    }

    public void RequestPickup(WorldPickup pickup)
    {
        if (!IsOwner || pickup == null || !pickup.IsSpawned)
            return;

        RequestPickupServerRpc(new NetworkObjectReference(pickup.NetworkObject));
    }

    [ServerRpc]
    private void RequestPickupServerRpc(NetworkObjectReference pickupReference)
    {
        if (inventory == null)
            return;

        if (!pickupReference.TryGet(out NetworkObject pickupObject))
            return;

        WorldPickup pickup = pickupObject.GetComponent<WorldPickup>();

        if (pickup == null || !pickup.IsSpawned)
            return;

        // Check the distance again on the server.
        if (Vector3.Distance(transform.position, pickup.transform.position) >
            interactionDistance + 0.5f)
            return;

        inventory.TryStoreOnServer(pickup);
    }
}