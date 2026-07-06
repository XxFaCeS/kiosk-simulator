using UnityEngine;
using UnityEngine.UI;
using Kiosk.Core;
using Kiosk.Customers;
using Kiosk.Upgrades;

namespace Kiosk.UI
{
    /// <summary>
    /// Kassenfenster: Scannen, Bezahlen, Alterspruefung, Paketannahme/-ausgabe
    /// und Lotto-Verkauf - je nach Kundenwunsch.
    /// </summary>
    public class CheckoutUI : MonoBehaviour
    {
        public bool IsOpen { get; private set; }

        GameObject _root;
        Text _title;
        Text _itemList;
        Text _info;
        GameObject _buttonRow;
        GameObject _agePanel;
        Text _ageText;
        CustomerAI _customer;
        float _scanCooldown;

        public void Build()
        {
            _root = ProceduralAssetGenerator.CreatePanel(transform, "Fenster",
                new Vector2(0.25f, 0.15f), new Vector2(0.75f, 0.85f), new Color(0.08f, 0.1f, 0.14f, 0.96f));

            var titleGo = ProceduralAssetGenerator.CreatePanel(_root.transform, "Titel",
                new Vector2(0f, 0.9f), new Vector2(1f, 1f), new Color(0.15f, 0.2f, 0.3f, 1f));
            _title = ProceduralAssetGenerator.CreateText(titleGo.transform, "Text", "Kasse", 24, TextAnchor.MiddleCenter);

            var listGo = ProceduralAssetGenerator.CreatePanel(_root.transform, "Artikel",
                new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.88f), new Color(0f, 0f, 0f, 0.35f));
            _itemList = ProceduralAssetGenerator.CreateText(listGo.transform, "Text", "", 18, TextAnchor.UpperLeft);

            var infoGo = ProceduralAssetGenerator.CreatePanel(_root.transform, "Info",
                new Vector2(0.05f, 0.24f), new Vector2(0.95f, 0.34f), Color.clear);
            _info = ProceduralAssetGenerator.CreateText(infoGo.transform, "Text", "", 20, TextAnchor.MiddleLeft);
            _info.color = new Color(0.95f, 0.85f, 0.4f);

            _buttonRow = ProceduralAssetGenerator.CreatePanel(_root.transform, "Buttons",
                new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.22f), Color.clear);
            var layout = _buttonRow.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            // Alterspruefungs-Panel (Ausweis)
            _agePanel = ProceduralAssetGenerator.CreatePanel(_root.transform, "Alterspruefung",
                new Vector2(0.15f, 0.3f), new Vector2(0.85f, 0.75f), new Color(0.5f, 0.15f, 0.15f, 0.97f));
            _ageText = ProceduralAssetGenerator.CreateText(_agePanel.transform, "Text", "", 22, TextAnchor.UpperCenter);
            var ageButtons = ProceduralAssetGenerator.CreatePanel(_agePanel.transform, "Buttons",
                new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.35f), Color.clear);
            var ageLayout = ageButtons.AddComponent<HorizontalLayoutGroup>();
            ageLayout.spacing = 10f;
            ageLayout.childForceExpandHeight = true;
            ageLayout.childForceExpandWidth = true;
            ProceduralAssetGenerator.CreateButton(ageButtons.transform, "Verkaufen", "Verkaufen", OnAgeSell);
            ProceduralAssetGenerator.CreateButton(ageButtons.transform, "Ablehnen", "Verkauf ablehnen", OnAgeRefuse);

            _agePanel.SetActive(false);
            _root.SetActive(false);
        }

        void Update()
        {
            if (_scanCooldown > 0f) _scanCooldown -= Time.deltaTime;
        }

        public void Open(CustomerAI customer)
        {
            _customer = customer;
            IsOpen = true;
            _root.SetActive(true);
            _agePanel.SetActive(false);

            switch (customer.Intent)
            {
                case CustomerIntent.Shopping:
                    _title.text = "Kasse - Einkauf";
                    Checkout.CashRegister.Instance.BeginTransaction(
                        new System.Collections.Generic.List<Products.ProductData>(customer.Basket));
                    BuildShoppingButtons();
                    RefreshShoppingList();
                    break;
                case CustomerIntent.PackageDropoff:
                    _title.text = "Paketannahme";
                    _itemList.text = "Der Kunde moechte ein Paket abgeben.";
                    _info.text = "";
                    BuildDropoffButtons();
                    break;
                case CustomerIntent.PackagePickup:
                    _title.text = "Paketabholung";
                    _itemList.text = "Der Kunde nennt den Code: " + _customer.PackageCode +
                        "\n\nWaehle das richtige Paket aus dem Paketregal:";
                    _info.text = "";
                    BuildPickupButtons();
                    break;
                case CustomerIntent.Lotto:
                    _title.text = "Lotto-Terminal (fiktiv)";
                    _itemList.text = "Der Kunde moechte einen fiktiven Lotto-Schein kaufen.\nWaehle den Scheintyp:";
                    _info.text = "";
                    BuildLottoButtons();
                    break;
            }
        }

        void ClearButtons()
        {
            for (int i = _buttonRow.transform.childCount - 1; i >= 0; i--)
                Destroy(_buttonRow.transform.GetChild(i).gameObject);
        }

        // ---------- Einkauf ----------

        void BuildShoppingButtons()
        {
            ClearButtons();
            ProceduralAssetGenerator.CreateButton(_buttonRow.transform, "Scan", "Artikel scannen", OnScan);
            ProceduralAssetGenerator.CreateButton(_buttonRow.transform, "Bar", "Bar kassieren", delegate { OnPay(Checkout.PaymentMethod.Cash); });
            if (Checkout.PaymentSystem.Instance.IsCardAvailable)
                ProceduralAssetGenerator.CreateButton(_buttonRow.transform, "Karte", "Kartenzahlung", delegate { OnPay(Checkout.PaymentMethod.Card); });
            ProceduralAssetGenerator.CreateButton(_buttonRow.transform, "Abbrechen", "Abbrechen", Cancel);
        }

        void RefreshShoppingList()
        {
            var register = Checkout.CashRegister.Instance;
            var upgrades = UpgradeManager.Instance;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Gescannt:");
            foreach (var p in register.ScannedItems)
            {
                float price = upgrades != null ? upgrades.GetAdjustedSellPrice(p) : p.SellPrice;
                sb.AppendLine("  " + p.DisplayName + "  -  " + price.ToString("F2") + " Euro" + (p.AgeRestricted ? "  [18+]" : ""));
            }
            if (register.RemainingItems.Count > 0)
            {
                sb.AppendLine("\nNoch zu scannen: " + register.RemainingItems.Count + " Artikel");
            }
            _itemList.text = sb.ToString();
            _info.text = "Summe: " + register.Total.ToString("F2") + " Euro" +
                (register.AllScanned ? "  -  Bereit zum Bezahlen" : "");
        }

        void OnScan()
        {
            if (_scanCooldown > 0f) return;
            var register = Checkout.CashRegister.Instance;
            if (register.ScanNext())
            {
                _scanCooldown = register.ScanDuration;
                RefreshShoppingList();
                _info.text = "Scan erfolgreich - naechster Artikel bereit.";
            }
        }

        void OnPay(Checkout.PaymentMethod method)
        {
            var register = Checkout.CashRegister.Instance;
            if (!register.AllScanned)
            {
                _info.text = "Erst alle Artikel scannen!";
                if (Economy.ReputationManager.Instance != null) Economy.ReputationManager.Instance.Add(-1f);
                return;
            }
            if (register.ScannedItems.Count == 0) { Cancel(); return; }

            if (register.HasAgeRestrictedItem)
            {
                ShowAgeCheck(method);
                return;
            }
            FinishSale(method);
        }

        Checkout.PaymentMethod _pendingMethod;

        void ShowAgeCheck(Checkout.PaymentMethod method)
        {
            _pendingMethod = method;
            _agePanel.SetActive(true);
            _ageText.text = "\nALTERSPRUEFUNG\n\nDer Einkauf enthaelt altersbeschraenkte (fiktive) Ware.\n\n" +
                "AUSWEIS DES KUNDEN\nAlter: " + _customer.Age + " Jahre\n\nMindestalter: 18";
        }

        void OnAgeSell()
        {
            _agePanel.SetActive(false);
            bool legal = AgeRestricted.AgeCheckSystem.Instance.ProcessSaleDecision(_customer);
            if (legal) FinishSale(_pendingMethod);
            else
            {
                Checkout.CashRegister.Instance.Clear();
                CloseWith(false);
            }
        }

        void OnAgeRefuse()
        {
            _agePanel.SetActive(false);
            AgeRestricted.AgeCheckSystem.Instance.ProcessRefusal(_customer);
            Checkout.CashRegister.Instance.Clear();
            CloseWith(false);
        }

        void FinishSale(Checkout.PaymentMethod method)
        {
            if (Checkout.PaymentSystem.Instance.CompleteSale(_customer, method))
                CloseWith(true);
        }

        // ---------- Pakete ----------

        void BuildDropoffButtons()
        {
            ClearButtons();
            ProceduralAssetGenerator.CreateButton(_buttonRow.transform, "Annehmen", "Paket scannen + annehmen", OnAcceptPackage);
            ProceduralAssetGenerator.CreateButton(_buttonRow.transform, "Ablehnen", "Ablehnen", delegate { CloseWith(false); });
        }

        void OnAcceptPackage()
        {
            var pkg = Packages.PackageSystem.Instance.AcceptPackage();
            if (pkg == null)
            {
                _info.text = "Paketregal ist voll!";
                return;
            }
            UIManager.Instance.ShowToast("Paket angenommen: " + pkg.Code + " (" + pkg.CustomerName + ")");
            CloseWith(true);
        }

        void BuildPickupButtons()
        {
            ClearButtons();
            var system = Packages.PackageSystem.Instance;
            int count = 0;
            foreach (var pkg in system.StoredPackages)
            {
                if (count >= 4) break;
                var captured = pkg;
                ProceduralAssetGenerator.CreateButton(_buttonRow.transform, "Paket_" + pkg.Code,
                    pkg.Code + "\n" + pkg.CustomerName, delegate { OnHandOut(captured); });
                count++;
            }
            ProceduralAssetGenerator.CreateButton(_buttonRow.transform, "KeinPaket", "Kein Paket da", delegate { CloseWith(false); });
        }

        void OnHandOut(Packages.PackageItem pkg)
        {
            bool correct = Packages.PackageSystem.Instance.HandOut(pkg, _customer.PackageCode);
            UIManager.Instance.ShowToast(correct
                ? "Richtiges Paket ausgegeben! +Geld +Ruf"
                : "Falsches Paket! Strafe und Rufverlust.");
            CloseWith(correct);
        }

        // ---------- Lotto ----------

        void BuildLottoButtons()
        {
            ClearButtons();
            foreach (var ticket in DefaultGameData.LottoTickets)
            {
                var captured = ticket;
                ProceduralAssetGenerator.CreateButton(_buttonRow.transform, "Los_" + ticket.Id,
                    ticket.DisplayName + "\n" + ticket.Price.ToString("F2") + " Euro",
                    delegate { OnSellTicket(captured); });
            }
            ProceduralAssetGenerator.CreateButton(_buttonRow.transform, "Abbrechen", "Abbrechen", Cancel);
        }

        void OnSellTicket(Lotto.LottoTicketData ticket)
        {
            var terminal = Lotto.LottoTerminal.Instance;
            if (terminal == null || !terminal.IsUnlocked)
            {
                _info.text = "Lotto-Terminal noch nicht freigeschaltet (Upgrade kaufen)!";
                return;
            }
            bool won = terminal.SellTicket(ticket);
            UIManager.Instance.ShowToast(won
                ? "Der Kunde hat einen kleinen Gewinn! +Ruf"
                : "Schein verkauft. Provision erhalten.");
            CloseWith(true);
        }

        // ---------- Abschluss ----------

        void CloseWith(bool happy)
        {
            IsOpen = false;
            _root.SetActive(false);
            var counter = Checkout.CheckoutCounter.Instance;
            if (counter != null) counter.ReleaseCustomer(happy);
            _customer = null;
            UIManager.Instance.CloseCheckout();
        }

        public void Cancel()
        {
            IsOpen = false;
            _root.SetActive(false);
            Checkout.CashRegister.Instance.Clear();
            var counter = Checkout.CheckoutCounter.Instance;
            if (counter != null) counter.CancelService();
            _customer = null;
            UIManager.Instance.CloseCheckout();
        }
    }
}
