using System.Collections.Generic;
using UnityEngine;
using Kiosk.Products;

namespace Kiosk.Checkout
{
    /// <summary>
    /// Scan-Logik der Kasse: Artikel einzeln scannen, Gesamtpreis berechnen.
    /// </summary>
    public class CashRegister : MonoBehaviour
    {
        public static CashRegister Instance { get; private set; }

        readonly List<ProductData> _toScan = new List<ProductData>();
        readonly List<ProductData> _scanned = new List<ProductData>();

        public event System.Action OnScanChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public IList<ProductData> RemainingItems { get { return _toScan; } }
        public IList<ProductData> ScannedItems { get { return _scanned; } }

        public void BeginTransaction(List<ProductData> basket)
        {
            _toScan.Clear();
            _scanned.Clear();
            if (basket != null) _toScan.AddRange(basket);
            Notify();
        }

        /// <summary>Wie lange ein Scan dauert (durch Upgrades verkuerzt).</summary>
        public float ScanDuration
        {
            get
            {
                float d = 0.5f;
                var up = Upgrades.UpgradeManager.Instance;
                if (up != null) d *= 1f - Mathf.Clamp01(up.GetEffectValue(Upgrades.UpgradeEffect.CheckoutSpeed));
                return Mathf.Max(0.1f, d);
            }
        }

        public bool ScanNext()
        {
            if (_toScan.Count == 0) return false;
            var item = _toScan[0];
            _toScan.RemoveAt(0);
            _scanned.Add(item);
            if (Audio.AudioManager.Instance != null)
                Audio.AudioManager.Instance.Play(Audio.SoundId.Scanner);
            Notify();
            return true;
        }

        public bool AllScanned { get { return _toScan.Count == 0; } }

        public float Total
        {
            get
            {
                float t = 0f;
                foreach (var p in _scanned) t += p.SellPrice;
                return t;
            }
        }

        public bool HasAgeRestrictedItem
        {
            get
            {
                foreach (var p in _scanned) if (p.AgeRestricted) return true;
                foreach (var p in _toScan) if (p.AgeRestricted) return true;
                return false;
            }
        }

        public void Clear()
        {
            _toScan.Clear();
            _scanned.Clear();
            Notify();
        }

        void Notify() { if (OnScanChanged != null) OnScanChanged(); }
    }
}
