using System.Collections.Generic;
using UnityEngine;

namespace Kiosk.Packages
{
    /// <summary>
    /// Paketannahme und -abholung. Pakete werden im Paketregal gespeichert.
    /// Richtige Ausgabe: Geld + Ruf. Falsche Ausgabe: Strafe.
    /// </summary>
    public class PackageSystem : MonoBehaviour
    {
        public static PackageSystem Instance { get; private set; }

        public int BaseCapacity = 6;
        public float AcceptReward = 1.50f;
        public float HandoutReward = 2.00f;
        public float WrongHandoutPenalty = 10f;

        int _bonusCapacity;
        readonly List<PackageItem> _stored = new List<PackageItem>();

        public event System.Action OnPackagesChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public int Capacity { get { return BaseCapacity + _bonusCapacity; } }
        public int StoredCount { get { return _stored.Count; } }
        public bool HasFreeSlot { get { return _stored.Count < Capacity; } }
        public IList<PackageItem> StoredPackages { get { return _stored; } }

        public void AddCapacity(int amount) { _bonusCapacity += amount; }

        /// <summary>Paket von Abgabe-Kunde annehmen und einlagern.</summary>
        public PackageItem AcceptPackage()
        {
            if (!HasFreeSlot) return null;
            var pkg = PackageItem.CreateRandom();
            _stored.Add(pkg);
            var eco = Economy.EconomyManager.Instance;
            if (eco != null) eco.AddRevenue(AcceptReward);
            if (Core.GameManager.Instance != null) Core.GameManager.Instance.AddXP(4);
            if (Audio.AudioManager.Instance != null)
                Audio.AudioManager.Instance.Play(Audio.SoundId.PackageScan);
            Notify();
            return pkg;
        }

        public string GetRandomStoredCode()
        {
            if (_stored.Count == 0) return null;
            return _stored[Random.Range(0, _stored.Count)].Code;
        }

        /// <summary>Paket ausgeben. Gibt true zurueck, wenn der Code stimmt.</summary>
        public bool HandOut(PackageItem package, string requestedCode)
        {
            if (package == null) return false;
            _stored.Remove(package);
            bool correct = package.Code == requestedCode;
            var eco = Economy.EconomyManager.Instance;
            var rep = Economy.ReputationManager.Instance;
            if (correct)
            {
                float reward = HandoutReward;
                var up = Upgrades.UpgradeManager.Instance;
                if (up != null) reward += up.GetEffectValue(Upgrades.UpgradeEffect.PackageBonus);
                if (eco != null) eco.AddRevenue(reward);
                if (rep != null) rep.Add(2f);
                if (Core.GameManager.Instance != null) Core.GameManager.Instance.AddXP(6);
            }
            else
            {
                if (eco != null) eco.ApplyPenalty(WrongHandoutPenalty);
                if (rep != null) rep.Add(-5f);
            }
            if (Audio.AudioManager.Instance != null)
                Audio.AudioManager.Instance.Play(Audio.SoundId.PackageScan);
            Notify();
            return correct;
        }

        public PackageItem FindByCode(string code)
        {
            foreach (var p in _stored) if (p.Code == code) return p;
            return null;
        }

        public List<PackageItem> GetSaveData() { return new List<PackageItem>(_stored); }

        public void LoadPackages(List<PackageItem> packages)
        {
            _stored.Clear();
            if (packages != null) _stored.AddRange(packages);
            Notify();
        }

        void Notify() { if (OnPackagesChanged != null) OnPackagesChanged(); }
    }
}
