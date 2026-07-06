using UnityEngine;

namespace Kiosk.Products
{
    public enum ProductCategory
    {
        Wasser, Softdrinks, Energydrinks, Kaffee, Chips, Schokolade, Bonbons, Kaugummi,
        Eis, Zeitungen, Magazine, TabakFiktiv, EZigarettenFiktiv, Feuerzeuge,
        LottoScheinFiktiv, RubbellosFiktiv, PrepaidKarten, HandyZubehoer, Batterien,
        Hygieneartikel, Paketmarken, Geschenkartikel, Saisonartikel
    }

    public enum ProductModelType { Box, Can, Bottle, Pack, Flat }

    /// <summary>
    /// Datendefinition eines Produkts. Wird zur Laufzeit aus DefaultGameData erzeugt
    /// oder per Editor-Tool als .asset gespeichert.
    /// </summary>
    [CreateAssetMenu(fileName = "Product", menuName = "Kiosk/Product")]
    public class ProductData : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public ProductCategory Category;
        public float BuyPrice;
        public float SellPrice;
        public int StorageSize = 1;
        public int ShelfSize = 1;
        [Range(0f, 1f)] public float Demand = 0.5f;
        public int UnlockLevel = 1;
        public bool AgeRestricted;
        public bool IsLottoProduct;
        public ProductModelType ModelType = ProductModelType.Box;
        public string MaterialName = "Material_Produkt_Snack";
        public Color ProductColor = Color.white;
        public Sprite Icon;

        public static ProductData Create(string id, string name, ProductCategory cat,
            float buy, float sell, float demand, int unlockLevel, bool age, bool lotto,
            ProductModelType model, string material, Color color)
        {
            var p = CreateInstance<ProductData>();
            p.name = id;
            p.Id = id;
            p.DisplayName = name;
            p.Category = cat;
            p.BuyPrice = buy;
            p.SellPrice = sell;
            p.Demand = demand;
            p.UnlockLevel = unlockLevel;
            p.AgeRestricted = age;
            p.IsLottoProduct = lotto;
            p.ModelType = model;
            p.MaterialName = material;
            p.ProductColor = color;
            return p;
        }
    }
}
