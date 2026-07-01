using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MagicArcher.UI
{
    public sealed class HealthBarUiService : ILateTickable
    {
        RectTransform _container;
        HealthBarUiView _prefab;
        RectTransform _canvasRect;
        Canvas _canvas;
        readonly List<HealthBarPresenter> _presenters = new();
        Camera _camera;

        public void Configure(RectTransform container, HealthBarUiView prefab, Canvas canvas)
        {
            if (container != null)
                _container = container;

            if (prefab != null)
                _prefab = prefab;

            if (canvas != null)
            {
                _canvas = canvas;
                _canvasRect = canvas.transform as RectTransform;
            }
        }

        public void Register(HealthBarPresenter presenter)
        {
            if (presenter == null)
                return;

            if (!_presenters.Contains(presenter))
                _presenters.Add(presenter);

            if (presenter.View == null && _prefab != null && _container != null)
            {
                var view = Object.Instantiate(_prefab, _container);
                view.ApplyProfile(presenter.Profile);
                presenter.View = view;
            }

            RefreshPresenter(presenter);
        }

        public void Unregister(HealthBarPresenter presenter)
        {
            if (presenter == null)
                return;

            if (presenter.View != null)
            {
                presenter.View.SetVisible(false);
                Object.Destroy(presenter.View.gameObject);
                presenter.View = null;
            }

            _presenters.Remove(presenter);
        }

        public void RefreshPresenter(HealthBarPresenter presenter)
        {
            var view = presenter?.View;
            if (view == null)
                return;

            view.SetFill(presenter.NormalizedFill);
            view.SetVisible(presenter.ShouldBeVisible());
        }

        public void LateTick()
        {
            if (_canvasRect == null || _presenters.Count == 0)
                return;

            if (_camera == null)
                _camera = Camera.main;

            if (_camera == null)
                return;

            var overlay = _canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay;

            for (var i = _presenters.Count - 1; i >= 0; i--)
            {
                var presenter = _presenters[i];
                if (presenter == null || !presenter.isActiveAndEnabled)
                    continue;

                UpdatePosition(presenter, overlay);
            }
        }

        void UpdatePosition(HealthBarPresenter presenter, bool overlay)
        {
            var view = presenter.View;
            if (view == null)
                return;

            var rect = view.transform as RectTransform;
            if (rect == null)
                return;

            if (!presenter.ShouldBeVisible())
            {
                view.SetVisible(false);
                return;
            }

            var worldPos = presenter.Anchor.position;
            var screenPoint = _camera.WorldToScreenPoint(worldPos);

            if (screenPoint.z <= 0f)
            {
                view.SetVisible(false);
                return;
            }

            view.SetVisible(true);

            var cameraForConversion = overlay ? null : _camera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvasRect,
                    screenPoint,
                    cameraForConversion,
                    out var localPoint))
                return;

            localPoint.y += presenter.Profile.ScreenOffsetY;
            rect.anchoredPosition = localPoint;
        }
    }
}
