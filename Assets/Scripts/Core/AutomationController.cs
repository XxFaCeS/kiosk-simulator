using UnityEngine;
using Kiosk.Customers;

namespace Kiosk.Core
{
    /// <summary>
    /// Automatisierungs-Upgrades: Mitarbeiter-Kasse, Self-Checkout und
    /// automatische Regalauffuellung.
    /// </summary>
    public class AutomationController : MonoBehaviour
    {
        float _employeeTimer;
        float _restockTimer;

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.IsPaused) return;
            var up = Upgrades.UpgradeManager.Instance;
            if (up == null) return;

            if (up.IsPurchased("mitarbeiter_kasse")) TickEmployee();
            if (up.IsPurchased("self_checkout")) TickSelfCheckout(up);
            if (up.IsPurchased("auto_regal")) TickAutoRestock();
        }

        void TickEmployee()
        {
            var queue = CustomerQueue.Instance;
            var counter = Checkout.CheckoutCounter.Instance;
            if (queue == null || counter == null || counter.CurrentCustomer != null) return;
            var front = queue.Front;
            if (front == null || !queue.IsFrontAndArrived(front) || front.IsBeingServed) return;

            _employeeTimer += Time.deltaTime;
            if (_employeeTimer < 8f) return;
            _employeeTimer = 0f;
            AutoServe(front, "Mitarbeiter");
        }

        void TickSelfCheckout(Upgrades.UpgradeManager up)
        {
            var queue = CustomerQueue.Instance;
            var counter = Checkout.CheckoutCounter.Instance;
            if (queue == null || counter == null || counter.CurrentCustomer != null) return;
            var front = queue.Front;
            if (front == null || !queue.IsFrontAndArrived(front) || front.IsBeingServed) return;
            if (front.Intent != CustomerIntent.Shopping) return;

            // Anteil der Kunden nutzt den Self-Checkout
            if (Random.value > up.GetEffectValue(Upgrades.UpgradeEffect.SelfCheckout) * Time.deltaTime) return;
            AutoServe(front, "Self-Checkout");
        }

        void AutoServe(CustomerAI customer, string label)
        {
            customer.IsBeingServed = true;
            if (customer.Intent == CustomerIntent.Shopping && customer.Basket.Count > 0)
            {
                bool restricted = false;
                foreach (var p in customer.Basket) if (p.AgeRestricted) restricted = true;
                if (restricted && !AgeRestricted.AgeCheckSystem.Instance.IsAdult(customer))
                {
                    // Mitarbeiter lehnt Minderjaehrige korrekt ab
                    AgeRestricted.AgeCheckSystem.Instance.ProcessRefusal(customer);
                    customer.FinishService(false);
                    return;
                }
                var register = Checkout.CashRegister.Instance;
                register.BeginTransaction(new System.Collections.Generic.List<Products.ProductData>(customer.Basket));
                while (register.ScanNext()) { }
                Checkout.PaymentSystem.Instance.CompleteSale(customer, Checkout.PaymentMethod.Card);
                if (UI.UIManager.Instance != null)
                    UI.UIManager.Instance.ShowToast(label + ": Kunde automatisch bedient.");
                customer.FinishService(true);
            }
            else
            {
                customer.FinishService(true);
            }
        }

        void TickAutoRestock()
        {
            _restockTimer += Time.deltaTime;
            if (_restockTimer < 12f) return;
            _restockTimer = 0f;
            foreach (var shelf in Shelves.Shelf.AllShelves)
                if (shelf.RestockFromInventory() > 0) break;
        }
    }
}
