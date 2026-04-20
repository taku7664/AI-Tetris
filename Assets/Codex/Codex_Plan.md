# Codex Tetris Implementation Plan

## Goal
- Build a playable single-player Tetris in `Assets/Codex`.
- Keep ReferenceScene object bindings (`FieldBox`, `PreviewBox`, `ScoreText`) and implement game logic in `Codex` namespace.

## Constraints
- Unity 6000.4.1f1
- InputAction package required
- TextMeshPro required
- All new files use `Codex_` prefix

## High-Level Architecture

### 1) Domain Data Layer
- `Codex_Tetromino`:
  - Tetromino type enum and per-rotation cell offsets.
  - Color table (I,O,T,S,Z,J,L exact RGB).
- `Codex_BoardState`:
  - Occupied cells and fixed block colors.
  - Collision checks, lock logic, line clear logic.

### 2) Simulation Layer
- `Codex_TetrisGame`:
  - Current piece, next piece, spawn, gravity timer, soft drop, hard drop, rotate/move.
  - Ghost landing position solver.
  - Score updates based on line clear count list.
  - GameOver and Restart state transitions.

### 3) Rendering Layer
- `Codex_BlockPool`:
  - SpriteRenderer object pooling for board, active piece, ghost, preview.
- `Codex_FieldRenderer`:
  - Renders fixed blocks + active block + ghost block inside `FieldBox` bounds.
  - Renders black available-area rectangle.
  - Maintains uniform cell size even when field dimensions change.
- `Codex_PreviewRenderer`:
  - Renders next tetromino in square cells inside `PreviewBox`.

### 4) Input Layer
- `Codex_InputActions.inputactions` asset in `Assets/Codex`.
- Action map: Gameplay
  - Move (Press, left/right + dpad left/right)
  - SoftDrop (Press, down + dpad down)
  - RotateCW (Pressed, up + dpad up)
  - HardDrop (Pressed, space + gamepad south)
  - Restart (Pressed, enter + gamepad start)

### 5) Composition Root
- `Codex_GameManager`
  - Serialized config:
    - `FieldWidth` (default 10)
    - `FieldHeight` (default 20)
    - `FallIntervalSeconds` (default 0.5)
    - `LineClearScores` (default 100,300,500,800)
  - Scene refs:
    - `FieldBox`, `PreviewBox`, `ScoreText`
  - Wires input -> simulation -> render pipeline.

## Delivery Steps
1. Implement core data and simulation classes.
2. Implement rendering + pool classes.
3. Create input actions asset and bind in manager.
4. Replace placeholder manager with orchestration logic.
5. Verify game loop requirements and edge cases.
