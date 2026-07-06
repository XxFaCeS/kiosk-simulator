using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Kiosk.Core;
using Kiosk.Orders;
using Kiosk.Products;

namespace Kiosk.UI
{
    /// <summary>
    /// Bestellmenue im Tablet: freigeschaltete Produkte mit Menge bestellen.
    /// Seitenweise Anzeige (8 Produkte pro Seite).
    /// </summary>
    public class OrderUI : MonoBehaviour
    {
        const int PerPage = 8;

        readonly Dictionary<string, int> _cart = new Dictionary<string, int>();
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
                new Vector2(0f, 0.15f), new Vector2(1f, 1f), new Color(0f, 0f, 0f, 0.3f));
            var layout = _rowContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            var footerGo = ProceduralAssetGenerator.CreatePanel(transform, "Fuss",
                new Vector2(0f, 0.08f), new Vector2(1f, 0.15f), Color.clear);
            _footer = ProceduralAssetGenerator.CreateText(footerGo.transform, "Text", "", 18, TextAnchor.MiddleLeft);

            var buttonBar = ProceduralAssetGenerator.CreatePanel(transform, "Aktionen",
                new Vector2(0f, 0f), new Vector2(1f, 0.08f), Color.clear);
            var barLayout = buttonBar.AddComponent<HorizontalLayoutGroup>();
            barLayout.spacing = 8f;
            barLayout.childForceExpandWidth = true;
            barLayout.childForceExpandHeight = true;
            ProceduralAssetGenerator.CreateButton(buttonBar.transform, "Zurueck", "< Seite", delegate { ChangePage(-1); });
            ProceduralAssetGenerator.CreateButton(buttonBar.transform, "Weiter", "Seite >", delegate { ChangePage(1); });
            ProceduralAssetGenerator.CreateButton(buttonBar.transform, "Leeren", "Warenkorb leeren", delegate { _cart.Clear(); Refresh(); });
            ProceduralAssetGenerator.CreateButton(buttonBar.transform, "Bestellen", "BESTELLEN", PlaceOrder);
        }

        List<ProductData> UnlockedProducts()
        {
            return UnlockManager.Instance != null
                ? UnlockManager.Instance.GetUnlockedProducts()
                : new List<ProductData>(DefaultGameData.Products);
        }

        void ChangePage(int delta)
        {
            int max = Mathf.Max(0, (UnlockedProducts().Count - 1) / PerPage);
            _page = Mathf.Clamp(_page + delta, 0, max);
            Refresh();
        }

        public void Refresh()
        {
            for (int i = _rowContainer.transform.childCount - 1; i >= 0; i--)
                Destroy(_rowContainer.transform.GetChild(i).gameObject);

            var products = UnlockedProducts();
            var order = OrderSystem.Instance;
            int start = _page * PerPage;
            for (int i = start; i < Mathf.Min(start + PerPage, products.Count); i++)
            {
                var p = products[i];
                var row = ProceduralAssetGenerator.CreatePanel(_rowContainer.transform, "Zeile_" + p.Id,
                    Vector2.zero, Vector2.one, new Color(1f, 1f, 1f, 0.06f));
                var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
                rowLayout.spacing = 6f;
                rowLayout.childForceExpandWidth = true;
                rowLayout.childForceExpandHeight = true;

                int inCart;
                _cart.TryGetValue(p.Id, out inCart);
                int inStock = Inventory.InventoryManager.Instance != null
                    ? Inventory.InventoryManager.Instance.GetCount(p.Id) : 0;
                float buy = order != null ? order.GetBuyPrice(p) : p.BuyPrice;

                string label = p.DisplayName + (p.AgeRestricted ? " [18+]" : "") +
                    "\nEK " + buy.ToString("F2") + " / VK " + p.SellPrice.ToString("F2") + "  |  Lager: " + inStock;
                var text = ProceduralAssetGenerator.CreateText(row.transform, "Name", label, 15, TextAnchor.MiddleLeft);
                var textLe = text.gameObject.AddComponent<LayoutElement>();
                textLe.flexibleWidth = 3f;

                var captured = p;
                ProceduralAssetGenerator.CreateButton(row.transform, "Minus", "-", delegate { ChangeAmount(captured, -1); });
                var amountText = ProceduralAssetGenerator.CreateText(row.transform, "Menge", inCart.ToString(), 18, TextAnchor.MiddleCenter);
                var amountLe = amountText.gameObject.AddComponent<LayoutElement>();
                amountLe.flexibleWidth = 0.5f;
                ProceduralAssetGenerator.CreateButton(row.transform, "Plus", "+", delegate { ChangeAmount(captured, 1); });
                ProceduralAssetGenerator.CreateButton(row.transform, "Plus5", "+5", delegate { ChangeAmount(captured, 5); });
            }
            RefreshFooter();
        }

        void ChangeAmount(ProductData product, int delta)
        {
            int current;
            _cart.TryGetValue(product.Id, out current);
            current = Mathf.Max(0, current + delta);
            if (current == 0) _cart.Remove(product.Id);
            else _cart[product.Id] = current;
            Refresh();
        }

        void RefreshFooter()
        {
            var order = OrderSystem.Instance;
            var supplier = DefaultGameData.Suppliers.Count > 0 ? DefaultGameData.Suppliers[0] : null;
            var lines = BuildLines();
            float cost = order != null ? order.CalculateCost(lines, supplier) : 0f;
            int units = 0;
            foreach (var l in lines) units += l.Amount;
            _footer.text = "Warenkorb: " + units + " Artikel  |  Kosten inkl. Lieferung: " + cost.ToString("F2") +
                " Euro  |  Lieferant: " + (supplier != null ? supplier.DisplayName : "-") + "  |  Seite " + (_page + 1);
        }

        List<OrderLine> BuildLines()
        {
            var lines = new List<OrderLine>();
            foreach (var kv in _cart)
                lines.Add(new OrderLine { ProductId = kv.Key, Amount = kv.Value });
            return lines;
        }

        void PlaceOrder()
        {
            var lines = BuildLines();
            if (lines.Count == 0)
            {
                UIManager.Instance.ShowToast("Warenkorb ist leer.");
                return;
            }
            var supplier = DefaultGameData.Suppliers.Count > 0 ? DefaultGameData.Suppliers[0] : null;
            if (OrderSystem.Instance != null && OrderSystem.Instance.PlaceOrder(lines, supplier))
            {
                _cart.Clear();
                Refresh();
            }
        }
    }
}
