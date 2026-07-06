using System.Collections.Generic;
using UnityEngine;
using Kiosk.Products;
using Kiosk.Upgrades;
using Kiosk.Lotto;
using Kiosk.Orders;

namespace Kiosk.Core
{
    /// <summary>
    /// Statische Startdatenbank: 46 fiktive Produkte, 25 Upgrades, Lieferanten,
    /// Lotto-Scheintypen. Kann per KioskEditorSetup zusaetzlich als .asset-Dateien
    /// exportiert werden; zur Laufzeit wird sie immer hier aufgebaut, damit das
    /// Spiel ohne Editor-Schritt spielbar ist.
    /// Keine echten Marken - alle Namen sind fiktiv.
    /// </summary>
    public static class DefaultGameData
    {
        static List<ProductData> _products;
        static List<UpgradeData> _upgrades;
        static List<SupplierData> _suppliers;
        static List<LottoTicketData> _lottoTickets;

        public static List<ProductData> Products
        {
            get { if (_products == null) BuildProducts(); return _products; }
        }

        public static List<UpgradeData> Upgrades
        {
            get { if (_upgrades == null) BuildUpgrades(); return _upgrades; }
        }

        public static List<SupplierData> Suppliers
        {
            get { if (_suppliers == null) BuildSuppliers(); return _suppliers; }
        }

        public static List<LottoTicketData> LottoTickets
        {
            get { if (_lottoTickets == null) BuildLottoTickets(); return _lottoTickets; }
        }

        public static ProductData GetProduct(string id)
        {
            foreach (var p in Products) if (p.Id == id) return p;
            return null;
        }

        public static UpgradeData GetUpgrade(string id)
        {
            foreach (var u in Upgrades) if (u.Id == id) return u;
            return null;
        }

        static void BuildProducts()
        {
            _products = new List<ProductData>();
            var P = _products;
            const string MG = "Material_Produkt_Getraenk";
            const string MS = "Material_Produkt_Snack";
            const string MZ = "Material_Produkt_Zeitung";
            const string MT = "Material_Produkt_Tabak_Fiktiv";

            // Wasser
            P.Add(ProductData.Create("wasser_still", "Bergquell Still", ProductCategory.Wasser, 0.30f, 0.90f, 0.85f, 1, false, false, ProductModelType.Bottle, MG, new Color(0.6f, 0.8f, 1f)));
            P.Add(ProductData.Create("wasser_sprudel", "Bergquell Sprudel", ProductCategory.Wasser, 0.30f, 0.95f, 0.80f, 1, false, false, ProductModelType.Bottle, MG, new Color(0.5f, 0.75f, 0.95f)));
            // Softdrinks
            P.Add(ProductData.Create("cola_fizz", "Fizz Cola", ProductCategory.Softdrinks, 0.55f, 1.60f, 0.90f, 1, false, false, ProductModelType.Can, MG, new Color(0.45f, 0.2f, 0.1f)));
            P.Add(ProductData.Create("limo_zitro", "Zitro Limo", ProductCategory.Softdrinks, 0.50f, 1.50f, 0.75f, 1, false, false, ProductModelType.Can, MG, new Color(0.9f, 0.9f, 0.3f)));
            P.Add(ProductData.Create("limo_orange", "Orangina Sun", ProductCategory.Softdrinks, 0.50f, 1.50f, 0.70f, 1, false, false, ProductModelType.Bottle, MG, new Color(1f, 0.6f, 0.15f)));
            P.Add(ProductData.Create("eistee_pfirsich", "Sommertee Pfirsich", ProductCategory.Softdrinks, 0.60f, 1.70f, 0.65f, 1, false, false, ProductModelType.Bottle, MG, new Color(0.95f, 0.7f, 0.45f)));
            // Energydrinks
            P.Add(ProductData.Create("energy_blitz", "Blitz Energy", ProductCategory.Energydrinks, 0.80f, 2.20f, 0.70f, 1, false, false, ProductModelType.Can, MG, new Color(0.2f, 0.9f, 0.9f)));
            P.Add(ProductData.Create("energy_turbo", "Turbo Volt", ProductCategory.Energydrinks, 0.85f, 2.40f, 0.60f, 2, false, false, ProductModelType.Can, MG, new Color(0.9f, 0.2f, 0.9f)));
            // Kaffee
            P.Add(ProductData.Create("kaffee_becher", "Kiosk-Kaffee To Go", ProductCategory.Kaffee, 0.35f, 1.80f, 0.75f, 4, false, false, ProductModelType.Can, MG, new Color(0.5f, 0.35f, 0.2f)));
            P.Add(ProductData.Create("kaffee_eis", "Eiskaffee Dream", ProductCategory.Kaffee, 0.70f, 2.20f, 0.55f, 4, false, false, ProductModelType.Bottle, MG, new Color(0.7f, 0.55f, 0.4f)));
            // Chips
            P.Add(ProductData.Create("chips_paprika", "Knusper Chips Paprika", ProductCategory.Chips, 0.90f, 2.30f, 0.85f, 1, false, false, ProductModelType.Pack, MS, new Color(0.9f, 0.35f, 0.15f)));
            P.Add(ProductData.Create("chips_salz", "Knusper Chips Salz", ProductCategory.Chips, 0.90f, 2.30f, 0.75f, 1, false, false, ProductModelType.Pack, MS, new Color(0.95f, 0.85f, 0.4f)));
            P.Add(ProductData.Create("nachos_kaese", "Nacho Fiesta Kaese", ProductCategory.Chips, 1.00f, 2.60f, 0.55f, 2, false, false, ProductModelType.Pack, MS, new Color(0.95f, 0.7f, 0.2f)));
            // Schokolade
            P.Add(ProductData.Create("schoko_voll", "Alpengold Vollmilch", ProductCategory.Schokolade, 0.70f, 1.90f, 0.85f, 1, false, false, ProductModelType.Flat, MS, new Color(0.45f, 0.28f, 0.15f)));
            P.Add(ProductData.Create("schoko_nuss", "Alpengold Nuss", ProductCategory.Schokolade, 0.80f, 2.10f, 0.70f, 1, false, false, ProductModelType.Flat, MS, new Color(0.5f, 0.32f, 0.18f)));
            P.Add(ProductData.Create("schokoriegel", "Krafties Riegel", ProductCategory.Schokolade, 0.40f, 1.20f, 0.90f, 1, false, false, ProductModelType.Flat, MS, new Color(0.6f, 0.3f, 0.1f)));
            // Bonbons
            P.Add(ProductData.Create("bonbon_frucht", "Fruchtzwerge Bonbons", ProductCategory.Bonbons, 0.50f, 1.40f, 0.60f, 1, false, false, ProductModelType.Pack, MS, new Color(0.95f, 0.4f, 0.5f)));
            P.Add(ProductData.Create("gummibaeren", "Gummi-Baerchis", ProductCategory.Bonbons, 0.60f, 1.60f, 0.80f, 1, false, false, ProductModelType.Pack, MS, new Color(0.3f, 0.85f, 0.3f)));
            // Kaugummi
            P.Add(ProductData.Create("kaugummi_mint", "FrischFix Mint", ProductCategory.Kaugummi, 0.35f, 1.10f, 0.65f, 1, false, false, ProductModelType.Flat, MS, new Color(0.4f, 0.9f, 0.7f)));
            P.Add(ProductData.Create("kaugummi_frucht", "FrischFix Frucht", ProductCategory.Kaugummi, 0.35f, 1.10f, 0.55f, 1, false, false, ProductModelType.Flat, MS, new Color(0.95f, 0.55f, 0.7f)));
            // Eis
            P.Add(ProductData.Create("eis_vanille", "Polar Vanille", ProductCategory.Eis, 0.60f, 1.80f, 0.60f, 2, false, false, ProductModelType.Pack, MS, new Color(0.98f, 0.95f, 0.8f)));
            P.Add(ProductData.Create("eis_schoko", "Polar Schoko-Crunch", ProductCategory.Eis, 0.70f, 2.00f, 0.55f, 2, false, false, ProductModelType.Pack, MS, new Color(0.5f, 0.3f, 0.2f)));
            // Zeitungen
            P.Add(ProductData.Create("zeitung_stadt", "Stadtkurier", ProductCategory.Zeitungen, 0.80f, 1.90f, 0.70f, 2, false, false, ProductModelType.Flat, MZ, new Color(0.9f, 0.9f, 0.88f)));
            P.Add(ProductData.Create("zeitung_sport", "Sportecho", ProductCategory.Zeitungen, 0.80f, 1.90f, 0.60f, 2, false, false, ProductModelType.Flat, MZ, new Color(0.85f, 0.9f, 0.95f)));
            // Magazine
            P.Add(ProductData.Create("magazin_tech", "TechWelt Magazin", ProductCategory.Magazine, 2.00f, 4.50f, 0.40f, 2, false, false, ProductModelType.Flat, MZ, new Color(0.4f, 0.6f, 0.9f)));
            P.Add(ProductData.Create("magazin_promi", "GlitzerBlick", ProductCategory.Magazine, 1.50f, 3.50f, 0.50f, 2, false, false, ProductModelType.Flat, MZ, new Color(0.95f, 0.5f, 0.8f)));
            // Fiktive Tabakwaren (altersbeschraenkt)
            P.Add(ProductData.Create("tabak_rauchwolke", "Rauchwolke Classic (fiktiv)", ProductCategory.TabakFiktiv, 4.50f, 8.50f, 0.65f, 6, true, false, ProductModelType.Flat, MT, new Color(0.75f, 0.15f, 0.15f)));
            P.Add(ProductData.Create("tabak_nebelgold", "Nebelgold (fiktiv)", ProductCategory.TabakFiktiv, 5.00f, 9.20f, 0.50f, 6, true, false, ProductModelType.Flat, MT, new Color(0.85f, 0.65f, 0.15f)));
            P.Add(ProductData.Create("tabak_drehset", "Wolkendreher Set (fiktiv)", ProductCategory.TabakFiktiv, 3.00f, 6.50f, 0.35f, 6, true, false, ProductModelType.Pack, MT, new Color(0.4f, 0.55f, 0.35f)));
            // Fiktive E-Zigaretten (altersbeschraenkt)
            P.Add(ProductData.Create("ezig_dampfstick", "DampfStick 500 (fiktiv)", ProductCategory.EZigarettenFiktiv, 3.50f, 7.90f, 0.45f, 6, true, false, ProductModelType.Can, MT, new Color(0.3f, 0.3f, 0.85f)));
            P.Add(ProductData.Create("ezig_nebelpen", "NebelPen Mini (fiktiv)", ProductCategory.EZigarettenFiktiv, 3.00f, 6.90f, 0.40f, 6, true, false, ProductModelType.Can, MT, new Color(0.6f, 0.3f, 0.75f)));
            // Feuerzeuge
            P.Add(ProductData.Create("feuerzeug_std", "Flamme Standard", ProductCategory.Feuerzeuge, 0.40f, 1.50f, 0.50f, 1, false, false, ProductModelType.Can, MS, new Color(0.9f, 0.4f, 0.1f)));
            // Fiktive Lotto-Scheine
            P.Add(ProductData.Create("lotto_6erglueck", "6er-Glueck Schein (fiktiv)", ProductCategory.LottoScheinFiktiv, 0.50f, 2.00f, 0.55f, 5, false, true, ProductModelType.Flat, MZ, new Color(0.95f, 0.85f, 0.2f)));
            P.Add(ProductData.Create("lotto_superzahl", "Superzahl Extra (fiktiv)", ProductCategory.LottoScheinFiktiv, 0.80f, 3.00f, 0.40f, 5, false, true, ProductModelType.Flat, MZ, new Color(0.9f, 0.7f, 0.1f)));
            // Fiktive Rubbellose
            P.Add(ProductData.Create("rubbel_glueckstern", "Glueckstern Rubbellos (fiktiv)", ProductCategory.RubbellosFiktiv, 0.40f, 1.50f, 0.60f, 5, false, true, ProductModelType.Flat, MZ, new Color(0.95f, 0.6f, 0.1f)));
            P.Add(ProductData.Create("rubbel_goldmine", "Goldmine Rubbellos (fiktiv)", ProductCategory.RubbellosFiktiv, 0.60f, 2.50f, 0.45f, 5, false, true, ProductModelType.Flat, MZ, new Color(1f, 0.8f, 0.2f)));
            // Prepaid
            P.Add(ProductData.Create("prepaid_10", "TeleFix Prepaid 10", ProductCategory.PrepaidKarten, 8.50f, 10.00f, 0.45f, 4, false, false, ProductModelType.Flat, MZ, new Color(0.2f, 0.7f, 0.9f)));
            P.Add(ProductData.Create("prepaid_20", "TeleFix Prepaid 20", ProductCategory.PrepaidKarten, 17.00f, 20.00f, 0.30f, 4, false, false, ProductModelType.Flat, MZ, new Color(0.15f, 0.55f, 0.8f)));
            // Handy-Zubehoer
            P.Add(ProductData.Create("ladekabel", "PowerLine Ladekabel", ProductCategory.HandyZubehoer, 2.50f, 6.90f, 0.35f, 4, false, false, ProductModelType.Pack, MS, new Color(0.25f, 0.25f, 0.3f)));
            P.Add(ProductData.Create("kopfhoerer", "SoundBuds Basic", ProductCategory.HandyZubehoer, 3.50f, 8.90f, 0.30f, 4, false, false, ProductModelType.Pack, MS, new Color(0.9f, 0.9f, 0.95f)));
            // Batterien
            P.Add(ProductData.Create("batterie_aa", "VoltMax AA 4er", ProductCategory.Batterien, 1.20f, 3.50f, 0.40f, 3, false, false, ProductModelType.Pack, MS, new Color(0.85f, 0.75f, 0.1f)));
            P.Add(ProductData.Create("batterie_aaa", "VoltMax AAA 4er", ProductCategory.Batterien, 1.20f, 3.50f, 0.35f, 3, false, false, ProductModelType.Pack, MS, new Color(0.8f, 0.7f, 0.15f)));
            // Hygieneartikel
            P.Add(ProductData.Create("taschentuch", "Softy Taschentuecher", ProductCategory.Hygieneartikel, 0.50f, 1.50f, 0.50f, 3, false, false, ProductModelType.Box, MS, new Color(0.9f, 0.95f, 1f)));
            P.Add(ProductData.Create("handdesinfekt", "CleanGo Handgel", ProductCategory.Hygieneartikel, 1.00f, 2.90f, 0.35f, 3, false, false, ProductModelType.Bottle, MS, new Color(0.6f, 0.9f, 0.85f)));
            // Paketmarken
            P.Add(ProductData.Create("paketmarke_s", "Paketmarke S", ProductCategory.Paketmarken, 2.50f, 3.90f, 0.35f, 3, false, false, ProductModelType.Flat, MZ, new Color(0.85f, 0.7f, 0.5f)));
            P.Add(ProductData.Create("paketmarke_m", "Paketmarke M", ProductCategory.Paketmarken, 4.00f, 5.90f, 0.25f, 3, false, false, ProductModelType.Flat, MZ, new Color(0.8f, 0.6f, 0.4f)));
            // Geschenkartikel
            P.Add(ProductData.Create("geschenkkarte", "Geschenkkarte Herzlich", ProductCategory.Geschenkartikel, 1.00f, 3.00f, 0.30f, 3, false, false, ProductModelType.Flat, MZ, new Color(0.95f, 0.4f, 0.4f)));
            P.Add(ProductData.Create("mini_plusch", "Mini-Pluesch Glueckskatze", ProductCategory.Geschenkartikel, 2.00f, 5.50f, 0.25f, 3, false, false, ProductModelType.Box, MS, new Color(0.95f, 0.75f, 0.85f)));
            // Saisonartikel
            P.Add(ProductData.Create("saison_sonnencreme", "SunBlock 30 (Saison)", ProductCategory.Saisonartikel, 2.50f, 6.50f, 0.30f, 3, false, false, ProductModelType.Bottle, MS, new Color(1f, 0.85f, 0.3f)));
            P.Add(ProductData.Create("saison_regenschirm", "Regenfix Schirm (Saison)", ProductCategory.Saisonartikel, 3.00f, 7.90f, 0.25f, 3, false, false, ProductModelType.Box, MS, new Color(0.3f, 0.4f, 0.7f)));
        }

        static void BuildUpgrades()
        {
            _upgrades = new List<UpgradeData>();
            var U = _upgrades;
            U.Add(UpgradeData.Create("regal_stufe2", "Regal Stufe 2", "Mehr Kapazitaet pro Regalslot (+4).", 350f, 1, null, UpgradeEffect.ShelfCapacity, 4f));
            U.Add(UpgradeData.Create("regal_stufe3", "Regal Stufe 3", "Noch mehr Kapazitaet pro Regalslot (+4).", 700f, 3, "regal_stufe2", UpgradeEffect.ShelfCapacity, 4f));
            U.Add(UpgradeData.Create("zweites_regal", "Zweites Regal", "Ein zusaetzliches Regal im Laden.", 500f, 2, null, UpgradeEffect.ExtraShelf, 1f));
            U.Add(UpgradeData.Create("kuehlschrank", "Getraenke-Kuehlschrank", "Kunden kaufen mehr Getraenke (+15% Nachfrage).", 600f, 2, null, UpgradeEffect.DrinkDemand, 0.15f));
            U.Add(UpgradeData.Create("premium_kuehlschrank", "Premium-Kuehlschrank", "Weitere +20% Getraenke-Nachfrage.", 1200f, 5, "kuehlschrank", UpgradeEffect.DrinkDemand, 0.20f));
            U.Add(UpgradeData.Create("zweite_kasse", "Zweite Kasse", "Verkuerzt die Warteschlange (Kunden warten geduldiger).", 1500f, 8, null, UpgradeEffect.Patience, 0.30f));
            U.Add(UpgradeData.Create("schnelle_kasse", "Schnellere Kasse", "Scannen geht schneller.", 800f, 3, null, UpgradeEffect.CheckoutSpeed, 0.30f));
            U.Add(UpgradeData.Create("kartenzahlung", "Kartenzahlung", "Kunden koennen mit Karte zahlen (schneller).", 400f, 2, null, UpgradeEffect.CardPayment, 1f));
            U.Add(UpgradeData.Create("self_checkout", "Self-Checkout", "Einige Kunden kassieren sich selbst ab.", 3000f, 10, "kartenzahlung", UpgradeEffect.SelfCheckout, 0.25f));
            U.Add(UpgradeData.Create("lager_gross", "Groesserer Lagerraum", "+50 Lagerkapazitaet.", 900f, 3, null, UpgradeEffect.StorageCapacity, 50f));
            U.Add(UpgradeData.Create("paketregal2", "Paketregal Stufe 2", "+6 Paketplaetze.", 500f, 4, null, UpgradeEffect.PackageCapacity, 6f));
            U.Add(UpgradeData.Create("paketstation", "Paketstation", "Automatische Paketabholung (Bonus je Paket).", 2500f, 10, "paketregal2", UpgradeEffect.PackageBonus, 1.5f));
            U.Add(UpgradeData.Create("lotto_terminal", "Lotto-Terminal", "Schaltet das fiktive Lotto-Terminal frei.", 1000f, 5, null, UpgradeEffect.LottoTerminal, 1f));
            U.Add(UpgradeData.Create("lotto_terminal2", "Lotto-Terminal Stufe 2", "+50% Lotto-Provision.", 2000f, 7, "lotto_terminal", UpgradeEffect.LottoCommission, 0.5f));
            U.Add(UpgradeData.Create("tabak_schrank", "Tabakwaren-Schrank", "Schaltet fiktive Tabakwaren hinter der Kasse frei.", 1200f, 6, null, UpgradeEffect.TobaccoCabinet, 1f));
            U.Add(UpgradeData.Create("sicherheitskamera", "Sicherheitskamera", "Weniger Diebstahl (-50%).", 700f, 4, null, UpgradeEffect.TheftReduction, 0.5f));
            U.Add(UpgradeData.Create("diebstahlschutz", "Diebstahlschutz", "Kein Diebstahl mehr.", 1500f, 7, "sicherheitskamera", UpgradeEffect.TheftReduction, 1f));
            U.Add(UpgradeData.Create("beleuchtung", "Bessere Beleuchtung", "+5 Ruf, Laden wirkt freundlicher.", 300f, 1, null, UpgradeEffect.Reputation, 5f));
            U.Add(UpgradeData.Create("klimaanlage", "Klimaanlage", "Kunden bleiben laenger geduldig (+20%).", 800f, 4, null, UpgradeEffect.Patience, 0.20f));
            U.Add(UpgradeData.Create("kaffeemaschine", "Kaffeemaschine", "Kaffee-Nachfrage +30%.", 900f, 4, null, UpgradeEffect.CoffeeDemand, 0.30f));
            U.Add(UpgradeData.Create("aussenwerbung", "Aussenwerbung", "Mehr Kunden (+20% Spawnrate).", 600f, 2, null, UpgradeEffect.CustomerRate, 0.20f));
            U.Add(UpgradeData.Create("schaufensterdeko", "Schaufensterdeko", "Mehr Kunden (+15% Spawnrate) und +3 Ruf.", 450f, 2, null, UpgradeEffect.CustomerRate, 0.15f));
            U.Add(UpgradeData.Create("mitarbeiter_kasse", "Mitarbeiter-Kasse", "Ein Mitarbeiter kassiert automatisch langsam ab.", 3500f, 8, null, UpgradeEffect.EmployeeCheckout, 1f));
            U.Add(UpgradeData.Create("auto_regal", "Automatische Regalauffuellung", "Regale fuellen sich langsam automatisch aus dem Lager.", 4000f, 9, null, UpgradeEffect.AutoRestock, 1f));
            U.Add(UpgradeData.Create("lieferantenrabatt", "Lieferantenrabatt", "-15% Einkaufspreise.", 1800f, 6, null, UpgradeEffect.SupplierDiscount, 0.15f));
        }

        static void BuildSuppliers()
        {
            _suppliers = new List<SupplierData>();
            _suppliers.Add(SupplierData.Create("grosshandel_nord", "Grosshandel Nord", 0f, 45f));
            _suppliers.Add(SupplierData.Create("express_lieferant", "Express-Lieferdienst", 12f, 20f));
        }

        static void BuildLottoTickets()
        {
            _lottoTickets = new List<LottoTicketData>();
            _lottoTickets.Add(LottoTicketData.Create("los_klein", "Mini-Glueck (fiktiv)", 2f, 0.6f, 0.25f, 5f));
            _lottoTickets.Add(LottoTicketData.Create("los_mittel", "Stern-Chance (fiktiv)", 5f, 1.5f, 0.15f, 20f));
            _lottoTickets.Add(LottoTicketData.Create("los_gross", "Jackpot-Traum (fiktiv)", 10f, 3f, 0.08f, 50f));
        }
    }
}
