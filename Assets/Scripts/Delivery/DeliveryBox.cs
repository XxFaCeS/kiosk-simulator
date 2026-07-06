using System.Collections.Generic;
using UnityEngine;
using Kiosk.Interaction;
using Kiosk.Orders;

namespace Kiosk.Delivery
{
    /// <summary>
    /// Lieferkarton in der Welt. Interaktion packt den Inhalt ins Lager.
    /// </summary>
    public class DeliveryBox : Interactable
    {
        readonly List<OrderLine> _contents = new List<OrderLine>();

        public void SetContents(List<OrderLine> lines)
        {
            _contents.Clear();
            if (lines != null) _contents.AddRange(lines);
            Prompt = "[E] Karton auspacken (" + TotalUnits() + " Artikel)";
        }

        int TotalUnits()
        {
            int t = 0;
            foreach (var l in _contents) t += l.Amount;
            return t;
        }

        public override void Interact(Player.PlayerInteractor interactor)
        {
            var inv = Inventory.InventoryManager.Instance;
            if (inv == null) return;
            int moved = 0;
            for (int i = _contents.Count - 1; i >= 0; i--)
            {
                if (inv.Add(_contents[i].ProductId, _contents[i].Amount))
                {
                    moved += _contents[i].Amount;
                    _contents.RemoveAt(i);
                }
            }
            var ui = UI.UIManager.Instance;
            if (moved > 0)
            {
                if (ui != null) ui.ShowToast(moved + " Artikel ins Lager gepackt.");
                if (Audio.AudioManager.Instance != null)
                    Audio.AudioManager.Instance.Play(Audio.SoundId.PackageScan);
                if (Core.GameManager.Instance != null) Core.GameManager.Instance.AddXP(3);
            }
            else if (ui != null)
            {
                ui.ShowToast("Lager voll! Zuerst Regale auffuellen.");
            }
            if (_contents.Count == 0) Destroy(gameObject);
            else Prompt = "[E] Karton auspacken (" + TotalUnits() + " Artikel)";
        }
    }
}
