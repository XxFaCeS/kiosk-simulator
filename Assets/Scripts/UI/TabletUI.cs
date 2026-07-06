using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Kiosk.Core;
using Kiosk.Products;
using Kiosk.Shelves;

namespace Kiosk.UI
{
    /// <summary>
    /// Tablet-Menue (Tab-Taste): Bestellung, Upgrades, Lager und Pakete.
    /// </summary>
    public class TabletUI : MonoBehaviour
    {
        public bool IsOpen { get; private set; }

        GameObject _root;
        OrderUI _orderUI;
        UpgradeUI _upgradeUI;
        GameObject _storagePage;
        Text _storageText;
        Text _goalText;
        Text _assignmentText;
        GameObject _packagePage;
        Text _packageText;

        int _selectedShelfIndex;
        int _selectedSlotIndex;
        int _selectedProductIndex;

        public void Build()
        {
            _root = ProceduralAssetGenerator.CreatePanel(transform, "Tablet",
                new Vector2(0.12f, 0.08f), new Vector2(0.88f, 0.92f), new Color(0.07f, 0.09f, 0.12f, 0.97f));

            var titleGo = ProceduralAssetGenerator.CreatePanel(_root.transform, "Titel",
                new Vector2(0f, 0.93f), new Vector2(1f, 1f), new Color(0.12f, 0.16f, 0.24f, 1f));
            ProceduralAssetGenerator.CreateText(titleGo.transform, "Text", "KIOSK-TABLET", 24, TextAnchor.MiddleCenter);

            var tabBar = ProceduralAssetGenerator.CreatePanel(_root.transform, "Tabs",
                new Vector2(0f, 0.85f), new Vector2(1f, 0.93f), Color.clear);
            var layout = tabBar.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            ProceduralAssetGenerator.CreateButton(tabBar.transform, "TabBestellung", "Bestellung", delegate { ShowPage(0); });
            ProceduralAssetGenerator.CreateButton(tabBar.transform, "TabUpgrades", "Upgrades", delegate { ShowPage(1); });
            ProceduralAssetGenerator.CreateButton(tabBar.transform, "TabLager", "Lager", delegate { ShowPage(2); });
            ProceduralAssetGenerator.CreateButton(tabBar.transform, "TabPakete", "Pakete", delegate { ShowPage(3); });
            ProceduralAssetGenerator.CreateButton(tabBar.transform, "TabSchliessen", "Schliessen [Tab]", delegate { UIManager.Instance.ToggleTablet(); });

            var content = ProceduralAssetGenerator.CreatePanel(_root.transform, "Inhalt",
                new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.84f), Color.clear);

            _orderUI = new GameObject("OrderUI").AddComponent<OrderUI>();
            _orderUI.transform.SetParent(content.transform, false);
            _orderUI.Build();

            _upgradeUI = new GameObject("UpgradeUI").AddComponent<UpgradeUI>();
            _upgradeUI.transform.SetParent(content.transform, false);
            _upgradeUI.Build();

            BuildStoragePage(content.transform);

            _packagePage = ProceduralAssetGenerator.CreatePanel(content.transform, "PaketSeite",
                Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.3f));
            _packageText = ProceduralAssetGenerator.CreateText(_packagePage.transform, "Text", "", 18, TextAnchor.UpperLeft);

            _root.SetActive(false);
        }

        void BuildStoragePage(Transform parent)
        {
            _storagePage = ProceduralAssetGenerator.CreatePanel(parent, "LagerSeite",
                Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.3f));

            var goalsPanel = ProceduralAssetGenerator.CreatePanel(_storagePage.transform, "Ziele",
                new Vector2(0f, 0.68f), new Vector2(1f, 1f), new Color(0.05f, 0.08f, 0.12f, 0.55f));
            _goalText = ProceduralAssetGenerator.CreateText(goalsPanel.transform, "Text", "", 16, TextAnchor.UpperLeft);

            var storagePanel = ProceduralAssetGenerator.CreatePanel(_storagePage.transform, "Bestand",
                new Vector2(0f, 0.22f), new Vector2(1f, 0.68f), new Color(0f, 0f, 0f, 0.18f));
            _storageText = ProceduralAssetGenerator.CreateText(storagePanel.transform, "Text", "", 17, TextAnchor.UpperLeft);

            var assignmentPanel = ProceduralAssetGenerator.CreatePanel(_storagePage.transform, "Zuweisung",
                new Vector2(0f, 0f), new Vector2(1f, 0.22f), new Color(0.02f, 0.02f, 0.04f, 0.25f));
            _assignmentText = ProceduralAssetGenerator.CreateText(assignmentPanel.transform, "Status", "", 16, TextAnchor.UpperLeft);

            var buttonBar = ProceduralAssetGenerator.CreatePanel(assignmentPanel.transform, "Buttons",
                new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.38f), Color.clear);
            var layout = buttonBar.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            ProceduralAssetGenerator.CreateButton(buttonBar.transform, "RegalZurueck", "Regal <", delegate { CycleShelf(-1); });
            ProceduralAssetGenerator.CreateButton(buttonBar.transform, "RegalWeiter", "Regal >", delegate { CycleShelf(1); });
            ProceduralAssetGenerator.CreateButton(buttonBar.transform, "SlotZurueck", "Slot <", delegate { CycleSlot(-1); });
            ProceduralAssetGenerator.CreateButton(buttonBar.transform, "SlotWeiter", "Slot >", delegate { CycleSlot(1); });
            ProceduralAssetGenerator.CreateButton(buttonBar.transform, "ProduktZurueck", "Produkt <", delegate { CycleProduct(-1); });
            ProceduralAssetGenerator.CreateButton(buttonBar.transform, "ProduktWeiter", "Produkt >", delegate { CycleProduct(1); });
            ProceduralAssetGenerator.CreateButton(buttonBar.transform, "Zuweisen", "Zuweisen + auffuellen", AssignSelection);
        }

        public void Open()
        {
            IsOpen = true;
            _root.SetActive(true);
            ShowPage(0);
        }

        public void Close()
        {
            IsOpen = false;
            _root.SetActive(false);
        }

        void ShowPage(int page)
        {
            _orderUI.gameObject.SetActive(page == 0);
            _upgradeUI.gameObject.SetActive(page == 1);
            _storagePage.SetActive(page == 2);
            _packagePage.SetActive(page == 3);
            if (page == 0) _orderUI.Refresh();
            if (page == 1) _upgradeUI.Refresh();
            if (page == 2) RefreshStorage();
            if (page == 3) RefreshPackages();
        }

        void RefreshStorage()
        {
            var inv = Inventory.InventoryManager.Instance;
            var storage = Inventory.StorageManager.Instance;
            var sb = new System.Text.StringBuilder();
            float fillPercent = storage != null && storage.Capacity > 0 ? storage.UsedUnits * 100f / storage.Capacity : 0f;
            sb.AppendLine("LAGERBESTAND  (" + (storage != null ? storage.UsedUnits + "/" + storage.Capacity : "?") + " | " + fillPercent.ToString("F0") + "% belegt)\n");
            if (inv != null)
            {
                var stock = inv.GetAllStock();
                if (stock.Count == 0) sb.AppendLine("Das Lager ist leer. Bestelle Ware im Tab 'Bestellung'.");
                foreach (var kv in stock)
                {
                    var p = DefaultGameData.GetProduct(kv.Key);
                    sb.AppendLine("  " + (p != null ? p.DisplayName : kv.Key) + "  x" + kv.Value);
                }
            }
            sb.AppendLine("\nRegale im Laden: Mit [E] am Regal auffuellen oder unten gezielt Slots zuweisen.");
            _storageText.text = sb.ToString();

            RefreshGoals();
            RefreshAssignment();
        }

        void RefreshGoals()
        {
            var eco = Economy.EconomyManager.Instance;
            var gm = GameManager.Instance;
            if (_goalText == null || eco == null) return;

            _goalText.text =
                "TAGESZIELE\n" +
                "- Umsatz: " + eco.DayRevenue.ToString("F2") + " / " + eco.DailyRevenueGoal.ToString("F2") + " Euro\n" +
                "- Kunden: " + eco.DayCustomersServed + " / " + eco.DailyCustomerGoal + "\n" +
                "- Bonus bei Erfüllung: +" + eco.DailyGoalBonus.ToString("F2") + " Euro\n\n" +
                "MEILENSTEINE\n" +
                "- Gesamtumsatz: " + eco.LifetimeRevenue.ToString("F2") + " / 500.00 Euro\n" +
                "- Bediente Kunden: " + eco.LifetimeCustomersServed + " / 25\n" +
                "- Levelziel: " + (gm != null ? gm.Level : 1) + " / 5";
        }

        void RefreshPackages()
        {
            var pkg = Packages.PackageSystem.Instance;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("PAKETREGAL  (" + (pkg != null ? pkg.StoredCount + "/" + pkg.Capacity : "?") + ")\n");
            if (pkg != null)
            {
                if (pkg.StoredCount == 0) sb.AppendLine("Keine Pakete eingelagert.");
                foreach (var item in pkg.StoredPackages)
                    sb.AppendLine("  " + item.Code + "  -  " + item.CustomerName);
            }
            var unlock = UnlockManager.Instance;
            if (unlock != null && !unlock.IsServiceUnlocked(ServiceType.Paketannahme))
                sb.AppendLine("\nPaketannahme wird ab Level 3 freigeschaltet.");
            _packageText.text = sb.ToString();
        }

        void CycleShelf(int delta)
        {
            var shelves = ValidShelves();
            if (shelves.Count == 0) return;
            _selectedShelfIndex = Wrap(_selectedShelfIndex + delta, shelves.Count);
            _selectedSlotIndex = Mathf.Clamp(_selectedSlotIndex, 0, shelves[_selectedShelfIndex].Slots.Count - 1);
            _selectedProductIndex = 0;
            RefreshAssignment();
        }

        void CycleSlot(int delta)
        {
            var shelf = SelectedShelf();
            if (shelf == null || shelf.Slots.Count == 0) return;
            _selectedSlotIndex = Wrap(_selectedSlotIndex + delta, shelf.Slots.Count);
            RefreshAssignment();
        }

        void CycleProduct(int delta)
        {
            var products = AssignableProducts();
            if (products.Count == 0) return;
            _selectedProductIndex = Wrap(_selectedProductIndex + delta, products.Count);
            RefreshAssignment();
        }

        void AssignSelection()
        {
            var shelf = SelectedShelf();
            var product = SelectedProduct();
            if (shelf == null || product == null)
            {
                UIManager.Instance.ShowToast("Keine passende Lagerware fuer die Zuweisung vorhanden.");
                return;
            }

            if (!shelf.AssignProductToSlot(_selectedSlotIndex, product))
            {
                UIManager.Instance.ShowToast("Slot ist noch mit anderer Ware belegt.");
                return;
            }

            int moved = shelf.RestockSlot(_selectedSlotIndex);
            UIManager.Instance.ShowToast(moved > 0
                ? product.DisplayName + " gezielt in Slot " + (_selectedSlotIndex + 1) + " eingeräumt."
                : product.DisplayName + " wurde zugewiesen. Kein Lagerbestand zum Auffüllen.");
            RefreshStorage();
        }

        void RefreshAssignment()
        {
            if (_assignmentText == null) return;
            var shelf = SelectedShelf();
            var product = SelectedProduct();
            if (shelf == null)
            {
                _assignmentText.text = "Keine Regale vorhanden.";
                return;
            }

            if (shelf.Slots.Count == 0) _selectedSlotIndex = 0;
            else _selectedSlotIndex = Mathf.Clamp(_selectedSlotIndex, 0, shelf.Slots.Count - 1);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SLOT-ZUWEISUNG");
            sb.AppendLine("Regal: " + shelf.name + "  |  Fach: " + (_selectedSlotIndex + 1) + "/" + shelf.Slots.Count);
            sb.AppendLine("Ausgewähltes Produkt: " + (product != null ? product.DisplayName : "keins"));
            sb.AppendLine();
            for (int i = 0; i < shelf.Slots.Count; i++)
            {
                var slot = shelf.Slots[i];
                sb.AppendLine((i == _selectedSlotIndex ? "> " : "  ") + "Slot " + (i + 1) + ": " + (slot != null ? slot.Summary : "-"));
            }
            _assignmentText.text = sb.ToString();
        }

        Shelf SelectedShelf()
        {
            var shelves = ValidShelves();
            if (shelves.Count == 0) return null;
            _selectedShelfIndex = Mathf.Clamp(_selectedShelfIndex, 0, shelves.Count - 1);
            return shelves[_selectedShelfIndex];
        }

        ProductData SelectedProduct()
        {
            var products = AssignableProducts();
            if (products.Count == 0) return null;
            _selectedProductIndex = Mathf.Clamp(_selectedProductIndex, 0, products.Count - 1);
            return products[_selectedProductIndex];
        }

        List<Shelf> ValidShelves()
        {
            var shelves = new List<Shelf>();
            foreach (var shelf in Shelf.AllShelves)
                if (shelf != null)
                    shelves.Add(shelf);
            return shelves;
        }

        List<ProductData> AssignableProducts()
        {
            var shelf = SelectedShelf();
            var result = new List<ProductData>();
            var inv = Inventory.InventoryManager.Instance;
            if (shelf == null || inv == null) return result;

            foreach (var kv in inv.GetAllStock())
            {
                if (kv.Value <= 0) continue;
                var product = DefaultGameData.GetProduct(kv.Key);
                if (product != null && shelf.CanStore(product))
                    result.Add(product);
            }
            result.Sort((a, b) => a.DisplayName.CompareTo(b.DisplayName));
            return result;
        }

        int Wrap(int value, int count)
        {
            if (count <= 0) return 0;
            value %= count;
            if (value < 0) value += count;
            return value;
        }
    }
}
