using UnityEngine;
using Kiosk.Customers;

namespace Kiosk.Checkout
{
    public enum PaymentMethod { Cash, Card }

    /// <summary>
    /// Bezahlvorgang: bucht Umsatz, XP, Ruf und schliesst die Transaktion ab.
    /// </summary>
    public class PaymentSystem : MonoBehaviour
    {
        public static PaymentSystem Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool IsCardAvailable
        {
            get
            {
                var up = Upgrades.UpgradeManager.Instance;
                return up != null && up.IsPurchased("kartenzahlung");
            }
        }

        /// <summary>Verkauf abschliessen. Gibt true zurueck, wenn erfolgreich.</summary>
        public bool CompleteSale(CustomerAI customer, PaymentMethod method)
        {
            var register = CashRegister.Instance;
            if (register == null || !register.AllScanned || register.ScannedItems.Count == 0) return false;
            if (method == PaymentMethod.Card && !IsCardAvailable) return false;

            float total = register.Total;
            var eco = Economy.EconomyManager.Instance;
            if (eco != null)
            {
                eco.AddRevenue(total);
                eco.RegisterCustomerServed();
            }
            var gm = Core.GameManager.Instance;
            if (gm != null) gm.AddXP(5 + register.ScannedItems.Count * 2);
            if (Economy.ReputationManager.Instance != null)
                Economy.ReputationManager.Instance.Add(1f);
            if (UI.UIManager.Instance != null)
                UI.UIManager.Instance.NotifySale(total);
            if (CheckoutCounter.Instance != null)
                CheckoutCounter.Instance.ShowPaymentFeedback(total);

            var audio = Audio.AudioManager.Instance;
            if (audio != null)
                audio.Play(method == PaymentMethod.Cash ? Audio.SoundId.Coins : Audio.SoundId.CardPayment);

            register.Clear();
            return true;
        }
    }
}
