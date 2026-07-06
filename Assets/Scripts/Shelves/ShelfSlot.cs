using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kiosk.Products;

namespace Kiosk.Shelves
{
    /// <summary>
    /// Einzelner Regalplatz: ein Produkttyp, begrenzte Kapazitaet, zeigt
    /// Platzhaltermodelle der Produkte an.
    /// </summary>
    public class ShelfSlot : MonoBehaviour
    {
        public ProductData Product;
        public int BaseCapacity = 8;
        public int Count { get; private set; }

        static int _bonusCapacity;
        public static void SetGlobalBonusCapacity(int bonus) { _bonusCapacity = bonus; }
        public static int GlobalBonusCapacity { get { return _bonusCapacity; } }

        public int Capacity { get { return BaseCapacity + _bonusCapacity; } }

        readonly List<GameObject> _visuals = new List<GameObject>();
        readonly List<Vector3> _visualTargetScales = new List<Vector3>();
        Coroutine _refreshRoutine;

        public void Assign(ProductData product)
        {
            if (Count == 0 || Product == null || Product == product)
                Product = product;
        }

        public string Summary
        {
            get
            {
                if (Product == null || Count <= 0) return "leer";
                return Product.DisplayName + " (" + Count + "/" + Capacity + ")";
            }
        }

        public void AddUnits(int amount)
        {
            int oldCount = Count;
            Count = Mathf.Min(Count + amount, Capacity);
            RefreshVisuals();
            if (Count > oldCount)
            {
                if (_refreshRoutine != null) StopCoroutine(_refreshRoutine);
                _refreshRoutine = StartCoroutine(AnimateVisuals());
            }
        }

        /// <summary>Entnimmt eine Einheit (Kunde oder Spieler). Gibt true bei Erfolg.</summary>
        public bool TakeUnit()
        {
            if (Count <= 0 || Product == null) return false;
            Count--;
            if (Count == 0) Product = null;
            RefreshVisuals();
            return true;
        }

        public void SetState(ProductData product, int count)
        {
            Product = product;
            Count = Mathf.Clamp(count, 0, Capacity);
            if (Count == 0) Product = null;
            RefreshVisuals();
        }

        public void RefreshVisuals()
        {
            foreach (var v in _visuals) if (v != null) Destroy(v);
            _visuals.Clear();
            _visualTargetScales.Clear();
            if (Product == null || Count <= 0) return;

            int shown = Mathf.Min(Count, 6);
            for (int i = 0; i < shown; i++)
            {
                var model = Core.ProceduralAssetGenerator.CreateProductModel(Product);
                model.transform.SetParent(transform, false);
                int row = i / 3;
                int col = i % 3;
                model.transform.localPosition = new Vector3(-0.14f + col * 0.14f, 0.02f, -0.06f + row * 0.14f);
                _visualTargetScales.Add(model.transform.localScale);
                _visuals.Add(model);
            }
        }

        IEnumerator AnimateVisuals()
        {
            float elapsed = 0f;
            foreach (var visual in _visuals)
                if (visual != null)
                    visual.transform.localScale = Vector3.zero;

            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 0.2f);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                for (int i = 0; i < _visuals.Count; i++)
                    if (_visuals[i] != null)
                        _visuals[i].transform.localScale = Vector3.LerpUnclamped(Vector3.zero, _visualTargetScales[i] * 1.08f, eased);
                yield return null;
            }

            for (int i = 0; i < _visuals.Count; i++)
                if (_visuals[i] != null)
                    _visuals[i].transform.localScale = _visualTargetScales[i];
            _refreshRoutine = null;
        }
    }
}
