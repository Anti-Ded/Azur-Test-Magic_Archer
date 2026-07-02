using System;
using UnityEngine;

namespace MagicArcher.UI
{
    [Serializable]
    public struct RectTransformLayoutPreset
    {
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 Pivot;
        public Vector2 AnchoredPosition;
        public Vector2 SizeDelta;

        public static RectTransformLayoutPreset TopRight(float marginX, float marginY, Vector2 size)
        {
            return new RectTransformLayoutPreset
            {
                AnchorMin = new Vector2(1f, 1f),
                AnchorMax = new Vector2(1f, 1f),
                Pivot = new Vector2(1f, 1f),
                AnchoredPosition = new Vector2(-marginX, -marginY),
                SizeDelta = size
            };
        }

        public static RectTransformLayoutPreset RightCenter(float marginX, Vector2 size)
        {
            return new RectTransformLayoutPreset
            {
                AnchorMin = new Vector2(1f, 0.5f),
                AnchorMax = new Vector2(1f, 0.5f),
                Pivot = new Vector2(1f, 0.5f),
                AnchoredPosition = new Vector2(-marginX, 0f),
                SizeDelta = size
            };
        }

        public static RectTransformLayoutPreset BottomCenter(float marginY, Vector2 size)
        {
            return new RectTransformLayoutPreset
            {
                AnchorMin = new Vector2(0.5f, 0f),
                AnchorMax = new Vector2(0.5f, 0f),
                Pivot = new Vector2(0.5f, 0f),
                AnchoredPosition = new Vector2(0f, marginY),
                SizeDelta = size
            };
        }

        public static RectTransformLayoutPreset From(RectTransform rect)
        {
            if (rect == null)
                return default;

            return new RectTransformLayoutPreset
            {
                AnchorMin = rect.anchorMin,
                AnchorMax = rect.anchorMax,
                Pivot = rect.pivot,
                AnchoredPosition = rect.anchoredPosition,
                SizeDelta = rect.sizeDelta
            };
        }

        public void Apply(RectTransform rect)
        {
            if (rect == null)
                return;

            rect.anchorMin = AnchorMin;
            rect.anchorMax = AnchorMax;
            rect.pivot = Pivot;
            rect.anchoredPosition = AnchoredPosition;
            rect.sizeDelta = SizeDelta;
        }
    }
}
