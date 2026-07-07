using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Kiosk.Placement;
using Kiosk.Shelves;

namespace Kiosk.SaveSystem
{
    [System.Serializable]
    public class StockEntry { public string ProductId; public int Count; }

    [System.Serializable]
    public class ShelfSlotEntry
    {
        public int ShelfIndex;
        public int SlotIndex;
        public string ProductId;
        public int Count;
        public string ShelfName;
        public Vector3 ShelfPosition;
    }

    [System.Serializable]
    public class PriceEntry { public string ProductId; public float SellPrice; }

    [System.Serializable]
    public class PlacedObjectEntry
    {
        public string ItemId;
        public Vector3 Position;
        public Vector3 EulerAngles;
    }

    [System.Serializable]
    public class DeliveryBoxEntry
    {
        public Vector3 Position;
        public List<Orders.OrderLine> Contents = new List<Orders.OrderLine>();
    }

    [System.Serializable]
    public class SaveGame
    {
        public float Money;
        public int Day;
        public int Level;
        public int XP;
        public float Reputation;
        public float TimeOfDay;
        public List<StockEntry> Inventory = new List<StockEntry>();
        public List<ShelfSlotEntry> ShelfStock = new List<ShelfSlotEntry>();
        public List<string> PurchasedUpgrades = new List<string>();
        public List<Packages.PackageItem> StoredPackages = new List<Packages.PackageItem>();
        public List<PriceEntry> Prices = new List<PriceEntry>();
        public List<PlacedObjectEntry> PlacedObjects = new List<PlacedObjectEntry>();
        public List<Delivery.PendingDeliverySaveData> PendingDeliveries = new List<Delivery.PendingDeliverySaveData>();
        public List<DeliveryBoxEntry> SpawnedDeliveryBoxes = new List<DeliveryBoxEntry>();
        public float[] WeekProfits = new float[7];
        public bool TutorialCompleted;
        public float LifetimeRevenue;
        public int LifetimeCustomersServed;
    }

    /// <summary>
    /// JSON-Speichersystem. Datei liegt in Application.persistentDataPath/kiosk_save.json.
    /// </summary>
    public class SaveLoadSystem : MonoBehaviour
    {
        const float ShelfExactMatchDistance = 0.05f;
        const float ShelfFallbackMatchDistance = 0.35f;

        public static SaveLoadSystem Instance { get; private set; }
        public bool TutorialCompleted { get; private set; }

        public static string SavePath
        {
            get { return Path.Combine(Application.persistentDataPath, "kiosk_save.json"); }
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public static bool SaveExists() { return File.Exists(SavePath); }

        public static void DeleteSave() { if (SaveExists()) File.Delete(SavePath); }

        public void Save()
        {
            var save = new SaveGame();
            var gm = Core.GameManager.Instance;
            if (gm != null) { save.Day = gm.Day; save.Level = gm.Level; save.XP = gm.XP; }
            var eco = Economy.EconomyManager.Instance;
            if (eco != null)
            {
                save.Money = eco.Money;
                save.WeekProfits = eco.WeekProfits;
                save.LifetimeRevenue = eco.LifetimeRevenue;
                save.LifetimeCustomersServed = eco.LifetimeCustomersServed;
            }
            var rep = Economy.ReputationManager.Instance;
            if (rep != null) save.Reputation = rep.Reputation;
            var cycle = Core.DayNightCycle.Instance;
            if (cycle != null) save.TimeOfDay = cycle.TimeOfDay;
            save.TutorialCompleted = TutorialCompleted;

            var inv = Inventory.InventoryManager.Instance;
            if (inv != null)
                foreach (var kv in inv.GetAllStock())
                    save.Inventory.Add(new StockEntry { ProductId = kv.Key, Count = kv.Value });

            for (int s = 0; s < Shelf.AllShelves.Count; s++)
            {
                var shelf = Shelf.AllShelves[s];
                for (int i = 0; i < shelf.Slots.Count; i++)
                {
                    var slot = shelf.Slots[i];
                    if (slot.Product != null && slot.Count > 0)
                        save.ShelfStock.Add(new ShelfSlotEntry
                        {
                            ShelfIndex = s,
                            SlotIndex = i,
                            ProductId = slot.Product.Id,
                            Count = slot.Count,
                            ShelfName = shelf.name,
                            ShelfPosition = shelf.transform.position
                        });
                }
            }

            var up = Upgrades.UpgradeManager.Instance;
            if (up != null) save.PurchasedUpgrades = up.GetPurchasedIds();

            var pkg = Packages.PackageSystem.Instance;
            if (pkg != null) save.StoredPackages = pkg.GetSaveData();
            var delivery = Delivery.DeliverySystem.Instance;
            if (delivery != null) save.PendingDeliveries = delivery.GetPendingDeliveryData();

            foreach (var marker in FindObjectsOfType<PlacedObjectMarker>())
            {
                if (marker == null || string.IsNullOrEmpty(marker.ItemId)) continue;
                save.PlacedObjects.Add(new PlacedObjectEntry
                {
                    ItemId = marker.ItemId,
                    Position = marker.transform.position,
                    EulerAngles = marker.transform.eulerAngles
                });
            }

            foreach (var box in FindObjectsOfType<Delivery.DeliveryBox>())
            {
                if (box == null) continue;
                save.SpawnedDeliveryBoxes.Add(new DeliveryBoxEntry
                {
                    Position = box.transform.position,
                    Contents = box.GetContentsSnapshot()
                });
            }

            foreach (var p in Core.DefaultGameData.Products)
                save.Prices.Add(new PriceEntry { ProductId = p.Id, SellPrice = p.SellPrice });

            File.WriteAllText(SavePath, JsonUtility.ToJson(save, true));
            if (UI.UIManager.Instance != null) UI.UIManager.Instance.ShowToast("Spiel gespeichert.");
        }

        public bool Load()
        {
            if (!SaveExists()) return false;
            SaveGame save;
            try { save = JsonUtility.FromJson<SaveGame>(File.ReadAllText(SavePath)); }
            catch { return false; }
            if (save == null) return false;

            var gm = Core.GameManager.Instance;
            if (gm != null) gm.SetDayLevelXP(Mathf.Max(1, save.Day), Mathf.Max(1, save.Level), save.XP);
            var eco = Economy.EconomyManager.Instance;
            if (eco != null)
            {
                eco.SetMoney(save.Money);
                if (save.WeekProfits != null && save.WeekProfits.Length == 7)
                    save.WeekProfits.CopyTo(eco.WeekProfits, 0);
                eco.SetLifetimeStats(save.LifetimeRevenue, save.LifetimeCustomersServed);
            }
            var rep = Economy.ReputationManager.Instance;
            if (rep != null) rep.SetReputation(save.Reputation);
            var cycle = Core.DayNightCycle.Instance;
            if (cycle != null && save.TimeOfDay > 0f) cycle.SetTime(save.TimeOfDay);

            var inv = Inventory.InventoryManager.Instance;
            if (inv != null)
            {
                var stock = new Dictionary<string, int>();
                foreach (var e in save.Inventory) stock[e.ProductId] = e.Count;
                inv.LoadStock(stock);
            }

            var up = Upgrades.UpgradeManager.Instance;
            if (up != null) up.LoadPurchased(save.PurchasedUpgrades);

            if (Core.SceneBootstrapper.Instance != null)
            {
                foreach (var existing in FindObjectsOfType<PlacedObjectMarker>())
                    if (existing != null)
                    {
                        existing.gameObject.SetActive(false);
                        Destroy(existing.gameObject);
                    }
                if (save.PlacedObjects != null)
                {
                    foreach (var entry in save.PlacedObjects)
                    {
                        if (entry == null || string.IsNullOrEmpty(entry.ItemId)) continue;
                        Core.SceneBootstrapper.Instance.CreatePlaceableObject(
                            entry.ItemId,
                            entry.Position,
                            Quaternion.Euler(entry.EulerAngles),
                            true);
                    }
                }
            }

            foreach (var e in save.ShelfStock)
            {
                var shelf = ResolveShelf(e);
                if (shelf == null) continue;
                if (e.SlotIndex < 0 || e.SlotIndex >= shelf.Slots.Count) continue;
                var product = Core.DefaultGameData.GetProduct(e.ProductId);
                if (product != null) shelf.Slots[e.SlotIndex].SetState(product, e.Count);
            }

            var pkg = Packages.PackageSystem.Instance;
            if (pkg != null) pkg.LoadPackages(save.StoredPackages);
            var delivery = Delivery.DeliverySystem.Instance;
            if (delivery != null) delivery.LoadPendingDeliveries(save.PendingDeliveries);
            foreach (var existingBox in FindObjectsOfType<Delivery.DeliveryBox>())
                if (existingBox != null) Destroy(existingBox.gameObject);
            int restoredBoxCount = 0;
            if (save.SpawnedDeliveryBoxes != null)
                foreach (var entry in save.SpawnedDeliveryBoxes)
                {
                    var go = Core.ProceduralAssetGenerator.CreateDeliveryBoxModel();
                    go.transform.position = entry.Position;
                    var box = go.AddComponent<Delivery.DeliveryBox>();
                    box.SetContents(entry.Contents);
                    restoredBoxCount++;
                }
            if (delivery != null) delivery.RestoreSpawnedBoxCount(restoredBoxCount);

            if (save.Prices != null)
                foreach (var e in save.Prices)
                {
                    var p = Core.DefaultGameData.GetProduct(e.ProductId);
                    if (p != null && e.SellPrice > 0f) p.SellPrice = e.SellPrice;
                }

            TutorialCompleted = save.TutorialCompleted;

            if (UI.UIManager.Instance != null) UI.UIManager.Instance.ShowToast("Spielstand geladen.");
            return true;
        }

        public void MarkTutorialCompleted()
        {
            TutorialCompleted = true;
        }

        Shelf ResolveShelf(ShelfSlotEntry entry)
        {
            if (entry == null) return null;

            Shelf bestMatch = null;
            float bestDistance = float.MaxValue;
            foreach (var shelf in Shelf.AllShelves)
            {
                if (shelf == null) continue;
                float distance = Vector3.Distance(shelf.transform.position, entry.ShelfPosition);
                if (!string.IsNullOrEmpty(entry.ShelfName) && shelf.name == entry.ShelfName && distance < ShelfExactMatchDistance)
                    return shelf;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestMatch = shelf;
                }
            }

            if (bestMatch != null && bestDistance < ShelfFallbackMatchDistance) return bestMatch;
            if (entry.ShelfIndex >= 0 && entry.ShelfIndex < Shelf.AllShelves.Count) return Shelf.AllShelves[entry.ShelfIndex];
            return null;
        }
    }
}
