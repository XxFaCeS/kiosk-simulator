using System.Collections.Generic;
using UnityEngine;

namespace Kiosk.Customers
{
    /// <summary>
    /// Spawnt Kunden waehrend der Oeffnungszeiten am Eingang.
    /// Spawnrate haengt von Ruf und Upgrades ab.
    /// </summary>
    public class CustomerSpawner : MonoBehaviour
    {
        public static CustomerSpawner Instance { get; private set; }

        public Vector3 EntrancePosition;
        public Vector3 ShopCenter;
        public float BaseSpawnInterval = 14f;
        public int MaxCustomers = 6;

        readonly List<CustomerAI> _active = new List<CustomerAI>();
        float _timer;

        public int ActiveCustomerCount { get { return _active.Count; } }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _timer = 3f;
        }

        void Update()
        {
            var gm = Core.GameManager.Instance;
            var cycle = Core.DayNightCycle.Instance;
            if (gm == null || gm.IsPaused) return;
            if (cycle == null || !cycle.ShopOpen) return;
            if (_active.Count >= MaxCustomers) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                SpawnCustomer();
                float rate = 1f;
                if (Economy.ReputationManager.Instance != null)
                    rate *= Economy.ReputationManager.Instance.SpawnRateMultiplier;
                var up = Upgrades.UpgradeManager.Instance;
                if (up != null) rate *= 1f + up.GetEffectValue(Upgrades.UpgradeEffect.CustomerRate);
                _timer = BaseSpawnInterval / Mathf.Max(0.2f, rate) * Random.Range(0.7f, 1.3f);
            }
        }

        void SpawnCustomer()
        {
            var intent = PickIntent();
            var go = Core.ProceduralAssetGenerator.CreateCustomerModel();
            go.name = "Kunde_" + intent;
            go.transform.position = EntrancePosition;
            var ai = go.AddComponent<CustomerAI>();
            ai.Init(intent, EntrancePosition, ShopCenter);
            if (intent == CustomerIntent.PackagePickup)
            {
                var pkg = Packages.PackageSystem.Instance;
                if (pkg != null) ai.PackageCode = pkg.GetRandomStoredCode();
            }
            _active.Add(ai);
            if (Audio.AudioManager.Instance != null)
                Audio.AudioManager.Instance.Play(Audio.SoundId.DoorBell);
        }

        CustomerIntent PickIntent()
        {
            var unlock = Core.UnlockManager.Instance;
            float r = Random.value;
            if (unlock != null)
            {
                var pkg = Packages.PackageSystem.Instance;
                if (unlock.IsServiceUnlocked(Core.ServiceType.Paketannahme))
                {
                    if (r < 0.08f && pkg != null && pkg.HasFreeSlot) return CustomerIntent.PackageDropoff;
                    if (r < 0.14f && pkg != null && pkg.StoredCount > 0) return CustomerIntent.PackagePickup;
                }
                if (unlock.IsServiceUnlocked(Core.ServiceType.Lotto) && r >= 0.14f && r < 0.22f)
                {
                    var up = Upgrades.UpgradeManager.Instance;
                    if (up != null && up.IsPurchased("lotto_terminal")) return CustomerIntent.Lotto;
                }
            }
            return CustomerIntent.Shopping;
        }

        public void NotifyDespawn(CustomerAI customer) { _active.Remove(customer); }

        public void DespawnAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
                if (_active[i] != null) Destroy(_active[i].gameObject);
            _active.Clear();
        }
    }
}
