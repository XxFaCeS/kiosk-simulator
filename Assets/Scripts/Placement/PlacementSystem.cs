using System.Collections.Generic;
using UnityEngine;

namespace Kiosk.Placement
{
    public enum PlacementCategory { Shelf, Accessory }

    [System.Serializable]
    public class PlacementItemDefinition
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public PlacementCategory Category;
        public float Cost;
    }

    /// <summary>
    /// Kauf- und Platzierungsmodus fuer Regale und Zubehoer.
    /// </summary>
    public class PlacementSystem : MonoBehaviour
    {
        public static PlacementSystem Instance { get; private set; }

        static readonly List<PlacementItemDefinition> Catalog = new List<PlacementItemDefinition>
        {
            new PlacementItemDefinition { Id = "shelf_standard", DisplayName = "Standardregal", Description = "2x2 Faecher fuer Basisware.", Category = PlacementCategory.Shelf, Cost = 120f },
            new PlacementItemDefinition { Id = "shelf_wide", DisplayName = "Breites Regal", Description = "3x2 Faecher fuer viel Laufkundschaft.", Category = PlacementCategory.Shelf, Cost = 180f },
            new PlacementItemDefinition { Id = "shelf_fridge", DisplayName = "Kuehlregal", Description = "Gekuehltes Regal fuer Getraenke und Frische.", Category = PlacementCategory.Shelf, Cost = 240f },
            new PlacementItemDefinition { Id = "shelf_tobacco", DisplayName = "Tabakschrank", Description = "Abgeschlossenes Spezialregal fuer 18+-Ware.", Category = PlacementCategory.Shelf, Cost = 260f },
            new PlacementItemDefinition { Id = "accessory_register", DisplayName = "Zweitkasse", Description = "Sichtbare Zusatzkasse als Ausbau der Theke.", Category = PlacementCategory.Accessory, Cost = 140f },
            new PlacementItemDefinition { Id = "accessory_baskets", DisplayName = "Einkaufskoerbe", Description = "Koerbe fuer Kunden im Eingangsbereich.", Category = PlacementCategory.Accessory, Cost = 55f },
            new PlacementItemDefinition { Id = "accessory_counter", DisplayName = "Bessere Theke", Description = "Dekoratives Theken-Modul mit mehr Stellflaeche.", Category = PlacementCategory.Accessory, Cost = 160f },
            new PlacementItemDefinition { Id = "accessory_storage_box", DisplayName = "Lagerboxen", Description = "Zusaetzliche sichtbare Lagerkisten.", Category = PlacementCategory.Accessory, Cost = 70f },
            new PlacementItemDefinition { Id = "accessory_signs", DisplayName = "Preisschilder", Description = "Kleine Schilder fuer Regale und Aktionen.", Category = PlacementCategory.Accessory, Cost = 35f },
            new PlacementItemDefinition { Id = "accessory_lamp", DisplayName = "Lampe", Description = "Warme Verkaufsflaechen-Beleuchtung.", Category = PlacementCategory.Accessory, Cost = 90f },
            new PlacementItemDefinition { Id = "accessory_ad_sign", DisplayName = "Werbeschild", Description = "Sichtbares Werbeschild fuer den Laden.", Category = PlacementCategory.Accessory, Cost = 85f },
            new PlacementItemDefinition { Id = "accessory_decor", DisplayName = "Dekoration", Description = "Pflanze und Deko fuer mehr Atmosphaere.", Category = PlacementCategory.Accessory, Cost = 45f }
        };

        public bool IsPlacing { get { return _previewRoot != null && _activeDefinition != null; } }

        PlacementItemDefinition _activeDefinition;
        GameObject _previewRoot;
        Renderer[] _previewRenderers;
        Color[] _previewBaseColors;
        Collider[] _previewColliders;
        Behaviour[] _previewBehaviours;
        Vector3 _previewExtents;
        bool _isPlacementValid;

        public static IReadOnlyList<PlacementItemDefinition> GetCatalog(PlacementCategory category)
        {
            return Catalog.FindAll(item => item.Category == category);
        }

        public static PlacementItemDefinition GetDefinition(string id)
        {
            return Catalog.Find(item => item.Id == id);
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (!IsPlacing) return;
            if (Core.GameManager.Instance != null && Core.GameManager.Instance.IsPaused) return;

            UpdatePreviewTransform();

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement(true);
                return;
            }

            if (_isPlacementValid && Input.GetMouseButtonDown(0))
                ConfirmPlacement();
        }

        public bool BeginPurchase(string itemId)
        {
            return BeginPurchase(GetDefinition(itemId));
        }

        public bool BeginPurchase(PlacementItemDefinition definition)
        {
            if (definition == null || IsPlacing || Core.SceneBootstrapper.Instance == null)
                return false;

            var eco = Economy.EconomyManager.Instance;
            if (eco == null || !eco.Spend(definition.Cost))
            {
                if (UI.UIManager.Instance != null)
                    UI.UIManager.Instance.ShowToast("Nicht genug Geld fuer " + (definition != null ? definition.DisplayName : "diesen Kauf") + "!");
                return false;
            }

            _previewRoot = Core.SceneBootstrapper.Instance.CreatePlaceableObject(definition.Id, Vector3.zero, Quaternion.identity, true);
            if (_previewRoot == null)
            {
                eco.DayExpenses = Mathf.Max(0f, eco.DayExpenses - definition.Cost);
                eco.SetMoney(eco.Money + definition.Cost);
                return false;
            }

            _activeDefinition = definition;
            PreparePreview();
            if (UI.UIManager.Instance != null)
                UI.UIManager.Instance.ShowToast(definition.DisplayName + " platzieren: Linksklick = bestaetigen, Rechtsklick/Escape = abbrechen.");
            return true;
        }

        public void CancelPlacement(bool refund)
        {
            if (!IsPlacing) return;

            if (refund)
            {
                var eco = Economy.EconomyManager.Instance;
                if (eco != null)
                {
                    eco.DayExpenses = Mathf.Max(0f, eco.DayExpenses - _activeDefinition.Cost);
                    eco.SetMoney(eco.Money + _activeDefinition.Cost);
                }
                if (UI.UIManager.Instance != null)
                    UI.UIManager.Instance.ShowToast(_activeDefinition.DisplayName + " storniert. Geld erstattet.");
            }

            if (_previewRoot != null) Destroy(_previewRoot);
            _previewRoot = null;
            _previewRenderers = null;
            _previewBaseColors = null;
            _previewColliders = null;
            _previewBehaviours = null;
            _activeDefinition = null;
        }

        void ConfirmPlacement()
        {
            if (!IsPlacing) return;

            RestorePlacedObject();
            if (UI.UIManager.Instance != null)
                UI.UIManager.Instance.ShowToast(_activeDefinition.DisplayName + " platziert.");
            _previewRoot = null;
            _previewRenderers = null;
            _previewBaseColors = null;
            _previewColliders = null;
            _previewBehaviours = null;
            _activeDefinition = null;
        }

        void PreparePreview()
        {
            _previewRenderers = _previewRoot.GetComponentsInChildren<Renderer>(true);
            _previewBaseColors = new Color[_previewRenderers.Length];
            _previewColliders = _previewRoot.GetComponentsInChildren<Collider>(true);
            _previewBehaviours = _previewRoot.GetComponentsInChildren<Behaviour>(true);

            for (int i = 0; i < _previewRenderers.Length; i++)
            {
                var renderer = _previewRenderers[i];
                var material = new Material(renderer.material);
                renderer.material = material;
                _previewBaseColors[i] = material.color;
                SetMaterialMode(material, true);
            }

            foreach (var collider in _previewColliders)
                collider.enabled = false;

            foreach (var behaviour in _previewBehaviours)
            {
                if (behaviour == this) continue;
                behaviour.enabled = false;
            }

            var bounds = CalculateBounds();
            _previewExtents = new Vector3(
                Mathf.Max(0.15f, bounds.extents.x),
                Mathf.Max(0.2f, bounds.extents.y),
                Mathf.Max(0.15f, bounds.extents.z));
        }

        void RestorePlacedObject()
        {
            for (int i = 0; i < _previewRenderers.Length; i++)
            {
                var renderer = _previewRenderers[i];
                renderer.material.color = _previewBaseColors != null && i < _previewBaseColors.Length
                    ? _previewBaseColors[i]
                    : renderer.material.color;
                SetMaterialMode(renderer.material, false);
            }
            foreach (var collider in _previewColliders)
                collider.enabled = true;
            foreach (var behaviour in _previewBehaviours)
            {
                if (behaviour == this) continue;
                behaviour.enabled = true;
            }
        }

        void UpdatePreviewTransform()
        {
            var camera = Camera.main;
            if (camera == null || _previewRoot == null) return;

            var ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 position;
            Quaternion rotation = Quaternion.Euler(0f, Mathf.Round(camera.transform.eulerAngles.y / 90f) * 90f, 0f);
            if (!TryGetPlacementPosition(ray, out position))
                position = camera.transform.position + camera.transform.forward * 2.5f;

            position.y = Mathf.Max(_previewExtents.y, position.y + _previewExtents.y);
            _previewRoot.transform.SetPositionAndRotation(position, rotation);
            _isPlacementValid = EvaluatePlacementValidity(position, rotation);
            UpdatePreviewTint();
        }

        bool TryGetPlacementPosition(Ray ray, out Vector3 position)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, 12f, ~0, QueryTriggerInteraction.Ignore))
            {
                position = hit.point;
                return hit.normal.y >= 0.6f;
            }

            var plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float distance))
            {
                position = ray.GetPoint(distance);
                return true;
            }

            position = Vector3.zero;
            return false;
        }

        bool EvaluatePlacementValidity(Vector3 position, Quaternion rotation)
        {
            Vector3 center = position + Vector3.up * (_previewExtents.y + 0.02f);
            Vector3 extents = new Vector3(_previewExtents.x * 0.95f, Mathf.Max(0.08f, _previewExtents.y - 0.04f), _previewExtents.z * 0.95f);
            var hits = Physics.OverlapBox(center, extents, rotation, ~0, QueryTriggerInteraction.Ignore);
            foreach (var hit in hits)
            {
                if (hit == null) continue;
                if (_previewRoot != null && hit.transform.IsChildOf(_previewRoot.transform)) continue;
                if (hit.bounds.max.y <= 0.15f) continue;
                if (hit.name.Contains("Boden")) continue;
                return false;
            }
            return true;
        }

        Bounds CalculateBounds()
        {
            if (_previewRenderers == null || _previewRenderers.Length == 0)
                return new Bounds(_previewRoot.transform.position, Vector3.one);

            var bounds = _previewRenderers[0].bounds;
            for (int i = 1; i < _previewRenderers.Length; i++)
                bounds.Encapsulate(_previewRenderers[i].bounds);
            return bounds;
        }

        void UpdatePreviewTint()
        {
            Color tint = _isPlacementValid
                ? new Color(0.4f, 1f, 0.45f, 0.45f)
                : new Color(1f, 0.35f, 0.35f, 0.45f);
            for (int i = 0; i < _previewRenderers.Length; i++)
            {
                var renderer = _previewRenderers[i];
                if (renderer == null) continue;
                Color baseColor = _previewBaseColors != null && i < _previewBaseColors.Length
                    ? _previewBaseColors[i]
                    : renderer.material.color;
                renderer.material.color = Color.Lerp(baseColor, tint, 0.6f);
            }
        }

        static void SetMaterialMode(Material material, bool transparent)
        {
            if (material == null) return;
            if (transparent)
            {
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
            else
            {
                material.SetFloat("_Mode", 0f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                var color = material.color;
                material.color = new Color(color.r, color.g, color.b, 1f);
            }
        }
    }
}
