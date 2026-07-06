using System.Collections.Generic;
using UnityEngine;
using Kiosk.Products;

namespace Kiosk.Customers
{
    /// <summary>
    /// Waehlt Kundenwuensche gewichtet nach Nachfragewert und Upgrades aus.
    /// </summary>
    public static class CustomerNeeds
    {
        /// <summary>Waehlt 1-3 gewuenschte Produkte aus den freigeschalteten Produkten.</summary>
        public static List<ProductData> PickDesiredProducts()
        {
            var result = new List<ProductData>();
            var unlocked = Core.UnlockManager.Instance != null
                ? Core.UnlockManager.Instance.GetUnlockedProducts()
                : new List<ProductData>(Core.DefaultGameData.Products);
            if (unlocked.Count == 0) return result;

            int wishes = Random.value < 0.5f ? 1 : (Random.value < 0.8f ? 2 : 3);
            for (int i = 0; i < wishes; i++)
            {
                var pick = WeightedPick(unlocked);
                if (pick != null && !result.Contains(pick)) result.Add(pick);
            }
            return result;
        }

        static ProductData WeightedPick(List<ProductData> products)
        {
            float total = 0f;
            foreach (var p in products) total += EffectiveDemand(p);
            if (total <= 0f) return null;
            float r = Random.value * total;
            foreach (var p in products)
            {
                r -= EffectiveDemand(p);
                if (r <= 0f) return p;
            }
            return products[products.Count - 1];
        }

        public static float EffectiveDemand(ProductData p)
        {
            float d = p.Demand;
            var upgrades = Upgrades.UpgradeManager.Instance;
            if (upgrades != null)
            {
                bool isDrink = p.Category == ProductCategory.Wasser || p.Category == ProductCategory.Softdrinks
                    || p.Category == ProductCategory.Energydrinks || p.Category == ProductCategory.Eis;
                if (isDrink) d *= 1f + upgrades.GetEffectValue(Upgrades.UpgradeEffect.DrinkDemand);
                if (p.Category == ProductCategory.Kaffee) d *= 1f + upgrades.GetEffectValue(Upgrades.UpgradeEffect.CoffeeDemand);
            }
            return d;
        }
    }
}
