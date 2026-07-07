using UnityEngine;

namespace Kiosk.Core
{
    /// <summary>
    /// Zentraler Spielzustand: Tag, Level, XP, Pause. Singleton.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public int Day = 1;
        public int Level = 1;
        public int XP = 0;
        public bool IsPaused { get; private set; }

        public event System.Action<int> OnLevelUp;
        public event System.Action OnXPChanged;
        public event System.Action<int> OnDayChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public int XPForNextLevel { get { return 100 + (Level - 1) * 75; } }

        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            XP += amount;
            while (XP >= XPForNextLevel)
            {
                XP -= XPForNextLevel;
                Level++;
                if (OnLevelUp != null) OnLevelUp(Level);
                if (Audio.AudioManager.Instance != null) Audio.AudioManager.Instance.Play(Audio.SoundId.DaySummary);
            }
            if (OnXPChanged != null) OnXPChanged();
        }

        public void StartNextDay()
        {
            Day++;
            if (OnDayChanged != null) OnDayChanged(Day);
        }

        public void SetDayLevelXP(int day, int level, int xp)
        {
            Day = day; Level = level; XP = xp;
            if (OnDayChanged != null) OnDayChanged(Day);
            if (OnXPChanged != null) OnXPChanged();
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
        }

        /// <summary>Cursor freigeben ohne Zeit anzuhalten (fuer UI-Fenster wie Kasse/Tablet).</summary>
        public void SetUIMode(bool uiOpen)
        {
            Cursor.lockState = uiOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = uiOpen;
            var player = FindObjectOfType<Player.PlayerController>();
            if (player != null) player.InputLocked = uiOpen;
        }
    }
}
