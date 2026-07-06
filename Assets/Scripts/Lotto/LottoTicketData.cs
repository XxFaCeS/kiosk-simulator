using UnityEngine;

namespace Kiosk.Lotto
{
    /// <summary>
    /// Fiktiver Lotto-Scheintyp. Kein echtes Gluecksspiel, kein echtes Geld,
    /// keine echten Marken - reines Spielsystem.
    /// </summary>
    [CreateAssetMenu(fileName = "LottoTicket", menuName = "Kiosk/LottoTicket")]
    public class LottoTicketData : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public float Price;
        public float Commission;
        [Range(0f, 1f)] public float WinChance;
        public float MaxWin;

        public static LottoTicketData Create(string id, string name, float price,
            float commission, float winChance, float maxWin)
        {
            var t = CreateInstance<LottoTicketData>();
            t.name = id;
            t.Id = id;
            t.DisplayName = name;
            t.Price = price;
            t.Commission = commission;
            t.WinChance = winChance;
            t.MaxWin = maxWin;
            return t;
        }
    }
}
