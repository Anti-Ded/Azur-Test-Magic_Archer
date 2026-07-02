using UnityEngine;

namespace MagicArcher.Gameplay.Enemies
{
    public static class EnemySpawnLayout
    {
        public static Vector3 GetPrewarmPosition(EnemyPathView path, int index, float spacing)
        {
            if (path == null || path.WaypointCount < 2)
                return path != null ? path.GetWaypointPosition(0) : Vector3.zero;

            var start = path.GetWaypointPosition(0);
            var end = path.GetWaypointPosition(1);
            var direction = end - start;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
                return start;

            return start + direction.normalized * (index * spacing);
        }

        public static Vector3 GetBossPrewarmPosition(EnemyPathView path, float spacing)
        {
            return GetPrewarmPosition(path, 0, spacing);
        }

        public static int GetRegularPrewarmIndex(int queueIndex)
        {
            return queueIndex + 1;
        }

        public static Vector3 GetPrewarmPosition(EnemyPathView path, int index)
        {
            return GetPrewarmPosition(path, index, 1.5f);
        }
    }
}
