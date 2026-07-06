using UnityEngine;
using Kiosk.Interaction;
using Kiosk.Customers;

namespace Kiosk.Checkout
{
    /// <summary>
    /// Kassentheke: Interaktion startet die Bedienung des vordersten Kunden.
    /// </summary>
    public class CheckoutCounter : Interactable
    {
        public static CheckoutCounter Instance { get; private set; }

        public CustomerAI CurrentCustomer { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public override string GetPrompt()
        {
            var queue = CustomerQueue.Instance;
            if (CurrentCustomer != null) return "[E] Kunde wird bedient...";
            if (queue != null && queue.Front != null && queue.IsFrontAndArrived(queue.Front))
                return "[E] Kunden bedienen";
            return "Kasse (keine wartenden Kunden)";
        }

        public override void Interact(Player.PlayerInteractor interactor)
        {
            if (CurrentCustomer != null) return;
            var queue = CustomerQueue.Instance;
            if (queue == null) return;
            var front = queue.Front;
            if (front == null || !queue.IsFrontAndArrived(front)) return;

            CurrentCustomer = front;
            front.IsBeingServed = true;
            front.State = CustomerState.BeingServed;
            var ui = UI.UIManager.Instance;
            if (ui != null) ui.OpenCheckout(front);
        }

        /// <summary>Bedienung beenden und Kunden entlassen.</summary>
        public void ReleaseCustomer(bool happy)
        {
            if (CurrentCustomer == null) return;
            var c = CurrentCustomer;
            CurrentCustomer = null;
            c.FinishService(happy);
        }

        public void CancelService()
        {
            if (CurrentCustomer == null) return;
            CurrentCustomer.IsBeingServed = false;
            var c = CurrentCustomer;
            CurrentCustomer = null;
            c.State = CustomerState.Waiting;
        }
    }
}
