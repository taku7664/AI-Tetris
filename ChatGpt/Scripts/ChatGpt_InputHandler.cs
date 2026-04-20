using UnityEngine;
using UnityEngine.InputSystem;

namespace ChatGpt
{
    /// <summary>
    /// Handles all player input via the Unity InputSystem (InputAction API).
    /// Supports both keyboard and gamepad (as required).
    ///
    /// Bindings (defined in ChatGpt_InputActions.inputactions):
    ///   MoveLeft  / MoveRight : ← → arrows  |  D-pad left/right
    ///   MoveDown              : ↓ arrow      |  D-pad down    (repeat)
    ///   RotateCW              : ↑ arrow      |  D-pad up
    ///   HardDrop              : Space        |  Button South (A)
    ///   Restart               : Enter        |  Start button
    /// </summary>
    public class ChatGpt_InputHandler : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────
        [SerializeField] private InputActionAsset _inputActions;

        // Repeat configuration for held keys
        [SerializeField] private float _repeatInitialDelay = 0.25f;
        [SerializeField] private float _repeatInterval     = 0.08f;

        // ── Runtime ──────────────────────────────────────────────────────────────
        private InputAction _moveLeft;
        private InputAction _moveRight;
        private InputAction _moveDown;
        private InputAction _rotateCW;
        private InputAction _hardDrop;
        private InputAction _restart;

        // Repeat timers for left/right/down
        private float _leftTimer;
        private float _rightTimer;
        private float _downTimer;

        // Events consumed by GameManager
        public System.Action OnMoveLeft;
        public System.Action OnMoveRight;
        public System.Action OnMoveDown;
        public System.Action OnRotateCW;
        public System.Action OnHardDrop;
        public System.Action OnRestart;

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            if (_inputActions == null)
            {
                Debug.LogError("[ChatGpt_InputHandler] InputActionAsset not assigned!");
                return;
            }

            var map = _inputActions.FindActionMap("ChatGpt_Gameplay", throwIfNotFound: true);

            _moveLeft  = map.FindAction("MoveLeft",  throwIfNotFound: true);
            _moveRight = map.FindAction("MoveRight", throwIfNotFound: true);
            _moveDown  = map.FindAction("MoveDown",  throwIfNotFound: true);
            _rotateCW  = map.FindAction("RotateCW",  throwIfNotFound: true);
            _hardDrop  = map.FindAction("HardDrop",  throwIfNotFound: true);
            _restart   = map.FindAction("Restart",   throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _inputActions?.Enable();
            if (_hardDrop != null) _hardDrop.performed += OnHardDropPerformed;
            if (_rotateCW != null) _rotateCW.performed += OnRotateCWPerformed;
            if (_restart  != null) _restart.performed  += OnRestartPerformed;
        }

        private void OnDisable()
        {
            _inputActions?.Disable();
            if (_hardDrop != null) _hardDrop.performed -= OnHardDropPerformed;
            if (_rotateCW != null) _rotateCW.performed -= OnRotateCWPerformed;
            if (_restart  != null) _restart.performed  -= OnRestartPerformed;
        }

        private void OnHardDropPerformed(InputAction.CallbackContext _) => OnHardDrop?.Invoke();
        private void OnRotateCWPerformed(InputAction.CallbackContext _) => OnRotateCW?.Invoke();
        private void OnRestartPerformed(InputAction.CallbackContext _)  => OnRestart?.Invoke();

        private void Update()
        {
            HandleRepeat(ref _leftTimer,  _moveLeft,  OnMoveLeft);
            HandleRepeat(ref _rightTimer, _moveRight, OnMoveRight);
            HandleRepeat(ref _downTimer,  _moveDown,  OnMoveDown);
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        /// <summary>
        /// Fires the action once on press, then repeatedly after an initial delay.
        /// This matches the standard game-feel for directional input "repeat".
        /// </summary>
        private void HandleRepeat(ref float timer, InputAction action, System.Action callback)
        {
            if (action == null || callback == null) return;

            bool held = action.IsPressed();

            if (action.WasPressedThisFrame())
            {
                callback.Invoke();
                timer = -_repeatInitialDelay;
            }
            else if (held)
            {
                timer += Time.deltaTime;
                if (timer >= _repeatInterval)
                {
                    callback.Invoke();
                    timer -= _repeatInterval;
                }
            }
            else
            {
                timer = 0f;
            }
        }
    }
}
