using MagicArcher.Gameplay.Combat;
using UnityEditor;
using UnityEngine;

namespace MagicArcher.Editor
{
    public static class CreateCombatPrefabsMenu
    {
        const string ProjectilePrefabPath = "Assets/_Source/Prefabs/Projectile.prefab";

        [MenuItem("Magic Archer/Create Combat Prefabs")]
        public static void CreatePrefabs()
        {
            CreateProjectilePrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Projectile prefab updated at Assets/_Source/Prefabs/Projectile.prefab");
        }

        static void CreateProjectilePrefab()
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            root.name = "Projectile";
            root.transform.localScale = Vector3.one * 0.2f;
            Object.DestroyImmediate(root.GetComponent<Collider>());
            root.AddComponent<ProjectileView>();
            PrefabUtility.SaveAsPrefabAsset(root, ProjectilePrefabPath);
            Object.DestroyImmediate(root);
        }
    }
}
