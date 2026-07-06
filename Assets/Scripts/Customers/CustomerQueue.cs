using System.Collections.Generic;
using UnityEngine;

namespace Kiosk.Customers
{
    /// <summary>
    /// Warteschlange an der Kasse. Verwaltet Positionen und ruft Kunden auf.
    /// </summary>
    public class CustomerQueue : MonoBehaviour
    {
        public static CustomerQueue Instance { get; private set; }

        public Vector3 QueueStart;
        public Vector3 QueueDirection = Vector3.back;
        public float Spacing = 1.1f;

        readonly List<CustomerAI> _queue = new List<CustomerAI>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public int Count { get { return _queue.Count; } }

        public void Join(CustomerAI customer)
        {
            if (!_queue.Contains(customer)) _queue.Add(customer);
        }

        public void Leave(CustomerAI customer)
        {
            _queue.Remove(customer);
        }

        public int IndexOf(CustomerAI customer) { return _queue.IndexOf(customer); }

        public Vector3 GetPositionFor(CustomerAI customer)
        {
            int idx = Mathf.Max(0, _queue.IndexOf(customer));
            return QueueStart + QueueDirection.normalized * Spacing * idx;
        }

        public CustomerAI Front
        {
            get { return _queue.Count > 0 ? _queue[0] : null; }
        }

        /// <summary>Ist der Kunde vorne und bereit, bedient zu werden?</summary>
        public bool IsFrontAndArrived(CustomerAI customer)
        {
            return Front == customer && customer.HasArrived;
        }
    }
}
