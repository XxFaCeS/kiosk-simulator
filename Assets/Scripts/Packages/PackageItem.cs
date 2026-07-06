using UnityEngine;

namespace Kiosk.Packages
{
    /// <summary>
    /// Ein angenommenes Paket mit Abholcode.
    /// </summary>
    [System.Serializable]
    public class PackageItem
    {
        public string Code;
        public string CustomerName;

        static readonly string[] Names =
        {
            "M. Vogel", "A. Brandt", "L. Sommer", "K. Winter", "T. Berger",
            "S. Falk", "J. Krause", "R. Lindner", "E. Hoffmann", "P. Steiner"
        };

        public static PackageItem CreateRandom()
        {
            return new PackageItem
            {
                Code = "PKT-" + Random.Range(100, 999),
                CustomerName = Names[Random.Range(0, Names.Length)]
            };
        }
    }
}
