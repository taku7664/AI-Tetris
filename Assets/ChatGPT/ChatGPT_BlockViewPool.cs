using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT
{
    public sealed class ChatGPT_BlockViewPool
    {
        private readonly Transform Parent;
        private readonly Sprite Sprite;
        private readonly int SortingOrder;
        private readonly List<SpriteRenderer> Renderers = new List<SpriteRenderer>();
        private int ActiveCount;

        public ChatGPT_BlockViewPool(Transform parent, Sprite sprite, int sortingOrder)
        {
            Parent = parent;
            Sprite = sprite;
            SortingOrder = sortingOrder;
        }

        public void BeginFrame()
        {
            for (int i = 0; i < ActiveCount; ++i)
            {
                Renderers[i].enabled = false;
            }

            ActiveCount = 0;
        }

        public void Draw(Vector3 worldPosition, float worldSize, Color color, float alpha = 1f)
        {
            SpriteRenderer renderer = Acquire();
            Color finalColor = color;
            finalColor.a = alpha;

            renderer.color = finalColor;
            renderer.transform.position = worldPosition;

            float spriteWidth = Mathf.Max(0.0001f, renderer.sprite.bounds.size.x);
            float scale = (worldSize / spriteWidth) * 0.92f;
            renderer.transform.localScale = new Vector3(scale, scale, 1f);
            renderer.enabled = true;
        }

        private SpriteRenderer Acquire()
        {
            if (ActiveCount >= Renderers.Count)
            {
                Renderers.Add(CreateRenderer(Renderers.Count));
            }

            return Renderers[ActiveCount++];
        }

        private SpriteRenderer CreateRenderer(int index)
        {
            GameObject blockObject = new GameObject($"Block_{index}");
            blockObject.transform.SetParent(Parent, false);

            SpriteRenderer renderer = blockObject.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite;
            renderer.sortingOrder = SortingOrder;
            renderer.enabled = false;
            return renderer;
        }
    }
}
