using UnityEngine;

namespace ChatGpt
{
    /// <summary>
    /// Manages the Tetris playing field grid.
    /// Stores placed cell colours, validates positions, clears completed lines,
    /// and refreshes the visual representation using pooled ChatGpt_Cell objects.
    /// </summary>
    public class ChatGpt_Board : MonoBehaviour
    {
        // ── Inspector refs ───────────────────────────────────────────────────────
        [SerializeField] private Transform _fieldBox;       // FieldBox RectTransform
        [SerializeField] private ChatGpt_Cell _cellPrefab;

        // ── Runtime ──────────────────────────────────────────────────────────────
        private int _width;
        private int _height;
        private Color?[,] _grid;                // null = empty, Color = occupied
        private ChatGpt_Cell[,] _visualCells;
        private ChatGpt_ObjectPool<ChatGpt_Cell> _pool;

        private float _cellSize;                // world-unit size per cell
        private Vector3 _origin;               // world position of cell (0,0)

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Initialise (or re-initialise) the board with new dimensions.
        /// Call this once from GameManager before the first spawn.
        /// </summary>
        public void Initialise(int width, int height)
        {
            bool dimensionsChanged = (width != _width || height != _height);

            _width  = width;
            _height = height;

            // On first run _pool is null; on restart we reuse the existing pool
            // to avoid leaking GameObjects.
            if (_pool == null || dimensionsChanged)
            {
                // Destroy old visual cells explicitly before replacing the pool
                if (_visualCells != null)
                {
                    for (int x = 0; x < _visualCells.GetLength(0); x++)
                        for (int y = 0; y < _visualCells.GetLength(1); y++)
                            if (_visualCells[x, y] != null)
                                Destroy(_visualCells[x, y].gameObject);
                    _visualCells = null;
                }
                _pool = new ChatGpt_ObjectPool<ChatGpt_Cell>(_cellPrefab, _fieldBox, width * height);
            }
            else
            {
                // Reuse pool – return any existing visual cells
                ReturnAllVisuals();
            }

            _grid        = new Color?[width, height];
            _visualCells = new ChatGpt_Cell[width, height];

            RecalculateCellSize();
        }

        /// <summary>
        /// Returns true when all cell positions in <paramref name="cells"/> are inside
        /// the board and not already occupied.
        /// </summary>
        public bool IsValidPosition(Vector2Int[] cells, Vector2Int pivot)
        {
            foreach (var c in cells)
            {
                Vector2Int pos = pivot + c;
                if (pos.x < 0 || pos.x >= _width) return false;
                if (pos.y < 0)                     return false;
                if (pos.y >= _height)              continue; // Standard Tetris: pieces may extend above the visible field during spawn; these cells are always valid
                if (_grid[pos.x, pos.y].HasValue)  return false;
            }
            return true;
        }

        /// <summary>
        /// Bake a tetromino into the grid permanently using absolute board coordinates.
        /// Returns the number of lines cleared.
        /// </summary>
        public int PlaceTetromino(Vector2Int[] absoluteCells, Color color)
        {
            foreach (var pos in absoluteCells)
            {
                if (pos.y >= 0 && pos.y < _height && pos.x >= 0 && pos.x < _width)
                    _grid[pos.x, pos.y] = color;
            }

            int cleared = ClearFullLines();
            RebuildVisuals();
            return cleared;
        }

        /// <summary>Check whether any placed cell occupies row >= height (top overflow → game over).</summary>
        public bool IsOverflowing()
        {
            for (int x = 0; x < _width; x++)
                if (_grid[x, _height - 1].HasValue)
                    return true;
            return false;
        }

        /// <summary>Clear all cells (used on restart).</summary>
        public void ClearBoard()
        {
            for (int x = 0; x < _width; x++)
                for (int y = 0; y < _height; y++)
                    _grid[x, y] = null;

            RebuildVisuals();
        }

        /// <summary>World-space position for a grid coordinate.</summary>
        public Vector3 CellToWorld(Vector2Int cell) =>
            _origin + new Vector3(cell.x * _cellSize, cell.y * _cellSize, 0f);

        public float CellSize => _cellSize;
        public int Width  => _width;
        public int Height => _height;

        // ── Private helpers ──────────────────────────────────────────────────────

        private void RecalculateCellSize()
        {
            // Fit the grid into the FieldBox rect
            RectTransform rt = _fieldBox as RectTransform;
            float boxW, boxH;
            if (rt != null)
            {
                boxW = rt.rect.width;
                boxH = rt.rect.height;
            }
            else
            {
                // Fallback: use the sprite/transform scale
                boxW = _fieldBox.localScale.x;
                boxH = _fieldBox.localScale.y;
            }

            float cellW = boxW / _width;
            float cellH = boxH / _height;
            _cellSize = Mathf.Min(cellW, cellH);

            // Centre the grid inside the box
            float gridW = _cellSize * _width;
            float gridH = _cellSize * _height;

            Vector3 boxWorld = _fieldBox.position;
            if (rt != null)
            {
                // rect pivot is centre by default; shift to bottom-left
                boxWorld = _fieldBox.TransformPoint(new Vector3(-rt.rect.width * 0.5f, -rt.rect.height * 0.5f, 0f));
            }

            _origin = boxWorld + new Vector3(
                (boxW - gridW) * 0.5f + _cellSize * 0.5f,
                (boxH - gridH) * 0.5f + _cellSize * 0.5f,
                0f);
        }

        private void ReturnAllVisuals()
        {
            if (_visualCells == null) return;
            for (int x = 0; x < _visualCells.GetLength(0); x++)
                for (int y = 0; y < _visualCells.GetLength(1); y++)
                    if (_visualCells[x, y] != null)
                    {
                        _pool.Return(_visualCells[x, y]);
                        _visualCells[x, y] = null;
                    }
        }

        private void RebuildVisuals()
        {
            // Return all old visual cells to pool
            ReturnAllVisuals();

            // Recreate visuals for occupied cells
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_grid[x, y].HasValue)
                    {
                        ChatGpt_Cell cell = _pool.Get();
                        cell.SetSize(_cellSize * 0.95f);
                        cell.SetColor(_grid[x, y].Value);
                        cell.SetPosition(CellToWorld(new Vector2Int(x, y)));
                        _visualCells[x, y] = cell;
                    }
                }
            }
        }

        private int ClearFullLines()
        {
            int cleared = 0;
            for (int y = _height - 1; y >= 0; y--)
            {
                if (IsLineFull(y))
                {
                    RemoveLine(y);
                    cleared++;
                    y++; // re-check the same row index after shifting down
                }
            }
            return cleared;
        }

        private bool IsLineFull(int row)
        {
            for (int x = 0; x < _width; x++)
                if (!_grid[x, row].HasValue) return false;
            return true;
        }

        private void RemoveLine(int row)
        {
            // Shift everything above down by 1
            for (int y = row; y < _height - 1; y++)
                for (int x = 0; x < _width; x++)
                    _grid[x, y] = _grid[x, y + 1];

            // Clear the top row
            for (int x = 0; x < _width; x++)
                _grid[x, _height - 1] = null;
        }

        // Re-compute layout when the screen/container changes at runtime
        private void OnRectTransformDimensionsChange()
        {
            if (_grid == null) return;
            RecalculateCellSize();
            RebuildVisuals();
        }
    }
}
