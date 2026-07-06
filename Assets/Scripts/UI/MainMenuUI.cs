using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Kiosk.Core;

namespace Kiosk.UI
{
    /// <summary>
    /// Hauptmenue: Neues Spiel, Weiterspielen, Beenden. Baut sich zur Laufzeit auf.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        void Start()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);

            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                es.transform.SetParent(transform, false);
            }

            var background = ProceduralAssetGenerator.CreatePanel(canvas.transform, "Hintergrund",
                Vector2.zero, Vector2.one, new Color(0.07f, 0.1f, 0.16f, 1f));

            var titleGo = ProceduralAssetGenerator.CreatePanel(background.transform, "Titel",
                new Vector2(0.2f, 0.72f), new Vector2(0.8f, 0.92f), Color.clear);
            var title = ProceduralAssetGenerator.CreateText(titleGo.transform, "Text",
                "KIOSK SIMULATOR", 64, TextAnchor.MiddleCenter);
            title.color = new Color(0.95f, 0.85f, 0.4f);
            var subGo = ProceduralAssetGenerator.CreatePanel(background.transform, "Untertitel",
                new Vector2(0.2f, 0.65f), new Vector2(0.8f, 0.72f), Color.clear);
            var sub = ProceduralAssetGenerator.CreateText(subGo.transform, "Text",
                "Dein eigener kleiner Laden - fiktiv, offline und werbefrei", 22, TextAnchor.MiddleCenter);
            sub.color = new Color(1f, 1f, 1f, 0.7f);

            var buttons = ProceduralAssetGenerator.CreatePanel(background.transform, "Buttons",
                new Vector2(0.35f, 0.15f), new Vector2(0.65f, 0.6f), Color.clear);
            var layout = buttons.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            ProceduralAssetGenerator.CreateButton(buttons.transform, "NeuesSpiel", "Neues Spiel", OnNewGame);
            var continueButton = ProceduralAssetGenerator.CreateButton(buttons.transform, "Weiter", "Weiterspielen", OnContinue);
            continueButton.interactable = SaveSystem.SaveLoadSystem.SaveExists();
            ProceduralAssetGenerator.CreateButton(buttons.transform, "Beenden", "Beenden", OnQuit);

            var version = ProceduralAssetGenerator.CreateText(background.transform, "Version",
                "MVP 1.0 - Alle Produkte und Systeme sind fiktiv.", 14, TextAnchor.LowerRight);
            version.color = new Color(1f, 1f, 1f, 0.4f);
        }

        void OnNewGame()
        {
            SaveSystem.SaveLoadSystem.DeleteSave();
            SceneBootstrapper.LoadSaveOnStart = false;
            SceneManager.LoadScene("KioskGame");
        }

        void OnContinue()
        {
            SceneBootstrapper.LoadSaveOnStart = true;
            SceneManager.LoadScene("KioskGame");
        }

        void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
