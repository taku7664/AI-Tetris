using UnityEngine;

namespace ChatGpt
{
    /// <summary>
    /// Creates and recycles ChatGpt_Tetromino pieces.
    /// Uses an internal object pool for the Tetromino GameObjects themselves
    /// (each Tetromino manages its own pool of ChatGpt_Cell visuals).
    /// </summary>
    public class ChatGpt_Spawner : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────
        [SerializeField] private ChatGpt_Tetromino _tetrominoPrefab;
        [SerializeField] private ChatGpt_Cell _cellPrefab;

        // ── Runtime ──────────────────────────────────────────────────────────────
        private ChatGpt_Board _board;
        private ChatGpt_ObjectPool<ChatGpt_Cell> _cellPool;

        private ChatGpt_TetrominoType _nextType;
        private bool _initialised;

        // ── Public API ───────────────────────────────────────────────────────────

        public void Initialise(ChatGpt_Board board)
        {
            _board     = board;
            _cellPool  = new ChatGpt_ObjectPool<ChatGpt_Cell>(_cellPrefab, transform, 60);
            _nextType  = RandomType();
            _initialised = true;
        }

        /// <summary>
        /// Spawn the next queued piece and immediately queue a new 'next' type.
        /// Returns null if the spawn position is blocked (= game over).
        /// </summary>
        public ChatGpt_Tetromino SpawnNext()
        {
            if (!_initialised) return null;

            ChatGpt_TetrominoType typeToSpawn = _nextType;
            _nextType = RandomType();

            Vector2Int pivot = GetSpawnPivot(typeToSpawn);

            // Check if spawn position is valid; if not, game over
            Vector2Int[] cells = ChatGpt_TetrominoData.BuildRotations(
                ChatGpt_TetrominoData.Cells[(int)typeToSpawn])[0];

            if (!_board.IsValidPosition(cells, pivot))
                return null;

            ChatGpt_Tetromino piece = Instantiate(_tetrominoPrefab, transform);
            piece.Spawn(typeToSpawn, pivot, _board, _cellPool);
            return piece;
        }

        /// <summary>The type that will be spawned on the next call to SpawnNext.</summary>
        public ChatGpt_TetrominoType NextType => _nextType;

        /// <summary>Destroy and recycle a piece after it has been placed.</summary>
        public void RecyclePiece(ChatGpt_Tetromino piece)
        {
            if (piece == null) return;
            piece.Recycle();
            Destroy(piece.gameObject);
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private ChatGpt_TetrominoType RandomType()
        {
            int count = System.Enum.GetValues(typeof(ChatGpt_TetrominoType)).Length;
            return (ChatGpt_TetrominoType)Random.Range(0, count);
        }

        private Vector2Int GetSpawnPivot(ChatGpt_TetrominoType type)
        {
            // Retrieve field dimensions via board helper (we expose width/height)
            // Spawn near the top-centre of the board
            int boardWidth  = _board.Width;
            int boardHeight = _board.Height;

            return new Vector2Int(boardWidth / 2, boardHeight - 2);
        }
    }
}
