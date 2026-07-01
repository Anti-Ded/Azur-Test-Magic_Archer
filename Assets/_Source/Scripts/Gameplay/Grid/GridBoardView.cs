using MagicArcher.Core;
using UnityEngine;

namespace MagicArcher.Gameplay.Grid
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public sealed class GridBoardView : MonoBehaviour
    {
        [SerializeField] Transform _origin;
        [SerializeField] float _cellSize = 2f;
        [SerializeField] float _yOffset = 0.02f;
        [SerializeField] Vector3 _cellScale = new(0.95f, 1f, 0.95f);
        [SerializeField] Material _cellMaterial;
        [SerializeField] Transform _cellsRoot;

        public float CellSize => _cellSize;

        void Awake()
        {
            if (!Application.isPlaying)
                return;

            if (_cellsRoot == null || _cellsRoot.childCount > 0)
                return;

            BuildCells(false);
        }

        public void Configure(Transform origin, float cellSize, Material cellMaterial, Transform cellsRoot)
        {
            _origin = origin;
            _cellSize = cellSize;
            _cellMaterial = cellMaterial;
            _cellsRoot = cellsRoot;
        }

        [ContextMenu("Rebuild Grid")]
        public void RebuildForEditor()
        {
            if (_cellsRoot == null)
            {
                Debug.LogWarning("GridBoardView: Cells Root is not assigned.", this);
                return;
            }

            ClearCells(true);
            BuildCells(true);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            if (_cellsRoot != null)
                UnityEditor.EditorUtility.SetDirty(_cellsRoot.gameObject);
#endif
        }

        void ClearCells(bool isEditor)
        {
            if (_cellsRoot == null)
                return;

            for (var i = _cellsRoot.childCount - 1; i >= 0; i--)
            {
                var child = _cellsRoot.GetChild(i).gameObject;
                if (isEditor)
                    DestroyImmediate(child);
                else
                    Destroy(child);
            }
        }

        void BuildCells(bool isEditor)
        {
            var originPosition = _origin != null ? _origin.position : transform.position;
            var offsetX = -(GameConstants.GridWidth - 1) * _cellSize * 0.5f;
            var offsetZ = -(GameConstants.GridHeight - 1) * _cellSize * 0.5f;

            for (var y = 0; y < GameConstants.GridHeight; y++)
            {
                for (var x = 0; x < GameConstants.GridWidth; x++)
                {
                    var cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    cell.name = $"Cell_{x}_{y}";
                    cell.transform.SetParent(_cellsRoot, false);

                    var position = originPosition + new Vector3(
                        offsetX + x * _cellSize,
                        _yOffset,
                        offsetZ + y * _cellSize);

                    cell.transform.position = position;
                    cell.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    cell.transform.localScale = _cellScale;

                    if (_cellMaterial != null)
                    {
                        var renderer = cell.GetComponent<MeshRenderer>();
                        renderer.sharedMaterial = _cellMaterial;
                    }

                    var collider = cell.GetComponent<Collider>();
                    if (collider == null)
                        continue;

                    if (isEditor)
                        DestroyImmediate(collider);
                    else
                        Destroy(collider);
                }
            }
        }
    }
}
