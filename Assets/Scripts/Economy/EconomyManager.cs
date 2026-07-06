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
        public float LifetimeRevenue { get; private set; }
        public int LifetimeCustomersServed { get; private set; }

        // Wochenstatistik (letzte 7 Tage Gewinn)
        public float[] WeekProfits = new float[7];
        bool _dailyGoalBonusApplied;

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
            LifetimeRevenue += amount;
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

        public int DailyCustomerGoal
        {
            get
            {
                int day = Core.GameManager.Instance != null ? Core.GameManager.Instance.Day : 1;
                return 2 + Mathf.Max(0, day - 1) * 2;
            }
        }

        public float DailyRevenueGoal
        {
            get
            {
                int day = Core.GameManager.Instance != null ? Core.GameManager.Instance.Day : 1;
                return 25f + Mathf.Max(0, day - 1) * 18f;
            }
        }

        public float DailyGoalBonus
        {
            get
            {
                int day = Core.GameManager.Instance != null ? Core.GameManager.Instance.Day : 1;
                return 15f + Mathf.Max(0, day - 1) * 6f;
            }
        }

        public bool DailyGoalsMet
        {
            get { return DayCustomersServed >= DailyCustomerGoal && DayRevenue >= DailyRevenueGoal; }
        }

        public void RegisterCustomerServed()
        {
            DayCustomersServed++;
            LifetimeCustomersServed++;
        }

        /// <summary>Fixkosten abziehen und Tagesgewinn berechnen. Gibt den Gewinn zurueck.</summary>
        public float CloseDay()
        {
            if (DailyGoalsMet)
            {
                if (!_dailyGoalBonusApplied)
                {
                    Money += DailyGoalBonus;
                    DayRevenue += DailyGoalBonus;
                    _dailyGoalBonusApplied = true;
                }
            }
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
            _dailyGoalBonusApplied = false;
        }

        public void SetLifetimeStats(float revenue, int customers)
        {
            LifetimeRevenue = Mathf.Max(0f, revenue);
            LifetimeCustomersServed = Mathf.Max(0, customers);
        }

        public float WeekProfit
        {
            get { float s = 0f; foreach (var p in WeekProfits) s += p; return s; }
        }
    }
}
