using UnityEngine;

namespace Kiosk.Interaction
{
    /// <summary>
    /// Basisklasse fuer alle interagierbaren Objekte. Benoetigt einen Collider.
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        public string Prompt = "[E] Interagieren";

        public virtual string GetPrompt() { return Prompt; }

        public abstract void Interact(Player.PlayerInteractor interactor);
    }
}
