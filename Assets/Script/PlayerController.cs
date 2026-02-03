using UnityEngine;
using UnityEngine.InputSystem; // Input System必須

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraRoot; // 首（カメラの追従先）

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float lookSensitivity = 1.0f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    // 内部変数
    private CharacterController _controller;
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _lookAction;
    
    private float _xRotation = 0f; // 上下の視点角度
    private Vector3 _velocity;     // 重力計算用
    private bool _isGrounded;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();

        // アクションの参照を取得 (Input Actionのアクション名と一致させる)
        _moveAction = _playerInput.actions["Move"];
        _lookAction = _playerInput.actions["Look"];

        // カーソルをロックして非表示にする
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleGravity();
        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        // 入力値の取得
        Vector2 lookInput = _lookAction.ReadValue<Vector2>();

        // 上下の回転 (CameraRootを回す)
        // Unityの座標系では、上を向くにはX軸をマイナスに回転させる
        _xRotation -= lookInput.y * lookSensitivity;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f); // 真上・真下に行き過ぎないよう制限

        cameraRoot.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        // 左右の回転 (体全体=Player自体を回す)
        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity);
    }

    private void HandleMovement()
    {
        // 入力値の取得
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();

        // 向いている方向に対しての移動ベクトルを作成
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // 移動実行
        _controller.Move(move * moveSpeed * Time.deltaTime);
    }

    private void HandleGravity()
    {
        _isGrounded = _controller.isGrounded;

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // 接地時は少しだけ下向きの力を残して安定させる
        }

        // 重力加算
        _velocity.y += gravity * Time.deltaTime;

        // 重力による移動実行
        _controller.Move(_velocity * Time.deltaTime);
    }
}