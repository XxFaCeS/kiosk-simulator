using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Kiosk.Products;

namespace Kiosk.Core
{
    /// <summary>
    /// Erzeugt alle Platzhalter-Assets zur Laufzeit: Materialien mit prozeduralen
    /// Texturen, Icons, Primitive-Modelle und UI-Bausteine.
    /// Hochwertige Assets koennen spaeter die hier erzeugten Platzhalter ersetzen:
    /// einfach Prefabs/Materialien mit denselben Namen in Assets/ ablegen und in
    /// SceneBootstrapper referenzieren.
    /// </summary>
    public static class ProceduralAssetGenerator
    {
        static readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        static readonly Dictionary<string, Sprite> _icons = new Dictionary<string, Sprite>();
        static Font _font;

        public static void ClearCache()
        {
            _materials.Clear();
            _icons.Clear();
        }

        // ---------- Materialien ----------

        public static Material GetMaterial(string name)
        {
            Material mat;
            if (_materials.TryGetValue(name, out mat) && mat != null) return mat;
            mat = BuildMaterial(name);
            _materials[name] = mat;
            return mat;
        }

        static Material BuildMaterial(string name)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Diffuse");
            var mat = new Material(shader) { name = name };
            switch (name)
            {
                case "Material_Boden":
                    mat.mainTexture = CheckerTexture(new Color(0.44f, 0.39f, 0.34f), new Color(0.33f, 0.3f, 0.27f), 10);
                    break;
                case "Material_Wand":
                    mat.mainTexture = NoiseTexture(new Color(0.88f, 0.85f, 0.76f), 0.035f);
                    break;
                case "Material_Decke":
                    mat.color = new Color(0.96f, 0.95f, 0.92f);
                    break;
                case "Material_Regal":
                    mat.mainTexture = NoiseTexture(new Color(0.56f, 0.36f, 0.2f), 0.05f);
                    break;
                case "Material_Kasse":
                    mat.color = new Color(0.18f, 0.22f, 0.28f);
                    break;
                case "Material_Kuehlschrank":
                    mat.mainTexture = StripeTexture(new Color(0.76f, 0.88f, 0.95f), new Color(0.62f, 0.78f, 0.9f));
                    mat.SetFloat("_Glossiness", 0.7f);
                    break;
                case "Material_Produkt_Getraenk":
                    mat.mainTexture = StripeTexture(new Color(0.2f, 0.5f, 0.9f), Color.white);
                    break;
                case "Material_Produkt_Snack":
                    mat.mainTexture = StripeTexture(new Color(0.9f, 0.6f, 0.2f), new Color(0.95f, 0.9f, 0.7f));
                    break;
                case "Material_Produkt_Zeitung":
                    mat.mainTexture = NoiseTexture(new Color(0.9f, 0.9f, 0.88f), 0.15f);
                    break;
                case "Material_Produkt_Tabak_Fiktiv":
                    mat.mainTexture = StripeTexture(new Color(0.5f, 0.2f, 0.15f), new Color(0.8f, 0.7f, 0.5f));
                    break;
                case "Material_Paket":
                    mat.mainTexture = NoiseTexture(new Color(0.76f, 0.59f, 0.35f), 0.08f);
                    break;
                case "Material_LottoTerminal":
                    mat.color = new Color(0.92f, 0.76f, 0.12f);
                    break;
                case "Material_Kunde":
                    mat.color = new Color(Random.Range(0.35f, 0.9f), Random.Range(0.35f, 0.9f), Random.Range(0.35f, 0.9f));
                    break;
                case "Material_Theke":
                    mat.mainTexture = NoiseTexture(new Color(0.38f, 0.25f, 0.16f), 0.05f);
                    break;
                default:
                    mat.color = Color.magenta;
                    break;
            }
            return mat;
        }

        public static Material GetProductMaterial(ProductData product)
        {
            var baseMat = GetMaterial(product.MaterialName);
            var mat = new Material(baseMat) { name = product.MaterialName + "_" + product.Id };
            mat.color = product.ProductColor;
            return mat;
        }

        // ---------- Texturen ----------

        public static Texture2D CheckerTexture(Color a, Color b, int cells)
        {
            int size = 128;
            var tex = new Texture2D(size, size) { wrapMode = TextureWrapMode.Repeat };
            int cell = size / cells;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, ((x / cell + y / cell) % 2 == 0) ? a : b);
            tex.Apply();
            return tex;
        }

        public static Texture2D NoiseTexture(Color baseColor, float amount)
        {
            int size = 64;
            var tex = new Texture2D(size, size) { wrapMode = TextureWrapMode.Repeat };
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float n = (Mathf.PerlinNoise(x * 0.2f, y * 0.2f) - 0.5f) * 2f * amount;
                    tex.SetPixel(x, y, new Color(baseColor.r + n, baseColor.g + n, baseColor.b + n));
                }
            tex.Apply();
            return tex;
        }

        public static Texture2D StripeTexture(Color a, Color b)
        {
            int size = 64;
            var tex = new Texture2D(size, size) { wrapMode = TextureWrapMode.Repeat };
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, (y / 8) % 2 == 0 ? a : b);
            tex.Apply();
            return tex;
        }

        // ---------- Icons ----------

        public static Sprite GetIcon(string name)
        {
            Sprite icon;
            if (_icons.TryGetValue(name, out icon) && icon != null) return icon;
            icon = BuildIcon(name);
            _icons[name] = icon;
            return icon;
        }

        static Sprite BuildIcon(string name)
        {
            Color color;
            switch (name)
            {
                case "Texture_Icon_Geld": color = new Color(0.95f, 0.8f, 0.2f); break;
                case "Texture_Icon_Bestand": color = new Color(0.4f, 0.6f, 0.9f); break;
                case "Texture_Icon_Bestellung": color = new Color(0.3f, 0.8f, 0.4f); break;
                case "Texture_Icon_Upgrade": color = new Color(0.8f, 0.4f, 0.9f); break;
                case "Texture_Icon_Paket": color = new Color(0.7f, 0.5f, 0.3f); break;
                case "Texture_Icon_Lotto": color = new Color(0.95f, 0.65f, 0.1f); break;
                case "Texture_Icon_Alter": color = new Color(0.9f, 0.25f, 0.25f); break;
                default: color = Color.gray; break;
            }
            int size = 32;
            var tex = new Texture2D(size, size);
            Vector2 c = new Vector2(size / 2f, size / 2f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    tex.SetPixel(x, y, d < size * 0.42f ? color : Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        // ---------- 3D-Modelle ----------

        public static GameObject CreatePrimitive(PrimitiveType type, string name, Vector3 scale, string materialName)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.material = GetMaterial(materialName);
            return go;
        }

        /// <summary>Produkt-Platzhaltermodell aus Primitives (ohne Collider).</summary>
        public static GameObject CreateProductModel(ProductData product)
        {
            GameObject go;
            switch (product.ModelType)
            {
                case ProductModelType.Can:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    go.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
                    break;
                case ProductModelType.Bottle:
                    go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    go.transform.localScale = new Vector3(0.06f, 0.09f, 0.06f);
                    break;
                case ProductModelType.Pack:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.localScale = new Vector3(0.1f, 0.14f, 0.05f);
                    break;
                case ProductModelType.Flat:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.localScale = new Vector3(0.12f, 0.02f, 0.09f);
                    break;
                default:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
                    break;
            }
            go.name = "Produkt_" + product.Id;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.material = GetProductMaterial(product);
            go.AddComponent<ProductInstance>().Bind(product);
            return go;
        }

        /// <summary>Einfaches Kundenmodell aus Primitives (Koerper + Kopf).</summary>
        public static GameObject CreateCustomerModel()
        {
            var root = new GameObject("Kunde");
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Koerper";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
            Object.Destroy(body.GetComponent<Collider>());
            var bodyMat = new Material(GetMaterial("Material_Kunde"));
            bodyMat.color = new Color(Random.Range(0.25f, 0.9f), Random.Range(0.25f, 0.9f), Random.Range(0.25f, 0.9f));
            body.GetComponent<Renderer>().material = bodyMat;

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Kopf";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.95f, 0f);
            head.transform.localScale = Vector3.one * 0.42f;
            Object.Destroy(head.GetComponent<Collider>());
            var headMat = new Material(GetMaterial("Material_Kunde"));
            headMat.color = new Color(0.9f, 0.75f, 0.6f);
            head.GetComponent<Renderer>().material = headMat;
            return root;
        }

        /// <summary>Lieferkarton-Platzhalter mit Collider.</summary>
        public static GameObject CreateDeliveryBoxModel()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Lieferkarton";
            go.transform.localScale = new Vector3(0.6f, 0.45f, 0.6f);
            go.transform.position += Vector3.up * 0.25f;
            go.GetComponent<Renderer>().material = GetMaterial("Material_Paket");
            return go;
        }

        // ---------- UI-Bausteine ----------

        public static Font GetFont()
        {
            if (_font != null) return _font;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return _font;
        }

        public static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return go;
        }

        public static Text CreateText(Transform parent, string name, string content, int size, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var text = go.GetComponent<Text>();
            text.text = content;
            text.font = GetFont();
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = anchor;
            text.raycastTarget = false;
            return text;
        }

        public static Button CreateButton(Transform parent, string name, string label, System.Action onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.22f, 0.35f, 0.55f, 0.95f);
            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.96f);
            colors.pressedColor = new Color(0.9f, 0.95f, 1f, 0.92f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.55f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            if (onClick != null)
                button.onClick.AddListener(() =>
                {
                    if (Audio.AudioManager.Instance != null) Audio.AudioManager.Instance.Play(Audio.SoundId.Click);
                    onClick();
                });
            var text = CreateText(go.transform, "Label", label, 16, TextAnchor.MiddleCenter);
            text.color = Color.white;
            go.AddComponent<ButtonAnimator>();
            return button;
        }

        public static EventSystem EnsureEventSystem()
        {
            EventSystem primary = null;
            foreach (var eventSystem in Object.FindObjectsOfType<EventSystem>())
            {
                if (eventSystem == null) continue;
                if (primary == null)
                {
                    primary = eventSystem;
                    if (primary.transform.parent != null)
                        primary.transform.SetParent(null, false);
                    if (primary.GetComponent<StandaloneInputModule>() == null)
                        primary.gameObject.AddComponent<StandaloneInputModule>();
                }
                else
                {
                    Object.Destroy(eventSystem.gameObject);
                }
            }

            if (primary != null) return primary;
            return new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).GetComponent<EventSystem>();
        }
    }

    public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        Vector3 _baseScale = Vector3.one;
        Image _image;
        Color _baseColor;
        bool _hovered;

        void Awake()
        {
            _baseScale = transform.localScale;
            _image = GetComponent<Image>();
            if (_image != null) _baseColor = _image.color;
        }

        void OnEnable()
        {
            _hovered = false;
            transform.localScale = _baseScale;
            if (_image != null) _image.color = _baseColor;
        }

        void Update()
        {
            var targetScale = _hovered ? _baseScale * 1.04f : _baseScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * 14f);
            if (_image != null)
            {
                var targetColor = _hovered ? _baseColor * 1.1f : _baseColor;
                targetColor.a = _baseColor.a;
                _image.color = Color.Lerp(_image.color, targetColor, Time.unscaledDeltaTime * 14f);
            }
        }

        public void OnPointerEnter(PointerEventData eventData) { _hovered = true; }
        public void OnPointerExit(PointerEventData eventData) { _hovered = false; }

        public void OnPointerDown(PointerEventData eventData)
        {
            transform.localScale = _baseScale * 0.96f;
            if (_image != null) _image.color = _baseColor * 0.9f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.localScale = _hovered ? _baseScale * 1.04f : _baseScale;
            if (_image != null) _image.color = _hovered ? _baseColor * 1.1f : _baseColor;
        }
    }
}
