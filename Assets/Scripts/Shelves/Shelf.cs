using System.Collections.Generic;
using UnityEngine;
using Kiosk.Products;
using Kiosk.Interaction;

namespace Kiosk.Shelves
{
    /// <summary>
    /// Regal mit mehreren Slots. Interaktion fuellt Slots aus dem Lager auf.
    /// Registriert sich in einer statischen Liste fuer die Kunden-KI.
    /// </summary>
    public class Shelf : Interactable
    {
        public static readonly List<Shelf> AllShelves = new List<Shelf>();

        public List<ShelfSlot> Slots = new List<ShelfSlot>();
        public bool IsTobaccoCabinet;
        public bool IsFridge;

        void OnEnable() { if (!AllShelves.Contains(this)) AllShelves.Add(this); }
        void OnDisable() { AllShelves.Remove(this); }

        public static void ResetRegistry() { AllShelves.Clear(); }

        public override string GetPrompt()
        {
            return "[E] Regal auffuellen (" + StockedUnits() + " Artikel)";
        }

        public int StockedUnits()
        {
            int c = 0;
            foreach (var s in Slots) c += s.Count;
            return c;
        }

        public bool HasEmptySlots()
        {
            foreach (var s in Slots) if (s.Count == 0) return true;
            return false;
        }

        /// <summary>Fuellt alle Slots aus dem Lager auf (zugewiesene Produkte zuerst, dann freie Slots).</summary>
        public override void Interact(Player.PlayerInteractor interactor)
        {
            int moved = RestockFromInventory();
            var ui = UI.UIManager.Instance;
            if (ui != null)
                ui.ShowToast(moved > 0 ? moved + " Artikel eingeraeumt." : "Nichts zum Einraeumen im Lager.");
            if (moved > 0 && Audio.AudioManager.Instance != null)
                Audio.AudioManager.Instance.Play(Audio.SoundId.Scanner);
        }

        public int RestockFromInventory()
        {
            var inv = Inventory.InventoryManager.Instance;
            if (inv == null) return 0;
            int moved = 0;

            // 1. Zugewiesene Produkte nachfuellen
            foreach (var slot in Slots)
            {
                if (slot.Product == null) continue;
                int space = slot.Capacity - slot.Count;
                int have = inv.GetCount(slot.Product.Id);
                int take = Mathf.Min(space, have);
                if (take > 0 && inv.Remove(slot.Product.Id, take))
                {
                    slot.AddUnits(take);
                    moved += take;
                }
            }

            // 2. Leere Slots mit verfuegbaren Lagerprodukten belegen
            foreach (var slot in Slots)
            {
                if (slot.Product != null) continue;
                foreach (var kv in inv.GetAllStock())
                {
                    var product = Core.DefaultGameData.GetProduct(kv.Key);
                    if (product == null || kv.Value <= 0) continue;
                    if (!CanStore(product)) continue;
                    int take = Mathf.Min(slot.Capacity, kv.Value);
                    if (inv.Remove(product.Id, take))
                    {
                        slot.Assign(product);
                        slot.AddUnits(take);
                        moved += take;
                    }
                    break;
                }
            }
            return moved;
        }

        public bool CanStore(ProductData product)
        {
            return product != null && IsTobaccoCabinet == product.AgeRestricted;
        }

        public bool AssignProductToSlot(int slotIndex, ProductData product)
        {
            if (product == null || !CanStore(product)) return false;
            if (slotIndex < 0 || slotIndex >= Slots.Count) return false;

            var slot = Slots[slotIndex];
            if (slot == null) return false;
            if (slot.Product != null && slot.Product != product && slot.Count > 0) return false;
            slot.Assign(product);
            slot.RefreshVisuals();
            return true;
        }

        public int RestockSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return 0;
            var inv = Inventory.InventoryManager.Instance;
            var slot = Slots[slotIndex];
            if (inv == null || slot == null || slot.Product == null) return 0;

            int space = slot.Capacity - slot.Count;
            int take = Mathf.Min(space, inv.GetCount(slot.Product.Id));
            if (take <= 0 || !inv.Remove(slot.Product.Id, take)) return 0;
            slot.AddUnits(take);
            return take;
        }

        /// <summary>Sucht einen Slot mit dem gewuenschten Produkt und Bestand.</summary>
        public ShelfSlot FindSlotWith(ProductData product)
        {
            foreach (var s in Slots)
                if (s.Product == product && s.Count > 0) return s;
            return null;
        }

        public static ShelfSlot FindProductInAnyShelf(ProductData product)
        {
            foreach (var shelf in AllShelves)
            {
                if (shelf == null) continue;
                var slot = shelf.FindSlotWith(product);
                if (slot != null) return slot;
            }
            return null;
        }
    }
}
