using UnityEngine;
using System.Collections.Generic;
using Kiosk.Interaction;
using Kiosk.Customers;
using Kiosk.Products;

namespace Kiosk.Checkout
{
    /// <summary>
    /// Kassentheke: Interaktion startet die Bedienung des vordersten Kunden.
    /// </summary>
    public class CheckoutCounter : Interactable
    {
        const float CounterItemsZStart = -0.65f;
        const float CounterItemsZRange = 1.3f;

        class CounterItemVisual
        {
            public ProductData Product;
            public Transform Transform;
            public Vector3 RestLocalPosition;
            public bool Scanned;
        }

        public static CheckoutCounter Instance { get; private set; }

        public CustomerAI CurrentCustomer { get; private set; }

        readonly List<CounterItemVisual> _counterItems = new List<CounterItemVisual>();
        CustomerAI _displayCustomer;
        Transform _itemsRoot;
        TextMesh _floatingMoneyText;
        float _floatingMoneyTimer;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            EnsureVisualRoots();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (_floatingMoneyText == null || _floatingMoneyTimer <= 0f) return;
            _floatingMoneyTimer -= Time.deltaTime;
            _floatingMoneyText.transform.position += Vector3.up * Time.deltaTime * 0.55f;
            var color = _floatingMoneyText.color;
            color.a = Mathf.Clamp01(_floatingMoneyTimer / 1f);
            _floatingMoneyText.color = color;
            if (_floatingMoneyTimer <= 0f)
            {
                Destroy(_floatingMoneyText.gameObject);
                _floatingMoneyText = null;
            }
        }

        public override string GetPrompt()
        {
            var queue = CustomerQueue.Instance;
            if (CurrentCustomer != null) return "[E] Kunde wird bedient...";
            if (queue != null && queue.Front != null && queue.IsFrontAndArrived(queue.Front))
                return "[E] Kunden bedienen";
            return "Kasse (keine wartenden Kunden)";
        }

        public override void Interact(Player.PlayerInteractor interactor)
        {
            if (CurrentCustomer != null) return;
            var queue = CustomerQueue.Instance;
            if (queue == null) return;
            var front = queue.Front;
            if (front == null || !queue.IsFrontAndArrived(front)) return;

            CurrentCustomer = front;
            if (front.Intent == CustomerIntent.Shopping && front.Basket.Count > 0 && !front.ItemsPlacedOnCounter)
                PlaceItemsForCustomer(front);
            front.IsBeingServed = true;
            front.State = CustomerState.BeingServed;
            var ui = UI.UIManager.Instance;
            if (ui != null) ui.OpenCheckout(front);
        }

        public bool PlaceItemsForCustomer(CustomerAI customer)
        {
            if (customer == null || customer.Intent != CustomerIntent.Shopping || customer.Basket.Count == 0) return false;
            if (_displayCustomer == customer && _counterItems.Count == customer.Basket.Count) return true;

            ClearCustomerItems(null);
            EnsureVisualRoots();
            _displayCustomer = customer;

            int count = Mathf.Max(1, customer.Basket.Count);
            float zStep = count > 1 ? CounterItemsZRange / Mathf.Max(1, count - 1) : 0f;
            for (int i = 0; i < customer.Basket.Count; i++)
            {
                var product = customer.Basket[i];
                var go = Core.ProceduralAssetGenerator.CreateProductModel(product);
                go.transform.SetParent(_itemsRoot, false);
                go.transform.localRotation = Quaternion.identity;
                var localPos = new Vector3(-0.18f, 0.52f, CounterItemsZStart + zStep * i);
                go.transform.localPosition = localPos;
                _counterItems.Add(new CounterItemVisual
                {
                    Product = product,
                    Transform = go.transform,
                    RestLocalPosition = localPos
                });
            }
            return true;
        }

        public void AnimateScannedItem(ProductData product)
        {
            for (int i = 0; i < _counterItems.Count; i++)
            {
                var item = _counterItems[i];
                if (item.Scanned) continue;
                if (product != null && item.Product != product) continue;

                item.Scanned = true;
                item.Transform.localPosition = item.RestLocalPosition + new Vector3(0.32f, 0.04f, 0f);
                item.Transform.localScale *= 0.92f;
                var renderer = item.Transform.GetComponentInChildren<Renderer>();
                if (renderer != null) renderer.material.color = Color.Lerp(renderer.material.color, Color.gray, 0.45f);
                return;
            }
        }

        public void ClearCustomerItems(CustomerAI customer)
        {
            if (customer != null && _displayCustomer != null && customer != _displayCustomer && customer != CurrentCustomer)
                return;

            for (int i = _counterItems.Count - 1; i >= 0; i--)
                if (_counterItems[i].Transform != null)
                    Destroy(_counterItems[i].Transform.gameObject);
            _counterItems.Clear();
            _displayCustomer = null;
        }

        public void ShowPaymentFeedback(float amount)
        {
            if (_floatingMoneyText != null) Destroy(_floatingMoneyText.gameObject);
            var go = new GameObject("GeldFeedback");
            go.transform.SetParent(transform, false);
            go.transform.position = transform.position + new Vector3(0.2f, 1.35f, 0f);
            _floatingMoneyText = go.AddComponent<TextMesh>();
            _floatingMoneyText.fontSize = 44;
            _floatingMoneyText.characterSize = 0.05f;
            _floatingMoneyText.anchor = TextAnchor.MiddleCenter;
            _floatingMoneyText.alignment = TextAlignment.Center;
            _floatingMoneyText.color = new Color(0.95f, 0.85f, 0.25f, 1f);
            _floatingMoneyText.text = "+" + amount.ToString("F2") + " Euro";
            go.AddComponent<UI.Billboard>();
            _floatingMoneyTimer = 1f;
        }

        void EnsureVisualRoots()
        {
            if (_itemsRoot != null) return;
            _itemsRoot = new GameObject("CounterItems").transform;
            _itemsRoot.SetParent(transform, false);
        }

        /// <summary>Bedienung beenden und Kunden entlassen.</summary>
        public void ReleaseCustomer(bool happy)
        {
            if (CurrentCustomer == null) return;
            var c = CurrentCustomer;
            CurrentCustomer = null;
            ClearCustomerItems(c);
            c.FinishService(happy);
        }

        public void CancelService()
        {
            if (CurrentCustomer == null) return;
            CurrentCustomer.IsBeingServed = false;
            var c = CurrentCustomer;
            CurrentCustomer = null;
            c.State = CustomerState.Waiting;
        }
    }
}
