using UnityEngine;

namespace Kiosk.Core
{
    /// <summary>
    /// Einziges Objekt in der MainMenu-Szene. Baut das Hauptmenue zur Laufzeit auf.
    /// </summary>
    public class MainMenuBootstrapper : MonoBehaviour
    {
        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;
            var go = new GameObject("MainMenuUI");
            go.AddComponent<Kiosk.UI.MainMenuUI>();
        }
    }
}
