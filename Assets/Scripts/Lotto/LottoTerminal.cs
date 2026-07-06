using UnityEngine;
using Kiosk.Interaction;

namespace Kiosk.Lotto
{
    /// <summary>
    /// Fiktives Lotto-Terminal. Verkauft Scheintypen an Lotto-Kunden und gibt
    /// dem Spieler Provision. Kleine Zufallsgewinne erhoehen die Kundenzufriedenheit.
    /// </summary>
    public class LottoTerminal : Interactable
    {
        public static LottoTerminal Instance { get; private set; }

        void Awake()
        {
            if (Instance == null) Instance = this;
            Prompt = "Lotto-Terminal (Kunden an der Kasse bedienen)";
        }

        public bool IsUnlocked
        {
            get
            {
                var up = Upgrades.UpgradeManager.Instance;
                return up != null && up.IsPurchased("lotto_terminal");
            }
        }

        public override string GetPrompt()
        {
            return IsUnlocked
                ? "Lotto-Terminal aktiv (fiktiv)"
                : "Lotto-Terminal (Upgrade noetig)";
        }

        public override void Interact(Player.PlayerInteractor interactor)
        {
            var ui = UI.UIManager.Instance;
            if (ui == null) return;
            ui.ShowToast(IsUnlocked
                ? "Lotto-Kunden kaufen Scheine an der Kasse."
                : "Kaufe das Upgrade 'Lotto-Terminal' im Tablet.");
        }

        /// <summary>Verkauf eines fiktiven Scheins an einen Kunden. Gibt true bei Gewinn des Kunden.</summary>
        public bool SellTicket(LottoTicketData ticket)
        {
            if (ticket == null) return false;
            float commission = ticket.Commission;
            var up = Upgrades.UpgradeManager.Instance;
            if (up != null) commission *= 1f + up.GetEffectValue(Upgrades.UpgradeEffect.LottoCommission);

            var eco = Economy.EconomyManager.Instance;
            if (eco != null) eco.AddRevenue(commission);
            if (Core.GameManager.Instance != null) Core.GameManager.Instance.AddXP(4);
            if (Audio.AudioManager.Instance != null)
                Audio.AudioManager.Instance.Play(Audio.SoundId.Lotto);

            bool customerWins = Random.value < ticket.WinChance;
            if (customerWins && Economy.ReputationManager.Instance != null)
                Economy.ReputationManager.Instance.Add(3f);
            return customerWins;
        }
    }
}
