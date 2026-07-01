using System;
using UnityEngine;

namespace MagicArcher.UI
{
    [Serializable]
    public struct HealthBarProfile
    {
        [Tooltip("Screen-space Y offset in canvas pixels above the anchor point.")]
        public float ScreenOffsetY;

        [Tooltip("Base bar size in canvas pixels before BarScale is applied.")]
        public Vector2 Size;

        [Tooltip("Size multiplier. Used for enemy bar scale tuning.")]
        public float BarScale;

        public Color FillColor;

        [Tooltip("Hide the bar while health is full (used for player units).")]
        public bool HideWhenFull;

        public Vector2 ScaledSize => Size * Mathf.Max(0.01f, BarScale);

        public static HealthBarProfile ArcherDefault => new HealthBarProfile
        {
            ScreenOffsetY = 100f,
            Size = new Vector2(72f, 10f),
            BarScale = 1f,
            FillColor = new Color(0.2f, 0.95f, 0.25f, 1f),
            HideWhenFull = true
        };

        public static HealthBarProfile OrcDefault => new HealthBarProfile
        {
            ScreenOffsetY = 90f,
            Size = new Vector2(48f, 7f),
            BarScale = 1f,
            FillColor = new Color(0.95f, 0.2f, 0.2f, 1f),
            HideWhenFull = false
        };

        public static HealthBarProfile OrcBossDefault => new HealthBarProfile
        {
            ScreenOffsetY = 90f,
            Size = new Vector2(48f, 7f),
            BarScale = 1.65f,
            FillColor = new Color(0.95f, 0.2f, 0.2f, 1f),
            HideWhenFull = false
        };
    }
}
