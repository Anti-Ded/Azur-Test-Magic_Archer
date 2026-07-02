using MagicArcher.Gameplay.Flow;
using UnityEngine;
using Zenject;

namespace MagicArcher.Gameplay.Enemies
{
    // NOTE: walk animation was not provided in the test assets; movement is translation-only.
    public sealed class EnemyMotor : MonoBehaviour
    {
        [SerializeField] float _moveSpeed = 1.6f;

        GamePhaseService _phases;
        EnemyPathView _path;

        int _waypointIndex;

        bool _moving;



        public bool IsMoving => _moving;



        public event System.Action ReachedGrid;

        [Inject]
        void Construct([Inject(Optional = true)] GamePhaseService phases = null)
        {
            _phases = phases;
        }

        public void ConfigureMoveSpeed(float moveSpeed)
        {
            _moveSpeed = Mathf.Max(0.01f, moveSpeed);
        }

        public void Begin(EnemyPathView path)
        {
            _path = path;
            _waypointIndex = ResolveStartWaypointIndex(path, transform.position);
            _moving = path != null && path.WaypointCount > 0;
        }

        public void Resume()
        {
            if (_path == null || _path.WaypointCount <= 0)
                return;

            _moving = true;
        }



        public void Stop()

        {

            _moving = false;

        }



        void Update()
        {
            if (!_moving || _path == null)
                return;

            if (IsCombatPaused())
                return;



            var target = _path.GetWaypointPosition(_waypointIndex);

            var step = _moveSpeed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, target, step);



            var flatTarget = target;

            flatTarget.y = transform.position.y;

            var forward = flatTarget - transform.position;

            if (forward.sqrMagnitude > 0.001f)

                transform.rotation = Quaternion.LookRotation(forward);



            if ((transform.position - target).sqrMagnitude > 0.05f)

                return;



            if (_waypointIndex >= _path.WaypointCount - 1)

            {

                _moving = false;

                ReachedGrid?.Invoke();

                return;

            }



            _waypointIndex++;
        }

        bool IsCombatPaused()
        {
            return _phases != null && _phases.IsTutorialCombatPaused;
        }

        static int ResolveStartWaypointIndex(EnemyPathView path, Vector3 position)
        {
            if (path == null || path.WaypointCount <= 1)
                return 0;

            const float reachedThresholdSqr = 0.05f * 0.05f;

            for (var i = 0; i < path.WaypointCount - 1; i++)
            {
                var from = path.GetWaypointPosition(i);
                var to = path.GetWaypointPosition(i + 1);
                from.y = position.y;
                to.y = position.y;

                var segment = to - from;
                if (segment.sqrMagnitude < 0.0001f)
                    continue;

                var progress = Vector3.Dot(position - from, segment.normalized);
                if (progress > segment.magnitude + 0.05f)
                    continue;

                if ((position - to).sqrMagnitude <= reachedThresholdSqr)
                    continue;

                return i + 1;
            }

            return path.WaypointCount - 1;
        }
    }
}
