using System.Collections.Generic;
using UnityEngine;

namespace Kiosk.Upgrades
{
    /// <summary>
    /// Kauf und Anwendung von Upgrades inkl. visueller Effekte im Laden.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        readonly HashSet<string> _purchased = new HashSet<string>();

        public event System.Action OnUpgradesChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool IsPurchased(string upgradeId) { return _purchased.Contains(upgradeId); }

        public List<string> GetPurchasedIds() { return new List<string>(_purchased); }

        public bool CanPurchase(UpgradeData upgrade, out string reason)
        {
            reason = null;
            if (upgrade == null) { reason = "Unbekanntes Upgrade"; return false; }
            if (_purchased.Contains(upgrade.Id)) { reason = "Bereits gekauft"; return false; }
            var gm = Core.GameManager.Instance;
            if (gm != null && gm.Level < upgrade.UnlockLevel) { reason = "Benoetigt Level " + upgrade.UnlockLevel; return false; }
            if (!string.IsNullOrEmpty(upgrade.RequiresUpgradeId) && !_purchased.Contains(upgrade.RequiresUpgradeId))
            {
                var req = Core.DefaultGameData.GetUpgrade(upgrade.RequiresUpgradeId);
                reason = "Benoetigt: " + (req != null ? req.DisplayName : upgrade.RequiresUpgradeId);
                return false;
            }
            var eco = Economy.EconomyManager.Instance;
            if (eco != null && !eco.CanAfford(upgrade.Cost)) { reason = "Nicht genug Geld"; return false; }
            return true;
        }

        public bool Purchase(UpgradeData upgrade)
        {
            string reason;
            if (!CanPurchase(upgrade, out reason)) return false;
            var eco = Economy.EconomyManager.Instance;
            if (eco != null && !eco.Spend(upgrade.Cost)) return false;
            ApplyUpgrade(upgrade.Id, true);
            if (Core.GameManager.Instance != null) Core.GameManager.Instance.AddXP(20);
            return true;
        }

        /// <summary>Wendet ein Upgrade an (beim Kauf oder Laden eines Spielstands).</summary>
        public void ApplyUpgrade(string upgradeId, bool notify)
        {
            if (_purchased.Contains(upgradeId)) return;
            _purchased.Add(upgradeId);
            var upgrade = Core.DefaultGameData.GetUpgrade(upgradeId);
            if (upgrade != null) ApplyEffect(upgrade);
            if (notify && OnUpgradesChanged != null) OnUpgradesChanged();
        }

        void ApplyEffect(UpgradeData upgrade)
        {
            switch (upgrade.Effect)
            {
                case UpgradeEffect.ShelfCapacity:
                    Shelves.ShelfSlot.SetGlobalBonusCapacity(
                        Shelves.ShelfSlot.GlobalBonusCapacity + Mathf.RoundToInt(upgrade.EffectValue));
                    break;
                case UpgradeEffect.StorageCapacity:
                    if (Inventory.StorageManager.Instance != null)
                        Inventory.StorageManager.Instance.AddBonusCapacity(Mathf.RoundToInt(upgrade.EffectValue));
                    break;
                case UpgradeEffect.PackageCapacity:
                    if (Packages.PackageSystem.Instance != null)
                        Packages.PackageSystem.Instance.AddCapacity(Mathf.RoundToInt(upgrade.EffectValue));
                    break;
                case UpgradeEffect.Reputation:
                    if (Economy.ReputationManager.Instance != null)
                        Economy.ReputationManager.Instance.Add(upgrade.EffectValue);
                    break;
            }
            // Visueller Effekt im Laden
            if (Core.SceneBootstrapper.Instance != null)
                Core.SceneBootstrapper.Instance.ApplyUpgradeVisual(upgrade.Id);
        }

        /// <summary>Summe der Effektwerte aller gekauften Upgrades mit diesem Effekt.</summary>
        public float GetEffectValue(UpgradeEffect effect)
        {
            float total = 0f;
            foreach (var id in _purchased)
            {
                var u = Core.DefaultGameData.GetUpgrade(id);
                if (u != null && u.Effect == effect) total += u.EffectValue;
            }
            return total;
        }

        public void LoadPurchased(List<string> ids)
        {
            _purchased.Clear();
            Shelves.ShelfSlot.SetGlobalBonusCapacity(0);
            if (Inventory.StorageManager.Instance != null)
                Inventory.StorageManager.Instance.SetBonusCapacity(0);
            if (ids != null)
                foreach (var id in ids) ApplyUpgrade(id, false);
            if (OnUpgradesChanged != null) OnUpgradesChanged();
        }
    }
}
