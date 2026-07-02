using MagicArcher.Gameplay.Grid;
using MagicArcher.Gameplay.Level;
using MagicArcher.Gameplay.Units;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace MagicArcher.Gameplay.Units
{
    public sealed class UnitDragController : MonoBehaviour
    {
        [SerializeField] float _dragHeight = 0.35f;

        IGridService _grid;
        UnitMergeService _merge;
        LevelRoot _level;
        Camera _camera;

        UnitView _dragged;
        int _fromX;
        int _fromY;
        bool _enabled;
        Plane _dragPlane;
        float _groundY;

        [Inject]
        void Construct(IGridService grid, UnitMergeService merge, LevelRoot level)
        {
            _grid = grid;
            _merge = merge;
            _level = level;
        }

        void Awake()
        {
            _camera = Camera.main;
            RefreshGroundHeight();
        }

        public void SetDragEnabled(bool enabled)
        {
            _enabled = enabled;
            if (!enabled)
                CancelDrag();
        }

        void Update()
        {
            if (!_enabled || _grid == null || _camera == null)
                return;

            if (Input.GetMouseButtonDown(0))
                TryBeginDrag();

            if (_dragged != null && Input.GetMouseButton(0))
                UpdateDragPosition();

            if (_dragged != null && Input.GetMouseButtonUp(0))
                EndDrag();
        }

        void TryBeginDrag()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (!TryGetWorldPointOnPlane(Input.mousePosition, out var worldPoint))
                return;

            if (!_grid.TryGetSlotFromWorld(worldPoint, out var slot))
                return;

            if (!_grid.TryGetUnit(slot.X, slot.Y, out var unit) || !unit.CanMerge)
                return;

            _dragged = unit;
            _fromX = slot.X;
            _fromY = slot.Y;
            _dragPlane = new Plane(Vector3.up, unit.transform.position + Vector3.up * _dragHeight);
        }

        void UpdateDragPosition()
        {
            if (!TryGetWorldPointOnPlane(Input.mousePosition, out var worldPoint))
                return;

            worldPoint.y += _dragHeight;
            _dragged.transform.position = worldPoint;
        }

        void EndDrag()
        {
            if (_dragged == null)
                return;

            if (!TryGetWorldPointOnPlane(Input.mousePosition, out var worldPoint))
            {
                SnapBack();
                return;
            }

            if (!_grid.TryGetSlotFromWorld(worldPoint, out var slot))
            {
                SnapBack();
                return;
            }

            if (_merge != null && _merge.TryMerge(_dragged, slot.X, slot.Y))
            {
                _dragged = null;
                return;
            }

            if (slot.X == _fromX && slot.Y == _fromY)
            {
                SnapBack();
                return;
            }

            if (_grid.TryGetUnit(slot.X, slot.Y, out _))
            {
                SnapBack();
                return;
            }

            if (_grid.TryMove(_fromX, _fromY, slot.X, slot.Y))
            {
                _dragged = null;
                return;
            }

            SnapBack();
        }

        void SnapBack()
        {
            if (_dragged == null)
                return;

            _dragged.transform.position = _grid.GetWorldPosition(_fromX, _fromY);
            _dragged = null;
        }

        void CancelDrag()
        {
            if (_dragged == null)
                return;

            SnapBack();
        }

        void RefreshGroundHeight()
        {
            if (_level != null && _level.GridOrigin != null)
                _groundY = _level.GridOrigin.position.y;
        }

        bool TryGetWorldPointOnPlane(Vector2 screenPosition, out Vector3 worldPoint)
        {
            worldPoint = default;
            if (_camera == null)
                return false;

            var ray = _camera.ScreenPointToRay(screenPosition);
            var plane = _dragged != null
                ? _dragPlane
                : new Plane(Vector3.up, new Vector3(0f, _groundY, 0f));

            if (!plane.Raycast(ray, out var distance))
                return false;

            worldPoint = ray.GetPoint(distance);
            return true;
        }
    }
}
