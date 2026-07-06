using UnityEngine;

namespace Kiosk.Player
{
    /// <summary>
    /// First-Person-Steuerung: WASD + Mausblick. Benoetigt CharacterController.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float MoveSpeed = 4f;
        public float MouseSensitivity = 2.2f;
        public bool CameraBobbingEnabled = true;
        public Camera PlayerCamera;

        CharacterController _controller;
        float _pitch;
        float _verticalVelocity;
        float _moveAmount;
        float _bobTimer;
        float _nodTimer;
        Vector3 _cameraBaseLocalPosition;

        public bool InputLocked { get; set; }

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (PlayerCamera == null) PlayerCamera = GetComponentInChildren<Camera>();
            if (PlayerCamera != null) _cameraBaseLocalPosition = PlayerCamera.transform.localPosition;
        }

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if (Core.GameManager.Instance != null && Core.GameManager.Instance.IsPaused) return;
            if (!InputLocked) Look();
            Move();
            UpdateCameraEffects();
        }

        void Look()
        {
            float mx = Input.GetAxis("Mouse X") * MouseSensitivity;
            float my = Input.GetAxis("Mouse Y") * MouseSensitivity;
            transform.Rotate(0f, mx, 0f);
            _pitch = Mathf.Clamp(_pitch - my, -85f, 85f);
        }

        void Move()
        {
            Vector3 input = Vector3.zero;
            if (!InputLocked)
            {
                input = transform.right * Input.GetAxisRaw("Horizontal")
                      + transform.forward * Input.GetAxisRaw("Vertical");
                if (input.sqrMagnitude > 1f) input.Normalize();
            }
            _moveAmount = input.magnitude;
            if (_controller.isGrounded) _verticalVelocity = -1f;
            else _verticalVelocity += Physics.gravity.y * Time.deltaTime;
            Vector3 motion = input * MoveSpeed + Vector3.up * _verticalVelocity;
            _controller.Move(motion * Time.deltaTime);
        }

        void UpdateCameraEffects()
        {
            if (PlayerCamera == null) return;

            float bobY = 0f;
            float bobX = 0f;
            if (CameraBobbingEnabled && _controller != null && _controller.isGrounded && _moveAmount > 0.05f && !InputLocked)
            {
                _bobTimer += Time.deltaTime * (8f + _moveAmount * 2f);
                bobY = Mathf.Sin(_bobTimer) * 0.045f;
                bobX = Mathf.Cos(_bobTimer * 0.5f) * 0.02f;
            }
            else
            {
                _bobTimer = Mathf.Lerp(_bobTimer, 0f, Time.deltaTime * 6f);
            }

            if (_nodTimer > 0f) _nodTimer = Mathf.Max(0f, _nodTimer - Time.deltaTime * 3.5f);
            float nodOffset = Mathf.Sin((1f - _nodTimer) * Mathf.PI) * 6f * _nodTimer;

            PlayerCamera.transform.localPosition = Vector3.Lerp(PlayerCamera.transform.localPosition,
                _cameraBaseLocalPosition + new Vector3(bobX, bobY, 0f), Time.deltaTime * 10f);
            PlayerCamera.transform.localRotation = Quaternion.Euler(_pitch + nodOffset, 0f, 0f);
        }

        public void PlayInteractNod()
        {
            _nodTimer = 1f;
        }
    }
}
