using System;
using MagicArcher.Gameplay.Economy;
using MagicArcher.Gameplay.Units;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MagicArcher.UI
{
    public sealed class BuyUnitButtonView : MonoBehaviour
    {
        static readonly int EffectAmountId = Shader.PropertyToID("_EffectAmount");
        static readonly int BrightnessAmountId = Shader.PropertyToID("_BrightnessAmount");

        [SerializeField] Button _button;
        [SerializeField] Image _background;
        [SerializeField] Image _icon;
        [SerializeField] Text _costLabel;
        [SerializeField] Text _titleLabel;
        [SerializeField] Color _enabledTextColor = Color.white;
        [SerializeField] Color _disabledTextColor = new(0.35f, 0.35f, 0.35f, 1f);
        [SerializeField] float _enabledEffectAmount;
        [SerializeField] float _disabledEffectAmount = 1f;
        [SerializeField] float _enabledBrightness = 1f;
        [SerializeField] float _disabledBrightness = 0.75f;

        Material _backgroundMaterial;
        Material _iconMaterial;

        IEconomyService _economy;
        UnitPurchaseService _purchase;
        bool _visible;

        public event Action Clicked;

        [Inject]
        void Construct(IEconomyService economy, UnitPurchaseService purchase)
        {
            _economy = economy;
            _purchase = purchase;
        }

        void Awake()
        {
            ResolveVisualRefs();
            CacheMaterials();

            if (_button != null)
            {
                _button.transition = Selectable.Transition.None;
                _button.onClick.AddListener(OnClick);
            }
        }

        void Start()
        {
            if (_economy != null)
                _economy.CoinsChanged += OnEconomyChanged;

            if (_purchase != null)
                _purchase.UnitPurchased += OnUnitPurchased;

            if (_visible)
                RefreshAffordState();
        }

        void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClick);

            if (_economy != null)
                _economy.CoinsChanged -= OnEconomyChanged;

            if (_purchase != null)
                _purchase.UnitPurchased -= OnUnitPurchased;
        }

        public void Show(int cost)
        {
            gameObject.SetActive(true);
            _visible = true;

            if (_costLabel != null)
                _costLabel.text = cost.ToString();

            if (_titleLabel != null)
                _titleLabel.text = "HIRE";

            RefreshAffordState();
        }

        public void Hide()
        {
            _visible = false;
            gameObject.SetActive(false);
        }

        public void SetInteractable(bool interactable)
        {
            if (_button != null)
                _button.interactable = interactable;

            ApplyAffordableVisual(interactable);
        }

        void ResolveVisualRefs()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_background == null && _button != null)
                _background = _button.targetGraphic as Image;

            if (_background == null)
                _background = GetComponent<Image>();

            if (_icon == null)
            {
                var iconTransform = transform.Find("Icon") ?? transform.Find("Image");
                if (iconTransform != null)
                    _icon = iconTransform.GetComponent<Image>();
            }
        }

        void CacheMaterials()
        {
            _backgroundMaterial = GetMaterialInstance(_background);
            _iconMaterial = GetMaterialInstance(_icon);
        }

        static Material GetMaterialInstance(Image image)
        {
            if (image == null || image.material == null)
                return null;

            if (!image.material.HasProperty(EffectAmountId))
                return null;

            return image.material;
        }

        void OnEconomyChanged(int _)
        {
            if (_visible)
                RefreshAffordState();
        }

        void OnUnitPurchased(UnitView _)
        {
            if (_visible)
                RefreshAffordState();
        }

        void RefreshAffordState()
        {
            var affordable = _purchase == null || _purchase.CanAfford();

            if (_button != null)
                _button.interactable = affordable;

            ApplyAffordableVisual(affordable);
        }

        void ApplyAffordableVisual(bool affordable)
        {
            var effect = affordable ? _enabledEffectAmount : _disabledEffectAmount;
            var brightness = affordable ? _enabledBrightness : _disabledBrightness;
            var textColor = affordable ? _enabledTextColor : _disabledTextColor;

            ApplyGrayscale(_backgroundMaterial, effect, brightness);
            ApplyGrayscale(_iconMaterial, effect, brightness);

            if (_costLabel != null)
                _costLabel.color = textColor;

            if (_titleLabel != null)
                _titleLabel.color = textColor;
        }

        static void ApplyGrayscale(Material material, float effectAmount, float brightness)
        {
            if (material == null)
                return;

            material.SetFloat(EffectAmountId, effectAmount);
            material.SetFloat(BrightnessAmountId, brightness);
        }

        void OnClick()
        {
            Clicked?.Invoke();
        }
    }
}
