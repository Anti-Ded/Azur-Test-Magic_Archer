using MagicArcher.Gameplay.Units;
using UnityEngine;

namespace MagicArcher.UI
{
    public sealed class MergeDragGhostView : MonoBehaviour
    {
        Animator _animator;

        void Awake()
        {
            _animator = GetComponentInChildren<Animator>(true);
        }

        public void AlignFrom(UnitView source)
        {
            if (source == null)
                return;

            transform.rotation = source.transform.rotation;

            var sourceAnimator = source.GetComponentInChildren<Animator>();
            CopyAnimatorState(sourceAnimator, _animator);
        }

        public void SetPosition(Vector3 worldPosition, float dragHeight)
        {
            worldPosition.y += dragHeight;
            transform.position = worldPosition;
        }

        static void CopyAnimatorState(Animator source, Animator target)
        {
            if (source == null || target == null)
                return;

            var state = source.GetCurrentAnimatorStateInfo(0);
            target.Play(state.shortNameHash, 0, state.normalizedTime);
            target.Update(0f);
        }
    }
}
