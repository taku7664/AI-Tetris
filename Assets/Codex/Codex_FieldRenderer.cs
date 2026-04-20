using UnityEngine;

namespace Codex
{
    public sealed class Codex_FieldRenderer
    {
        private readonly int FieldWidth;
        private readonly int FieldHeight;
        private readonly Codex_BlockPool CellPool;
        private readonly SpriteRenderer AvailableAreaRenderer;
        private readonly Transform RuntimeRoot;

        private readonly float CellSize;
        private readonly Vector2 AreaMin;

        public Codex_FieldRenderer(GameObject FieldBoxObject, int FieldWidth, int FieldHeight)
        {
            this.FieldWidth = Mathf.Max(1, FieldWidth);
            this.FieldHeight = Mathf.Max(1, FieldHeight);

            SpriteRenderer FieldBoxRenderer = FieldBoxObject.GetComponent<SpriteRenderer>();
            if (FieldBoxRenderer == null)
            {
                throw new MissingComponentException("FieldBox requires SpriteRenderer.");
            }

            Bounds Bounds = FieldBoxRenderer.bounds;
            CellSize = Mathf.Min(Bounds.size.x / this.FieldWidth, Bounds.size.y / this.FieldHeight);
            CellSize = Mathf.Max(0.0001f, CellSize);

            Vector2 AreaSize = new Vector2(CellSize * this.FieldWidth, CellSize * this.FieldHeight);
            AreaMin = new Vector2(Bounds.center.x - AreaSize.x * 0.5f, Bounds.center.y - AreaSize.y * 0.5f);

            RuntimeRoot = new GameObject("Codex_FieldRuntime").transform;
            RuntimeRoot.position = Vector3.zero;
            RuntimeRoot.rotation = Quaternion.identity;
            RuntimeRoot.localScale = Vector3.one;

            Sprite SquareSprite = Codex_RenderUtils.GetOrCreateSquareSprite();

            GameObject AvailableAreaObject = new GameObject("Codex_FieldAvailableArea");
            AvailableAreaObject.transform.SetParent(RuntimeRoot, false);
            AvailableAreaRenderer = AvailableAreaObject.AddComponent<SpriteRenderer>();
            AvailableAreaRenderer.sprite = SquareSprite;
            AvailableAreaRenderer.color = Color.black;
            AvailableAreaRenderer.sortingOrder = 1;
            AvailableAreaRenderer.transform.position =
                new Vector3(AreaMin.x + AreaSize.x * 0.5f, AreaMin.y + AreaSize.y * 0.5f, 0f);
            AvailableAreaRenderer.transform.localScale = new Vector3(AreaSize.x, AreaSize.y, 1f);

            CellPool = new Codex_BlockPool(RuntimeRoot, SquareSprite, "Codex_FieldCell");
        }

        public void Render(Codex_BoardState BoardState, Codex_PieceState ActivePiece, Codex_PieceState GhostPiece, bool IsGameOver)
        {
            CellPool.BeginFrame();
            DrawPlacedBlocks(BoardState);

            if (!IsGameOver)
            {
                DrawPiece(GhostPiece, true, 3);
            }

            DrawPiece(ActivePiece, false, 4);
        }

        public void Dispose()
        {
            CellPool.Dispose();
            if (AvailableAreaRenderer != null)
            {
                Object.Destroy(AvailableAreaRenderer.gameObject);
            }

            if (RuntimeRoot != null)
            {
                Object.Destroy(RuntimeRoot.gameObject);
            }
        }

        private void DrawPlacedBlocks(Codex_BoardState BoardState)
        {
            for (int Y = 0; Y < FieldHeight; Y++)
            {
                for (int X = 0; X < FieldWidth; X++)
                {
                    if (!BoardState.TryGetCellType(X, Y, out Codex_TetrominoType Type))
                    {
                        continue;
                    }

                    DrawCell(X, Y, Codex_TetrominoData.GetColor(Type), 2);
                }
            }
        }

        private void DrawPiece(Codex_PieceState Piece, bool IsGhost, int SortingOrder)
        {
            Vector2Int[] Cells = Codex_TetrominoData.GetCells(Piece.Type, Piece.Rotation);
            Color PieceColor = Codex_TetrominoData.GetColor(Piece.Type);
            if (IsGhost)
            {
                PieceColor.a = 0.2f;
            }

            for (int Index = 0; Index < Cells.Length; Index++)
            {
                Vector2Int GridPosition = Piece.Pivot + Cells[Index];
                if (GridPosition.x < 0 || GridPosition.x >= FieldWidth || GridPosition.y < 0 || GridPosition.y >= FieldHeight)
                {
                    continue;
                }

                DrawCell(GridPosition.x, GridPosition.y, PieceColor, SortingOrder);
            }
        }

        private void DrawCell(int X, int Y, Color ColorValue, int SortingOrder)
        {
            Vector3 WorldPosition = new Vector3(
                AreaMin.x + ((X + 0.5f) * CellSize),
                AreaMin.y + ((Y + 0.5f) * CellSize),
                0f);

            CellPool.DrawBlock(WorldPosition, CellSize, ColorValue, SortingOrder);
        }
    }
}
