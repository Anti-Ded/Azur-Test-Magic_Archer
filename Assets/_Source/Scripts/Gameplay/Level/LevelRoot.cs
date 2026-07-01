using UnityEngine;

namespace MagicArcher.Gameplay.Level
{
    public sealed class LevelRoot : MonoBehaviour
    {
        [SerializeField] Transform _environmentRoot;
        [SerializeField] Transform _gridRoot;
        [SerializeField] Transform _gridOrigin;
        [SerializeField] Transform _unitsRoot;
        [SerializeField] Transform _enemiesRoot;
        [SerializeField] Transform _enemyPathRoot;
        [SerializeField] float _cellSize = 2f;

        public Transform EnvironmentRoot => _environmentRoot;
        public Transform GridRoot => _gridRoot;
        public Transform GridOrigin => _gridOrigin != null ? _gridOrigin : _gridRoot;
        public Transform UnitsRoot => _unitsRoot;
        public Transform EnemiesRoot => _enemiesRoot;
        public Transform EnemyPathRoot => _enemyPathRoot;
        public float CellSize => _cellSize;

#if UNITY_EDITOR
        public void SetEditorReferences(
            Transform environmentRoot,
            Transform gridRoot,
            Transform gridOrigin,
            Transform unitsRoot,
            Transform enemiesRoot,
            Transform enemyPathRoot)
        {
            _environmentRoot = environmentRoot;
            _gridRoot = gridRoot;
            _gridOrigin = gridOrigin;
            _unitsRoot = unitsRoot;
            _enemiesRoot = enemiesRoot;
            _enemyPathRoot = enemyPathRoot;
        }
#endif
    }
}
