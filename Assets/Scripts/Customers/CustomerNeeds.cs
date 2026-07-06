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
            int wishes = Random.value < 0.5f ? 1 : (Random.value < 0.8f ? 2 : 3);
            return PickDesiredProducts(wishes);
        }

        public static List<ProductData> PickDesiredProducts(int wishes)
        {
            var result = new List<ProductData>();
            var unlocked = Core.UnlockManager.Instance != null
                ? Core.UnlockManager.Instance.GetUnlockedProducts()
                : new List<ProductData>(Core.DefaultGameData.Products);
            if (unlocked.Count == 0) return result;

            wishes = Mathf.Clamp(wishes, 1, Mathf.Min(5, unlocked.Count));
            for (int i = 0; i < wishes; i++)
            {
                var pick = WeightedPick(unlocked, result);
                if (pick != null && !result.Contains(pick)) result.Add(pick);
            }
            return result;
        }

        public static ProductData PickBonusProduct(List<ProductData> excluded)
        {
            var unlocked = Core.UnlockManager.Instance != null
                ? Core.UnlockManager.Instance.GetUnlockedProducts()
                : new List<ProductData>(Core.DefaultGameData.Products);
            return WeightedPick(unlocked, excluded);
        }

        static ProductData WeightedPick(List<ProductData> products, List<ProductData> excluded)
        {
            float total = 0f;
            foreach (var p in products)
                if (excluded == null || !excluded.Contains(p))
                    total += EffectiveDemand(p);
            if (total <= 0f) return null;
            float r = Random.value * total;
            foreach (var p in products)
            {
                if (excluded != null && excluded.Contains(p)) continue;
                r -= EffectiveDemand(p);
                if (r <= 0f) return p;
            }
            for (int i = products.Count - 1; i >= 0; i--)
                if (excluded == null || !excluded.Contains(products[i]))
                    return products[i];
            return null;
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
