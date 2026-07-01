using UnityEngine;

namespace MagicArcher.Gameplay.Level
{
    public sealed class GameplayCameraLayout : MonoBehaviour
    {
        [SerializeField] Camera _camera;
        [SerializeField] Vector3 _landscapePosition = new(0.58f, 19.51f, -18.1f);
        [SerializeField] Vector3 _landscapeEuler = new(69.254f, 0f, 0f);
        [SerializeField] float _landscapeFov = 60f;
        [SerializeField] Vector3 _portraitPosition = new(0.58f, 21.5f, -20.5f);
        [SerializeField] Vector3 _portraitEuler = new(69.254f, 0f, 0f);
        [SerializeField] float _portraitFov = 64f;

        bool? _isLandscape;

        void Awake()
        {
            if (_camera == null)
                _camera = GetComponent<Camera>();

            ApplyCurrentLayout(force: true);
        }

        void Update()
        {
            ApplyCurrentLayout(force: false);
        }

        void ApplyCurrentLayout(bool force)
        {
            if (_camera == null)
                return;

            var landscape = Screen.width >= Screen.height;
            if (!force && _isLandscape.HasValue && _isLandscape.Value == landscape)
                return;

            _isLandscape = landscape;
            var position = landscape ? _landscapePosition : _portraitPosition;
            var euler = landscape ? _landscapeEuler : _portraitEuler;
            var fov = landscape ? _landscapeFov : _portraitFov;

            transform.SetPositionAndRotation(position, Quaternion.Euler(euler));
            _camera.fieldOfView = fov;
        }
    }
}
