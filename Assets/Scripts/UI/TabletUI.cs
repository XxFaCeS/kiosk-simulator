using UnityEngine;
using UnityEngine.UI;
using Kiosk.Core;

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
        GameObject _packagePage;
        Text _packageText;

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

            _storagePage = ProceduralAssetGenerator.CreatePanel(content.transform, "LagerSeite",
                Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.3f));
            _storageText = ProceduralAssetGenerator.CreateText(_storagePage.transform, "Text", "", 18, TextAnchor.UpperLeft);

            _packagePage = ProceduralAssetGenerator.CreatePanel(content.transform, "PaketSeite",
                Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.3f));
            _packageText = ProceduralAssetGenerator.CreateText(_packagePage.transform, "Text", "", 18, TextAnchor.UpperLeft);

            _root.SetActive(false);
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
            sb.AppendLine("LAGERBESTAND  (" + (storage != null ? storage.UsedUnits + "/" + storage.Capacity : "?") + ")\n");
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
            sb.AppendLine("\nRegale im Laden: Mit [E] am Regal auffuellen.");
            _storageText.text = sb.ToString();
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
    }
}
