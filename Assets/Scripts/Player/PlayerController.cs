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
        public Camera PlayerCamera;

        CharacterController _controller;
        float _pitch;
        float _verticalVelocity;

        public bool InputLocked { get; set; }

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (PlayerCamera == null) PlayerCamera = GetComponentInChildren<Camera>();
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
        }

        void Look()
        {
            float mx = Input.GetAxis("Mouse X") * MouseSensitivity;
            float my = Input.GetAxis("Mouse Y") * MouseSensitivity;
            transform.Rotate(0f, mx, 0f);
            _pitch = Mathf.Clamp(_pitch - my, -85f, 85f);
            if (PlayerCamera != null)
                PlayerCamera.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
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
            if (_controller.isGrounded) _verticalVelocity = -1f;
            else _verticalVelocity += Physics.gravity.y * Time.deltaTime;
            Vector3 motion = input * MoveSpeed + Vector3.up * _verticalVelocity;
            _controller.Move(motion * Time.deltaTime);
        }
    }
}
