using UnityEngine;

namespace Kiosk.Economy
{
    /// <summary>
    /// Ruf des Ladens (0-100). Beeinflusst Kundenspawnrate.
    /// </summary>
    public class ReputationManager : MonoBehaviour
    {
        public static ReputationManager Instance { get; private set; }

        [Range(0f, 100f)] public float Reputation = 50f;

        public event System.Action OnReputationChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Add(float amount)
        {
            Reputation = Mathf.Clamp(Reputation + amount, 0f, 100f);
            if (OnReputationChanged != null) OnReputationChanged();
        }

        public void SetReputation(float value)
        {
            Reputation = Mathf.Clamp(value, 0f, 100f);
            if (OnReputationChanged != null) OnReputationChanged();
        }

        /// <summary>Multiplikator fuer Kundenspawnrate: 0.5 bei Ruf 0, 1.5 bei Ruf 100.</summary>
        public float SpawnRateMultiplier { get { return 0.5f + Reputation / 100f; } }
    }
}
