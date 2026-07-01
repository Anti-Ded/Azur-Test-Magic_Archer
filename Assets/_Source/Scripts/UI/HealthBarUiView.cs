using UnityEngine;
using UnityEngine.UI;

namespace MagicArcher.UI
{
    public sealed class HealthBarUiView : MonoBehaviour
    {
        [SerializeField] Image _back;
        [SerializeField] Image _fill;

        RectTransform _rect;

        void Awake()
        {
            _rect = transform as RectTransform;
        }

        public void ApplyProfile(HealthBarProfile profile)
        {
            if (_rect != null)
                _rect.sizeDelta = profile.ScaledSize;

            if (_fill != null)
                _fill.color = profile.FillColor;

            if (_back != null)
            {
                var fill = profile.FillColor;
                _back.color = new Color(fill.r * 0.25f, fill.g * 0.25f, fill.b * 0.25f, 0.95f);
            }
        }

        public void SetFill(float normalized)
        {
            if (_fill == null)
                return;

            _fill.fillAmount = Mathf.Clamp01(normalized);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
