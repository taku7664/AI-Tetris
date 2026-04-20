using UnityEngine;

namespace Codex
{
    public static class Codex_RenderUtils
    {
        private static Texture2D CachedTexture;
        private static Sprite CachedSprite;

        public static Sprite GetOrCreateSquareSprite()
        {
            if (CachedTexture == null)
            {
                CachedTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                {
                    name = "Codex_WhitePixel",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                };
                CachedTexture.SetPixel(0, 0, Color.white);
                CachedTexture.Apply();
            }

            if (CachedSprite == null)
            {
                CachedSprite = Sprite.Create(
                    CachedTexture,
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0.5f, 0.5f),
                    1f);
                CachedSprite.name = "Codex_WhiteSquare";
            }

            return CachedSprite;
        }
    }
}
