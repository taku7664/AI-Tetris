using TMPro;
using UnityEngine;

namespace ChatGpt
{
    /// <summary>
    /// Manages HUD elements: score label and game-over overlay.
    /// </summary>
    public class ChatGpt_UIManager : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────
        [SerializeField] private TMP_Text _scoreLabel;
        [SerializeField] private GameObject _gameOverOverlay;

        // ── Public API ───────────────────────────────────────────────────────────

        public void UpdateScore(int score)
        {
            if (_scoreLabel != null)
                _scoreLabel.text = $"SCORE\n{score:D6}";
        }

        public void ShowGameOver(bool show)
        {
            if (_gameOverOverlay != null)
                _gameOverOverlay.SetActive(show);
        }
    }
}
