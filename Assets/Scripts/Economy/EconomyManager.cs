using UnityEngine;

namespace Kiosk.Economy
{
    /// <summary>
    /// Geld, Umsatz, Kosten, Tagesabschluss und Wochenstatistik.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        public float Money = 2500f;
        public float RentPerDay = 60f;
        public float ElectricityPerDay = 25f;

        // Tagesstatistik
        public float DayRevenue;
        public float DayExpenses;
        public float DayPenalties;
        public int DayCustomersServed;

        // Wochenstatistik (letzte 7 Tage Gewinn)
        public float[] WeekProfits = new float[7];

        public event System.Action OnMoneyChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool CanAfford(float amount) { return Money >= amount; }

        public void AddRevenue(float amount)
        {
            Money += amount;
            DayRevenue += amount;
            Notify();
        }

        public bool Spend(float amount)
        {
            if (!CanAfford(amount)) return false;
            Money -= amount;
            DayExpenses += amount;
            Notify();
            return true;
        }

        public void ApplyPenalty(float amount)
        {
            Money -= amount;
            DayPenalties += amount;
            Notify();
        }

        public void SetMoney(float amount) { Money = amount; Notify(); }

        void Notify() { if (OnMoneyChanged != null) OnMoneyChanged(); }

        /// <summary>Fixkosten abziehen und Tagesgewinn berechnen. Gibt den Gewinn zurueck.</summary>
        public float CloseDay()
        {
            float fixedCosts = RentPerDay + ElectricityPerDay;
            Money -= fixedCosts;
            DayExpenses += fixedCosts;
            float profit = DayRevenue - DayExpenses - DayPenalties;
            for (int i = WeekProfits.Length - 1; i > 0; i--) WeekProfits[i] = WeekProfits[i - 1];
            WeekProfits[0] = profit;
            Notify();
            return profit;
        }

        public void ResetDayStats()
        {
            DayRevenue = 0f;
            DayExpenses = 0f;
            DayPenalties = 0f;
            DayCustomersServed = 0;
        }

        public float WeekProfit
        {
            get { float s = 0f; foreach (var p in WeekProfits) s += p; return s; }
        }
    }
}
