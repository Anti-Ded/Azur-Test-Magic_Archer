using System;
using UnityEngine;
using UnityEngine.UI;

namespace MagicArcher.UI
{
    public sealed class OrientationLayoutController : MonoBehaviour
    {
        static readonly Vector2 LandscapeReference = new(1920f, 1080f);
        static readonly Vector2 PortraitReference = new(1080f, 1920f);

        [SerializeField] CanvasScaler _canvasScaler;
        [SerializeField] RectTransform _coinHudTarget;
        [SerializeField] RectTransform _buyUnitButton;
        [SerializeField] TutorialOverlayView _tutorialOverlay;
        [SerializeField] TutorialPurchasePanelView _tutorialPurchasePanel;

        [SerializeField] RectTransformLayoutPreset _coinHudLandscape =
            RectTransformLayoutPreset.TopRight(40f, 40f, new Vector2(160f, 60f));

        [SerializeField] RectTransformLayoutPreset _coinHudPortrait =
            RectTransformLayoutPreset.TopRight(40f, 40f, new Vector2(160f, 60f));

        [SerializeField] RectTransformLayoutPreset _buyButtonLandscape =
            RectTransformLayoutPreset.RightCenter(40f, new Vector2(140f, 140f));

        [SerializeField] RectTransformLayoutPreset _buyButtonPortrait =
            RectTransformLayoutPreset.BottomCenter(48f, new Vector2(240f, 88f));

        bool? _isLandscape;

        public bool IsLandscape => _isLandscape ?? Screen.width >= Screen.height;
        public event Action<bool> LayoutChanged;

        void Awake()
        {
            if (_canvasScaler == null)
                _canvasScaler = GetComponent<CanvasScaler>();

            if (_tutorialOverlay == null)
                _tutorialOverlay = GetComponentInChildren<TutorialOverlayView>(true);

            if (_tutorialPurchasePanel == null)
                _tutorialPurchasePanel = GetComponentInChildren<TutorialPurchasePanelView>(true);

            ApplyCurrentLayout(force: true);
        }

        void Update()
        {
            ApplyCurrentLayout(force: false);
        }

        void OnRectTransformDimensionsChange()
        {
            ApplyCurrentLayout(force: true);
        }

        public void ApplyCurrentLayout(bool force = false)
        {
            var landscape = Screen.width >= Screen.height;
            if (!force && _isLandscape.HasValue && _isLandscape.Value == landscape)
                return;

            _isLandscape = landscape;
            ApplyScaler(landscape);
            ApplyUi(landscape);
            ApplyTutorialPurchasePanelLayout(landscape);
            _tutorialOverlay?.RefreshActiveHint();
            LayoutChanged?.Invoke(landscape);
        }

        void ApplyTutorialPurchasePanelLayout(bool landscape)
        {
            if (_tutorialPurchasePanel == null || !_tutorialPurchasePanel.gameObject.activeInHierarchy)
                return;

            if (landscape)
                _tutorialPurchasePanel.ApplyHorizontalLayout();
            else
                _tutorialPurchasePanel.ApplyVerticalLayout();
        }

        void ApplyScaler(bool landscape)
        {
            if (_canvasScaler == null)
                return;

            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvasScaler.referenceResolution = landscape ? LandscapeReference : PortraitReference;
            _canvasScaler.matchWidthOrHeight = landscape ? 0f : 1f;
        }

        void ApplyUi(bool landscape)
        {
            if (landscape)
            {
                _coinHudLandscape.Apply(_coinHudTarget);
                _buyButtonLandscape.Apply(_buyUnitButton);
            }
            else
            {
                _coinHudPortrait.Apply(_coinHudTarget);
                _buyButtonPortrait.Apply(_buyUnitButton);
            }
        }
    }
}
