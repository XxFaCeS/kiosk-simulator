using System.Collections.Generic;
using UnityEngine;
using Kiosk.Products;

namespace Kiosk.Core
{
    /// <summary>
    /// Freischaltungen nach Level: Produkte und Services.
    /// Level 1: Snacks/Getraenke, 2: Zeitungen, 3: Paketannahme, 4: Kaffee/Prepaid,
    /// 5: Lotto, 6: fiktive Tabakwaren, 8: Mitarbeiter, 10: Self-Checkout/Paketstation.
    /// </summary>
    public class UnlockManager : MonoBehaviour
    {
        public static UnlockManager Instance { get; private set; }

        public event System.Action OnUnlocksChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnLevelUp += HandleLevelUp;
        }

        void HandleLevelUp(int level)
        {
            if (OnUnlocksChanged != null) OnUnlocksChanged();
            if (UI.UIManager.Instance != null)
                UI.UIManager.Instance.ShowToast("Level " + level + " erreicht! Neue Inhalte freigeschaltet.");
        }

        public int CurrentLevel { get { return GameManager.Instance != null ? GameManager.Instance.Level : 1; } }

        public bool IsProductUnlocked(ProductData product)
        {
            if (product == null) return false;
            if (product.UnlockLevel > CurrentLevel) return false;
            if (product.AgeRestricted && !IsServiceUnlocked(ServiceType.Tabakwaren)) return false;
            if (product.IsLottoProduct && !IsServiceUnlocked(ServiceType.Lotto)) return false;
            return true;
        }

        public bool IsServiceUnlocked(ServiceType service)
        {
            int lvl = CurrentLevel;
            switch (service)
            {
                case ServiceType.SnacksGetraenke: return lvl >= 1;
                case ServiceType.ZeitungenMagazine: return lvl >= 2;
                case ServiceType.Paketannahme: return lvl >= 3;
                case ServiceType.KaffeePrepaid: return lvl >= 4;
                case ServiceType.Lotto: return lvl >= 5;
                case ServiceType.Tabakwaren: return lvl >= 6;
                case ServiceType.Mitarbeiter: return lvl >= 8;
                case ServiceType.SelfCheckout: return lvl >= 10;
                case ServiceType.Paketstation: return lvl >= 10;
                default: return false;
            }
        }

        public List<ProductData> GetUnlockedProducts()
        {
            var list = new List<ProductData>();
            foreach (var p in DefaultGameData.Products)
                if (IsProductUnlocked(p)) list.Add(p);
            return list;
        }
    }

    public enum ServiceType
    {
        SnacksGetraenke, ZeitungenMagazine, Paketannahme, KaffeePrepaid,
        Lotto, Tabakwaren, Mitarbeiter, SelfCheckout, Paketstation
    }
}
