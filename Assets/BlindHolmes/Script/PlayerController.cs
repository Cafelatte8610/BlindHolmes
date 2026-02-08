using UnityEngine;
using UnityEngine.InputSystem;

namespace BlindHolmes
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Transform cameraRoot;

        [Header("Settings")] [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float lookSensitivity = 1.0f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpHeight = 1.5f;
        private CharacterController _controller;
        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _lookAction;

        private float _xRotation = 0f;
        private Vector3 _velocity;
        private bool _isGrounded;

        private bool _canAction;

        void Start()
        {
            _controller = GetComponent<CharacterController>();
            _playerInput = GetComponent<PlayerInput>();

            _moveAction = _playerInput.actions["Move"];
            _lookAction = _playerInput.actions["Look"];

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _canAction = true;
        }

        public void OperationUI()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _canAction = false;
        }

        public void ClosedUI()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _canAction = true;
        }

        void Update()
        {
            if (!_canAction) return;
            HandleGravity();
            HandleLook();
            HandleMovement();
        }

        // 視点移動
        private void HandleLook()
        {
            Vector2 lookInput = _lookAction.ReadValue<Vector2>();


            _xRotation -= lookInput.y * lookSensitivity;
            _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);

            cameraRoot.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

            transform.Rotate(Vector3.up * lookInput.x * lookSensitivity);
        }

        // 移動
        private void HandleMovement()
        {

            Vector2 moveInput = _moveAction.ReadValue<Vector2>();

            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

            _controller.Move(move * moveSpeed * Time.deltaTime);
        }

        private void HandleGravity()
        {
            _isGrounded = _controller.isGrounded;

            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            _velocity.y += gravity * Time.deltaTime;

            _controller.Move(_velocity * Time.deltaTime);
        }
    }
}