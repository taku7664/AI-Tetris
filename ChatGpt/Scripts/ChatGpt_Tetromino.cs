using UnityEngine;

namespace ChatGpt
{
    /// <summary>
    /// Represents the currently-falling tetromino piece.
    /// Handles movement, SRS rotation, ghost-piece calculation, and hard-drop.
    /// Visual cells are pooled ChatGpt_Cell objects managed here.
    /// </summary>
    public class ChatGpt_Tetromino : MonoBehaviour
    {
        // ── State ────────────────────────────────────────────────────────────────
        private ChatGpt_TetrominoType _type;
        private int _rotationIndex;
        private Vector2Int _pivot;          // board-space pivot
        private Vector2Int[][] _rotations;  // all 4 rotation states
        private Color _color;

        // Visual cells for the active piece
        private ChatGpt_Cell[] _cells;
        private ChatGpt_ObjectPool<ChatGpt_Cell> _pool;

        // Ghost-piece cells (shows where it will land)
        private ChatGpt_Cell[] _ghostCells;

        // References
        private ChatGpt_Board _board;

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Spawn this tetromino on the board. Called by the Spawner.
        /// </summary>
        public void Spawn(ChatGpt_TetrominoType type, Vector2Int spawnPivot,
                          ChatGpt_Board board, ChatGpt_ObjectPool<ChatGpt_Cell> pool)
        {
            _type          = type;
            _pivot         = spawnPivot;
            _board         = board;
            _pool          = pool;
            _rotationIndex = 0;
            _color         = ChatGpt_TetrominoData.Colors[(int)type];
            _rotations     = ChatGpt_TetrominoData.BuildRotations(
                                 ChatGpt_TetrominoData.Cells[(int)type]);

            // Allocate visual cells
            int cellCount = _rotations[0].Length;
            _cells      = new ChatGpt_Cell[cellCount];
            _ghostCells = new ChatGpt_Cell[cellCount];

            for (int i = 0; i < cellCount; i++)
            {
                _cells[i] = _pool.Get();
                _cells[i].SetSize(_board.CellSize * 0.95f);
                _cells[i].SetColor(_color);

                _ghostCells[i] = _pool.Get();
                _ghostCells[i].SetSize(_board.CellSize * 0.95f);
                Color ghost = _color;
                ghost.a = 0.30f;
                _ghostCells[i].SetColor(ghost);
            }

            UpdateVisuals();
        }

        /// <summary>Attempt to move one step downward. Returns false if blocked.</summary>
        public bool MoveDown()  => TryMove(Vector2Int.down);
        public bool MoveLeft()  => TryMove(Vector2Int.left);
        public bool MoveRight() => TryMove(Vector2Int.right);

        /// <summary>Rotate clockwise using SRS wall kicks. Returns false if not possible.</summary>
        public bool RotateCW()
        {
            int nextRot = (_rotationIndex + 1) % 4;
            return TryRotate(nextRot);
        }

        /// <summary>
        /// Drop the piece instantly to the lowest valid position.
        /// Returns the number of rows dropped.
        /// </summary>
        public int HardDrop()
        {
            int rows = 0;
            while (TryMove(Vector2Int.down))
                rows++;
            return rows;
        }

        /// <summary>Current cells in board-space.</summary>
        public Vector2Int[] GetBoardCells()
        {
            Vector2Int[] current = _rotations[_rotationIndex];
            Vector2Int[] result  = new Vector2Int[current.Length];
            for (int i = 0; i < current.Length; i++)
                result[i] = _pivot + current[i];
            return result;
        }

        public Color Color => _color;
        public ChatGpt_TetrominoType Type => _type;

        /// <summary>Return all visual cells to the pool (call before destroying/recycling).</summary>
        public void Recycle()
        {
            if (_cells != null)
                foreach (var c in _cells)
                    if (c != null) _pool.Return(c);

            if (_ghostCells != null)
                foreach (var c in _ghostCells)
                    if (c != null) _pool.Return(c);

            _cells      = null;
            _ghostCells = null;
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private bool TryMove(Vector2Int delta)
        {
            Vector2Int newPivot = _pivot + delta;
            if (_board.IsValidPosition(_rotations[_rotationIndex], newPivot))
            {
                _pivot = newPivot;
                UpdateVisuals();
                return true;
            }
            return false;
        }

        private bool TryRotate(int nextRot)
        {
            Vector2Int[,] kicks = ChatGpt_TetrominoData.GetWallKicks(_type);
            Vector2Int[] nextCells = _rotations[nextRot];

            for (int k = 0; k < 5; k++)
            {
                Vector2Int kick   = kicks[_rotationIndex, k];
                Vector2Int newPivot = _pivot + kick;
                if (_board.IsValidPosition(nextCells, newPivot))
                {
                    _pivot         = newPivot;
                    _rotationIndex = nextRot;
                    UpdateVisuals();
                    return true;
                }
            }
            return false;
        }

        private void UpdateVisuals()
        {
            Vector2Int[] current = _rotations[_rotationIndex];

            // Compute ghost position
            Vector2Int ghostPivot = _pivot;
            while (_board.IsValidPosition(current, ghostPivot + Vector2Int.down))
                ghostPivot += Vector2Int.down;

            for (int i = 0; i < current.Length; i++)
            {
                // Render actual piece on top layer (z = -0.1)
                Vector3 worldPos = _board.CellToWorld(_pivot + current[i]);
                worldPos.z = -0.1f;
                _cells[i].SetPosition(worldPos);
                _cells[i].SetSize(_board.CellSize * 0.95f);

                // Render ghost piece slightly behind (z = 0)
                Vector3 ghostPos = _board.CellToWorld(ghostPivot + current[i]);
                ghostPos.z = 0f;
                _ghostCells[i].SetPosition(ghostPos);
                _ghostCells[i].SetSize(_board.CellSize * 0.95f);

                // Hide ghost if it overlaps the actual piece
                _ghostCells[i].gameObject.SetActive(ghostPivot != _pivot);
            }
        }
    }
}
