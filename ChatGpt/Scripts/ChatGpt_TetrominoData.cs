using UnityEngine;

namespace ChatGpt
{
    // All 12 tetromino types (10 or more required)
    public enum ChatGpt_TetrominoType
    {
        I = 0,
        O = 1,
        T = 2,
        S = 3,
        Z = 4,
        J = 5,
        L = 6,
        Cross = 7,  // + shape (cross/plus)
        U = 8,      // U shape
        S2 = 9,     // skew variant
        Z2 = 10,    // skew variant
        Corner = 11 // L-corner 2x2 with 1 extra
    }

    /// <summary>
    /// Holds all static shape and color data for every tetromino type.
    /// Each shape is defined as a list of (col, row) offsets relative to a pivot cell.
    /// Rotations are computed on the fly using SRS (Super Rotation System).
    /// </summary>
    public static class ChatGpt_TetrominoData
    {
        // --- Colors ---
        public static readonly Color[] Colors = new Color[]
        {
            new Color(0.00f, 0.85f, 1.00f), // I  - Cyan
            new Color(1.00f, 0.85f, 0.00f), // O  - Yellow
            new Color(0.60f, 0.00f, 0.85f), // T  - Purple
            new Color(0.00f, 0.80f, 0.10f), // S  - Green
            new Color(1.00f, 0.10f, 0.10f), // Z  - Red
            new Color(0.00f, 0.20f, 1.00f), // J  - Blue
            new Color(1.00f, 0.55f, 0.00f), // L  - Orange
            new Color(1.00f, 0.40f, 0.80f), // Cross  - Pink
            new Color(0.20f, 1.00f, 0.80f), // U      - Teal
            new Color(0.80f, 1.00f, 0.20f), // S2     - Lime
            new Color(0.90f, 0.40f, 0.10f), // Z2     - Burnt Orange
            new Color(0.50f, 0.50f, 1.00f), // Corner - Periwinkle
        };

        // --- Cell offsets for spawn rotation (rotation index 0) ---
        // Coordinates: (column offset, row offset), row increases upward
        public static readonly Vector2Int[][] Cells = new Vector2Int[][]
        {
            // I:  . X X X X .
            new Vector2Int[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0) },
            // O:  X X
            //     X X
            new Vector2Int[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(1,1) },
            // T:  . X .
            //     X X X
            new Vector2Int[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1) },
            // S:  . X X
            //     X X .
            new Vector2Int[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(1,1) },
            // Z:  X X .
            //     . X X
            new Vector2Int[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-1,1), new Vector2Int(0,1) },
            // J:  X . .
            //     X X X
            new Vector2Int[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-1,1) },
            // L:  . . X
            //     X X X
            new Vector2Int[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1) },
            // Cross (+):  . X .
            //             X X X
            //             . X .
            new Vector2Int[] { new Vector2Int(0,-1), new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1) },
            // U:  X . X
            //     X X X
            new Vector2Int[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-1,1), new Vector2Int(1,1) },
            // S2 (reverse S, wider):  . X X
            //                          X X .
            //                          X . .
            new Vector2Int[] { new Vector2Int(-1,0), new Vector2Int(-1,1), new Vector2Int(0,1), new Vector2Int(0,2), new Vector2Int(1,2) },
            // Z2 (reverse Z, wider):  X X .
            //                          . X X
            //                          . . X
            new Vector2Int[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-1,1), new Vector2Int(0,1), new Vector2Int(-1,2) },
            // Corner:  X X
            //           X .
            //           X .
            new Vector2Int[] { new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2), new Vector2Int(1,2) },
        };

        // --- SRS Wall-kick data ---
        // Indexed by [currentRotationIndex, kickTestIndex] giving (col, row) offset to try
        public static readonly Vector2Int[,] WallKicksJLSTZ = new Vector2Int[,]
        {
            // 0 -> 1
            { new Vector2Int( 0,0), new Vector2Int(-1,0), new Vector2Int(-1, 1), new Vector2Int(0,-2), new Vector2Int(-1,-2) },
            // 1 -> 2
            { new Vector2Int( 0,0), new Vector2Int( 1,0), new Vector2Int( 1,-1), new Vector2Int(0, 2), new Vector2Int( 1, 2) },
            // 2 -> 3
            { new Vector2Int( 0,0), new Vector2Int( 1,0), new Vector2Int( 1, 1), new Vector2Int(0,-2), new Vector2Int( 1,-2) },
            // 3 -> 0
            { new Vector2Int( 0,0), new Vector2Int(-1,0), new Vector2Int(-1,-1), new Vector2Int(0, 2), new Vector2Int(-1, 2) },
        };

        public static readonly Vector2Int[,] WallKicksI = new Vector2Int[,]
        {
            // 0 -> 1
            { new Vector2Int( 0,0), new Vector2Int(-2,0), new Vector2Int( 1,0), new Vector2Int(-2,-1), new Vector2Int( 1, 2) },
            // 1 -> 2
            { new Vector2Int( 0,0), new Vector2Int(-1,0), new Vector2Int( 2,0), new Vector2Int(-1, 2), new Vector2Int( 2,-1) },
            // 2 -> 3
            { new Vector2Int( 0,0), new Vector2Int( 2,0), new Vector2Int(-1,0), new Vector2Int( 2, 1), new Vector2Int(-1,-2) },
            // 3 -> 0
            { new Vector2Int( 0,0), new Vector2Int( 1,0), new Vector2Int(-2,0), new Vector2Int( 1,-2), new Vector2Int(-2, 1) },
        };

        // Pieces with no wall kicks (O) or handled separately
        public static readonly Vector2Int[,] WallKicksNone = new Vector2Int[,]
        {
            { new Vector2Int(0,0), new Vector2Int(0,0), new Vector2Int(0,0), new Vector2Int(0,0), new Vector2Int(0,0) },
            { new Vector2Int(0,0), new Vector2Int(0,0), new Vector2Int(0,0), new Vector2Int(0,0), new Vector2Int(0,0) },
            { new Vector2Int(0,0), new Vector2Int(0,0), new Vector2Int(0,0), new Vector2Int(0,0), new Vector2Int(0,0) },
            { new Vector2Int(0,0), new Vector2Int(0,0), new Vector2Int(0,0), new Vector2Int(0,0), new Vector2Int(0,0) },
        };

        /// <summary>Returns the appropriate wall-kick table for a given tetromino type.</summary>
        public static Vector2Int[,] GetWallKicks(ChatGpt_TetrominoType type)
        {
            switch (type)
            {
                case ChatGpt_TetrominoType.I:
                    return WallKicksI;
                case ChatGpt_TetrominoType.O:
                    return WallKicksNone;
                default:
                    return WallKicksJLSTZ;
            }
        }

        /// <summary>Rotate a set of cell offsets 90 degrees clockwise around (0,0).</summary>
        public static Vector2Int[] RotateCW(Vector2Int[] cells)
        {
            Vector2Int[] result = new Vector2Int[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                // CW rotation: (x, y) -> (y, -x)
                result[i] = new Vector2Int(cells[i].y, -cells[i].x);
            }
            return result;
        }

        /// <summary>Builds all 4 rotation states for a given spawn orientation.</summary>
        public static Vector2Int[][] BuildRotations(Vector2Int[] spawnCells)
        {
            Vector2Int[][] rots = new Vector2Int[4][];
            rots[0] = spawnCells;
            for (int i = 1; i < 4; i++)
                rots[i] = RotateCW(rots[i - 1]);
            return rots;
        }
    }
}
