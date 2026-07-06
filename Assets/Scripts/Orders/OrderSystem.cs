using System.Collections.Generic;
using UnityEngine;
using Kiosk.Products;

namespace Kiosk.Orders
{
    [System.Serializable]
    public class OrderLine
    {
        public string ProductId;
        public int Amount;
    }

    /// <summary>
    /// Warenbestellung ueber das Tablet. Kosten werden sofort abgezogen,
    /// die Lieferung kommt nach einem Timer ueber das DeliverySystem.
    /// </summary>
    public class OrderSystem : MonoBehaviour
    {
        public static OrderSystem Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public float GetBuyPrice(ProductData product)
        {
            float price = product.BuyPrice;
            var up = Upgrades.UpgradeManager.Instance;
            if (up != null)
                price *= 1f - Mathf.Clamp01(up.GetEffectValue(Upgrades.UpgradeEffect.SupplierDiscount));
            return price;
        }

        public float CalculateCost(List<OrderLine> lines, SupplierData supplier)
        {
            float total = supplier != null ? supplier.DeliveryFee : 0f;
            foreach (var line in lines)
            {
                var p = Core.DefaultGameData.GetProduct(line.ProductId);
                if (p != null) total += GetBuyPrice(p) * line.Amount;
            }
            return total;
        }

        /// <summary>Bestellung aufgeben. Gibt true bei Erfolg zurueck.</summary>
        public bool PlaceOrder(List<OrderLine> lines, SupplierData supplier)
        {
            if (lines == null || lines.Count == 0) return false;
            int units = 0;
            foreach (var l in lines) units += l.Amount;
            if (units <= 0) return false;

            var storage = Inventory.StorageManager.Instance;
            if (storage != null && units > storage.FreeUnits)
            {
                if (UI.UIManager.Instance != null)
                    UI.UIManager.Instance.ShowToast("Nicht genug Platz im Lager!");
                return false;
            }

            float cost = CalculateCost(lines, supplier);
            var eco = Economy.EconomyManager.Instance;
            if (eco == null || !eco.Spend(cost))
            {
                if (UI.UIManager.Instance != null)
                    UI.UIManager.Instance.ShowToast("Nicht genug Geld fuer die Bestellung!");
                return false;
            }

            var delivery = Delivery.DeliverySystem.Instance;
            if (delivery != null)
                delivery.ScheduleDelivery(lines, supplier != null ? supplier.DeliveryTimeSeconds : 45f);

            if (UI.UIManager.Instance != null)
                UI.UIManager.Instance.ShowToast("Bestellung aufgegeben (" + cost.ToString("F2") + " Euro). Lieferung unterwegs...");
            return true;
        }
    }
}
