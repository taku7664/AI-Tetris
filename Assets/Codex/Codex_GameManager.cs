using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codex
{
    public sealed class Codex_GameManager : MonoBehaviour
    {
        private const string ActionMapName = "Gameplay";
        private const string MoveLeftActionName = "MoveLeft";
        private const string MoveRightActionName = "MoveRight";
        private const string SoftDropActionName = "SoftDrop";
        private const string RotateActionName = "RotateCW";
        private const string HardDropActionName = "HardDrop";
        private const string RestartActionName = "Restart";

        private const float HorizontalRepeatDelaySeconds = 0.16f;
        private const float HorizontalRepeatRateSeconds = 0.06f;

        [Header("Scene References")]
        [SerializeField] private GameObject FieldBox;
        [SerializeField] private GameObject PreviewBox;
        [SerializeField] private TMP_Text ScoreText;

        [Header("Data")]
        [SerializeField] private int FieldWidth = 10;
        [SerializeField] private int FieldHeight = 20;
        [SerializeField] private float FallIntervalSeconds = 0.5f;
        [SerializeField] private List<int> LineClearScores = new List<int> { 100, 300, 500, 800 };

        [Header("Input")]
        [SerializeField] private InputActionAsset InputActionsAsset;

        private Codex_TetrisGame TetrisGame;
        private Codex_FieldRenderer FieldRenderer;
        private Codex_PreviewRenderer PreviewRenderer;

        private InputActionMap GameplayActionMap;
        private InputAction MoveLeftAction;
        private InputAction MoveRightAction;
        private InputAction SoftDropAction;
        private InputAction RotateAction;
        private InputAction HardDropAction;
        private InputAction RestartAction;

        private int ActiveHorizontalDirection;
        private float HorizontalRepeatTimer;
        private bool IsInitialized;
        private bool IsInputBound;

        private void Awake()
        {
            InitializeIfNeeded();
        }

        private void OnEnable()
        {
            InitializeIfNeeded();
            BindInput();
        }

        private void OnDisable()
        {
            UnbindInput();
        }

        private void OnDestroy()
        {
            UnbindInput();

            if (FieldRenderer != null)
            {
                FieldRenderer.Dispose();
                FieldRenderer = null;
            }

            if (PreviewRenderer != null)
            {
                PreviewRenderer.Dispose();
                PreviewRenderer = null;
            }
        }

        private void Update()
        {
            if (!IsInitialized || TetrisGame == null)
            {
                return;
            }

            if (!TetrisGame.IsGameOver)
            {
                HandleHorizontalInput(Time.deltaTime);
                bool IsSoftDropPressed = SoftDropAction != null && SoftDropAction.IsPressed();
                TetrisGame.Tick(Time.deltaTime, IsSoftDropPressed);
            }

            RefreshView();
        }

        private void InitializeIfNeeded()
        {
            if (IsInitialized)
            {
                return;
            }

            if (FieldBox == null || PreviewBox == null || ScoreText == null)
            {
                Debug.LogError("Codex_GameManager: Scene references are not assigned.", this);
                enabled = false;
                return;
            }

            FieldWidth = Mathf.Max(4, FieldWidth);
            FieldHeight = Mathf.Max(8, FieldHeight);
            FallIntervalSeconds = Mathf.Max(0.01f, FallIntervalSeconds);

            if (LineClearScores == null || LineClearScores.Count == 0)
            {
                LineClearScores = new List<int> { 100, 300, 500, 800 };
            }

            if (InputActionsAsset == null)
            {
                InputActionsAsset = Resources.Load<InputActionAsset>("Codex_InputActions");
            }

            if (InputActionsAsset == null)
            {
                Debug.LogError("Codex_GameManager: Codex_InputActions asset not found in Resources.", this);
                enabled = false;
                return;
            }

            TetrisGame = new Codex_TetrisGame(FieldWidth, FieldHeight, FallIntervalSeconds, LineClearScores);
            FieldRenderer = new Codex_FieldRenderer(FieldBox, FieldWidth, FieldHeight);
            PreviewRenderer = new Codex_PreviewRenderer(PreviewBox);

            IsInitialized = true;
            RefreshView();
        }

        private void BindInput()
        {
            if (IsInputBound || InputActionsAsset == null)
            {
                return;
            }

            GameplayActionMap = InputActionsAsset.FindActionMap(ActionMapName, true);
            MoveLeftAction = GameplayActionMap.FindAction(MoveLeftActionName, true);
            MoveRightAction = GameplayActionMap.FindAction(MoveRightActionName, true);
            SoftDropAction = GameplayActionMap.FindAction(SoftDropActionName, true);
            RotateAction = GameplayActionMap.FindAction(RotateActionName, true);
            HardDropAction = GameplayActionMap.FindAction(HardDropActionName, true);
            RestartAction = GameplayActionMap.FindAction(RestartActionName, true);

            RotateAction.performed += OnRotatePerformed;
            HardDropAction.performed += OnHardDropPerformed;
            RestartAction.performed += OnRestartPerformed;

            GameplayActionMap.Enable();
            IsInputBound = true;
        }

        private void UnbindInput()
        {
            if (!IsInputBound)
            {
                return;
            }

            RotateAction.performed -= OnRotatePerformed;
            HardDropAction.performed -= OnHardDropPerformed;
            RestartAction.performed -= OnRestartPerformed;

            if (GameplayActionMap != null)
            {
                GameplayActionMap.Disable();
            }

            IsInputBound = false;
        }

        private void HandleHorizontalInput(float DeltaTime)
        {
            int RequestedDirection = 0;

            bool IsLeftPressed = MoveLeftAction != null && MoveLeftAction.IsPressed();
            bool IsRightPressed = MoveRightAction != null && MoveRightAction.IsPressed();

            if (IsLeftPressed ^ IsRightPressed)
            {
                RequestedDirection = IsLeftPressed ? -1 : 1;
            }

            if (RequestedDirection == 0)
            {
                ActiveHorizontalDirection = 0;
                HorizontalRepeatTimer = 0f;
                return;
            }

            if (RequestedDirection != ActiveHorizontalDirection)
            {
                ActiveHorizontalDirection = RequestedDirection;
                HorizontalRepeatTimer = HorizontalRepeatDelaySeconds;
                TetrisGame.TryMoveHorizontal(ActiveHorizontalDirection);
                return;
            }

            HorizontalRepeatTimer -= DeltaTime;
            while (HorizontalRepeatTimer <= 0f)
            {
                TetrisGame.TryMoveHorizontal(ActiveHorizontalDirection);
                HorizontalRepeatTimer += HorizontalRepeatRateSeconds;
            }
        }

        private void OnRotatePerformed(InputAction.CallbackContext Context)
        {
            if (!IsInitialized || TetrisGame.IsGameOver)
            {
                return;
            }

            TetrisGame.TryRotateClockwise();
        }

        private void OnHardDropPerformed(InputAction.CallbackContext Context)
        {
            if (!IsInitialized || TetrisGame.IsGameOver)
            {
                return;
            }

            TetrisGame.HardDrop();
        }

        private void OnRestartPerformed(InputAction.CallbackContext Context)
        {
            if (!IsInitialized || !TetrisGame.IsGameOver)
            {
                return;
            }

            RestartGame();
        }

        private void RestartGame()
        {
            TetrisGame.StartNewGame();
            ActiveHorizontalDirection = 0;
            HorizontalRepeatTimer = 0f;
            RefreshView();
        }

        private void RefreshView()
        {
            if (!IsInitialized)
            {
                return;
            }

            Codex_PieceState GhostPiece = TetrisGame.GetGhostPiece();
            FieldRenderer.Render(TetrisGame.BoardState, TetrisGame.CurrentPiece, GhostPiece, TetrisGame.IsGameOver);
            PreviewRenderer.Render(TetrisGame.NextPieceType, TetrisGame.IsGameOver);
            ScoreText.text = TetrisGame.IsGameOver
                ? $"Score: {TetrisGame.Score}  GAME OVER"
                : $"Score: {TetrisGame.Score}";
        }
    }
}
