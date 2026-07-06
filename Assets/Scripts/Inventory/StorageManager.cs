using UnityEngine;

namespace Kiosk.Inventory
{
    /// <summary>
    /// Physischer Lagerraum: Kapazitaet (erweiterbar durch Upgrades).
    /// </summary>
    public class StorageManager : MonoBehaviour
    {
        public static StorageManager Instance { get; private set; }

        public int BaseCapacity = 100;
        int _bonusCapacity;

        public int Capacity { get { return BaseCapacity + _bonusCapacity; } }
        public int UsedUnits { get { return InventoryManager.Instance != null ? InventoryManager.Instance.TotalUnits : 0; } }
        public int FreeUnits { get { return Capacity - UsedUnits; } }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void AddBonusCapacity(int amount) { _bonusCapacity += amount; }
        public void SetBonusCapacity(int amount) { _bonusCapacity = amount; }
        public int BonusCapacity { get { return _bonusCapacity; } }
    }
}
