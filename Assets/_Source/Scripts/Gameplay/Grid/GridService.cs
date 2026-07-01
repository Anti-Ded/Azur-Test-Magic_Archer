using MagicArcher.Gameplay.Units;
using MagicArcher.Core;
using UnityEngine;

namespace MagicArcher.Gameplay.Grid
{
    public sealed class GridService : IGridService
    {
        readonly Transform _origin;
        readonly float _cellSize;
        readonly UnitView[,] _cells;

        public int Width => GameConstants.GridWidth;
        public int Height => GameConstants.GridHeight;

        public GridService(Transform origin, float cellSize = 1f)
        {
            _origin = origin;
            _cellSize = cellSize;
            _cells = new UnitView[Width, Height];
        }

        public bool TryGetEmptySlot(out GridSlot slot)
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    if (_cells[x, y] != null)
                        continue;

                    slot = new GridSlot(x, y);
                    return true;
                }
            }

            slot = default;
            return false;
        }

        public bool TryPlace(int x, int y, UnitView unit)
        {
            if (!IsInside(x, y) || _cells[x, y] != null)
                return false;

            _cells[x, y] = unit;
            unit.transform.position = GetWorldPosition(x, y);
            unit.SetGridPosition(x, y);
            return true;
        }

        public bool TryRemove(int x, int y, out UnitView unit)
        {
            unit = null;
            if (!IsInside(x, y))
                return false;

            unit = _cells[x, y];
            if (unit == null)
                return false;

            _cells[x, y] = null;
            return true;
        }

        public bool TryGetUnit(int x, int y, out UnitView unit)
        {
            unit = null;
            if (!IsInside(x, y))
                return false;

            unit = _cells[x, y];
            return unit != null;
        }

        public void ForEachUnit(System.Action<UnitView> action)
        {
            if (action == null)
                return;

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var unit = _cells[x, y];
                    if (unit != null)
                        action(unit);
                }
            }
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            var offsetX = -(Width - 1) * _cellSize * 0.5f;
            var offsetZ = -(Height - 1) * _cellSize * 0.5f;
            return _origin.position + new Vector3(
                offsetX + x * _cellSize,
                0f,
                offsetZ + y * _cellSize);
        }

        public bool TryMove(int fromX, int fromY, int toX, int toY)
        {
            if (!TryRemove(fromX, fromY, out var unit))
                return false;

            if (TryPlace(toX, toY, unit))
                return true;

            TryPlace(fromX, fromY, unit);
            return false;
        }

        public bool TryGetSlotFromWorld(Vector3 worldPosition, out GridSlot slot)
        {
            var offsetX = -(Width - 1) * _cellSize * 0.5f;
            var offsetZ = -(Height - 1) * _cellSize * 0.5f;
            var local = worldPosition - _origin.position;

            var x = Mathf.RoundToInt((local.x - offsetX) / _cellSize);
            var y = Mathf.RoundToInt((local.z - offsetZ) / _cellSize);

            if (!IsInside(x, y))
            {
                slot = default;
                return false;
            }

            slot = new GridSlot(x, y);
            return true;
        }

        public void ClearAllUnits()
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var unit = _cells[x, y];
                    if (unit == null)
                        continue;

                    _cells[x, y] = null;
                    if (unit != null)
                        UnityEngine.Object.Destroy(unit.gameObject);
                }
            }
        }

        bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}
