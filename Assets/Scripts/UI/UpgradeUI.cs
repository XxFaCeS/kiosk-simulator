using UnityEngine;
using UnityEngine.UI;
using Kiosk.Core;
using Kiosk.Upgrades;

namespace Kiosk.UI
{
    /// <summary>
    /// Upgrade-Menue im Tablet: 25 Shop-Upgrades kaufen (seitenweise).
    /// </summary>
    public class UpgradeUI : MonoBehaviour
    {
        const int PerPage = 6;

        int _page;
        GameObject _rowContainer;
        Text _footer;

        public void Build()
        {
            var rt = gameObject.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _rowContainer = ProceduralAssetGenerator.CreatePanel(transform, "Zeilen",
                new Vector2(0f, 0.12f), new Vector2(1f, 1f), new Color(0f, 0f, 0f, 0.3f));
            var layout = _rowContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            var footerGo = ProceduralAssetGenerator.CreatePanel(transform, "Fuss",
                new Vector2(0f, 0.06f), new Vector2(1f, 0.12f), Color.clear);
            _footer = ProceduralAssetGenerator.CreateText(footerGo.transform, "Text", "", 16, TextAnchor.MiddleLeft);

            var buttonBar = ProceduralAssetGenerator.CreatePanel(transform, "Aktionen",
                new Vector2(0f, 0f), new Vector2(1f, 0.06f), Color.clear);
            var barLayout = buttonBar.AddComponent<HorizontalLayoutGroup>();
            barLayout.spacing = 8f;
            barLayout.childForceExpandWidth = true;
            barLayout.childForceExpandHeight = true;
            ProceduralAssetGenerator.CreateButton(buttonBar.transform, "Zurueck", "< Seite", delegate { ChangePage(-1); });
            ProceduralAssetGenerator.CreateButton(buttonBar.transform, "Weiter", "Seite >", delegate { ChangePage(1); });
        }

        void ChangePage(int delta)
        {
            int max = Mathf.Max(0, (DefaultGameData.Upgrades.Count - 1) / PerPage);
            _page = Mathf.Clamp(_page + delta, 0, max);
            Refresh();
        }

        public void Refresh()
        {
            for (int i = _rowContainer.transform.childCount - 1; i >= 0; i--)
                Destroy(_rowContainer.transform.GetChild(i).gameObject);

            var upgrades = DefaultGameData.Upgrades;
            var manager = UpgradeManager.Instance;
            int start = _page * PerPage;
            for (int i = start; i < Mathf.Min(start + PerPage, upgrades.Count); i++)
            {
                var u = upgrades[i];
                var row = ProceduralAssetGenerator.CreatePanel(_rowContainer.transform, "Zeile_" + u.Id,
                    Vector2.zero, Vector2.one, new Color(1f, 1f, 1f, 0.06f));
                var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
                rowLayout.spacing = 6f;
                rowLayout.childForceExpandWidth = true;
                rowLayout.childForceExpandHeight = true;

                string label = u.DisplayName + "  (" + u.Cost.ToString("F0") + " Euro, Level " + u.UnlockLevel + ")\n" + u.Description;
                var text = ProceduralAssetGenerator.CreateText(row.transform, "Name", label, 14, TextAnchor.MiddleLeft);
                var textLe = text.gameObject.AddComponent<LayoutElement>();
                textLe.flexibleWidth = 4f;

                if (manager != null && manager.IsPurchased(u.Id))
                {
                    var owned = ProceduralAssetGenerator.CreateText(row.transform, "Gekauft", "GEKAUFT", 15, TextAnchor.MiddleCenter);
                    owned.color = new Color(0.4f, 0.9f, 0.4f);
                    var ownedLe = owned.gameObject.AddComponent<LayoutElement>();
                    ownedLe.flexibleWidth = 1f;
                }
                else
                {
                    string reason;
                    bool can = manager != null && manager.CanPurchase(u, out reason);
                    var captured = u;
                    var button = ProceduralAssetGenerator.CreateButton(row.transform, "Kaufen",
                        can ? "Kaufen" : Reason(manager, u), delegate { Buy(captured); });
                    button.interactable = can;
                }
            }
            _footer.text = "Seite " + (_page + 1) + " / " + (Mathf.Max(0, (upgrades.Count - 1) / PerPage) + 1);
        }

        string Reason(UpgradeManager manager, UpgradeData u)
        {
            string reason = null;
            if (manager != null) manager.CanPurchase(u, out reason);
            return reason ?? "Gesperrt";
        }

        void Buy(UpgradeData upgrade)
        {
            if (UpgradeManager.Instance != null && UpgradeManager.Instance.Purchase(upgrade))
                UIManager.Instance.ShowToast("Upgrade gekauft: " + upgrade.DisplayName);
            Refresh();
        }
    }
}
