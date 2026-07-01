using UnityEngine;
using UnityEngine.UI;

namespace MagicArcher.UI
{
    public sealed class EndgameOverlayView : MonoBehaviour
    {
        [SerializeField] GameObject _root;
        [SerializeField] Text _titleLabel;
        [SerializeField] Text _hintLabel;

        void Awake()
        {
            Hide();
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
            if (_root != null)
                _root.SetActive(true);

            if (_titleLabel != null)
                _titleLabel.text = title;

            if (_hintLabel != null)
                _hintLabel.text = hint;
        }

        public void Hide()
        {
            if (_root != null)
                _root.SetActive(false);
        }
    }
}
