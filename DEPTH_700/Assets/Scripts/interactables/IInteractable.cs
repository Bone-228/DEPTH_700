public interface IInteractable
{
    string InteractionName { get; }
    void Interact(PlayerInteraction player);
}