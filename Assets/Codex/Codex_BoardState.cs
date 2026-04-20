using UnityEngine;

namespace Codex
{
    public sealed class Codex_BoardState
    {
        private readonly Codex_TetrominoType?[,] OccupiedTypes;

        public Codex_BoardState(int Width, int Height)
        {
            this.Width = Mathf.Max(1, Width);
            this.Height = Mathf.Max(1, Height);
            OccupiedTypes = new Codex_TetrominoType?[this.Width, this.Height];
        }

        public int Width { get; }
        public int Height { get; }

        public void Clear()
        {
            for (int X = 0; X < Width; X++)
            {
                for (int Y = 0; Y < Height; Y++)
                {
                    OccupiedTypes[X, Y] = null;
                }
            }
        }

        public bool CanPlace(Codex_PieceState Piece)
        {
            Vector2Int[] PieceCells = Codex_TetrominoData.GetCells(Piece.Type, Piece.Rotation);
            for (int Index = 0; Index < PieceCells.Length; Index++)
            {
                Vector2Int GridPosition = Piece.Pivot + PieceCells[Index];
                if (IsBlocked(GridPosition.x, GridPosition.y))
                {
                    return false;
                }
            }

            return true;
        }

        public bool LockPiece(Codex_PieceState Piece)
        {
            bool ExceededTop = false;
            Vector2Int[] PieceCells = Codex_TetrominoData.GetCells(Piece.Type, Piece.Rotation);

            for (int Index = 0; Index < PieceCells.Length; Index++)
            {
                Vector2Int GridPosition = Piece.Pivot + PieceCells[Index];

                if (GridPosition.y >= Height)
                {
                    ExceededTop = true;
                    continue;
                }

                if (GridPosition.y < 0 || GridPosition.x < 0 || GridPosition.x >= Width)
                {
                    continue;
                }

                OccupiedTypes[GridPosition.x, GridPosition.y] = Piece.Type;
            }

            return ExceededTop;
        }

        public int ClearCompletedLines()
        {
            int ClearedLineCount = 0;

            for (int RowIndex = 0; RowIndex < Height; RowIndex++)
            {
                if (!IsLineFull(RowIndex))
                {
                    continue;
                }

                CollapseRow(RowIndex);
                ClearedLineCount++;
                RowIndex--;
            }

            return ClearedLineCount;
        }

        public bool TryGetCellType(int X, int Y, out Codex_TetrominoType Type)
        {
            Type = default;
            if (X < 0 || X >= Width || Y < 0 || Y >= Height)
            {
                return false;
            }

            Codex_TetrominoType? StoredType = OccupiedTypes[X, Y];
            if (!StoredType.HasValue)
            {
                return false;
            }

            Type = StoredType.Value;
            return true;
        }

        private bool IsBlocked(int X, int Y)
        {
            if (X < 0 || X >= Width || Y < 0)
            {
                return true;
            }

            if (Y >= Height)
            {
                return false;
            }

            return OccupiedTypes[X, Y].HasValue;
        }

        private bool IsLineFull(int RowIndex)
        {
            for (int X = 0; X < Width; X++)
            {
                if (!OccupiedTypes[X, RowIndex].HasValue)
                {
                    return false;
                }
            }

            return true;
        }

        private void CollapseRow(int RowIndex)
        {
            for (int Y = RowIndex; Y < Height - 1; Y++)
            {
                for (int X = 0; X < Width; X++)
                {
                    OccupiedTypes[X, Y] = OccupiedTypes[X, Y + 1];
                }
            }

            int TopRow = Height - 1;
            for (int X = 0; X < Width; X++)
            {
                OccupiedTypes[X, TopRow] = null;
            }
        }
    }
}
