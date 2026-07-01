using UnityEngine;
using Zenject;

namespace MagicArcher.UI
{
    public sealed class HealthBarPresenter : MonoBehaviour
    {
        const float FullHealthThreshold = 0.999f;

        [SerializeField] Transform _anchor;
        [SerializeField] HealthBarProfile _profile = HealthBarProfile.ArcherDefault;

        HealthBarUiService _service;
        IHealthBarProvider _provider;
        HealthBarUiView _view;

        public Transform Anchor => _anchor != null ? _anchor : transform;
        public HealthBarProfile Profile => _profile;
        public float NormalizedFill => _provider != null ? _provider.NormalizedHealth : 1f;

        internal HealthBarUiView View
        {
            get => _view;
            set => _view = value;
        }

        public bool ShouldBeVisible()
        {
            if (!Profile.HideWhenFull)
                return true;

            return NormalizedFill < FullHealthThreshold;
        }

        public void SetProfile(HealthBarProfile profile)
        {
            _profile = profile;
            _view?.ApplyProfile(_profile);
            _service?.RefreshPresenter(this);
        }

        void Awake()
        {
            _provider = GetComponent<IHealthBarProvider>();
        }

        [Inject]
        void Construct([Inject(Optional = true)] HealthBarUiService service = null)
        {
            _service = service;
        }

        void OnEnable()
        {
            TryRegister();
        }

        void Start()
        {
            TryRegister();
        }

        void TryRegister()
        {
            if (_provider != null)
                _provider.HealthChanged -= OnHealthChanged;

            if (_provider != null)
                _provider.HealthChanged += OnHealthChanged;

            _service?.Register(this);
        }

        void OnDisable()
        {
            if (_provider != null)
                _provider.HealthChanged -= OnHealthChanged;

            _service?.Unregister(this);
        }

        void OnHealthChanged()
        {
            _service?.RefreshPresenter(this);
        }
    }
}
