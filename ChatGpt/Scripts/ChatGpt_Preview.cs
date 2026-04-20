using UnityEngine;

namespace ChatGpt
{
    /// <summary>
    /// Renders the "next piece" preview in the PreviewBox UI element.
    /// Preview cells are laid out on a fixed 4×4 grid that is always square.
    /// </summary>
    public class ChatGpt_Preview : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────
        [SerializeField] private Transform _previewBox;   // The PreviewBox container
        [SerializeField] private ChatGpt_Cell _cellPrefab;

        // ── Runtime ──────────────────────────────────────────────────────────────
        private ChatGpt_ObjectPool<ChatGpt_Cell> _pool;
        private ChatGpt_Cell[] _currentCells;

        private const int PreviewGridSize = 4; // 4×4 preview grid
        private float _cellSize;
        private Vector3 _origin;

        // ── Public API ───────────────────────────────────────────────────────────

        public void Initialise()
        {
            _pool = new ChatGpt_ObjectPool<ChatGpt_Cell>(_cellPrefab, _previewBox, 8);
            RecalculateLayout();
        }

        /// <summary>Update the preview to display <paramref name="type"/>.</summary>
        public void ShowNext(ChatGpt_TetrominoType type)
        {
            // Return previous cells
            if (_currentCells != null)
                foreach (var c in _currentCells)
                    if (c != null) _pool.Return(c);

            Vector2Int[] cells = ChatGpt_TetrominoData.Cells[(int)type];
            Color color        = ChatGpt_TetrominoData.Colors[(int)type];

            _currentCells = new ChatGpt_Cell[cells.Length];

            // Find bounds to centre the piece in the preview grid
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            foreach (var c in cells)
            {
                if (c.x < minX) minX = c.x;
                if (c.x > maxX) maxX = c.x;
                if (c.y < minY) minY = c.y;
                if (c.y > maxY) maxY = c.y;
            }

            float offsetX = -((minX + maxX) * 0.5f) * _cellSize;
            float offsetY = -((minY + maxY) * 0.5f) * _cellSize;
            Vector3 centreOffset = new Vector3(offsetX, offsetY, 0f);

            for (int i = 0; i < cells.Length; i++)
            {
                ChatGpt_Cell cell = _pool.Get();
                cell.SetSize(_cellSize * 0.90f);
                cell.SetColor(color);
                Vector3 pos = _origin + new Vector3(cells[i].x * _cellSize, cells[i].y * _cellSize, -0.1f) + centreOffset;
                cell.SetPosition(pos);
                _currentCells[i] = cell;
            }
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private void RecalculateLayout()
        {
            RectTransform rt = _previewBox as RectTransform;
            float boxW, boxH;
            if (rt != null)
            {
                boxW = rt.rect.width;
                boxH = rt.rect.height;
            }
            else
            {
                boxW = _previewBox.localScale.x;
                boxH = _previewBox.localScale.y;
            }

            // Cells must be square; fit the 4-cell grid into the box
            _cellSize = Mathf.Min(boxW, boxH) / PreviewGridSize;

            Vector3 boxWorld = _previewBox.position;
            if (rt != null)
                boxWorld = _previewBox.TransformPoint(Vector3.zero); // centre of RT

            // Origin = centre of box (pieces are centred around it)
            _origin = boxWorld;
        }

        private void OnRectTransformDimensionsChange()
        {
            RecalculateLayout();
        }
    }
}
