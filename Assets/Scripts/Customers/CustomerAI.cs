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

    public enum CustomerProfile
    {
        Regular, Hurried, Loyal, BargainHunter, Senior
    }

    /// <summary>
    /// Einfache Wegpunkt-Kunden-KI: betreten, Produkte holen, anstellen,
    /// bezahlen, verlassen. Geduld sinkt beim Warten.
    /// </summary>
    public class CustomerAI : MonoBehaviour
    {
        public CustomerIntent Intent = CustomerIntent.Shopping;
        public CustomerState State = CustomerState.Entering;
        public CustomerProfile Profile { get; private set; }
        public string ProfileLabel { get; private set; }
        public int Age;
        public float MoveSpeed = 2.2f;
        public float MaxPatience = 60f;

        public readonly List<ProductData> Basket = new List<ProductData>();
        public string PackageCode;   // fuer Paketabholung
        public float Patience { get; private set; }
        public bool HasArrived { get; private set; }
        public bool IsBeingServed { get; set; }

        readonly List<ProductData> _wishlist = new List<ProductData>();
        int _wishIndex;
        Vector3 _target;
        Vector3 _entrancePos;
        ShelfSlot _targetSlot;
        float _actionTimer;
        float _animTime;

        Transform _body;
        Transform _head;
        Vector3 _bodyBaseLocalPos;
        Vector3 _headBaseLocalPos;
        TextMesh _statusText;

        public void Init(CustomerIntent intent, Vector3 entrance, Vector3 shopCenter)
        {
            Intent = intent;
            _entrancePos = entrance;
            Age = Random.Range(14, 80);

            PickProfile();
            ApplyProfileStats();

            float patienceMult = 1f;
            var up = Upgrades.UpgradeManager.Instance;
            if (up != null) patienceMult += up.GetEffectValue(Upgrades.UpgradeEffect.Patience);
            Patience = MaxPatience * patienceMult;

            if (intent == CustomerIntent.Shopping)
                BuildWishlist();
            State = CustomerState.Entering;
            _target = shopCenter;

            CacheVisuals();
            ApplyProfileLook();
            EnsureStatusDisplay();
            UpdateStatusDisplay();
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
                    break;
                case CustomerState.Leaving:
                    if (MoveTowards(_target)) State = CustomerState.Despawn;
                    break;
                case CustomerState.Despawn:
                    if (CustomerSpawner.Instance != null) CustomerSpawner.Instance.NotifyDespawn(this);
                    Destroy(gameObject);
                    break;
            }

            UpdateAnimation();
            UpdateStatusDisplay();
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
                    _target = new Vector3(slot.transform.position.x, transform.position.y, slot.transform.position.z + 1.0f);
                    State = CustomerState.WalkingToShelf;
                    return;
                }
                _wishIndex++;
                if (Economy.ReputationManager.Instance != null)
                    Economy.ReputationManager.Instance.Add(-0.5f);
            }
            if (Basket.Count > 0) GoToQueue();
            else LeaveShop(false);
        }

        void TakeProduct()
        {
            bool hasWish = _wishIndex < _wishlist.Count;
            ProductData wantedProduct = hasWish ? _wishlist[_wishIndex] : null;
            bool correctSlot = _targetSlot != null && wantedProduct != null && _targetSlot.Product == wantedProduct;
            if (correctSlot && _targetSlot.TakeUnit())
                Basket.Add(wantedProduct);
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

            Vector3 pos = queue.GetPositionFor(this);
            if (!MoveTowards(pos)) return;

            if (IsBeingServed) { State = CustomerState.BeingServed; return; }

            Patience -= Time.deltaTime;
            if (Patience <= 0f) AbandonQueue();
        }

        void AbandonQueue()
        {
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

        void PickProfile()
        {
            float roll = Random.value;
            if (roll < 0.18f) Profile = CustomerProfile.Hurried;
            else if (roll < 0.36f) Profile = CustomerProfile.Loyal;
            else if (roll < 0.56f) Profile = CustomerProfile.BargainHunter;
            else if (roll < 0.72f) Profile = CustomerProfile.Senior;
            else Profile = CustomerProfile.Regular;
        }

        void ApplyProfileStats()
        {
            switch (Profile)
            {
                case CustomerProfile.Hurried:
                    ProfileLabel = "Eilig";
                    MoveSpeed = 2.9f;
                    MaxPatience = 34f;
                    break;
                case CustomerProfile.Loyal:
                    ProfileLabel = "Stammkunde";
                    MoveSpeed = 2.15f;
                    MaxPatience = 82f;
                    break;
                case CustomerProfile.BargainHunter:
                    ProfileLabel = "Sparfuchs";
                    MoveSpeed = 2.1f;
                    MaxPatience = 68f;
                    break;
                case CustomerProfile.Senior:
                    ProfileLabel = "Senior";
                    MoveSpeed = 1.55f;
                    MaxPatience = 88f;
                    break;
                default:
                    ProfileLabel = "Standard";
                    MoveSpeed = 2.2f;
                    MaxPatience = 60f;
                    break;
            }
        }

        void BuildWishlist()
        {
            int desired = 2;
            switch (Profile)
            {
                case CustomerProfile.Hurried: desired = 1; break;
                case CustomerProfile.Loyal: desired = 3; break;
                case CustomerProfile.BargainHunter: desired = 2; break;
                case CustomerProfile.Senior: desired = 2; break;
            }

            _wishlist.Clear();
            _wishlist.AddRange(CustomerNeeds.PickDesiredProducts(desired));
            while (_wishlist.Count < desired)
            {
                var bonus = CustomerNeeds.PickBonusProduct(_wishlist);
                if (bonus == null) break;
                _wishlist.Add(bonus);
            }

            if (Profile == CustomerProfile.BargainHunter)
                _wishlist.Sort((a, b) => a.SellPrice.CompareTo(b.SellPrice));
        }

        void CacheVisuals()
        {
            _body = transform.Find("Koerper");
            _head = transform.Find("Kopf");
            if (_body != null) _bodyBaseLocalPos = _body.localPosition;
            if (_head != null) _headBaseLocalPos = _head.localPosition;
        }

        void ApplyProfileLook()
        {
            Color bodyColor = new Color(0.55f, 0.7f, 0.95f);
            float scale = 1f;
            switch (Profile)
            {
                case CustomerProfile.Hurried: bodyColor = new Color(0.95f, 0.5f, 0.3f); scale = 1.02f; break;
                case CustomerProfile.Loyal: bodyColor = new Color(0.3f, 0.72f, 0.42f); scale = 1.08f; break;
                case CustomerProfile.BargainHunter: bodyColor = new Color(0.95f, 0.82f, 0.25f); scale = 0.98f; break;
                case CustomerProfile.Senior: bodyColor = new Color(0.7f, 0.64f, 0.85f); scale = 0.92f; break;
            }

            transform.localScale = Vector3.one * scale;
            if (_body != null)
            {
                var renderer = _body.GetComponent<Renderer>();
                if (renderer != null) renderer.material.color = bodyColor;
            }
        }

        void EnsureStatusDisplay()
        {
            var go = new GameObject("Status");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 2.55f, 0f);
            _statusText = go.AddComponent<TextMesh>();
            _statusText.fontSize = 40;
            _statusText.characterSize = 0.045f;
            _statusText.anchor = TextAnchor.MiddleCenter;
            _statusText.alignment = TextAlignment.Center;
            _statusText.color = Color.white;
        }

        void UpdateStatusDisplay()
        {
            if (_statusText == null) return;

            var camera = Camera.main;
            if (camera != null)
            {
                _statusText.transform.rotation = Quaternion.LookRotation(_statusText.transform.position - camera.transform.position);
                _statusText.transform.Rotate(0f, 180f, 0f);
            }

            float patience01 = MaxPatience > 0.01f ? Mathf.Clamp01(Patience / MaxPatience) : 0f;
            string stateLabel = State == CustomerState.BeingServed ? "Bestellt"
                : State == CustomerState.Waiting ? "Wartet"
                : State == CustomerState.WalkingToQueue ? "Zur Kasse"
                : State == CustomerState.WalkingToShelf ? "Sucht Ware"
                : State == CustomerState.Leaving ? "Geht"
                : "Im Laden";

            string wishLabel = "";
            if (Intent == CustomerIntent.Shopping && _wishIndex < _wishlist.Count)
                wishLabel = "\nWunsch: " + _wishlist[_wishIndex].DisplayName;
            else if (Intent == CustomerIntent.PackagePickup && !string.IsNullOrEmpty(PackageCode))
                wishLabel = "\nCode: " + PackageCode;
            else if (Intent == CustomerIntent.PackageDropoff)
                wishLabel = "\nWunsch: Paket abgeben";
            else if (Intent == CustomerIntent.Lotto)
                wishLabel = "\nWunsch: Lotto";

            _statusText.text = ProfileLabel + "\n" + stateLabel + "  " + Mathf.CeilToInt(Mathf.Max(0f, Patience)) + "s" + wishLabel;
            _statusText.color = Color.Lerp(new Color(1f, 0.35f, 0.35f), new Color(0.55f, 1f, 0.65f), patience01);
        }

        void UpdateAnimation()
        {
            _animTime += Time.deltaTime * Mathf.Max(1f, MoveSpeed);
            bool walking = State == CustomerState.Entering || State == CustomerState.WalkingToShelf
                || State == CustomerState.WalkingToQueue || State == CustomerState.Leaving;

            if (_body != null)
            {
                Vector3 bodyOffset = Vector3.zero;
                if (walking)
                    bodyOffset = new Vector3(0f, Mathf.Abs(Mathf.Sin(_animTime * 6f)) * 0.07f, 0f);
                else if (State == CustomerState.Waiting)
                    bodyOffset = new Vector3(Mathf.Sin(_animTime * 1.8f) * 0.03f, 0f, 0f);
                _body.localPosition = Vector3.Lerp(_body.localPosition, _bodyBaseLocalPos + bodyOffset, Time.deltaTime * 8f);
            }

            if (_head != null)
            {
                Vector3 headOffset = Vector3.zero;
                float headTilt = 0f;
                if (walking)
                    headOffset = new Vector3(0f, Mathf.Abs(Mathf.Cos(_animTime * 6f)) * 0.03f, 0f);
                else if (State == CustomerState.BeingServed)
                    headTilt = Mathf.Sin(_animTime * 10f) * 10f;
                else if (State == CustomerState.Waiting)
                    headTilt = Mathf.Sin(_animTime * 2f) * 4f;
                _head.localPosition = Vector3.Lerp(_head.localPosition, _headBaseLocalPos + headOffset, Time.deltaTime * 8f);
                _head.localRotation = Quaternion.Lerp(_head.localRotation, Quaternion.Euler(0f, 0f, headTilt), Time.deltaTime * 8f);
            }
        }
    }
}
