using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Kiosk.Core;
using Kiosk.Delivery;
using Kiosk.Placement;
using Kiosk.Products;
using Kiosk.Shelves;

namespace Kiosk.UI
{
    /// <summary>
    /// Tablet-Menue (Tab-Taste): Bestellung, Regale, Zubehoer, Upgrades, Lieferstatus und Lager.
    /// </summary>
    public class TabletUI : MonoBehaviour
    {
        public bool IsOpen { get; private set; }

        GameObject _root;
        RectTransform _rootRect;
        Text _moneyText;
        OrderUI _orderUI;
        UpgradeUI _upgradeUI;
        GameObject _shelfPage;
        GameObject _accessoryPage;
        Transform _shelfRows;
        Transform _accessoryRows;
        Text _shelfIntro;
        Text _accessoryIntro;
        GameObject _deliveryPage;
        Text _deliveryText;
        GameObject _storagePage;
        Text _storageText;
        Text _goalText;
        Text _assignmentText;
        int _currentPage;
        int _selectedShelfIndex;
        int _selectedSlotIndex;
        int _selectedProductIndex;
        float _refreshTimer;
        Coroutine _animationRoutine;

        public void Build()
        {
            _root = ProceduralAssetGenerator.CreatePanel(transform, "Tablet",
                new Vector2(0.12f, 0.08f), new Vector2(0.88f, 0.92f), new Color(0.07f, 0.09f, 0.12f, 0.97f));
            _rootRect = _root.GetComponent<RectTransform>();
            _rootRect.localScale = new Vector3(0.92f, 0.92f, 1f);

            var header = ProceduralAssetGenerator.CreatePanel(_root.transform, "Header",
                new Vector2(0f, 0.92f), new Vector2(1f, 1f), new Color(0.12f, 0.16f, 0.24f, 1f));
            var title = ProceduralAssetGenerator.CreateText(header.transform, "Titel", "KIOSK-TABLET", 24, TextAnchor.MiddleLeft);
            title.rectTransform.offsetMin = new Vector2(22f, 0f);
            title.rectTransform.offsetMax = new Vector2(-260f, 0f);
            _moneyText = ProceduralAssetGenerator.CreateText(header.transform, "Kontostand", "", 20, TextAnchor.MiddleRight);
            _moneyText.color = new Color(0.95f, 0.88f, 0.4f);
            _moneyText.rectTransform.offsetMin = new Vector2(420f, 0f);
            _moneyText.rectTransform.offsetMax = new Vector2(-22f, 0f);

            var tabBar = ProceduralAssetGenerator.CreatePanel(_root.transform, "Tabs",
                new Vector2(0f, 0.84f), new Vector2(1f, 0.92f), Color.clear);
            var tabLayout = tabBar.AddComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = 6f;
            tabLayout.childForceExpandWidth = true;
            tabLayout.childForceExpandHeight = true;
            AddTabButton(tabBar.transform, "TabBestellung", "Produkte", 0);
            AddTabButton(tabBar.transform, "TabRegale", "Regale", 1);
            AddTabButton(tabBar.transform, "TabZubehoer", "Zubehoer", 2);
            AddTabButton(tabBar.transform, "TabUpgrades", "Upgrades", 3);
            AddTabButton(tabBar.transform, "TabLieferungen", "Lieferstatus", 4);
            AddTabButton(tabBar.transform, "TabLager", "Lager", 5);
            ProceduralAssetGenerator.CreateButton(tabBar.transform, "TabSchliessen", "Schliessen [Tab]", delegate { UIManager.Instance.ToggleTablet(); });

            var content = ProceduralAssetGenerator.CreatePanel(_root.transform, "Inhalt",
                new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.83f), Color.clear);

            _orderUI = new GameObject("OrderUI").AddComponent<OrderUI>();
            _orderUI.transform.SetParent(content.transform, false);
            _orderUI.Build();

            _shelfPage = BuildShopPage(content.transform, "RegalSeite", "Regale kaufen", PlacementCategory.Shelf, out _shelfIntro, out _shelfRows);
            _accessoryPage = BuildShopPage(content.transform, "ZubehoerSeite", "Zubehoer kaufen", PlacementCategory.Accessory, out _accessoryIntro, out _accessoryRows);
            RefreshShopPage(PlacementCategory.Shelf);
            RefreshShopPage(PlacementCategory.Accessory);

            _upgradeUI = new GameObject("UpgradeUI").AddComponent<UpgradeUI>();
            _upgradeUI.transform.SetParent(content.transform, false);
            _upgradeUI.Build();

            _deliveryPage = ProceduralAssetGenerator.CreatePanel(content.transform, "LieferSeite",
                Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.25f));
            _deliveryText = ProceduralAssetGenerator.CreateText(_deliveryPage.transform, "Text", "", 18, TextAnchor.UpperLeft);
            _deliveryText.rectTransform.offsetMin = new Vector2(14f, 14f);
            _deliveryText.rectTransform.offsetMax = new Vector2(-14f, -14f);

            BuildStoragePage(content.transform);

            _root.SetActive(false);
        }

        void AddTabButton(Transform parent, string name, string label, int page)
        {
            ProceduralAssetGenerator.CreateButton(parent, name, label, delegate { ShowPage(page); });
        }

        GameObject BuildShopPage(Transform parent, string name, string title, PlacementCategory category, out Text intro, out Transform rows)
        {
            var page = ProceduralAssetGenerator.CreatePanel(parent, name, Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.26f));
            var introPanel = ProceduralAssetGenerator.CreatePanel(page.transform, "Intro",
                new Vector2(0f, 0.84f), new Vector2(1f, 1f), new Color(0.05f, 0.08f, 0.12f, 0.55f));
            intro = ProceduralAssetGenerator.CreateText(introPanel.transform, "Text", title, 17, TextAnchor.UpperLeft);
            intro.rectTransform.offsetMin = new Vector2(12f, 10f);
            intro.rectTransform.offsetMax = new Vector2(-12f, -10f);

            var rowsPanel = ProceduralAssetGenerator.CreatePanel(page.transform, "Rows",
                new Vector2(0f, 0f), new Vector2(1f, 0.84f), Color.clear);
            var layout = rowsPanel.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            rows = rowsPanel.transform;
            return page;
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
            if (IsOpen) return;
            IsOpen = true;
            _root.SetActive(true);
            ShowPage(0);
            StartAnimation(true);
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            StartAnimation(false);
        }

        void Update()
        {
            if (!IsOpen) return;
            RefreshHeader();
            _refreshTimer -= Time.unscaledDeltaTime;
            if (_refreshTimer <= 0f)
            {
                if (_currentPage == 0 || _currentPage == 3 || _currentPage == 4 || _currentPage == 5)
                    RefreshActivePage();
                _refreshTimer = 0.35f;
            }
        }

        void StartAnimation(bool opening)
        {
            if (_animationRoutine != null) StopCoroutine(_animationRoutine);
            _animationRoutine = StartCoroutine(AnimateTablet(opening));
        }

        IEnumerator AnimateTablet(bool opening)
        {
            float duration = 0.18f;
            float elapsed = 0f;
            Vector3 startScale = opening ? new Vector3(0.92f, 0.92f, 1f) : _rootRect.localScale;
            Vector3 endScale = opening ? Vector3.one : new Vector3(0.92f, 0.92f, 1f);
            Vector2 startPos = opening ? new Vector2(0f, -30f) : _rootRect.anchoredPosition;
            Vector2 endPos = opening ? Vector2.zero : new Vector2(0f, -30f);
            if (opening) _rootRect.anchoredPosition = startPos;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = 1f - Mathf.Pow(1f - t, 3f);
                _rootRect.localScale = Vector3.Lerp(startScale, endScale, t);
                _rootRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            _rootRect.localScale = endScale;
            _rootRect.anchoredPosition = endPos;
            if (!opening) _root.SetActive(false);
            _animationRoutine = null;
        }

        void ShowPage(int page)
        {
            _currentPage = page;
            _orderUI.gameObject.SetActive(page == 0);
            _shelfPage.SetActive(page == 1);
            _accessoryPage.SetActive(page == 2);
            _upgradeUI.gameObject.SetActive(page == 3);
            _deliveryPage.SetActive(page == 4);
            _storagePage.SetActive(page == 5);
            RefreshActivePage();
        }

        void RefreshActivePage()
        {
            switch (_currentPage)
            {
                case 0: _orderUI.Refresh(); break;
                case 1: RefreshShopPage(PlacementCategory.Shelf); break;
                case 2: RefreshShopPage(PlacementCategory.Accessory); break;
                case 3: _upgradeUI.Refresh(); break;
                case 4: RefreshDeliveryStatus(); break;
                case 5: RefreshStorage(); break;
            }
        }

        void RefreshHeader()
        {
            var eco = Economy.EconomyManager.Instance;
            _moneyText.text = eco != null
                ? "Kontostand: " + eco.Money.ToString("F2") + " Euro"
                : "Kontostand: -";
        }

        void RefreshShopPage(PlacementCategory category)
        {
            var rows = category == PlacementCategory.Shelf ? _shelfRows : _accessoryRows;
            var intro = category == PlacementCategory.Shelf ? _shelfIntro : _accessoryIntro;
            if (rows == null || intro == null) return;

            for (int i = rows.childCount - 1; i >= 0; i--)
                Destroy(rows.GetChild(i).gameObject);

            var items = PlacementSystem.GetCatalog(category);
            intro.text = category == PlacementCategory.Shelf
                ? "Regale werden nach dem Kauf als Ghost-Vorschau auf dem Boden platziert. Linksklick bestaetigt, Rechtsklick/Escape storniert mit Rueckerstattung."
                : "Zubehoer ist sichtbarer Ladenausbau. Kaufe ein Objekt und platziere es direkt im Laden.";

            foreach (var item in items)
            {
                var row = ProceduralAssetGenerator.CreatePanel(rows, "Row_" + item.Id,
                    Vector2.zero, Vector2.one, new Color(1f, 1f, 1f, 0.06f));
                var layout = row.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 6f;
                layout.padding = new RectOffset(8, 8, 6, 6);
                layout.childForceExpandHeight = true;
                layout.childForceExpandWidth = true;

                var label = ProceduralAssetGenerator.CreateText(row.transform, "Text",
                    item.DisplayName + "\n" + item.Description + "\nKosten: " + item.Cost.ToString("F2") + " Euro", 15, TextAnchor.MiddleLeft);
                var labelLayout = label.gameObject.AddComponent<LayoutElement>();
                labelLayout.flexibleWidth = 3f;

                var captured = item;
                ProceduralAssetGenerator.CreateButton(row.transform, "Kaufen", "Kaufen + platzieren", delegate { StartPlacement(captured); });
            }
        }

        void StartPlacement(PlacementItemDefinition definition)
        {
            if (PlacementSystem.Instance == null || definition == null) return;
            if (PlacementSystem.Instance.BeginPurchase(definition))
                UIManager.Instance.ToggleTablet();
        }

        void RefreshDeliveryStatus()
        {
            var delivery = DeliverySystem.Instance;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("LIEFERSTATUS\n");
            if (delivery == null || delivery.PendingCount == 0)
            {
                sb.AppendLine("Keine laufenden Warenbestellungen.");
            }
            else
            {
                int index = 1;
                foreach (var pending in delivery.GetPendingSnapshot())
                {
                    int units = 0;
                    if (pending != null && pending.Lines != null)
                        foreach (var line in pending.Lines) units += line.Amount;
                    sb.AppendLine(index + ". Bestellung: " + units + " Artikel  |  Restzeit: " + Mathf.CeilToInt(pending != null ? pending.Timer : 0f) + "s");
                    index++;
                }
            }

            var pkg = Packages.PackageSystem.Instance;
            if (pkg != null)
            {
                sb.AppendLine("\nPAKETE");
                sb.AppendLine("Eingelagerte Pakete: " + pkg.StoredCount + "/" + pkg.Capacity);
            }

            sb.AppendLine("\nLieferzone: Kartons erscheinen am Eingang und koennen mit [E] ausgepackt werden.");
            _deliveryText.text = sb.ToString();
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
                if (stock.Count == 0) sb.AppendLine("Das Lager ist leer. Bestelle Ware im Tab 'Produkte'.");
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
                "- Bonus bei Erfuellung: +" + eco.DailyGoalBonus.ToString("F2") + " Euro\n\n" +
                "MEILENSTEINE\n" +
                "- Gesamtumsatz: " + eco.LifetimeRevenue.ToString("F2") + " / 500.00 Euro\n" +
                "- Bediente Kunden: " + eco.LifetimeCustomersServed + " / 25\n" +
                "- Levelziel: " + (gm != null ? gm.Level : 1) + " / 5";
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
                ? product.DisplayName + " gezielt in Slot " + (_selectedSlotIndex + 1) + " eingeraeumt."
                : product.DisplayName + " wurde zugewiesen. Kein Lagerbestand zum Auffuellen.");
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
            sb.AppendLine("Ausgewaehltes Produkt: " + (product != null ? product.DisplayName : "keins"));
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
