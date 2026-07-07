using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Kiosk.Placement;
using Kiosk.Shelves;

namespace Kiosk.Core
{
    /// <summary>
    /// Baut die komplette KioskGame-Szene zur Laufzeit auf: Raum, Einrichtung,
    /// Licht, Spieler, Manager, Wegpunkte und UI. Einziges Objekt in der Szene.
    /// Hochwertige Assets koennen spaeter die hier erzeugten Platzhalter ersetzen.
    /// </summary>
    public class SceneBootstrapper : MonoBehaviour
    {
        public static SceneBootstrapper Instance { get; private set; }

        /// <summary>Wird vom Hauptmenue gesetzt, wenn "Weiterspielen" gewaehlt wurde.</summary>
        public static bool LoadSaveOnStart;

        readonly Dictionary<string, GameObject> _upgradeVisuals = new Dictionary<string, GameObject>();
        Light _mainLight;
        Transform _counterTop;
        int _extraShelfCount;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Time.timeScale = 1f;
            Shelf.ResetRegistry();
            ProceduralAssetGenerator.ClearCache();
            DefaultGameData.ResetRuntimeData();
            CreateManagers();
        }

        void Start()
        {
            BuildRoom();
            BuildFurniture();
            BuildLighting();
            BuildPlayer();
            BuildUI();
            SetupGameStart();
        }

        void CreateManagers()
        {
            var managers = new GameObject("Managers");
            managers.AddComponent<GameManager>();
            managers.AddComponent<DayNightCycle>();
            managers.AddComponent<UnlockManager>();
            managers.AddComponent<Economy.EconomyManager>();
            managers.AddComponent<Economy.ReputationManager>();
            managers.AddComponent<Inventory.InventoryManager>();
            managers.AddComponent<Inventory.StorageManager>();
            managers.AddComponent<Upgrades.UpgradeManager>();
            managers.AddComponent<Orders.OrderSystem>();
            managers.AddComponent<Delivery.DeliverySystem>();
            managers.AddComponent<Packages.PackageSystem>();
            managers.AddComponent<AgeRestricted.AgeCheckSystem>();
            managers.AddComponent<Checkout.CashRegister>();
            managers.AddComponent<Checkout.PaymentSystem>();
            managers.AddComponent<Customers.CustomerQueue>();
            managers.AddComponent<Customers.CustomerSpawner>();
            managers.AddComponent<PlacementSystem>();
            managers.AddComponent<SaveSystem.SaveLoadSystem>();
            managers.AddComponent<Audio.AudioManager>();
            managers.AddComponent<AutomationController>();
        }

        // ---------- Raum ----------

        void BuildRoom()
        {
            var room = new GameObject("Kiosk_Raum");

            // Boden (10 x 8 m)
            var floor = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "Boden",
                new Vector3(10f, 0.1f, 8f), "Material_Boden");
            floor.transform.SetParent(room.transform);
            floor.transform.position = new Vector3(0f, -0.05f, 0f);

            // Decke
            var ceiling = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "Decke",
                new Vector3(10f, 0.1f, 8f), "Material_Decke");
            ceiling.transform.SetParent(room.transform);
            ceiling.transform.position = new Vector3(0f, 3f, 0f);

            // Waende (Eingang: Luecke in der Suedwand bei x = -1..1)
            CreateWall(room.transform, "Wand_Nord", new Vector3(0f, 1.5f, 4f), new Vector3(10f, 3f, 0.2f));
            CreateWall(room.transform, "Wand_Ost", new Vector3(5f, 1.5f, 0f), new Vector3(0.2f, 3f, 8f));
            CreateWall(room.transform, "Wand_West", new Vector3(-5f, 1.5f, 0f), new Vector3(0.2f, 3f, 8f));
            CreateWall(room.transform, "Wand_Sued_Links", new Vector3(-3f, 1.5f, -4f), new Vector3(4f, 3f, 0.2f));
            CreateWall(room.transform, "Wand_Sued_Rechts", new Vector3(3f, 1.5f, -4f), new Vector3(4f, 3f, 0.2f));
            // Tuersturz ueber dem Eingang
            CreateWall(room.transform, "Eingang_Sturz", new Vector3(0f, 2.6f, -4f), new Vector3(2f, 0.8f, 0.2f));

            // Aussenboden vor dem Eingang
            var outside = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "Boden_Aussen",
                new Vector3(6f, 0.1f, 5f), "Material_Boden");
            outside.transform.SetParent(room.transform);
            outside.transform.position = new Vector3(0f, -0.05f, -6.5f);
        }

        void CreateWall(Transform parent, string name, Vector3 pos, Vector3 scale)
        {
            var wall = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, name, scale, "Material_Wand");
            wall.transform.SetParent(parent);
            wall.transform.position = pos;
        }

        // ---------- Einrichtung ----------

        void BuildFurniture()
        {
            // Zwei Basis-Regale an der Westwand
            CreateShelf("Regal_1", new Vector3(-3.5f, 0f, 0.5f), false, false);
            CreateShelf("Regal_2", new Vector3(-3.5f, 0f, 2.5f), false, false);

            // Kassentheke rechts
            var counter = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "Theke",
                new Vector3(0.9f, 1f, 2.4f), "Material_Theke");
            counter.transform.position = new Vector3(2.6f, 0.5f, -1f);
            _counterTop = counter.transform;
            counter.AddComponent<Checkout.CheckoutCounter>();

            // Kasse auf der Theke
            var register = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "Kasse",
                new Vector3(0.4f, 0.3f, 0.4f), "Material_Kasse");
            register.transform.SetParent(counter.transform, true);
            register.transform.position = new Vector3(2.6f, 1.15f, -0.4f);
            Object.Destroy(register.GetComponent<Collider>());

            // Warteschlange vor der Theke
            var queue = Customers.CustomerQueue.Instance;
            if (queue != null)
            {
                queue.QueueStart = new Vector3(1.7f, 0f, -1f);
                queue.QueueDirection = new Vector3(-0.3f, 0f, -1f).normalized;
            }

            // Lotto-Terminal neben der Theke (aktiv erst nach Upgrade)
            var lotto = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "LottoTerminal",
                new Vector3(0.7f, 1.8f, 0.5f), "Material_LottoTerminal");
            lotto.transform.position = new Vector3(3f, 0.9f, 1.2f);
            lotto.AddComponent<Lotto.LottoTerminal>();

            // Paketregal hinter der Theke
            var packageShelf = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "Paketregal",
                new Vector3(1.6f, 1.8f, 0.5f), "Material_Paket");
            packageShelf.transform.position = new Vector3(4.2f, 0.9f, -3f);

            // Lagerbereich hinten rechts
            var storageZone = new GameObject("Lagerbereich");
            storageZone.transform.position = new Vector3(3.5f, 0f, 3.2f);
            for (int i = 0; i < 3; i++)
            {
                var crate = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "Lagerkiste_" + i,
                    new Vector3(0.6f, 0.5f, 0.6f), "Material_Paket");
                crate.transform.SetParent(storageZone.transform);
                crate.transform.position = new Vector3(2.8f + (i % 2) * 0.7f, 0.25f + (i / 2) * 0.55f, 3.2f);
            }

            // Spawn- und Lieferpunkte
            var spawner = Customers.CustomerSpawner.Instance;
            if (spawner != null)
            {
                spawner.EntrancePosition = new Vector3(0f, 0f, -6f);
                spawner.ShopCenter = new Vector3(-1f, 0f, -1f);
            }
            var delivery = Delivery.DeliverySystem.Instance;
            if (delivery != null)
                delivery.DeliveryDropPoint = new Vector3(1.2f, 0f, -3.2f);
        }

        Shelf CreateShelf(string name, Vector3 position, bool isFridge, bool isTobacco)
        {
            return CreateShelf(name, position, Quaternion.identity, isFridge, isTobacco, 2, 2, 1.6f);
        }

        Shelf CreateShelf(string name, Vector3 position, Quaternion rotation, bool isFridge, bool isTobacco, int columns, int levels, float width)
        {
            var root = new GameObject(name);
            root.transform.position = position;
            root.transform.rotation = rotation;

            string mat = isFridge ? "Material_Kuehlschrank" : "Material_Regal";
            var body = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "Korpus",
                new Vector3(width, 1.8f, 0.5f), mat);
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);

            var shelf = root.AddComponent<Shelf>();
            shelf.IsFridge = isFridge;
            shelf.IsTobaccoCabinet = isTobacco;

            float step = columns > 1 ? width / columns : 0f;
            float startX = -width * 0.5f + step * 0.5f;

            for (int level = 0; level < levels; level++)
                for (int i = 0; i < columns; i++)
                {
                    var board = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "Brett",
                        new Vector3(Mathf.Max(0.42f, step - 0.1f), 0.04f, 0.45f), mat);
                    board.transform.SetParent(root.transform, false);
                    board.transform.localPosition = new Vector3(startX + i * step, 0.7f + level * 0.6f, 0.3f);
                    Object.Destroy(board.GetComponent<Collider>());

                    var slotGo = new GameObject("Slot_" + level + "_" + i);
                    slotGo.transform.SetParent(root.transform, false);
                    slotGo.transform.localPosition = new Vector3(startX + i * step, 0.74f + level * 0.6f, 0.3f);
                    var slot = slotGo.AddComponent<ShelfSlot>();
                    shelf.Slots.Add(slot);
                }
            return shelf;
        }

        public GameObject CreatePlaceableObject(string itemId, Vector3 position, Quaternion rotation, bool dynamicPlacement)
        {
            GameObject root = null;
            switch (itemId)
            {
                case "shelf_standard":
                    root = CreateShelf("Standardregal", position, rotation, false, false, 2, 2, 1.6f).gameObject;
                    break;
                case "shelf_wide":
                    root = CreateShelf("Breitregal", position, rotation, false, false, 3, 2, 2.3f).gameObject;
                    break;
                case "shelf_fridge":
                    root = CreateShelf("Kuehlregal", position, rotation, true, false, 2, 2, 1.7f).gameObject;
                    break;
                case "shelf_tobacco":
                    root = CreateShelf("Tabakschrank", position, rotation, false, true, 2, 2, 1.4f).gameObject;
                    ApplyTobaccoCabinetLook(root);
                    break;
                case "accessory_register":
                    root = CreateAccessoryRoot("Zweitkasse", position, rotation);
                    AttachChild(root.transform, PrimitiveType.Cube, "Sockel", new Vector3(0f, 0.5f, 0f), new Vector3(0.9f, 1f, 0.8f), "Material_Theke");
                    AttachChild(root.transform, PrimitiveType.Cube, "Kasse", new Vector3(0f, 1.15f, 0f), new Vector3(0.4f, 0.3f, 0.4f), "Material_Kasse");
                    break;
                case "accessory_baskets":
                    root = CreateAccessoryRoot("Einkaufskoerbe", position, rotation);
                    for (int i = 0; i < 3; i++)
                        AttachChild(root.transform, PrimitiveType.Cube, "Korb_" + i, new Vector3(-0.22f + i * 0.22f, 0.16f, 0f), new Vector3(0.18f, 0.12f, 0.18f), "Material_LottoTerminal");
                    AttachChild(root.transform, PrimitiveType.Cube, "Unterlage", new Vector3(0f, 0.04f, 0f), new Vector3(0.85f, 0.08f, 0.45f), "Material_Theke");
                    break;
                case "accessory_counter":
                    root = CreateAccessoryRoot("Thekenmodul", position, rotation);
                    AttachChild(root.transform, PrimitiveType.Cube, "Modul", new Vector3(0f, 0.55f, 0f), new Vector3(1.4f, 1.1f, 0.8f), "Material_Theke");
                    break;
                case "accessory_storage_box":
                    root = CreateAccessoryRoot("Lagerboxen", position, rotation);
                    for (int i = 0; i < 2; i++)
                        AttachChild(root.transform, PrimitiveType.Cube, "Box_" + i, new Vector3(-0.22f + i * 0.44f, 0.24f, 0f), new Vector3(0.35f, 0.45f, 0.35f), "Material_Paket");
                    break;
                case "accessory_signs":
                    root = CreateAccessoryRoot("Preisschilder", position, rotation);
                    for (int i = 0; i < 2; i++)
                    {
                        AttachChild(root.transform, PrimitiveType.Cube, "Pfosten_" + i, new Vector3(-0.18f + i * 0.36f, 0.25f, 0f), new Vector3(0.04f, 0.5f, 0.04f), "Material_Kasse");
                        AttachChild(root.transform, PrimitiveType.Cube, "Schild_" + i, new Vector3(-0.18f + i * 0.36f, 0.48f, 0f), new Vector3(0.22f, 0.12f, 0.03f), "Material_Wand");
                    }
                    break;
                case "accessory_lamp":
                    root = CreateAccessoryRoot("Lampe", position, rotation);
                    AttachChild(root.transform, PrimitiveType.Cylinder, "Standfuss", new Vector3(0f, 0.65f, 0f), new Vector3(0.08f, 0.65f, 0.08f), "Material_Kasse");
                    AttachChild(root.transform, PrimitiveType.Sphere, "Leuchte", new Vector3(0f, 1.35f, 0f), new Vector3(0.32f, 0.22f, 0.32f), "Material_LottoTerminal");
                    var light = root.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.range = 7f;
                    light.intensity = 0.75f;
                    light.color = new Color(1f, 0.86f, 0.68f);
                    break;
                case "accessory_ad_sign":
                    root = CreateAccessoryRoot("Werbeschild", position, rotation);
                    AttachChild(root.transform, PrimitiveType.Cube, "Pfosten", new Vector3(0f, 0.8f, 0f), new Vector3(0.08f, 1.6f, 0.08f), "Material_Kasse");
                    AttachChild(root.transform, PrimitiveType.Cube, "Schild", new Vector3(0f, 1.55f, 0f), new Vector3(1.3f, 0.5f, 0.08f), "Material_LottoTerminal");
                    break;
                case "accessory_decor":
                    root = CreateAccessoryRoot("Dekoration", position, rotation);
                    AttachChild(root.transform, PrimitiveType.Cylinder, "Topf", new Vector3(0f, 0.15f, 0f), new Vector3(0.22f, 0.15f, 0.22f), "Material_Paket");
                    AttachChild(root.transform, PrimitiveType.Sphere, "Pflanze", new Vector3(0f, 0.48f, 0f), new Vector3(0.42f, 0.42f, 0.42f), "Material_Kunde");
                    break;
            }

            if (root != null && dynamicPlacement)
            {
                var marker = root.GetComponent<PlacedObjectMarker>();
                if (marker == null) marker = root.AddComponent<PlacedObjectMarker>();
                marker.ItemId = itemId;
            }
            return root;
        }

        GameObject CreateAccessoryRoot(string name, Vector3 position, Quaternion rotation)
        {
            var root = new GameObject(name);
            root.transform.SetPositionAndRotation(position, rotation);
            return root;
        }

        GameObject AttachChild(Transform parent, PrimitiveType primitive, string name, Vector3 localPosition, Vector3 localScale, string materialName)
        {
            var child = ProceduralAssetGenerator.CreatePrimitive(primitive, name, localScale, materialName);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            return child;
        }

        // ---------- Licht / Spieler ----------

        void BuildLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.72f, 0.7f, 0.68f);
            var lightGo = new GameObject("Sonne");
            _mainLight = lightGo.AddComponent<Light>();
            _mainLight.type = LightType.Directional;
            _mainLight.intensity = 0.85f;
            _mainLight.color = new Color(1f, 0.95f, 0.88f);
            _mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            if (DayNightCycle.Instance != null) DayNightCycle.Instance.SunLight = _mainLight;

            var interior = new GameObject("Deckenlicht");
            var pl = interior.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.range = 12f;
            pl.intensity = 1.25f;
            pl.color = new Color(1f, 0.88f, 0.72f);
            interior.transform.position = new Vector3(0f, 2.6f, 0f);

            CreateCeilingLight("Deckenlicht_Regale", new Vector3(-2.8f, 2.5f, 1.3f), 8f, 0.7f);
            CreateCeilingLight("Deckenlicht_Kasse", new Vector3(2.6f, 2.35f, -1f), 7f, 0.9f);
        }

        void CreateCeilingLight(string name, Vector3 position, float range, float intensity)
        {
            var go = new GameObject(name);
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = range;
            light.intensity = intensity;
            light.color = new Color(1f, 0.84f, 0.68f);
            go.transform.position = position;
        }

        void BuildPlayer()
        {
            var player = new GameObject("Spieler");
            player.transform.position = new Vector3(3.8f, 0f, -1f);
            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            var camGo = new GameObject("Spieler_Kamera");
            camGo.transform.SetParent(player.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.nearClipPlane = 0.05f;
            camGo.AddComponent<AudioListener>();

            var controller = player.AddComponent<Player.PlayerController>();
            controller.PlayerCamera = cam;
            var interactor = player.AddComponent<Player.PlayerInteractor>();
            interactor.PlayerCamera = cam;
            player.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        }

        void BuildUI()
        {
            var uiGo = new GameObject("UI");
            uiGo.AddComponent<UI.UIManager>();
        }

        // ---------- Spielstart ----------

        void SetupGameStart()
        {
            bool loadedSave = false;
            if (LoadSaveOnStart && SaveSystem.SaveLoadSystem.Instance != null
                && SaveSystem.SaveLoadSystem.Instance.Load())
            {
                LoadSaveOnStart = false;
                loadedSave = true;
            }
            else
            {
                LoadSaveOnStart = false;
                GiveStartingStock();
            }

            var cycle = DayNightCycle.Instance;
            if (cycle != null)
            {
                cycle.OnDayEnded += HandleDayEnded;
                if (!loadedSave) cycle.BeginDay();
            }
        }

        void GiveStartingStock()
        {
            var inv = Inventory.InventoryManager.Instance;
            if (inv == null) return;
            inv.Add("wasser_still", 10);
            inv.Add("cola_fizz", 10);
            inv.Add("chips_paprika", 8);
            inv.Add("schokoriegel", 10);
            inv.Add("gummibaeren", 6);
            inv.Add("kaugummi_mint", 6);

            // Regale vorab befuellen, damit sofort verkauft werden kann
            foreach (var shelf in Shelf.AllShelves)
                if (shelf != null)
                    shelf.RestockFromInventory();
        }

        void HandleDayEnded()
        {
            var eco = Economy.EconomyManager.Instance;
            float profit = eco != null ? eco.CloseDay() : 0f;
            if (Audio.AudioManager.Instance != null)
                Audio.AudioManager.Instance.Play(Audio.SoundId.DaySummary);
            if (UI.UIManager.Instance != null)
                UI.UIManager.Instance.ShowDaySummary(profit);
        }

        /// <summary>Vom DaySummary-UI aufgerufen: naechsten Tag starten und speichern.</summary>
        public void StartNextDay()
        {
            var eco = Economy.EconomyManager.Instance;
            if (eco != null) eco.ResetDayStats();
            if (GameManager.Instance != null) GameManager.Instance.StartNextDay();
            if (DayNightCycle.Instance != null) DayNightCycle.Instance.BeginDay();
            if (SaveSystem.SaveLoadSystem.Instance != null) SaveSystem.SaveLoadSystem.Instance.Save();
        }

        // ---------- Visuelle Upgrade-Effekte ----------

        public void ApplyUpgradeVisual(string upgradeId)
        {
            if (_upgradeVisuals.ContainsKey(upgradeId)) return;
            GameObject visual = null;
            switch (upgradeId)
            {
                case "zweites_regal":
                    _extraShelfCount++;
                    visual = CreateShelf("Regal_Extra_" + _extraShelfCount,
                        new Vector3(-1.2f, 0f, 2.5f), false, false).gameObject;
                    break;
                case "kuehlschrank":
                    visual = CreateShelf("Kuehlschrank", new Vector3(-3.5f, 0f, -1.5f), true, false).gameObject;
                    break;
                case "premium_kuehlschrank":
                    visual = CreateShelf("Premium_Kuehlschrank", new Vector3(-1.2f, 0f, -1.5f), true, false).gameObject;
                    break;
                case "tabak_schrank":
                    visual = CreateShelf("Tabakschrank", new Vector3(4.2f, 0f, 0.2f), false, true).gameObject;
                    ApplyTobaccoCabinetLook(visual);
                    break;
                case "zweite_kasse":
                    visual = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "Kasse_2",
                        new Vector3(0.4f, 0.3f, 0.4f), "Material_Kasse");
                    visual.transform.position = new Vector3(2.6f, 1.15f, -1.6f);
                    break;
                case "kaffeemaschine":
                    visual = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cylinder, "Kaffeemaschine",
                        new Vector3(0.25f, 0.25f, 0.25f), "Material_Kasse");
                    visual.transform.position = new Vector3(2.6f, 1.3f, 0.1f);
                    break;
                case "sicherheitskamera":
                    visual = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "Sicherheitskamera",
                        new Vector3(0.2f, 0.2f, 0.35f), "Material_Kasse");
                    visual.transform.position = new Vector3(4.6f, 2.7f, 3.6f);
                    visual.transform.rotation = Quaternion.Euler(20f, -135f, 0f);
                    break;
                case "beleuchtung":
                    if (_mainLight != null) _mainLight.intensity += 0.2f;
                    var extraLight = new GameObject("Zusatzlicht");
                    var el = extraLight.AddComponent<Light>();
                    el.type = LightType.Point;
                    el.range = 10f;
                    el.intensity = 0.8f;
                    extraLight.transform.position = new Vector3(-2.5f, 2.6f, 1.5f);
                    visual = extraLight;
                    break;
                case "klimaanlage":
                    visual = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "Klimaanlage",
                        new Vector3(1f, 0.4f, 0.3f), "Material_Kuehlschrank");
                    visual.transform.position = new Vector3(0f, 2.6f, 3.8f);
                    break;
                case "aussenwerbung":
                    visual = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "Aussenwerbung",
                        new Vector3(2.4f, 0.6f, 0.1f), "Material_LottoTerminal");
                    visual.transform.position = new Vector3(0f, 3.3f, -4.2f);
                    break;
                case "schaufensterdeko":
                    visual = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Sphere, "Schaufensterdeko",
                        new Vector3(0.5f, 0.5f, 0.5f), "Material_LottoTerminal");
                    visual.transform.position = new Vector3(-2f, 1f, -3.6f);
                    break;
                case "paketstation":
                    visual = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "Paketstation",
                        new Vector3(1.2f, 2f, 0.8f), "Material_Paket");
                    visual.transform.position = new Vector3(2.5f, 1f, -5.5f);
                    break;
                case "paketregal2":
                    visual = ProceduralAssetGenerator.CreatePrimitive(PrimitiveType.Cube, "Paketregal_2",
                        new Vector3(1.6f, 1.8f, 0.5f), "Material_Paket");
                    visual.transform.position = new Vector3(4.2f, 0.9f, -2.2f);
                    break;
            }
            if (visual != null) _upgradeVisuals[upgradeId] = visual;
        }

        void ApplyTobaccoCabinetLook(GameObject shelfGo)
        {
            foreach (var renderer in shelfGo.GetComponentsInChildren<Renderer>())
                if (renderer.gameObject.name == "Korpus")
                    renderer.material = ProceduralAssetGenerator.GetMaterial("Material_Produkt_Tabak_Fiktiv");
        }
    }
}
