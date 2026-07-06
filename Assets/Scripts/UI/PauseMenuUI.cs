using UnityEngine;
using UnityEngine.SceneManagement;
using Kiosk.Core;

namespace Kiosk.UI
{
    /// <summary>
    /// Pausenmenue (Escape): Weiter, Speichern, Laden, Hauptmenue.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        public bool IsOpen { get; private set; }

        GameObject _root;

        public void Build()
        {
            _root = ProceduralAssetGenerator.CreatePanel(transform, "Fenster",
                new Vector2(0.35f, 0.2f), new Vector2(0.65f, 0.8f), new Color(0.05f, 0.05f, 0.08f, 0.97f));

            var titleGo = ProceduralAssetGenerator.CreatePanel(_root.transform, "Titel",
                new Vector2(0f, 0.88f), new Vector2(1f, 1f), new Color(0.15f, 0.15f, 0.25f, 1f));
            ProceduralAssetGenerator.CreateText(titleGo.transform, "Text", "PAUSE", 26, TextAnchor.MiddleCenter);

            var buttons = ProceduralAssetGenerator.CreatePanel(_root.transform, "Buttons",
                new Vector2(0.15f, 0.08f), new Vector2(0.85f, 0.85f), Color.clear);
            var layout = buttons.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            ProceduralAssetGenerator.CreateButton(buttons.transform, "Weiter", "Weiterspielen", Close);
            ProceduralAssetGenerator.CreateButton(buttons.transform, "Speichern", "Spiel speichern", OnSave);
            ProceduralAssetGenerator.CreateButton(buttons.transform, "Laden", "Spiel laden", OnLoad);
            ProceduralAssetGenerator.CreateButton(buttons.transform, "Hauptmenue", "Zum Hauptmenue", OnMainMenu);

            _root.SetActive(false);
        }

        public void Open()
        {
            IsOpen = true;
            _root.SetActive(true);
            GameManager.Instance.SetPaused(true);
        }

        public void Close()
        {
            IsOpen = false;
            _root.SetActive(false);
            GameManager.Instance.SetPaused(false);
        }

        void OnSave()
        {
            if (SaveSystem.SaveLoadSystem.Instance != null)
                SaveSystem.SaveLoadSystem.Instance.Save();
        }

        void OnLoad()
        {
            if (SaveSystem.SaveLoadSystem.SaveExists())
            {
                Close();
                SceneBootstrapper.LoadSaveOnStart = true;
                Time.timeScale = 1f;
                SceneManager.LoadScene("KioskGame");
            }
            else
            {
                UIManager.Instance.ShowToast("Kein Spielstand vorhanden.");
            }
        }

        void OnMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
