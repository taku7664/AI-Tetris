using UnityEngine;

namespace Codex
{
    public sealed class Codex_PreviewRenderer
    {
        private const int PreviewGridSize = 4;
        private static readonly Color GameOverBlockColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private readonly Codex_BlockPool CellPool;
        private readonly Transform RuntimeRoot;
        private readonly float CellSize;
        private readonly Vector2 AreaMin;

        public Codex_PreviewRenderer(GameObject PreviewBoxObject)
        {
            SpriteRenderer PreviewBoxRenderer = PreviewBoxObject.GetComponent<SpriteRenderer>();
            if (PreviewBoxRenderer == null)
            {
                throw new MissingComponentException("PreviewBox requires SpriteRenderer.");
            }

            Bounds Bounds = PreviewBoxRenderer.bounds;
            CellSize = Mathf.Min(Bounds.size.x / PreviewGridSize, Bounds.size.y / PreviewGridSize);
            CellSize = Mathf.Max(0.0001f, CellSize);

            Vector2 AreaSize = new Vector2(CellSize * PreviewGridSize, CellSize * PreviewGridSize);
            AreaMin = new Vector2(Bounds.center.x - AreaSize.x * 0.5f, Bounds.center.y - AreaSize.y * 0.5f);

            RuntimeRoot = new GameObject("Codex_PreviewRuntime").transform;
            RuntimeRoot.position = Vector3.zero;
            RuntimeRoot.rotation = Quaternion.identity;
            RuntimeRoot.localScale = Vector3.one;

            Sprite SquareSprite = Codex_RenderUtils.GetOrCreateSquareSprite();
            CellPool = new Codex_BlockPool(RuntimeRoot, SquareSprite, "Codex_PreviewCell");
        }

        public void Render(Codex_TetrominoType PieceType, bool IsGameOver)
        {
            CellPool.BeginFrame();

            Vector2Int[] Cells = Codex_TetrominoData.GetCells(PieceType, 0);
            Color PieceColor = IsGameOver ? GameOverBlockColor : Codex_TetrominoData.GetColor(PieceType);

            int MinX = int.MaxValue;
            int MaxX = int.MinValue;
            int MinY = int.MaxValue;
            int MaxY = int.MinValue;

            for (int Index = 0; Index < Cells.Length; Index++)
            {
                MinX = Mathf.Min(MinX, Cells[Index].x);
                MaxX = Mathf.Max(MaxX, Cells[Index].x);
                MinY = Mathf.Min(MinY, Cells[Index].y);
                MaxY = Mathf.Max(MaxY, Cells[Index].y);
            }

            float PieceWidth = (MaxX - MinX) + 1f;
            float PieceHeight = (MaxY - MinY) + 1f;
            float OffsetX = ((PreviewGridSize - PieceWidth) * 0.5f) - MinX;
            float OffsetY = ((PreviewGridSize - PieceHeight) * 0.5f) - MinY;

            for (int Index = 0; Index < Cells.Length; Index++)
            {
                float GridX = Cells[Index].x + OffsetX;
                float GridY = Cells[Index].y + OffsetY;
                Vector3 Position = new Vector3(
                    AreaMin.x + ((GridX + 0.5f) * CellSize),
                    AreaMin.y + ((GridY + 0.5f) * CellSize),
                    0f);

                CellPool.DrawBlock(Position, CellSize, PieceColor, 2);
            }
        }

        public void Dispose()
        {
            CellPool.Dispose();
            if (RuntimeRoot != null)
            {
                Object.Destroy(RuntimeRoot.gameObject);
            }
        }
    }
}
