using UnityEngine;

namespace Kiosk.Orders
{
    /// <summary>
    /// Lieferantendaten: Liefergebuehr und Lieferzeit (in Spielminuten Echtzeit-Sekunden).
    /// </summary>
    [CreateAssetMenu(fileName = "Supplier", menuName = "Kiosk/Supplier")]
    public class SupplierData : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public float DeliveryFee;
        public float DeliveryTimeSeconds = 45f;

        public static SupplierData Create(string id, string name, float fee, float timeSeconds)
        {
            var s = CreateInstance<SupplierData>();
            s.name = id;
            s.Id = id;
            s.DisplayName = name;
            s.DeliveryFee = fee;
            s.DeliveryTimeSeconds = timeSeconds;
            return s;
        }
    }
}
