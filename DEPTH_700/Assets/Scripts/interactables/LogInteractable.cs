using UnityEngine;

public class LogInteractable : MonoBehaviour, IInteractable
{
    public string InteractionName => gameObject.name;

    public void Interact(PlayerInteraction player)
    {
        // PlayerInteraction logs the name already.
    }
}