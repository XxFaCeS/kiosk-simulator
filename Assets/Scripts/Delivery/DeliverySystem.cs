using System.Collections.Generic;
using UnityEngine;
using Kiosk.Orders;

namespace Kiosk.Delivery
{
    [System.Serializable]
    public class PendingDeliverySaveData
    {
        public List<OrderLine> Lines = new List<OrderLine>();
        public float Timer;
    }

    class PendingDelivery
    {
        public List<OrderLine> Lines;
        public float Timer;
    }

    /// <summary>
    /// Verwaltet ausstehende Lieferungen und spawnt Lieferkartons am Eingang.
    /// </summary>
    public class DeliverySystem : MonoBehaviour
    {
        const float MinLoadedDeliveryTimer = 0.1f;
        const float DefaultLoadedDeliveryTimer = 1f;

        public static DeliverySystem Instance { get; private set; }

        public Vector3 DeliveryDropPoint;

        readonly List<PendingDelivery> _pending = new List<PendingDelivery>();
        int _boxOffset;

        public event System.Action OnPendingDeliveriesChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public int PendingCount { get { return _pending.Count; } }

        public void ScheduleDelivery(List<OrderLine> lines, float timeSeconds)
        {
            _pending.Add(new PendingDelivery
            {
                Lines = new List<OrderLine>(lines),
                Timer = timeSeconds
            });
            NotifyPendingChanged();
        }

        void Update()
        {
            if (Core.GameManager.Instance != null && Core.GameManager.Instance.IsPaused) return;
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                _pending[i].Timer -= Time.deltaTime;
                if (_pending[i].Timer <= 0f)
                {
                    SpawnBoxes(_pending[i].Lines);
                    _pending.RemoveAt(i);
                    NotifyPendingChanged();
                    if (UI.UIManager.Instance != null)
                        UI.UIManager.Instance.ShowToast("Lieferung angekommen! Kartons am Eingang.");
                    if (Audio.AudioManager.Instance != null)
                        Audio.AudioManager.Instance.Play(Audio.SoundId.DoorBell);
                }
            }
        }

        void SpawnBoxes(List<OrderLine> lines)
        {
            // Max. 12 Einheiten pro Karton
            var boxLines = new List<OrderLine>();
            int unitsInBox = 0;
            foreach (var line in lines)
            {
                int remaining = line.Amount;
                while (remaining > 0)
                {
                    int take = Mathf.Min(remaining, 12 - unitsInBox);
                    boxLines.Add(new OrderLine { ProductId = line.ProductId, Amount = take });
                    remaining -= take;
                    unitsInBox += take;
                    if (unitsInBox >= 12)
                    {
                        CreateBox(boxLines);
                        boxLines = new List<OrderLine>();
                        unitsInBox = 0;
                    }
                }
            }
            if (boxLines.Count > 0) CreateBox(boxLines);
        }

        void CreateBox(List<OrderLine> lines)
        {
            var go = Core.ProceduralAssetGenerator.CreateDeliveryBoxModel();
            go.transform.position = DeliveryDropPoint
                + new Vector3((_boxOffset % 3) * 0.7f, 0f, (_boxOffset / 3 % 3) * 0.7f);
            _boxOffset++;
            var box = go.AddComponent<DeliveryBox>();
            box.SetContents(lines);
        }

        public List<PendingDeliverySaveData> GetPendingDeliveryData()
        {
            var data = new List<PendingDeliverySaveData>();
            foreach (var pending in _pending)
                data.Add(new PendingDeliverySaveData
                {
                    Lines = new List<OrderLine>(pending.Lines),
                    Timer = pending.Timer
                });
            return data;
        }

        public void LoadPendingDeliveries(List<PendingDeliverySaveData> deliveries)
        {
            _pending.Clear();
            if (deliveries != null)
                foreach (var delivery in deliveries)
                    _pending.Add(new PendingDelivery
                    {
                        Lines = delivery != null && delivery.Lines != null ? new List<OrderLine>(delivery.Lines) : new List<OrderLine>(),
                        Timer = delivery != null ? Mathf.Max(MinLoadedDeliveryTimer, delivery.Timer) : DefaultLoadedDeliveryTimer
                    });
            NotifyPendingChanged();
        }

        public List<PendingDeliverySaveData> GetPendingSnapshot()
        {
            return GetPendingDeliveryData();
        }

        public void RestoreSpawnedBoxCount(int count)
        {
            _boxOffset = Mathf.Max(_boxOffset, Mathf.Max(0, count));
        }

        void NotifyPendingChanged()
        {
            if (OnPendingDeliveriesChanged != null) OnPendingDeliveriesChanged();
        }
    }
}
