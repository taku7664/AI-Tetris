using System.Collections.Generic;
using UnityEngine;

namespace Codex
{
    public sealed class Codex_BlockPool
    {
        private readonly Stack<SpriteRenderer> InactiveBlocks = new Stack<SpriteRenderer>();
        private readonly List<SpriteRenderer> ActiveBlocks = new List<SpriteRenderer>();
        private readonly Transform ParentTransform;
        private readonly Sprite BlockSprite;
        private readonly string BlockNamePrefix;

        private int CreatedCount;

        public Codex_BlockPool(Transform ParentTransform, Sprite BlockSprite, string BlockNamePrefix)
        {
            this.ParentTransform = ParentTransform;
            this.BlockSprite = BlockSprite;
            this.BlockNamePrefix = BlockNamePrefix;
        }

        public void BeginFrame()
        {
            for (int Index = 0; Index < ActiveBlocks.Count; Index++)
            {
                SpriteRenderer Renderer = ActiveBlocks[Index];
                Renderer.gameObject.SetActive(false);
                InactiveBlocks.Push(Renderer);
            }

            ActiveBlocks.Clear();
        }

        public void DrawBlock(Vector3 Position, float Size, Color ColorValue, int SortingOrder)
        {
            SpriteRenderer Renderer = AcquireRenderer();
            Renderer.sprite = BlockSprite;
            Renderer.color = ColorValue;
            Renderer.sortingOrder = SortingOrder;

            Transform RendererTransform = Renderer.transform;
            RendererTransform.position = Position;
            RendererTransform.localScale = new Vector3(Size, Size, 1f);

            Renderer.gameObject.SetActive(true);
            ActiveBlocks.Add(Renderer);
        }

        public void Dispose()
        {
            for (int Index = 0; Index < ActiveBlocks.Count; Index++)
            {
                if (ActiveBlocks[Index] != null)
                {
                    Object.Destroy(ActiveBlocks[Index].gameObject);
                }
            }

            ActiveBlocks.Clear();

            while (InactiveBlocks.Count > 0)
            {
                SpriteRenderer Renderer = InactiveBlocks.Pop();
                if (Renderer != null)
                {
                    Object.Destroy(Renderer.gameObject);
                }
            }
        }

        private SpriteRenderer AcquireRenderer()
        {
            if (InactiveBlocks.Count > 0)
            {
                return InactiveBlocks.Pop();
            }

            GameObject BlockObject = new GameObject($"{BlockNamePrefix}_{CreatedCount}");
            CreatedCount++;

            Transform BlockTransform = BlockObject.transform;
            BlockTransform.SetParent(ParentTransform, false);

            SpriteRenderer Renderer = BlockObject.AddComponent<SpriteRenderer>();
            Renderer.sprite = BlockSprite;
            return Renderer;
        }
    }
}
