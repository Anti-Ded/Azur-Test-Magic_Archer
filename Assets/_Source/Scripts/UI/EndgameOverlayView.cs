using UnityEngine;
using UnityEngine.UI;

namespace MagicArcher.UI
{
    public sealed class EndgameOverlayView : MonoBehaviour
    {
        [SerializeField] GameObject _root;
        [SerializeField] Text _titleLabel;
        [SerializeField] Text _hintLabel;

        bool _usesChildVisuals;

        void Awake()
        {
            _usesChildVisuals = _root == null || _root == gameObject;
            SetVisualsVisible(false);
        }

        public void ShowVictory()
        {
            Show("VICTORY", "Tap to continue");
        }

        public void ShowDefeat()
        {
            Show("DEFEAT", "Tap to continue");
        }

        void Show(string title, string hint)
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            SetVisualsVisible(true);

            if (_titleLabel != null)
                _titleLabel.text = title;

            if (_hintLabel != null)
                _hintLabel.text = hint;
        }

        public void Hide()
        {
            SetVisualsVisible(false);
        }

        void SetVisualsVisible(bool visible)
        {
            if (!_usesChildVisuals && _root != null)
            {
                _root.SetActive(visible);
                return;
            }

            for (var i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(visible);
        }
    }
}
