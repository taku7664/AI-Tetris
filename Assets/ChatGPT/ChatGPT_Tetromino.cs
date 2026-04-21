using UnityEngine;

namespace ChatGPT
{
    public enum ChatGPT_PieceType
    {
        I = 0,
        O = 1,
        T = 2,
        S = 3,
        Z = 4,
        J = 5,
        L = 6,
    }

    public readonly struct ChatGPT_PieceState
    {
        public ChatGPT_PieceState(ChatGPT_PieceType type, Vector2Int position, int rotation)
        {
            Type = type;
            Position = position;
            Rotation = rotation;
        }

        public ChatGPT_PieceType Type { get; }
        public Vector2Int Position { get; }
        public int Rotation { get; }

        public ChatGPT_PieceState WithPosition(Vector2Int position)
        {
            return new ChatGPT_PieceState(Type, position, Rotation);
        }

        public ChatGPT_PieceState WithRotation(int rotation)
        {
            return new ChatGPT_PieceState(Type, Position, rotation);
        }
    }

    public static class ChatGPT_Tetromino
    {
        public static readonly ChatGPT_PieceType[] AllTypes =
        {
            ChatGPT_PieceType.I,
            ChatGPT_PieceType.O,
            ChatGPT_PieceType.T,
            ChatGPT_PieceType.S,
            ChatGPT_PieceType.Z,
            ChatGPT_PieceType.J,
            ChatGPT_PieceType.L,
        };

        private static readonly Vector2Int[][] ICells =
        {
            new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
            new[] { new Vector2Int(1, -1), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) },
            new[] { new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
            new[] { new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) },
        };

        private static readonly Vector2Int[][] OCells =
        {
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
        };

        private static readonly Vector2Int[][] TCells =
        {
            new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) },
            new[] { new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 0) },
            new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, -1) },
            new[] { new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0) },
        };

        private static readonly Vector2Int[][] SCells =
        {
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1) },
            new[] { new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1) },
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1) },
            new[] { new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1) },
        };

        private static readonly Vector2Int[][] ZCells =
        {
            new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            new[] { new Vector2Int(1, -1), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) },
            new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            new[] { new Vector2Int(1, -1), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) },
        };

        private static readonly Vector2Int[][] JCells =
        {
            new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, 1) },
            new[] { new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, -1) },
            new[] { new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(-1, -1) },
        };

        private static readonly Vector2Int[][] LCells =
        {
            new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1) },
            new[] { new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, -1) },
            new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, -1) },
            new[] { new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(-1, 1) },
        };

        public static Vector2Int[] GetCells(ChatGPT_PieceType type, int rotation)
        {
            int normalizedRotation = ((rotation % 4) + 4) % 4;
            return type switch
            {
                ChatGPT_PieceType.I => ICells[normalizedRotation],
                ChatGPT_PieceType.O => OCells[normalizedRotation],
                ChatGPT_PieceType.T => TCells[normalizedRotation],
                ChatGPT_PieceType.S => SCells[normalizedRotation],
                ChatGPT_PieceType.Z => ZCells[normalizedRotation],
                ChatGPT_PieceType.J => JCells[normalizedRotation],
                _ => LCells[normalizedRotation],
            };
        }

        public static Color GetColor(ChatGPT_PieceType type)
        {
            return type switch
            {
                ChatGPT_PieceType.I => new Color32(0, 255, 255, 255),
                ChatGPT_PieceType.O => new Color32(255, 255, 0, 255),
                ChatGPT_PieceType.T => new Color32(128, 0, 128, 255),
                ChatGPT_PieceType.S => new Color32(0, 255, 0, 255),
                ChatGPT_PieceType.Z => new Color32(255, 0, 0, 255),
                ChatGPT_PieceType.J => new Color32(0, 0, 255, 255),
                _ => new Color32(255, 165, 0, 255),
            };
        }
    }
}
