using UnityEngine;
using Kiosk.Interaction;

namespace Kiosk.Player
{
    /// <summary>
    /// Raycast-Interaktion: E zum Interagieren, Tab fuer Tablet, Escape fuer Pause.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        public float InteractRange = 3f;
        public Camera PlayerCamera;

        Interactable _current;

        void Awake()
        {
            if (PlayerCamera == null) PlayerCamera = GetComponentInChildren<Camera>();
        }

        void Update()
        {
            var gm = Core.GameManager.Instance;
            var ui = UI.UIManager.Instance;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (ui != null) ui.TogglePauseOrCloseWindow();
                return;
            }
            if (gm != null && gm.IsPaused) return;

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (ui != null) ui.ToggleTablet();
                return;
            }

            if (ui != null && ui.AnyWindowOpen)
            {
                ClearCurrent();
                return;
            }

            ScanForInteractable();

            if (_current != null && Input.GetKeyDown(KeyCode.E))
                _current.Interact(this);
        }

        void ScanForInteractable()
        {
            Interactable found = null;
            if (PlayerCamera != null)
            {
                Ray ray = new Ray(PlayerCamera.transform.position, PlayerCamera.transform.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, InteractRange))
                    found = hit.collider.GetComponentInParent<Interactable>();
            }
            if (found != _current)
            {
                _current = found;
                var ui = UI.UIManager.Instance;
                if (ui != null)
                    ui.SetInteractionPrompt(_current != null ? _current.GetPrompt() : null);
            }
        }

        void ClearCurrent()
        {
            if (_current != null)
            {
                _current = null;
                if (UI.UIManager.Instance != null) UI.UIManager.Instance.SetInteractionPrompt(null);
            }
        }
    }
}
