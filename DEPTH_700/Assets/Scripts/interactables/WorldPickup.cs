using Unity.Netcode;
using UnityEngine;

public class WorldPickup : NetworkBehaviour, IInteractable
{
    [SerializeField] private string itemId = "Item";

    private readonly NetworkVariable<bool> isHeld = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Collider[] itemColliders;
    private bool[] colliderDefaults;
    private Rigidbody[] itemBodies;
    private bool[] bodyKinematicDefaults;
    private bool[] bodyGravityDefaults;

    public string ItemId => itemId;
    public bool IsHeld => isHeld.Value;
    public string InteractionName => gameObject.name;

    private void Awake()
    {
        CachePhysicsComponents();
    }

    public override void OnNetworkSpawn()
    {
        isHeld.OnValueChanged += OnHeldChanged;
        ApplyHeldState(isHeld.Value);
    }

    public override void OnNetworkDespawn()
    {
        isHeld.OnValueChanged -= OnHeldChanged;
    }

    public void Interact(PlayerInteraction player)
    {
        if (player != null)
            player.RequestPickup(this);
    }

    public void SetHeldOnServer(bool value)
    {
        if (IsServer)
            isHeld.Value = value;
    }

    private void OnHeldChanged(bool previousValue, bool newValue)
    {
        ApplyHeldState(newValue);
    }

    private void ApplyHeldState(bool held)
    {
        if (itemColliders == null)
            CachePhysicsComponents();

        for (int i = 0; i < itemColliders.Length; i++)
        {
            if (itemColliders[i] != null)
                itemColliders[i].enabled = held ? false : colliderDefaults[i];
        }

        for (int i = 0; i < itemBodies.Length; i++)
        {
            if (itemBodies[i] == null)
                continue;

            itemBodies[i].isKinematic = held || bodyKinematicDefaults[i];
            itemBodies[i].useGravity = held ? false : bodyGravityDefaults[i];

            if (held)
            {
                itemBodies[i].linearVelocity = Vector3.zero;
                itemBodies[i].angularVelocity = Vector3.zero;
            }
        }
    }

    private void CachePhysicsComponents()
    {
        itemColliders = GetComponentsInChildren<Collider>(true);
        colliderDefaults = new bool[itemColliders.Length];
        for (int i = 0; i < itemColliders.Length; i++)
            colliderDefaults[i] = itemColliders[i].enabled;

        itemBodies = GetComponentsInChildren<Rigidbody>(true);
        bodyKinematicDefaults = new bool[itemBodies.Length];
        bodyGravityDefaults = new bool[itemBodies.Length];
        for (int i = 0; i < itemBodies.Length; i++)
        {
            bodyKinematicDefaults[i] = itemBodies[i].isKinematic;
            bodyGravityDefaults[i] = itemBodies[i].useGravity;
        }
    }
}
