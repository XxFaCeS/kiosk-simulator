using System.Collections.Generic;
using UnityEngine;
using Kiosk.Products;

namespace Kiosk.Inventory
{
    /// <summary>
    /// Logischer Lagerbestand: Produkt-ID -> Anzahl im Lager (Hinterzimmer).
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        readonly Dictionary<string, int> _stock = new Dictionary<string, int>();

        public event System.Action OnInventoryChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public int GetCount(string productId)
        {
            int c;
            return _stock.TryGetValue(productId, out c) ? c : 0;
        }

        public int TotalUnits
        {
            get { int t = 0; foreach (var kv in _stock) t += kv.Value; return t; }
        }

        public bool Add(string productId, int amount)
        {
            if (amount <= 0) return false;
            var storage = StorageManager.Instance;
            if (storage != null && TotalUnits + amount > storage.Capacity) return false;
            _stock[productId] = GetCount(productId) + amount;
            Notify();
            return true;
        }

        public bool Remove(string productId, int amount)
        {
            if (amount <= 0 || GetCount(productId) < amount) return false;
            _stock[productId] -= amount;
            if (_stock[productId] <= 0) _stock.Remove(productId);
            Notify();
            return true;
        }

        public Dictionary<string, int> GetAllStock() { return new Dictionary<string, int>(_stock); }

        public void LoadStock(Dictionary<string, int> stock)
        {
            _stock.Clear();
            if (stock != null)
                foreach (var kv in stock) _stock[kv.Key] = kv.Value;
            Notify();
        }

        void Notify() { if (OnInventoryChanged != null) OnInventoryChanged(); }
    }
}
