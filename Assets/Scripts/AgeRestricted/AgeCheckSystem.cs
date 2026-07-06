using UnityEngine;
using Kiosk.Customers;

namespace Kiosk.AgeRestricted
{
    /// <summary>
    /// Alterspruefung fuer fiktive altersbeschraenkte Produkte.
    /// Verkauf an Minderjaehrige gibt Strafe, korrektes Ablehnen gibt Rufbonus.
    /// </summary>
    public class AgeCheckSystem : MonoBehaviour
    {
        public static AgeCheckSystem Instance { get; private set; }

        public int MinimumAge = 18;
        public float UnderageSalePenalty = 150f;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool IsAdult(CustomerAI customer)
        {
            return customer != null && customer.Age >= MinimumAge;
        }

        /// <summary>Spieler verkauft trotz Altersbeschraenkung. Bei Minderjaehrigen: Strafe.</summary>
        public bool ProcessSaleDecision(CustomerAI customer)
        {
            if (IsAdult(customer)) return true;
            var eco = Economy.EconomyManager.Instance;
            if (eco != null) eco.ApplyPenalty(UnderageSalePenalty);
            if (Economy.ReputationManager.Instance != null)
                Economy.ReputationManager.Instance.Add(-10f);
            if (UI.UIManager.Instance != null)
                UI.UIManager.Instance.ShowToast("Strafe: Verkauf an Minderjaehrige! -" + UnderageSalePenalty + " Euro");
            return false;
        }

        /// <summary>Spieler lehnt Verkauf ab. Korrekt bei Minderjaehrigen: Rufbonus.</summary>
        public void ProcessRefusal(CustomerAI customer)
        {
            if (!IsAdult(customer))
            {
                if (Economy.ReputationManager.Instance != null)
                    Economy.ReputationManager.Instance.Add(4f);
                if (Core.GameManager.Instance != null) Core.GameManager.Instance.AddXP(5);
                if (UI.UIManager.Instance != null)
                    UI.UIManager.Instance.ShowToast("Korrekt abgelehnt! +Ruf");
            }
            else
            {
                if (Economy.ReputationManager.Instance != null)
                    Economy.ReputationManager.Instance.Add(-2f);
                if (UI.UIManager.Instance != null)
                    UI.UIManager.Instance.ShowToast("Kunde war volljaehrig... -Ruf");
            }
        }
    }
}
