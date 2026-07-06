using UnityEngine;

namespace Kiosk.Products
{
    /// <summary>
    /// Physische Instanz eines Produkts in der Welt (im Regal, in Kundenhand usw.).
    /// </summary>
    public class ProductInstance : MonoBehaviour
    {
        public ProductData Data;

        public void Bind(ProductData data)
        {
            Data = data;
        }
    }
}
