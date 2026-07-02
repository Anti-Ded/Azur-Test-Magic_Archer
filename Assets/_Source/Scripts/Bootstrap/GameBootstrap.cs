using MagicArcher.Core.Cta;
using MagicArcher.StateMachine;
using UnityEngine;
using Zenject;

namespace MagicArcher.Bootstrap
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        GameStateMachine _stateMachine;
        ICtaService _ctaService;

        [Inject]
        void Construct(GameStateMachine stateMachine, ICtaService ctaService)
        {
            _stateMachine = stateMachine;
            _ctaService = ctaService;
        }

        void Update()
        {
            if (_stateMachine == null)
                return;

            _stateMachine.Tick();
        }

        void LateUpdate()
        {
            if (_ctaService == null)
                return;

            if (Input.GetMouseButtonDown(0))
                _ctaService.TryInvoke();
        }
    }
}
