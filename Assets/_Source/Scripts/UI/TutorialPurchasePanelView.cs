using System;
using UnityEngine;
using UnityEngine.UI;

namespace MagicArcher.UI
{
    public sealed class TutorialPurchasePanelView : MonoBehaviour
    {
        [SerializeField] Image _dimmer;
        [SerializeField] Image _panelBackground;
        [SerializeField] Image _heroImageA;
        [SerializeField] Image _heroImageB;
        [SerializeField] BuyUnitButtonView _buyButton;
        [SerializeField] TutorialPurchaseLayoutGroup _horizontalLayout = TutorialPurchaseLayoutGroup.LandscapeDefault();
        [SerializeField] TutorialPurchaseLayoutGroup _verticalLayout = TutorialPurchaseLayoutGroup.PortraitDefault();

        public BuyUnitButtonView BuyButton => _buyButton;

        public RectTransform BuyButtonRect =>
            _buyButton != null ? _buyButton.transform as RectTransform : null;

        public void Show(int cost)
        {
            gameObject.SetActive(true);
            ApplyLayoutForCurrentOrientation();

            if (_dimmer != null)
            {
                _dimmer.raycastTarget = true;
                _dimmer.transform.SetAsFirstSibling();
            }

            _buyButton?.Show(cost);
        }

        public void Hide()
        {
            _buyButton?.Hide();
            gameObject.SetActive(false);
        }

        public void ApplyHorizontalLayout()
        {
            ApplyLayout(_horizontalLayout);
        }

        public void ApplyVerticalLayout()
        {
            ApplyLayout(_verticalLayout);
        }

        public void GetHorizontalValues()
        {
            _horizontalLayout = CaptureCurrentLayout();
            MarkDirty();
        }

        public void GetVerticalValues()
        {
            _verticalLayout = CaptureCurrentLayout();
            MarkDirty();
        }

        void ApplyLayoutForCurrentOrientation()
        {
            if (Screen.width >= Screen.height)
                ApplyHorizontalLayout();
            else
                ApplyVerticalLayout();
        }

        void ApplyLayout(TutorialPurchaseLayoutGroup layout)
        {
            layout.HeroA.Apply(GetHeroRectA());
            layout.HeroB.Apply(GetHeroRectB());
            layout.BuyButton.Apply(BuyButtonRect);
        }

        RectTransform GetHeroRectA()
        {
            return _heroImageA != null ? _heroImageA.transform as RectTransform : null;
        }

        RectTransform GetHeroRectB()
        {
            return _heroImageB != null ? _heroImageB.transform as RectTransform : null;
        }

        TutorialPurchaseLayoutGroup CaptureCurrentLayout()
        {
            return new TutorialPurchaseLayoutGroup
            {
                HeroA = RectTransformLayoutPreset.From(GetHeroRectA()),
                HeroB = RectTransformLayoutPreset.From(GetHeroRectB()),
                BuyButton = RectTransformLayoutPreset.From(BuyButtonRect)
            };
        }

        void MarkDirty()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        void Reset()
        {
            _horizontalLayout = TutorialPurchaseLayoutGroup.LandscapeDefault();
            _verticalLayout = TutorialPurchaseLayoutGroup.PortraitDefault();
        }
    }

    [Serializable]
    public struct TutorialPurchaseLayoutGroup
    {
        public RectTransformLayoutPreset HeroA;
        public RectTransformLayoutPreset HeroB;
        public RectTransformLayoutPreset BuyButton;

        public static TutorialPurchaseLayoutGroup LandscapeDefault()
        {
            return new TutorialPurchaseLayoutGroup
            {
                HeroA = Center(-130f, 70f, new Vector2(220f, 280f)),
                HeroB = Center(130f, 70f, new Vector2(220f, 280f)),
                BuyButton = Center(0f, -250f, new Vector2(320f, 120f))
            };
        }

        public static TutorialPurchaseLayoutGroup PortraitDefault()
        {
            return new TutorialPurchaseLayoutGroup
            {
                HeroA = Center(-95f, 140f, new Vector2(170f, 220f)),
                HeroB = Center(95f, 140f, new Vector2(170f, 220f)),
                BuyButton = Center(0f, -200f, new Vector2(300f, 110f))
            };
        }

        static RectTransformLayoutPreset Center(float x, float y, Vector2 size)
        {
            return new RectTransformLayoutPreset
            {
                AnchorMin = new Vector2(0.5f, 0.5f),
                AnchorMax = new Vector2(0.5f, 0.5f),
                Pivot = new Vector2(0.5f, 0.5f),
                AnchoredPosition = new Vector2(x, y),
                SizeDelta = size
            };
        }
    }
}
