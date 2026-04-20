using System.Collections.Generic;
using UnityEngine;

namespace Codex
{
    public enum Codex_TetrominoType
    {
        I,
        O,
        T,
        S,
        Z,
        J,
        L,
    }

    public readonly struct Codex_PieceState
    {
        public Codex_PieceState(Codex_TetrominoType Type, int Rotation, Vector2Int Pivot)
        {
            this.Type = Type;
            this.Rotation = Rotation;
            this.Pivot = Pivot;
        }

        public Codex_TetrominoType Type { get; }
        public int Rotation { get; }
        public Vector2Int Pivot { get; }

        public Codex_PieceState WithPivot(Vector2Int Pivot)
        {
            return new Codex_PieceState(Type, Rotation, Pivot);
        }

        public Codex_PieceState WithRotation(int Rotation)
        {
            return new Codex_PieceState(Type, Rotation, Pivot);
        }
    }

    public static class Codex_TetrominoData
    {
        private static readonly Codex_TetrominoType[] AllTypes =
        {
            Codex_TetrominoType.I,
            Codex_TetrominoType.O,
            Codex_TetrominoType.T,
            Codex_TetrominoType.S,
            Codex_TetrominoType.Z,
            Codex_TetrominoType.J,
            Codex_TetrominoType.L,
        };

        private static readonly Dictionary<Codex_TetrominoType, Vector2Int[][]> Rotations =
            new Dictionary<Codex_TetrominoType, Vector2Int[][]>
            {
                {
                    Codex_TetrominoType.I,
                    new[]
                    {
                        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
                        new[] { new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(1, -2) },
                        new[] { new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1), new Vector2Int(2, -1) },
                        new[] { new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(0, -1), new Vector2Int(0, -2) },
                    }
                },
                {
                    Codex_TetrominoType.O,
                    new[]
                    {
                        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
                        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
                        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
                        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
                    }
                },
                {
                    Codex_TetrominoType.T,
                    new[]
                    {
                        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) },
                        new[] { new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(0, -1), new Vector2Int(1, 0) },
                        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, -1) },
                        new[] { new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) },
                    }
                },
                {
                    Codex_TetrominoType.S,
                    new[]
                    {
                        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
                        new[] { new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, -1) },
                        new[] { new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(1, 0) },
                        new[] { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, -1) },
                    }
                },
                {
                    Codex_TetrominoType.Z,
                    new[]
                    {
                        new[] { new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(1, 0) },
                        new[] { new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(0, 0), new Vector2Int(0, -1) },
                        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, -1), new Vector2Int(1, -1) },
                        new[] { new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, -1) },
                    }
                },
                {
                    Codex_TetrominoType.J,
                    new[]
                    {
                        new[] { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0) },
                        new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(0, 0), new Vector2Int(0, -1) },
                        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, -1) },
                        new[] { new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(0, -1), new Vector2Int(-1, -1) },
                    }
                },
                {
                    Codex_TetrominoType.L,
                    new[]
                    {
                        new[] { new Vector2Int(1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0) },
                        new[] { new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(0, -1), new Vector2Int(1, -1) },
                        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, -1) },
                        new[] { new Vector2Int(0, 1), new Vector2Int(-1, 1), new Vector2Int(0, 0), new Vector2Int(0, -1) },
                    }
                },
            };

        private static readonly Dictionary<Codex_TetrominoType, Color> Colors =
            new Dictionary<Codex_TetrominoType, Color>
            {
                { Codex_TetrominoType.I, new Color(0f, 1f, 1f, 1f) },
                { Codex_TetrominoType.O, new Color(1f, 1f, 0f, 1f) },
                { Codex_TetrominoType.T, new Color(128f / 255f, 0f, 128f / 255f, 1f) },
                { Codex_TetrominoType.S, new Color(0f, 1f, 0f, 1f) },
                { Codex_TetrominoType.Z, new Color(1f, 0f, 0f, 1f) },
                { Codex_TetrominoType.J, new Color(0f, 0f, 1f, 1f) },
                { Codex_TetrominoType.L, new Color(1f, 165f / 255f, 0f, 1f) },
            };

        public static IReadOnlyList<Codex_TetrominoType> GetAllTypes()
        {
            return AllTypes;
        }

        public static Vector2Int[] GetCells(Codex_TetrominoType Type, int Rotation)
        {
            int RotationCount = Rotations[Type].Length;
            int NormalizedRotation = ((Rotation % RotationCount) + RotationCount) % RotationCount;
            return Rotations[Type][NormalizedRotation];
        }

        public static Color GetColor(Codex_TetrominoType Type)
        {
            return Colors[Type];
        }
    }
}
