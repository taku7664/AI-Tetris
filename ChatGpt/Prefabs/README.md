# ChatGpt Prefabs

## Required Prefabs

Place these prefabs in `ChatGpt/Prefabs/` and assign them to the scene objects.

### ChatGpt_Cell (Prefab)
Single visual block used for both the board, active piece, ghost piece, and preview.

**Components required:**
- `Transform`
- `SpriteRenderer` — assign a plain white square sprite (or Unity default white sprite)
- `ChatGpt_Cell` script

### ChatGpt_Tetromino (Prefab)
Container for a falling piece's visual cells.

**Components required:**
- `Transform`
- `ChatGpt_Tetromino` script
  - `_cellPrefab` → ChatGpt_Cell prefab

---

## Scene Setup (ChatGpt_Scene)

### Canvas (Screen Space – Overlay)
```
Canvas
├── FieldBox          (UI Image, acts as background for the playing field)
│   └── Position: centre-left area of screen, e.g. anchored at centre
├── PreviewBox        (UI Image, acts as background for next-piece preview)
│   └── Position: top-right area, fixed square size e.g. 160×160
├── ScoreLabel        (TextMeshProUGUI)
└── GameOverOverlay   (Panel with "GAME OVER" text + "Press Enter to restart")
```

### ChatGpt_GameManager (Empty GameObject)
Attach all scripts and assign references:
- `ChatGpt_GameManager`
  - `_fieldWidth`: 10
  - `_fieldHeight`: 20
  - `_fallSpeedSeconds`: 0.5
  - `_scorePerLine`: 100
  - `_board` → ChatGpt_Board component (on same or child object)
  - `_spawner` → ChatGpt_Spawner component
  - `_preview` → ChatGpt_Preview component
  - `_inputHandler` → ChatGpt_InputHandler component
  - `_uiManager` → ChatGpt_UIManager component

- `ChatGpt_Board`
  - `_fieldBox` → FieldBox RectTransform
  - `_cellPrefab` → ChatGpt_Cell prefab

- `ChatGpt_Spawner`
  - `_tetrominoPrefab` → ChatGpt_Tetromino prefab
  - `_cellPrefab` → ChatGpt_Cell prefab

- `ChatGpt_Preview`
  - `_previewBox` → PreviewBox RectTransform
  - `_cellPrefab` → ChatGpt_Cell prefab

- `ChatGpt_InputHandler`
  - `_inputActions` → ChatGpt_InputActions asset

- `ChatGpt_UIManager`
  - `_scoreLabel` → ScoreLabel TextMeshProUGUI
  - `_gameOverOverlay` → GameOverOverlay GameObject

### Camera
- Use an Orthographic camera sized to contain the canvas.
