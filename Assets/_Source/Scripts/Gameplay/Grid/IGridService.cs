using MagicArcher.Gameplay.Units;
using UnityEngine;

namespace MagicArcher.Gameplay.Grid
{
    public readonly struct GridSlot
    {
        public readonly int X;
        public readonly int Y;

        public GridSlot(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public interface IGridService
    {
        int Width { get; }
        int Height { get; }
        bool TryGetEmptySlot(out GridSlot slot);
        bool TryPlace(int x, int y, UnitView unit);
        bool TryRemove(int x, int y, out UnitView unit);
        bool TryGetUnit(int x, int y, out UnitView unit);
        void ForEachUnit(System.Action<UnitView> action);
        Vector3 GetWorldPosition(int x, int y);
        bool TryMove(int fromX, int fromY, int toX, int toY);
        bool TryGetSlotFromWorld(Vector3 worldPosition, out GridSlot slot);
        void ClearAllUnits();
    }
}
