using System.Collections.Generic;
using UnityEngine;
using Kiosk.Products;
using Kiosk.Shelves;

namespace Kiosk.Customers
{
    public enum CustomerIntent { Shopping, PackageDropoff, PackagePickup, Lotto }

    public enum CustomerState
    {
        Entering, FindingProduct, WalkingToShelf, TakingProduct,
        WalkingToQueue, Waiting, BeingServed, Leaving, Despawn
    }

    /// <summary>
    /// Einfache Wegpunkt-Kunden-KI: betreten, Produkte holen, anstellen,
    /// bezahlen, verlassen. Geduld sinkt beim Warten.
    /// </summary>
    public class CustomerAI : MonoBehaviour
    {
        public CustomerIntent Intent = CustomerIntent.Shopping;
        public CustomerState State = CustomerState.Entering;
        public int Age;
        public float MoveSpeed = 2.2f;
        public float MaxPatience = 60f;

        public readonly List<ProductData> Basket = new List<ProductData>();
        public string PackageCode;   // fuer Paketabholung
        public float Patience { get; private set; }
        public bool HasArrived { get; private set; }
        public bool IsBeingServed { get; set; }

        List<ProductData> _wishlist = new List<ProductData>();
        int _wishIndex;
        Vector3 _target;
        Vector3 _entrancePos;
        ShelfSlot _targetSlot;
        float _actionTimer;

        public void Init(CustomerIntent intent, Vector3 entrance, Vector3 shopCenter)
        {
            Intent = intent;
            _entrancePos = entrance;
            Age = Random.Range(14, 80);
            float patienceMult = 1f;
            var up = Upgrades.UpgradeManager.Instance;
            if (up != null) patienceMult += up.GetEffectValue(Upgrades.UpgradeEffect.Patience);
            Patience = MaxPatience * patienceMult;

            if (intent == CustomerIntent.Shopping)
                _wishlist = CustomerNeeds.PickDesiredProducts();
            State = CustomerState.Entering;
            _target = shopCenter;
        }

        void Update()
        {
            if (Core.GameManager.Instance != null && Core.GameManager.Instance.IsPaused) return;

            switch (State)
            {
                case CustomerState.Entering:
                    if (MoveTowards(_target)) NextGoal();
                    break;
                case CustomerState.FindingProduct:
                    FindNextProduct();
                    break;
                case CustomerState.WalkingToShelf:
                    if (MoveTowards(_target)) { State = CustomerState.TakingProduct; _actionTimer = 1.2f; }
                    break;
                case CustomerState.TakingProduct:
                    _actionTimer -= Time.deltaTime;
                    if (_actionTimer <= 0f) TakeProduct();
                    break;
                case CustomerState.WalkingToQueue:
                    UpdateQueueMovement();
                    break;
                case CustomerState.Waiting:
                    UpdateWaiting();
                    break;
                case CustomerState.BeingServed:
                    // Spieler bedient ueber CheckoutUI; Geduld pausiert.
                    break;
                case CustomerState.Leaving:
                    if (MoveTowards(_target)) State = CustomerState.Despawn;
                    break;
                case CustomerState.Despawn:
                    if (CustomerSpawner.Instance != null) CustomerSpawner.Instance.NotifyDespawn(this);
                    Destroy(gameObject);
                    break;
            }
        }

        bool MoveTowards(Vector3 target)
        {
            HasArrived = false;
            Vector3 flatTarget = new Vector3(target.x, transform.position.y, target.z);
            Vector3 delta = flatTarget - transform.position;
            if (delta.magnitude < 0.15f) { HasArrived = true; return true; }
            transform.position += delta.normalized * MoveSpeed * Time.deltaTime;
            if (delta.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(delta.normalized), Time.deltaTime * 8f);
            return false;
        }

        void NextGoal()
        {
            if (Intent == CustomerIntent.Shopping) State = CustomerState.FindingProduct;
            else GoToQueue();
        }

        void FindNextProduct()
        {
            while (_wishIndex < _wishlist.Count)
            {
                var wish = _wishlist[_wishIndex];
                var slot = Shelf.FindProductInAnyShelf(wish);
                if (slot != null)
                {
                    _targetSlot = slot;
                    _target = slot.transform.position + slot.transform.forward * 0f;
                    _target = new Vector3(slot.transform.position.x, transform.position.y, slot.transform.position.z + 1.0f);
                    State = CustomerState.WalkingToShelf;
                    return;
                }
                // Produkt nicht verfuegbar -> leicht unzufrieden
                _wishIndex++;
                if (Economy.ReputationManager.Instance != null)
                    Economy.ReputationManager.Instance.Add(-0.5f);
            }
            // Alle Wuensche geprueft
            if (Basket.Count > 0) GoToQueue();
            else LeaveShop(false);
        }

        void TakeProduct()
        {
            if (_targetSlot != null && _targetSlot.Product == _wishlist[_wishIndex] && _targetSlot.TakeUnit())
                Basket.Add(_wishlist[_wishIndex]);
            _wishIndex++;
            State = CustomerState.FindingProduct;
        }

        void GoToQueue()
        {
            var queue = CustomerQueue.Instance;
            if (queue == null) { LeaveShop(false); return; }
            queue.Join(this);
            State = CustomerState.WalkingToQueue;
        }

        void UpdateQueueMovement()
        {
            var queue = CustomerQueue.Instance;
            if (queue == null) { LeaveShop(false); return; }
            if (MoveTowards(queue.GetPositionFor(this)))
                State = CustomerState.Waiting;
        }

        void UpdateWaiting()
        {
            var queue = CustomerQueue.Instance;
            if (queue == null) { LeaveShop(false); return; }

            // Position in der Schlange nachruecken
            Vector3 pos = queue.GetPositionFor(this);
            if (!MoveTowards(pos)) return;

            if (IsBeingServed) { State = CustomerState.BeingServed; return; }

            Patience -= Time.deltaTime;
            if (Patience <= 0f) AbandonQueue();
        }

        void AbandonQueue()
        {
            // Unzufrieden gehen; evtl. Diebstahl der Korbware
            if (Economy.ReputationManager.Instance != null)
                Economy.ReputationManager.Instance.Add(-3f);
            if (Audio.AudioManager.Instance != null)
                Audio.AudioManager.Instance.Play(Audio.SoundId.CustomerUnhappy);

            if (Basket.Count > 0)
            {
                float theftChance = 0.35f;
                var up = Upgrades.UpgradeManager.Instance;
                if (up != null) theftChance *= 1f - Mathf.Clamp01(up.GetEffectValue(Upgrades.UpgradeEffect.TheftReduction));
                if (Random.value > theftChance)
                {
                    // Ware wird zurueckgelassen -> zurueck ins Lager
                    var inv = Inventory.InventoryManager.Instance;
                    if (inv != null)
                        foreach (var p in Basket) inv.Add(p.Id, 1);
                }
                else if (UI.UIManager.Instance != null)
                {
                    UI.UIManager.Instance.ShowToast("Ein Kunde hat Ware gestohlen!");
                }
                Basket.Clear();
            }
            LeaveShop(true);
        }

        /// <summary>Bedienung abgeschlossen (bezahlt oder abgelehnt).</summary>
        public void FinishService(bool happy)
        {
            IsBeingServed = false;
            Basket.Clear();
            if (Audio.AudioManager.Instance != null)
                Audio.AudioManager.Instance.Play(happy ? Audio.SoundId.CustomerHappy : Audio.SoundId.CustomerUnhappy);
            LeaveShop(!happy);
        }

        public void LeaveShop(bool unhappy)
        {
            var queue = CustomerQueue.Instance;
            if (queue != null) queue.Leave(this);
            _target = _entrancePos;
            State = CustomerState.Leaving;
        }
    }
}
