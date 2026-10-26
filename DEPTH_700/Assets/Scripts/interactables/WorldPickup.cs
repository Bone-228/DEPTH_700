using Unity.Netcode;
using UnityEngine;

public class WorldPickup : NetworkBehaviour, IInteractable
{
    [SerializeField] private string itemId = "Item";

    public string ItemId => itemId;

    // This is the name printed in the Console.
    public string InteractionName => gameObject.name;

    public void Interact(PlayerInteraction player)
    {
        if (player != null)
            player.RequestPickup(this);
    }
}