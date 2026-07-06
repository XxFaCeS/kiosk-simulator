using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Kiosk.Core;

namespace Kiosk.UI
{
    /// <summary>
    /// Baut Canvas, EventSystem und das Ingame-HUD auf und verwaltet alle Fenster
    /// (Tablet, Kasse, Tagesabschluss, Pause).
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        public Canvas Canvas { get; private set; }

        Text _moneyText;
        Text _clockText;
        Text _dayText;
        Text _repText;
        Text _xpText;
        Text _storageText;
        Text _promptText;
        Text _toastText;
        float _toastTimer;

        TabletUI _tablet;
        CheckoutUI _checkout;
        DaySummaryUI _daySummary;
        PauseMenuUI _pauseMenu;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildCanvas();
            BuildHUD();
            BuildWindows();
        }

        void Start()
        {
            HookEvents();
            RefreshHUD();
        }

        void BuildCanvas()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            Canvas = canvasGo.GetComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);

            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                es.transform.SetParent(transform, false);
            }
        }

        void BuildHUD()
        {
            var hud = ProceduralAssetGenerator.CreatePanel(Canvas.transform, "HUD",
                Vector2.zero, Vector2.one, Color.clear);

            var topBar = ProceduralAssetGenerator.CreatePanel(hud.transform, "TopBar",
                new Vector2(0f, 0.94f), new Vector2(1f, 1f), new Color(0f, 0f, 0f, 0.55f));
            _moneyText = MakeBarText(topBar.transform, "Geld", 0.00f, 0.18f);
            _dayText = MakeBarText(topBar.transform, "Tag", 0.19f, 0.30f);
            _clockText = MakeBarText(topBar.transform, "Uhr", 0.31f, 0.42f);
            _repText = MakeBarText(topBar.transform, "Ruf", 0.43f, 0.58f);
            _xpText = MakeBarText(topBar.transform, "XP", 0.59f, 0.78f);
            _storageText = MakeBarText(topBar.transform, "Lager", 0.79f, 1f);

            var promptGo = ProceduralAssetGenerator.CreatePanel(hud.transform, "Prompt",
                new Vector2(0.3f, 0.06f), new Vector2(0.7f, 0.12f), new Color(0f, 0f, 0f, 0.5f));
            _promptText = ProceduralAssetGenerator.CreateText(promptGo.transform, "Text", "", 20, TextAnchor.MiddleCenter);
            promptGo.SetActive(false);

            var toastGo = ProceduralAssetGenerator.CreatePanel(hud.transform, "Toast",
                new Vector2(0.25f, 0.85f), new Vector2(0.75f, 0.92f), new Color(0.1f, 0.1f, 0.2f, 0.7f));
            _toastText = ProceduralAssetGenerator.CreateText(toastGo.transform, "Text", "", 20, TextAnchor.MiddleCenter);
            toastGo.SetActive(false);

            // Fadenkreuz
            var crosshair = ProceduralAssetGenerator.CreatePanel(hud.transform, "Fadenkreuz",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(1f, 1f, 1f, 0.8f));
            var rt = crosshair.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(4f, 4f);

            var hint = ProceduralAssetGenerator.CreateText(hud.transform, "Steuerung",
                "WASD: Laufen | E: Interagieren | Tab: Tablet | Esc: Pause", 14, TextAnchor.LowerLeft);
            hint.color = new Color(1f, 1f, 1f, 0.5f);
        }

        Text MakeBarText(Transform parent, string name, float xMin, float xMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(xMin, 0f);
            rt.anchorMax = new Vector2(xMax, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return ProceduralAssetGenerator.CreateText(go.transform, "Text", "", 18, TextAnchor.MiddleCenter);
        }

        void BuildWindows()
        {
            _tablet = new GameObject("TabletUI").AddComponent<TabletUI>();
            _tablet.transform.SetParent(Canvas.transform, false);
            _tablet.Build();

            _checkout = new GameObject("CheckoutUI").AddComponent<CheckoutUI>();
            _checkout.transform.SetParent(Canvas.transform, false);
            _checkout.Build();

            _daySummary = new GameObject("DaySummaryUI").AddComponent<DaySummaryUI>();
            _daySummary.transform.SetParent(Canvas.transform, false);
            _daySummary.Build();

            _pauseMenu = new GameObject("PauseMenuUI").AddComponent<PauseMenuUI>();
            _pauseMenu.transform.SetParent(Canvas.transform, false);
            _pauseMenu.Build();
        }

        void HookEvents()
        {
            if (Economy.EconomyManager.Instance != null)
                Economy.EconomyManager.Instance.OnMoneyChanged += RefreshHUD;
            if (Economy.ReputationManager.Instance != null)
                Economy.ReputationManager.Instance.OnReputationChanged += RefreshHUD;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnXPChanged += RefreshHUD;
                GameManager.Instance.OnDayChanged += delegate { RefreshHUD(); };
            }
            if (Inventory.InventoryManager.Instance != null)
                Inventory.InventoryManager.Instance.OnInventoryChanged += RefreshHUD;
        }

        void Update()
        {
            if (_clockText != null && DayNightCycle.Instance != null)
            {
                var cycle = DayNightCycle.Instance;
                _clockText.text = cycle.ClockText + (cycle.ShopOpen ? " (offen)" : " (zu)");
            }
            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.unscaledDeltaTime;
                if (_toastTimer <= 0f && _toastText != null)
                    _toastText.transform.parent.gameObject.SetActive(false);
            }
        }

        public void RefreshHUD()
        {
            var eco = Economy.EconomyManager.Instance;
            if (_moneyText != null && eco != null)
                _moneyText.text = "Geld: " + eco.Money.ToString("F2") + " Euro";
            var gm = GameManager.Instance;
            if (_dayText != null && gm != null) _dayText.text = "Tag " + gm.Day;
            if (_xpText != null && gm != null)
                _xpText.text = "Level " + gm.Level + "  (" + gm.XP + "/" + gm.XPForNextLevel + " XP)";
            var rep = Economy.ReputationManager.Instance;
            if (_repText != null && rep != null)
                _repText.text = "Ruf: " + Mathf.RoundToInt(rep.Reputation) + "/100";
            var storage = Inventory.StorageManager.Instance;
            if (_storageText != null && storage != null)
                _storageText.text = "Lager: " + storage.UsedUnits + "/" + storage.Capacity;
        }

        // ---------- Fensterverwaltung ----------

        public bool AnyWindowOpen
        {
            get
            {
                return (_tablet != null && _tablet.IsOpen)
                    || (_checkout != null && _checkout.IsOpen)
                    || (_daySummary != null && _daySummary.IsOpen)
                    || (_pauseMenu != null && _pauseMenu.IsOpen);
            }
        }

        public void ToggleTablet()
        {
            if (_checkout.IsOpen || _daySummary.IsOpen || _pauseMenu.IsOpen) return;
            if (_tablet.IsOpen) _tablet.Close(); else _tablet.Open();
            GameManager.Instance.SetUIMode(_tablet.IsOpen);
        }

        public void TogglePauseOrCloseWindow()
        {
            if (_checkout.IsOpen) { _checkout.Cancel(); GameManager.Instance.SetUIMode(false); return; }
            if (_tablet.IsOpen) { _tablet.Close(); GameManager.Instance.SetUIMode(false); return; }
            if (_daySummary.IsOpen) return;
            if (_pauseMenu.IsOpen) _pauseMenu.Close(); else _pauseMenu.Open();
        }

        public void OpenCheckout(Customers.CustomerAI customer)
        {
            _checkout.Open(customer);
            GameManager.Instance.SetUIMode(true);
        }

        public void CloseCheckout()
        {
            GameManager.Instance.SetUIMode(false);
        }

        public void ShowDaySummary(float profit)
        {
            _daySummary.Open(profit);
            GameManager.Instance.SetUIMode(true);
        }

        public void SetInteractionPrompt(string prompt)
        {
            if (_promptText == null) return;
            bool show = !string.IsNullOrEmpty(prompt);
            _promptText.transform.parent.gameObject.SetActive(show);
            if (show) _promptText.text = prompt;
        }

        public void ShowToast(string message)
        {
            if (_toastText == null) return;
            _toastText.transform.parent.gameObject.SetActive(true);
            _toastText.text = message;
            _toastTimer = 3f;
        }
    }
}
