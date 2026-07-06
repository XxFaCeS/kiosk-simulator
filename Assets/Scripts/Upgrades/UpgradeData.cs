using UnityEngine;

namespace Kiosk.Upgrades
{
    public enum UpgradeEffect
    {
        ShelfCapacity, ExtraShelf, DrinkDemand, CoffeeDemand, Patience, CheckoutSpeed,
        CardPayment, SelfCheckout, StorageCapacity, PackageCapacity, PackageBonus,
        LottoTerminal, LottoCommission, TobaccoCabinet, TheftReduction, Reputation,
        CustomerRate, EmployeeCheckout, AutoRestock, SupplierDiscount, ProductMargin
    }

    /// <summary>
    /// Datendefinition eines Shop-Upgrades.
    /// </summary>
    [CreateAssetMenu(fileName = "Upgrade", menuName = "Kiosk/Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public float Cost;
        public int UnlockLevel = 1;
        public string RequiresUpgradeId;
        public UpgradeEffect Effect;
        public float EffectValue;

        public static UpgradeData Create(string id, string name, string description,
            float cost, int unlockLevel, string requires, UpgradeEffect effect, float value)
        {
            var u = CreateInstance<UpgradeData>();
            u.name = id;
            u.Id = id;
            u.DisplayName = name;
            u.Description = description;
            u.Cost = cost;
            u.UnlockLevel = unlockLevel;
            u.RequiresUpgradeId = requires;
            u.Effect = effect;
            u.EffectValue = value;
            return u;
        }
    }
}
