using System;
using System.Collections.Generic;
using UnityEngine;

namespace Codex
{
    public sealed class Codex_TetrisGame
    {
        private static readonly Vector2Int[] RotationKickOffsets =
        {
            Vector2Int.zero,
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.left * 2,
            Vector2Int.right * 2,
            Vector2Int.up,
        };

        private readonly List<int> LineClearScores;
        private readonly List<Codex_TetrominoType> PieceBag;
        private readonly System.Random RandomGenerator;

        private float FallTimer;

        public Codex_TetrisGame(int FieldWidth, int FieldHeight, float FallIntervalSeconds, IList<int> LineClearScores)
        {
            BoardState = new Codex_BoardState(FieldWidth, FieldHeight);
            this.FallIntervalSeconds = Mathf.Max(0.01f, FallIntervalSeconds);
            this.LineClearScores = new List<int>(LineClearScores ?? Array.Empty<int>());
            PieceBag = new List<Codex_TetrominoType>(7);
            RandomGenerator = new System.Random();

            StartNewGame();
        }

        public Codex_BoardState BoardState { get; }
        public Codex_PieceState CurrentPiece { get; private set; }
        public Codex_TetrominoType NextPieceType { get; private set; }
        public int Score { get; private set; }
        public bool IsGameOver { get; private set; }

        private float FallIntervalSeconds { get; }

        public void StartNewGame()
        {
            BoardState.Clear();
            PieceBag.Clear();
            Score = 0;
            IsGameOver = false;
            FallTimer = 0f;

            NextPieceType = DequeuePieceType();
            SpawnNextPiece();
        }

        public void Tick(float DeltaTime, bool IsSoftDropPressed)
        {
            if (IsGameOver)
            {
                return;
            }

            float SoftDropMultiplier = IsSoftDropPressed ? 8f : 1f;
            FallTimer += Mathf.Max(0f, DeltaTime) * SoftDropMultiplier;

            while (FallTimer >= FallIntervalSeconds)
            {
                FallTimer -= FallIntervalSeconds;
                if (!TryStepDown())
                {
                    break;
                }
            }
        }

        public bool TryMoveHorizontal(int Direction)
        {
            if (Direction == 0 || IsGameOver)
            {
                return false;
            }

            int ClampedDirection = Mathf.Clamp(Direction, -1, 1);
            return TryMove(new Vector2Int(ClampedDirection, 0));
        }

        public bool TryRotateClockwise()
        {
            if (IsGameOver)
            {
                return false;
            }

            int TargetRotation = (CurrentPiece.Rotation + 1) % 4;
            for (int Index = 0; Index < RotationKickOffsets.Length; Index++)
            {
                Vector2Int TargetPivot = CurrentPiece.Pivot + RotationKickOffsets[Index];
                Codex_PieceState CandidatePiece = new Codex_PieceState(CurrentPiece.Type, TargetRotation, TargetPivot);
                if (!BoardState.CanPlace(CandidatePiece))
                {
                    continue;
                }

                CurrentPiece = CandidatePiece;
                return true;
            }

            return false;
        }

        public void HardDrop()
        {
            if (IsGameOver)
            {
                return;
            }

            while (TryMove(Vector2Int.down))
            {
            }

            LockCurrentPiece();
        }

        public Codex_PieceState GetGhostPiece()
        {
            Codex_PieceState GhostPiece = CurrentPiece;

            while (BoardState.CanPlace(GhostPiece.WithPivot(GhostPiece.Pivot + Vector2Int.down)))
            {
                GhostPiece = GhostPiece.WithPivot(GhostPiece.Pivot + Vector2Int.down);
            }

            return GhostPiece;
        }

        private bool TryStepDown()
        {
            if (TryMove(Vector2Int.down))
            {
                return true;
            }

            LockCurrentPiece();
            return false;
        }

        private bool TryMove(Vector2Int Offset)
        {
            Codex_PieceState CandidatePiece = CurrentPiece.WithPivot(CurrentPiece.Pivot + Offset);
            if (!BoardState.CanPlace(CandidatePiece))
            {
                return false;
            }

            CurrentPiece = CandidatePiece;
            return true;
        }

        private void LockCurrentPiece()
        {
            bool ExceededTop = BoardState.LockPiece(CurrentPiece);
            int ClearedLineCount = BoardState.ClearCompletedLines();

            if (ClearedLineCount > 0)
            {
                Score += ResolveLineClearScore(ClearedLineCount);
            }

            if (ExceededTop)
            {
                IsGameOver = true;
                return;
            }

            SpawnNextPiece();
        }

        private int ResolveLineClearScore(int ClearedLineCount)
        {
            if (ClearedLineCount <= 0 || LineClearScores.Count == 0)
            {
                return 0;
            }

            int Index = Mathf.Min(ClearedLineCount, LineClearScores.Count) - 1;
            return Mathf.Max(0, LineClearScores[Index]);
        }

        private void SpawnNextPiece()
        {
            Vector2Int SpawnPivot = GetSpawnPivot(NextPieceType);
            CurrentPiece = new Codex_PieceState(NextPieceType, 0, SpawnPivot);
            NextPieceType = DequeuePieceType();

            if (BoardState.CanPlace(CurrentPiece))
            {
                return;
            }

            IsGameOver = true;
        }

        private Vector2Int GetSpawnPivot(Codex_TetrominoType PieceType)
        {
            int SpawnX = (BoardState.Width / 2) - 1;
            if (PieceType == Codex_TetrominoType.O)
            {
                SpawnX = BoardState.Width / 2 - 1;
            }

            int SpawnY = BoardState.Height;
            return new Vector2Int(SpawnX, SpawnY);
        }

        private Codex_TetrominoType DequeuePieceType()
        {
            if (PieceBag.Count == 0)
            {
                RefillPieceBag();
            }

            Codex_TetrominoType NextType = PieceBag[PieceBag.Count - 1];
            PieceBag.RemoveAt(PieceBag.Count - 1);
            return NextType;
        }

        private void RefillPieceBag()
        {
            PieceBag.Clear();

            IReadOnlyList<Codex_TetrominoType> Types = Codex_TetrominoData.GetAllTypes();
            for (int Index = 0; Index < Types.Count; Index++)
            {
                PieceBag.Add(Types[Index]);
            }

            for (int Index = PieceBag.Count - 1; Index > 0; Index--)
            {
                int SwapIndex = RandomGenerator.Next(Index + 1);
                (PieceBag[Index], PieceBag[SwapIndex]) = (PieceBag[SwapIndex], PieceBag[Index]);
            }
        }
    }
}
