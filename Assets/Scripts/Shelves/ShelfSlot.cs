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

        public void Assign(ProductData product)
        {
            Product = product;
        }

        public void AddUnits(int amount)
        {
            Count = Mathf.Min(Count + amount, Capacity);
            RefreshVisuals();
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
            if (Product == null || Count <= 0) return;

            int shown = Mathf.Min(Count, 6);
            for (int i = 0; i < shown; i++)
            {
                var model = Core.ProceduralAssetGenerator.CreateProductModel(Product);
                model.transform.SetParent(transform, false);
                int row = i / 3;
                int col = i % 3;
                model.transform.localPosition = new Vector3(-0.14f + col * 0.14f, 0.02f, -0.06f + row * 0.14f);
                _visuals.Add(model);
            }
        }
    }
}
