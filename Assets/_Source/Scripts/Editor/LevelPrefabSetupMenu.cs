using MagicArcher.Gameplay.Enemies;
using MagicArcher.Gameplay.Grid;
using MagicArcher.Gameplay.Level;
using UnityEditor;
using UnityEngine;

namespace MagicArcher.Editor
{
    public static class LevelPrefabSetupMenu
    {
        const string LevelPrefabPath = "Assets/_Source/Prefabs/Level.prefab";
        const string ForestModelPath = "Assets/Third Party/AssetsFotTestTask/Models/Forest 2.fbx";
        const string GridCellMaterialPath = "Assets/_Source/Materials/GridCell.mat";
        const string SquadCellTexturePath = "Assets/Third Party/AssetsFotTestTask/Sprites/SquadCell.png";

        [MenuItem("Magic Archer/Setup Level Prefab")]
        public static void SetupLevelPrefab()
        {
            EnsureGridCellMaterial();

            var prefabRoot = PrefabUtility.LoadPrefabContents(LevelPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"Level prefab not found: {LevelPrefabPath}");
                return;
            }

            try
            {
                CleanupPlaceholders(prefabRoot.transform);
                var levelRoot = GetOrAdd<LevelRoot>(prefabRoot);

                var environment = GetOrCreateChild(prefabRoot.transform, "Environment");
                var grid = GetOrCreateChild(prefabRoot.transform, "Grid");
                var gridOrigin = GetOrCreateChild(grid, "GridOrigin");
                var cells = GetOrCreateChild(grid, "Cells");
                var units = GetOrCreateChild(prefabRoot.transform, "Units");
                var enemies = GetOrCreateChild(prefabRoot.transform, "Enemies");
                var enemyPath = GetOrCreateChild(prefabRoot.transform, "EnemyPath");

                levelRoot.SetEditorReferences(environment, grid, gridOrigin, units, enemies, enemyPath);

                SetupForest(environment);
                SetupGrid(grid, gridOrigin, cells);
                SetupEnemyPath(enemyPath);

                EditorUtility.SetDirty(prefabRoot);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, LevelPrefabPath);
                Debug.Log("Level prefab setup complete.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        static void CleanupPlaceholders(Transform root)
        {
            var plane = root.Find("Plane");
            if (plane != null)
                Object.DestroyImmediate(plane.gameObject);

            var cube = root.Find("Cube");
            if (cube != null)
                Object.DestroyImmediate(cube.gameObject);
        }

        static void SetupForest(Transform environmentRoot)
        {
            var existing = environmentRoot.Find("Forest");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            var forestAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ForestModelPath);
            if (forestAsset == null)
            {
                Debug.LogWarning($"Forest model not found: {ForestModelPath}");
                return;
            }

            var forest = (GameObject)PrefabUtility.InstantiatePrefab(forestAsset, environmentRoot);
            forest.name = "Forest";
            forest.transform.localPosition = Vector3.zero;
            forest.transform.localRotation = Quaternion.identity;
            forest.transform.localScale = Vector3.one;
        }

        static void SetupGrid(Transform gridRoot, Transform gridOrigin, Transform cellsRoot)
        {
            gridOrigin.localPosition = new Vector3(0f, 0f, 0f);
            cellsRoot.localPosition = Vector3.zero;

            var board = GetOrAdd<GridBoardView>(gridRoot.gameObject);
            var material = AssetDatabase.LoadAssetAtPath<Material>(GridCellMaterialPath);
            board.Configure(gridOrigin, 2f, material, cellsRoot);
            board.RebuildForEditor();
        }

        static void SetupEnemyPath(Transform enemyPathRoot)
        {
            var path = GetOrAdd<EnemyPathView>(enemyPathRoot.gameObject);
            var waypoints = new Transform[4];

            for (var i = 0; i < waypoints.Length; i++)
            {
                var point = GetOrCreateChild(enemyPathRoot, $"Waypoint_{i}");
                point.localPosition = new Vector3(-8f + i * 2.5f, 0f, -2f + i * 0.75f);
                waypoints[i] = point;
            }

            path.SetWaypoints(waypoints);
        }

        static void EnsureGridCellMaterial()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(GridCellMaterialPath);
            if (existing != null)
                return;

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(SquadCellTexturePath);
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Unlit/Texture");

            var material = new Material(shader);
            if (texture != null)
                material.mainTexture = texture;

            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.renderQueue = 3000;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

            AssetDatabase.CreateAsset(material, GridCellMaterialPath);
            AssetDatabase.SaveAssets();
        }

        static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        static Transform GetOrCreateChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
                return child;

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        static void ClearChildren(Transform root)
        {
            for (var i = root.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(root.GetChild(i).gameObject);
        }
    }
}