using UnityEngine;
using UnityEngine.UI;
using Kiosk.Core;

namespace Kiosk.UI
{
    /// <summary>
    /// Tagesabschluss: Umsatz, Kosten, Strafen, Gewinn und Wochenstatistik.
    /// </summary>
    public class DaySummaryUI : MonoBehaviour
    {
        public bool IsOpen { get; private set; }

        GameObject _root;
        Text _text;

        public void Build()
        {
            _root = ProceduralAssetGenerator.CreatePanel(transform, "Fenster",
                new Vector2(0.3f, 0.2f), new Vector2(0.7f, 0.8f), new Color(0.08f, 0.12f, 0.1f, 0.97f));

            var titleGo = ProceduralAssetGenerator.CreatePanel(_root.transform, "Titel",
                new Vector2(0f, 0.88f), new Vector2(1f, 1f), new Color(0.12f, 0.25f, 0.16f, 1f));
            ProceduralAssetGenerator.CreateText(titleGo.transform, "Text", "TAGESABSCHLUSS", 26, TextAnchor.MiddleCenter);

            var body = ProceduralAssetGenerator.CreatePanel(_root.transform, "Inhalt",
                new Vector2(0.06f, 0.18f), new Vector2(0.94f, 0.86f), Color.clear);
            _text = ProceduralAssetGenerator.CreateText(body.transform, "Text", "", 20, TextAnchor.UpperLeft);

            var buttonGo = ProceduralAssetGenerator.CreatePanel(_root.transform, "Buttons",
                new Vector2(0.25f, 0.04f), new Vector2(0.75f, 0.14f), Color.clear);
            ProceduralAssetGenerator.CreateButton(buttonGo.transform, "Weiter", "Naechster Tag starten", OnNextDay);

            _root.SetActive(false);
        }

        public void Open(float profit)
        {
            IsOpen = true;
            _root.SetActive(true);
            var eco = Economy.EconomyManager.Instance;
            var gm = GameManager.Instance;
            var sb = new System.Text.StringBuilder();
            if (gm != null) sb.AppendLine("Tag " + gm.Day + " beendet.\n");
            if (eco != null)
            {
                sb.AppendLine("Umsatz:            " + eco.DayRevenue.ToString("F2") + " Euro");
                sb.AppendLine("Ausgaben:          " + eco.DayExpenses.ToString("F2") + " Euro");
                sb.AppendLine("  (inkl. Miete " + eco.RentPerDay.ToString("F0") + " + Strom " + eco.ElectricityPerDay.ToString("F0") + ")");
                sb.AppendLine("Strafen:           " + eco.DayPenalties.ToString("F2") + " Euro");
                sb.AppendLine("Bediente Kunden:   " + eco.DayCustomersServed);
                sb.AppendLine();
                sb.AppendLine("TAGESGEWINN:       " + profit.ToString("F2") + " Euro");
                sb.AppendLine("Wochengewinn (7T): " + eco.WeekProfit.ToString("F2") + " Euro");
                sb.AppendLine();
                sb.AppendLine("Kontostand:        " + eco.Money.ToString("F2") + " Euro");
            }
            _text.text = sb.ToString();
        }

        void OnNextDay()
        {
            IsOpen = false;
            _root.SetActive(false);
            GameManager.Instance.SetUIMode(false);
            if (SceneBootstrapper.Instance != null)
                SceneBootstrapper.Instance.StartNextDay();
        }
    }
}
