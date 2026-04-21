using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChatGPT
{
    public sealed class ChatGPT_GameManager : MonoBehaviour
    {
        private const int PreviewGridSize = 4;
        private const float GhostAlpha = 0.2f;
        private const float HoldRepeatDelay = 0.15f;
        private const float HorizontalRepeatInterval = 0.08f;
        private const float SoftDropRepeatInterval = 0.04f;

        [SerializeField] private GameObject FieldBox;
        [SerializeField] private GameObject PreviewBox;
        [SerializeField] private TMP_Text ScoreText;

        [SerializeField] private int FieldWidth = 10;
        [SerializeField] private int FieldHeight = 20;
        [SerializeField] private float FallIntervalSeconds = 0.5f;
        [SerializeField] private List<int> ScorePerLine = new List<int> { 100, 300, 500, 800 };

        private int[,] Board;
        private SpriteRenderer FieldBoxRenderer;
        private SpriteRenderer PreviewBoxRenderer;
        private Transform FieldRenderRoot;
        private Transform PreviewRenderRoot;
        private GameObject FieldBackgroundObject;
        private SpriteRenderer FieldBackgroundRenderer;
        private ChatGPT_BlockViewPool FieldPool;
        private ChatGPT_BlockViewPool PreviewPool;

        private Vector3 FieldOrigin;
        private float CellSize;
        private float FieldActualWidth;
        private float FieldActualHeight;
        private Vector3 PreviewCenter;
        private float PreviewCellSize;

        private readonly List<ChatGPT_PieceType> Bag = new List<ChatGPT_PieceType>();
        private ChatGPT_PieceState CurrentPiece;
        private ChatGPT_PieceType NextPieceType;
        private bool HasSpawnedFirstPiece;
        private float FallTimer;
        private int Score;
        private bool IsGameOver;
        private bool IsInitialized;

        private InputAction MoveLeftAction;
        private InputAction MoveRightAction;
        private InputAction SoftDropAction;
        private InputAction RotateAction;
        private InputAction HardDropAction;
        private InputAction RestartAction;

        private bool WasMoveLeftPressed;
        private bool WasMoveRightPressed;
        private bool WasSoftDropPressed;
        private float NextMoveLeftTime;
        private float NextMoveRightTime;
        private float NextSoftDropTime;

        private void OnEnable()
        {
            SetupInput();
            EnableInput();
        }

        private void OnDisable()
        {
            DisableInput();
            DisposeInput();
        }

        private void Start()
        {
            InitializeGame();
        }

        private void Update()
        {
            if (!IsInitialized)
            {
                return;
            }

            if (RestartAction != null && RestartAction.WasPressedThisFrame())
            {
                ResetGame();
                return;
            }

            if (IsGameOver)
            {
                RenderGame();
                UpdateScoreText();
                return;
            }

            if (RotateAction != null && RotateAction.WasPressedThisFrame())
            {
                TryRotateClockwise();
            }

            if (HardDropAction != null && HardDropAction.WasPressedThisFrame())
            {
                HardDrop();
            }

            float currentTime = Time.time;
            HandleHorizontalInput(currentTime);
            HandleSoftDropInput(currentTime);
            UpdateFalling();
            RenderGame();
            UpdateScoreText();
        }

        private void InitializeGame()
        {
            FieldWidth = Mathf.Max(4, FieldWidth);
            FieldHeight = Mathf.Max(4, FieldHeight);
            FallIntervalSeconds = Mathf.Max(0.05f, FallIntervalSeconds);

            if (ScorePerLine == null || ScorePerLine.Count == 0)
            {
                ScorePerLine = new List<int> { 100, 300, 500, 800 };
            }

            if (FieldBox == null || PreviewBox == null || ScoreText == null)
            {
                Debug.LogError("ChatGPT_GameManager requires FieldBox, PreviewBox, and ScoreText bindings.");
                return;
            }

            FieldBoxRenderer = FieldBox.GetComponent<SpriteRenderer>();
            PreviewBoxRenderer = PreviewBox.GetComponent<SpriteRenderer>();
            if (FieldBoxRenderer == null || PreviewBoxRenderer == null)
            {
                Debug.LogError("FieldBox and PreviewBox must contain SpriteRenderer components.");
                return;
            }

            Board = new int[FieldWidth, FieldHeight];
            CreateRenderRoots();
            RecalculateLayout();
            CreateOrUpdateBackground();

            FieldPool = new ChatGPT_BlockViewPool(FieldRenderRoot, FieldBoxRenderer.sprite, FieldBoxRenderer.sortingOrder + 2);
            PreviewPool = new ChatGPT_BlockViewPool(PreviewRenderRoot, PreviewBoxRenderer.sprite, PreviewBoxRenderer.sortingOrder + 1);

            ResetGame();
            IsInitialized = true;
        }

        private void CreateRenderRoots()
        {
            if (FieldRenderRoot == null)
            {
                GameObject fieldRootObject = new GameObject("ChatGPT_FieldRenderRoot");
                fieldRootObject.transform.SetParent(transform, false);
                FieldRenderRoot = fieldRootObject.transform;
            }

            if (PreviewRenderRoot == null)
            {
                GameObject previewRootObject = new GameObject("ChatGPT_PreviewRenderRoot");
                previewRootObject.transform.SetParent(transform, false);
                PreviewRenderRoot = previewRootObject.transform;
            }
        }

        private void RecalculateLayout()
        {
            Bounds fieldBounds = FieldBoxRenderer.bounds;
            CellSize = Mathf.Min(fieldBounds.size.x / FieldWidth, fieldBounds.size.y / FieldHeight);
            FieldActualWidth = CellSize * FieldWidth;
            FieldActualHeight = CellSize * FieldHeight;
            FieldOrigin = fieldBounds.center + new Vector3(-FieldActualWidth * 0.5f, -FieldActualHeight * 0.5f, 0f);

            Bounds previewBounds = PreviewBoxRenderer.bounds;
            PreviewCellSize = Mathf.Min(previewBounds.size.x / PreviewGridSize, previewBounds.size.y / PreviewGridSize);
            PreviewCenter = previewBounds.center;
        }

        private void CreateOrUpdateBackground()
        {
            if (FieldBackgroundObject == null)
            {
                FieldBackgroundObject = new GameObject("ChatGPT_FieldBackground");
                FieldBackgroundObject.transform.SetParent(FieldRenderRoot, false);
                FieldBackgroundRenderer = FieldBackgroundObject.AddComponent<SpriteRenderer>();
                FieldBackgroundRenderer.sprite = FieldBoxRenderer.sprite;
                FieldBackgroundRenderer.sortingOrder = FieldBoxRenderer.sortingOrder + 1;
                FieldBackgroundRenderer.color = Color.black;
            }

            float spriteWidth = Mathf.Max(0.0001f, FieldBackgroundRenderer.sprite.bounds.size.x);
            float spriteHeight = Mathf.Max(0.0001f, FieldBackgroundRenderer.sprite.bounds.size.y);
            FieldBackgroundObject.transform.position = FieldOrigin + new Vector3(FieldActualWidth * 0.5f, FieldActualHeight * 0.5f, 0f);
            FieldBackgroundObject.transform.localScale = new Vector3(FieldActualWidth / spriteWidth, FieldActualHeight / spriteHeight, 1f);
        }

        private void ResetGame()
        {
            Array.Clear(Board, 0, Board.Length);
            Bag.Clear();
            Score = 0;
            FallTimer = 0f;
            HasSpawnedFirstPiece = false;
            IsGameOver = false;
            WasMoveLeftPressed = false;
            WasMoveRightPressed = false;
            WasSoftDropPressed = false;

            NextPieceType = DrawFromBag();
            SpawnNextPiece();
            RenderGame();
            UpdateScoreText();
        }

        private void UpdateScoreText()
        {
            if (ScoreText == null)
            {
                return;
            }

            ScoreText.text = $"Score: {Score}";
        }

        private void SetupInput()
        {
            if (MoveLeftAction != null)
            {
                return;
            }

            MoveLeftAction = new InputAction("MoveLeft", InputActionType.Button);
            MoveLeftAction.AddBinding("<Keyboard>/leftArrow");
            MoveLeftAction.AddBinding("<Gamepad>/dpad/left");

            MoveRightAction = new InputAction("MoveRight", InputActionType.Button);
            MoveRightAction.AddBinding("<Keyboard>/rightArrow");
            MoveRightAction.AddBinding("<Gamepad>/dpad/right");

            SoftDropAction = new InputAction("SoftDrop", InputActionType.Button);
            SoftDropAction.AddBinding("<Keyboard>/downArrow");
            SoftDropAction.AddBinding("<Gamepad>/dpad/down");

            RotateAction = new InputAction("Rotate", InputActionType.Button);
            RotateAction.AddBinding("<Keyboard>/upArrow");
            RotateAction.AddBinding("<Gamepad>/dpad/up");

            HardDropAction = new InputAction("HardDrop", InputActionType.Button);
            HardDropAction.AddBinding("<Keyboard>/space");
            HardDropAction.AddBinding("<Gamepad>/buttonSouth");

            RestartAction = new InputAction("Restart", InputActionType.Button);
            RestartAction.AddBinding("<Keyboard>/enter");
            RestartAction.AddBinding("<Gamepad>/start");
        }

        private void EnableInput()
        {
            MoveLeftAction?.Enable();
            MoveRightAction?.Enable();
            SoftDropAction?.Enable();
            RotateAction?.Enable();
            HardDropAction?.Enable();
            RestartAction?.Enable();
        }

        private void DisableInput()
        {
            MoveLeftAction?.Disable();
            MoveRightAction?.Disable();
            SoftDropAction?.Disable();
            RotateAction?.Disable();
            HardDropAction?.Disable();
            RestartAction?.Disable();
        }

        private void DisposeInput()
        {
            MoveLeftAction?.Dispose();
            MoveLeftAction = null;
            MoveRightAction?.Dispose();
            MoveRightAction = null;
            SoftDropAction?.Dispose();
            SoftDropAction = null;
            RotateAction?.Dispose();
            RotateAction = null;
            HardDropAction?.Dispose();
            HardDropAction = null;
            RestartAction?.Dispose();
            RestartAction = null;
        }

        private void HandleHorizontalInput(float currentTime)
        {
            bool leftPressed = MoveLeftAction != null && MoveLeftAction.IsPressed();
            bool rightPressed = MoveRightAction != null && MoveRightAction.IsPressed();

            if (leftPressed && rightPressed)
            {
                WasMoveLeftPressed = true;
                WasMoveRightPressed = true;
                return;
            }

            ProcessRepeat(leftPressed, ref WasMoveLeftPressed, ref NextMoveLeftTime, currentTime, HorizontalRepeatInterval, Vector2Int.left);
            ProcessRepeat(rightPressed, ref WasMoveRightPressed, ref NextMoveRightTime, currentTime, HorizontalRepeatInterval, Vector2Int.right);
        }

        private void HandleSoftDropInput(float currentTime)
        {
            bool softDropPressed = SoftDropAction != null && SoftDropAction.IsPressed();
            if (!softDropPressed)
            {
                WasSoftDropPressed = false;
                return;
            }

            if (!WasSoftDropPressed)
            {
                SoftDropStep();
                NextSoftDropTime = currentTime + HoldRepeatDelay;
                WasSoftDropPressed = true;
                return;
            }

            if (currentTime >= NextSoftDropTime)
            {
                SoftDropStep();
                NextSoftDropTime = currentTime + SoftDropRepeatInterval;
            }
        }

        private void ProcessRepeat(bool isPressed, ref bool wasPressed, ref float nextTime, float currentTime, float repeatInterval, Vector2Int moveDelta)
        {
            if (!isPressed)
            {
                wasPressed = false;
                return;
            }

            if (!wasPressed)
            {
                TryMove(moveDelta);
                nextTime = currentTime + HoldRepeatDelay;
                wasPressed = true;
                return;
            }

            if (currentTime >= nextTime)
            {
                TryMove(moveDelta);
                nextTime = currentTime + repeatInterval;
            }
        }

        private void SoftDropStep()
        {
            if (!TryMove(Vector2Int.down))
            {
                LockCurrentPiece();
            }

            FallTimer = 0f;
        }

        private void UpdateFalling()
        {
            FallTimer += Time.deltaTime;
            if (FallTimer < FallIntervalSeconds)
            {
                return;
            }

            FallTimer = 0f;
            if (!TryMove(Vector2Int.down))
            {
                LockCurrentPiece();
            }
        }

        private bool TryMove(Vector2Int delta)
        {
            ChatGPT_PieceState candidate = CurrentPiece.WithPosition(CurrentPiece.Position + delta);
            if (!CanPlace(candidate))
            {
                return false;
            }

            CurrentPiece = candidate;
            return true;
        }

        private void TryRotateClockwise()
        {
            ChatGPT_PieceState rotatedPiece = CurrentPiece.WithRotation(CurrentPiece.Rotation + 1);
            Vector2Int[] kickOffsets =
            {
                Vector2Int.zero,
                Vector2Int.right,
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.up + Vector2Int.right,
                Vector2Int.up + Vector2Int.left,
                Vector2Int.right * 2,
                Vector2Int.left * 2,
            };

            for (int i = 0; i < kickOffsets.Length; ++i)
            {
                ChatGPT_PieceState candidate = rotatedPiece.WithPosition(rotatedPiece.Position + kickOffsets[i]);
                if (!CanPlace(candidate))
                {
                    continue;
                }

                CurrentPiece = candidate;
                return;
            }
        }

        private void HardDrop()
        {
            ChatGPT_PieceState ghostPiece = GetGhostPiece(CurrentPiece);
            CurrentPiece = ghostPiece;
            LockCurrentPiece();
        }

        private void LockCurrentPiece()
        {
            bool overflowed = false;
            Vector2Int[] cells = ChatGPT_Tetromino.GetCells(CurrentPiece.Type, CurrentPiece.Rotation);
            int tileValue = (int)CurrentPiece.Type + 1;

            for (int i = 0; i < cells.Length; ++i)
            {
                Vector2Int boardPosition = CurrentPiece.Position + cells[i];
                if (boardPosition.y >= FieldHeight)
                {
                    overflowed = true;
                    continue;
                }

                if (boardPosition.y < 0 || boardPosition.x < 0 || boardPosition.x >= FieldWidth)
                {
                    continue;
                }

                Board[boardPosition.x, boardPosition.y] = tileValue;
            }

            if (overflowed)
            {
                IsGameOver = true;
                return;
            }

            int clearedLineCount = ClearCompletedLines();
            ApplyScore(clearedLineCount);
            SpawnNextPiece();
        }

        private int ClearCompletedLines()
        {
            int clearedLineCount = 0;
            int row = 0;
            while (row < FieldHeight)
            {
                if (!IsRowFull(row))
                {
                    ++row;
                    continue;
                }

                CollapseFromRow(row);
                ++clearedLineCount;
            }

            return clearedLineCount;
        }

        private bool IsRowFull(int row)
        {
            for (int x = 0; x < FieldWidth; ++x)
            {
                if (Board[x, row] == 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void CollapseFromRow(int row)
        {
            for (int y = row; y < FieldHeight - 1; ++y)
            {
                for (int x = 0; x < FieldWidth; ++x)
                {
                    Board[x, y] = Board[x, y + 1];
                }
            }

            for (int x = 0; x < FieldWidth; ++x)
            {
                Board[x, FieldHeight - 1] = 0;
            }
        }

        private void ApplyScore(int clearedLineCount)
        {
            if (clearedLineCount <= 0)
            {
                return;
            }

            if (clearedLineCount - 1 < ScorePerLine.Count)
            {
                Score += ScorePerLine[clearedLineCount - 1];
                return;
            }

            Score += clearedLineCount * 100;
        }

        private void SpawnNextPiece()
        {
            CurrentPiece = new ChatGPT_PieceState(NextPieceType, new Vector2Int(FieldWidth / 2, FieldHeight), 0);
            NextPieceType = DrawFromBag();
            HasSpawnedFirstPiece = true;
            FallTimer = 0f;

            if (!CanPlace(CurrentPiece))
            {
                IsGameOver = true;
            }
        }

        private ChatGPT_PieceType DrawFromBag()
        {
            if (Bag.Count == 0)
            {
                RefillBag();
            }

            int lastIndex = Bag.Count - 1;
            ChatGPT_PieceType selectedType = Bag[lastIndex];
            Bag.RemoveAt(lastIndex);
            return selectedType;
        }

        private void RefillBag()
        {
            Bag.Clear();
            for (int i = 0; i < ChatGPT_Tetromino.AllTypes.Length; ++i)
            {
                Bag.Add(ChatGPT_Tetromino.AllTypes[i]);
            }

            for (int i = Bag.Count - 1; i > 0; --i)
            {
                int swapIndex = UnityEngine.Random.Range(0, i + 1);
                (Bag[i], Bag[swapIndex]) = (Bag[swapIndex], Bag[i]);
            }
        }

        private bool CanPlace(ChatGPT_PieceState piece)
        {
            Vector2Int[] cells = ChatGPT_Tetromino.GetCells(piece.Type, piece.Rotation);
            for (int i = 0; i < cells.Length; ++i)
            {
                Vector2Int boardPosition = piece.Position + cells[i];
                if (boardPosition.x < 0 || boardPosition.x >= FieldWidth)
                {
                    return false;
                }

                if (boardPosition.y < 0)
                {
                    return false;
                }

                if (boardPosition.y >= FieldHeight)
                {
                    continue;
                }

                if (Board[boardPosition.x, boardPosition.y] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        private ChatGPT_PieceState GetGhostPiece(ChatGPT_PieceState piece)
        {
            ChatGPT_PieceState ghostPiece = piece;
            while (CanPlace(ghostPiece.WithPosition(ghostPiece.Position + Vector2Int.down)))
            {
                ghostPiece = ghostPiece.WithPosition(ghostPiece.Position + Vector2Int.down);
            }

            return ghostPiece;
        }

        private void RenderGame()
        {
            if (FieldPool == null || PreviewPool == null)
            {
                return;
            }

            FieldPool.BeginFrame();
            PreviewPool.BeginFrame();
            DrawBoard();

            if (!IsGameOver)
            {
                ChatGPT_PieceState ghostPiece = GetGhostPiece(CurrentPiece);
                DrawPiece(ghostPiece, GhostAlpha);
                DrawPiece(CurrentPiece, 1f);
            }

            DrawPreview();
        }

        private void DrawBoard()
        {
            for (int y = 0; y < FieldHeight; ++y)
            {
                for (int x = 0; x < FieldWidth; ++x)
                {
                    int cellValue = Board[x, y];
                    if (cellValue <= 0)
                    {
                        continue;
                    }

                    ChatGPT_PieceType type = (ChatGPT_PieceType)(cellValue - 1);
                    Color color = ChatGPT_Tetromino.GetColor(type);
                    FieldPool.Draw(GetBoardCellCenter(new Vector2Int(x, y)), CellSize, color);
                }
            }
        }

        private void DrawPiece(ChatGPT_PieceState piece, float alpha)
        {
            Vector2Int[] cells = ChatGPT_Tetromino.GetCells(piece.Type, piece.Rotation);
            Color color = ChatGPT_Tetromino.GetColor(piece.Type);
            for (int i = 0; i < cells.Length; ++i)
            {
                Vector2Int boardPosition = piece.Position + cells[i];
                if (boardPosition.y < 0 || boardPosition.y >= FieldHeight)
                {
                    continue;
                }

                FieldPool.Draw(GetBoardCellCenter(boardPosition), CellSize, color, alpha);
            }
        }

        private void DrawPreview()
        {
            Vector2Int[] previewCells = ChatGPT_Tetromino.GetCells(NextPieceType, 0);
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            for (int i = 0; i < previewCells.Length; ++i)
            {
                minX = Mathf.Min(minX, previewCells[i].x);
                maxX = Mathf.Max(maxX, previewCells[i].x);
                minY = Mathf.Min(minY, previewCells[i].y);
                maxY = Mathf.Max(maxY, previewCells[i].y);
            }

            float previewWidth = (maxX - minX + 1) * PreviewCellSize;
            float previewHeight = (maxY - minY + 1) * PreviewCellSize;
            Vector3 bottomLeft = PreviewCenter + new Vector3(-previewWidth * 0.5f, -previewHeight * 0.5f, 0f);
            Color color = ChatGPT_Tetromino.GetColor(NextPieceType);

            for (int i = 0; i < previewCells.Length; ++i)
            {
                float localX = (previewCells[i].x - minX + 0.5f) * PreviewCellSize;
                float localY = (previewCells[i].y - minY + 0.5f) * PreviewCellSize;
                Vector3 worldPosition = bottomLeft + new Vector3(localX, localY, 0f);
                PreviewPool.Draw(worldPosition, PreviewCellSize, color);
            }
        }

        private Vector3 GetBoardCellCenter(Vector2Int boardPosition)
        {
            float x = FieldOrigin.x + (boardPosition.x + 0.5f) * CellSize;
            float y = FieldOrigin.y + (boardPosition.y + 0.5f) * CellSize;
            return new Vector3(x, y, 0f);
        }
    }
}
