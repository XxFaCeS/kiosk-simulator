using System;
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
        CharacterController _characterController;

        void Awake()
        {
            if (PlayerCamera == null) PlayerCamera = GetComponentInChildren<Camera>();
            _characterController = GetComponent<CharacterController>();
        }

        void Update()
        {
            var gm = Core.GameManager.Instance;
            var ui = UI.UIManager.Instance;
            var placement = Placement.PlacementSystem.Instance;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (placement != null && placement.IsPlacing)
                {
                    placement.CancelPlacement(true);
                    return;
                }
                if (ui != null) ui.TogglePauseOrCloseWindow();
                return;
            }
            if (gm != null && gm.IsPaused) return;

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (placement != null && placement.IsPlacing) return;
                if (ui != null) ui.ToggleTablet();
                return;
            }

            if ((ui != null && ui.AnyWindowOpen) || (placement != null && placement.IsPlacing))
            {
                ClearCurrent();
                return;
            }

            ScanForInteractable();

            if (_current != null && Input.GetKeyDown(KeyCode.E))
            {
                var controller = GetComponent<PlayerController>();
                if (controller != null) controller.PlayInteractNod();
                _current.Interact(this);
            }
        }

        void ScanForInteractable()
        {
            Interactable found = null;
            if (PlayerCamera != null)
            {
                Ray ray = new Ray(PlayerCamera.transform.position, PlayerCamera.transform.forward);
                var hits = Physics.RaycastAll(ray, InteractRange, ~0, QueryTriggerInteraction.Ignore);
                Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                foreach (var hit in hits)
                {
                    if (hit.collider == null) continue;
                    if (_characterController != null && hit.collider == _characterController) continue;
                    if (hit.collider.transform.IsChildOf(transform)) continue;
                    found = hit.collider.GetComponentInParent<Interactable>();
                    if (found != null) break;
                }
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
