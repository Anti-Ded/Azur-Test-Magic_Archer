using DG.Tweening;
using UnityEngine;

namespace MagicArcher.Gameplay.Units
{
    public sealed class MergeVfx : MonoBehaviour
    {
        [SerializeField] float _duration = 0.5f;

        public void Play(Transform target)
        {
            if (target == null)
                return;

            DOTween.Kill(target);
            target.localScale = Vector3.one * 0.15f;
            var sequence = DOTween.Sequence();
            sequence.Append(target.DOScale(1.2f, _duration * 0.55f).SetEase(Ease.OutBack));
            sequence.Append(target.DOScale(1f, _duration * 0.45f).SetEase(Ease.OutQuad));
        }
    }
}
