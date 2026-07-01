using UnityEngine;

namespace MagicArcher.Gameplay.Enemies
{
    public sealed class EnemyPathView : MonoBehaviour
    {
        [SerializeField] Transform[] _waypoints;

        public int WaypointCount => _waypoints != null ? _waypoints.Length : 0;

        public Vector3 GetWaypointPosition(int index)
        {
            if (_waypoints == null || _waypoints.Length == 0)
                return transform.position;

            index = Mathf.Clamp(index, 0, _waypoints.Length - 1);
            return _waypoints[index] != null ? _waypoints[index].position : transform.position;
        }

#if UNITY_EDITOR
        public void SetWaypoints(Transform[] waypoints)
        {
            _waypoints = waypoints;
        }

        void OnDrawGizmos()
        {
            if (_waypoints == null || _waypoints.Length == 0)
                return;

            Gizmos.color = Color.yellow;
            for (var i = 0; i < _waypoints.Length; i++)
            {
                if (_waypoints[i] == null)
                    continue;

                Gizmos.DrawSphere(_waypoints[i].position, 0.15f);
                if (i > 0 && _waypoints[i - 1] != null)
                    Gizmos.DrawLine(_waypoints[i - 1].position, _waypoints[i].position);
            }
        }
#endif
    }
}
