using UnityEngine;

namespace ChatGpt
{
    /// <summary>
    /// Represents a single visual cell on the board or within a tetromino.
    /// Wraps a SpriteRenderer so the rest of the code only talks to this component.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ChatGpt_Cell : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>Set the tint colour of this cell.</summary>
        public void SetColor(Color color)
        {
            _spriteRenderer.color = color;
        }

        /// <summary>Move this cell to a world position.</summary>
        public void SetPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        /// <summary>Scale the cell sprite to a given world-unit size.</summary>
        public void SetSize(float size)
        {
            transform.localScale = Vector3.one * size;
        }
    }
}
