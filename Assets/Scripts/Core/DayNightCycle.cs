using UnityEngine;

namespace Kiosk.Core
{
    /// <summary>
    /// Tageszeit-System. Ein Spieltag laeuft von OpenHour bis CloseHour,
    /// danach Tagesabschluss. Beleuchtung wird der Uhrzeit angepasst.
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        public static DayNightCycle Instance { get; private set; }

        [Tooltip("Echtzeit-Minuten pro Spieltag")]
        public float RealMinutesPerDay = 12f;
        public float OpenHour = 8f;
        public float CloseHour = 20f;

        public float TimeOfDay { get; private set; }
        public bool ShopOpen { get; private set; }
        public bool DayRunning { get; private set; }

        public Light SunLight;

        public event System.Action OnShopClosed;
        public event System.Action OnDayEnded;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            TimeOfDay = OpenHour;
        }

        public void BeginDay()
        {
            TimeOfDay = OpenHour;
            ShopOpen = true;
            DayRunning = true;
        }

        public void SetTime(float hour)
        {
            TimeOfDay = Mathf.Clamp(hour, 0f, 24f);
            DayRunning = TimeOfDay < CloseHour;
            ShopOpen = TimeOfDay >= OpenHour && TimeOfDay < CloseHour;
        }

        void Update()
        {
            if (!DayRunning) return;
            float hoursPerSecond = (CloseHour - OpenHour) / (RealMinutesPerDay * 60f);
            TimeOfDay += Time.deltaTime * hoursPerSecond;

            if (ShopOpen && TimeOfDay >= CloseHour)
            {
                ShopOpen = false;
                if (OnShopClosed != null) OnShopClosed();
            }

            // Tag endet, wenn geschlossen und keine Kunden mehr im Laden sind.
            if (!ShopOpen)
            {
                var spawner = Customers.CustomerSpawner.Instance;
                if (spawner == null || spawner.ActiveCustomerCount == 0)
                {
                    DayRunning = false;
                    if (OnDayEnded != null) OnDayEnded();
                }
            }

            UpdateLighting();
        }

        void UpdateLighting()
        {
            if (SunLight == null) return;
            float t = Mathf.InverseLerp(OpenHour, CloseHour, TimeOfDay);
            SunLight.transform.rotation = Quaternion.Euler(Mathf.Lerp(20f, 160f, t), -30f, 0f);
            SunLight.intensity = Mathf.Lerp(1.1f, 0.35f, Mathf.Abs(t - 0.5f) * 2f);
            SunLight.color = Color.Lerp(new Color(1f, 0.96f, 0.88f), new Color(1f, 0.6f, 0.4f), Mathf.Clamp01((t - 0.7f) / 0.3f));
        }

        public string ClockText
        {
            get
            {
                int h = Mathf.FloorToInt(TimeOfDay);
                int m = Mathf.FloorToInt((TimeOfDay - h) * 60f);
                return string.Format("{0:00}:{1:00}", h, m);
            }
        }
    }
}
