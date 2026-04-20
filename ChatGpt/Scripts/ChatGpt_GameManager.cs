using UnityEngine;

namespace ChatGpt
{
    /// <summary>
    /// Central game controller.
    ///
    /// SerializeFields (as required):
    ///   - Field width  (default 10)
    ///   - Field height (default 20)
    ///   - Fall speed in seconds per row (default 0.5)
    ///   - Score added per cleared line (default 100)
    ///
    /// Owns the main game-loop state machine (Playing / GameOver).
    /// Wires together all subsystems: Board, Spawner, Preview, InputHandler, UIManager.
    /// </summary>
    public class ChatGpt_GameManager : MonoBehaviour
    {
        // ── Inspector – configurable data fields ──────────────────────────────
        [Header("Field Settings")]
        [SerializeField] private int _fieldWidth  = 10;
        [SerializeField] private int _fieldHeight = 20;

        [Header("Gameplay Settings")]
        [SerializeField] private float _fallSpeedSeconds = 0.5f;
        [SerializeField] private int   _scorePerLine     = 100;

        [Header("References")]
        [SerializeField] private ChatGpt_Board         _board;
        [SerializeField] private ChatGpt_Spawner       _spawner;
        [SerializeField] private ChatGpt_Preview       _preview;
        [SerializeField] private ChatGpt_InputHandler  _inputHandler;
        [SerializeField] private ChatGpt_UIManager     _uiManager;

        // ── Runtime state ─────────────────────────────────────────────────────
        private ChatGpt_Tetromino _activePiece;
        private float _fallTimer;
        private int   _score;
        private bool  _isGameOver;
        private bool  _isPlaying;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Start()
        {
            StartGame();
        }

        private void OnEnable()
        {
            if (_inputHandler == null) return;
            _inputHandler.OnMoveLeft  += HandleMoveLeft;
            _inputHandler.OnMoveRight += HandleMoveRight;
            _inputHandler.OnMoveDown  += HandleMoveDown;
            _inputHandler.OnRotateCW  += HandleRotateCW;
            _inputHandler.OnHardDrop  += HandleHardDrop;
            _inputHandler.OnRestart   += HandleRestart;
        }

        private void OnDisable()
        {
            if (_inputHandler == null) return;
            _inputHandler.OnMoveLeft  -= HandleMoveLeft;
            _inputHandler.OnMoveRight -= HandleMoveRight;
            _inputHandler.OnMoveDown  -= HandleMoveDown;
            _inputHandler.OnRotateCW  -= HandleRotateCW;
            _inputHandler.OnHardDrop  -= HandleHardDrop;
            _inputHandler.OnRestart   -= HandleRestart;
        }

        private void Update()
        {
            if (!_isPlaying) return;

            _fallTimer += Time.deltaTime;
            if (_fallTimer >= _fallSpeedSeconds)
            {
                _fallTimer -= _fallSpeedSeconds;
                StepDown();
            }
        }

        // ── Input handlers ────────────────────────────────────────────────────

        private void HandleMoveLeft()
        {
            if (!_isPlaying || _activePiece == null) return;
            _activePiece.MoveLeft();
        }

        private void HandleMoveRight()
        {
            if (!_isPlaying || _activePiece == null) return;
            _activePiece.MoveRight();
        }

        private void HandleMoveDown()
        {
            if (!_isPlaying || _activePiece == null) return;
            StepDown();
            // Reset auto-fall timer so that fast-drop doesn't double-step
            _fallTimer = 0f;
        }

        private void HandleRotateCW()
        {
            if (!_isPlaying || _activePiece == null) return;
            _activePiece.RotateCW();
        }

        private void HandleHardDrop()
        {
            if (!_isPlaying || _activePiece == null) return;
            _activePiece.HardDrop();
            LockPiece();
        }

        private void HandleRestart()
        {
            StartGame();
        }

        // ── Game flow ─────────────────────────────────────────────────────────

        private void StartGame()
        {
            // Reset state
            _score      = 0;
            _isGameOver = false;
            _isPlaying  = false;
            _fallTimer  = 0f;

            // Initialise subsystems
            _board.Initialise(_fieldWidth, _fieldHeight);
            _spawner.Initialise(_board);
            _preview.Initialise();

            // Show initial UI
            _uiManager?.UpdateScore(_score);
            _uiManager?.ShowGameOver(false);

            // Update preview with the first queued piece
            _preview.ShowNext(_spawner.NextType);

            // Recycle any lingering active piece
            if (_activePiece != null)
            {
                _spawner.RecyclePiece(_activePiece);
                _activePiece = null;
            }

            SpawnNext();
        }

        private void SpawnNext()
        {
            _activePiece = _spawner.SpawnNext();

            if (_activePiece == null)
            {
                // Spawn position blocked → game over
                GameOver();
                return;
            }

            // Update preview for the new 'next' piece
            _preview.ShowNext(_spawner.NextType);

            _isPlaying = true;
            _fallTimer = 0f;
        }

        private void StepDown()
        {
            if (_activePiece == null) return;

            bool moved = _activePiece.MoveDown();
            if (!moved)
                LockPiece();
        }

        private void LockPiece()
        {
            if (_activePiece == null) return;

            // Bake into the board and score cleared lines
            int cleared = _board.PlaceTetromino(
                _activePiece.GetBoardCells(),
                _activePiece.Color);

            _score += cleared * _scorePerLine;
            _uiManager?.UpdateScore(_score);

            _spawner.RecyclePiece(_activePiece);
            _activePiece = null;

            if (_board.IsOverflowing())
            {
                GameOver();
                return;
            }

            SpawnNext();
        }

        private void GameOver()
        {
            _isGameOver = true;
            _isPlaying  = false;
            _uiManager?.ShowGameOver(true);
            Debug.Log($"[ChatGpt_GameManager] Game Over! Final score: {_score}");
        }
    }
}
