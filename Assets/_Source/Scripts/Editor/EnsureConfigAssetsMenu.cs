using MagicArcher.Core.Config;
using MagicArcher.Gameplay.Enemies;
using MagicArcher.Gameplay.Units;
using MagicArcher.Installers;
using UnityEditor;
using UnityEngine;

namespace MagicArcher.Editor
{
    public static class EnsureConfigAssetsMenu
    {
        const string OrcPrefabPath = CombatPrefabPaths.Orc;
        const string ArcherPrefabPath = CombatPrefabPaths.Archer;

        [MenuItem("Magic Archer/Ensure Config Assets")]
        public static void EnsureConfigAssets()
        {
            EnsureFolder("Assets/_Source", "Configs");
            var regularEnemy = EnsureRegularEnemyConfig();
            var bossEnemy = EnsureBossEnemyConfig();
            var regularUnit = EnsureRegularUnitConfig();
            var upgradedUnit = EnsureUpgradedUnitConfig();
            AssignConfigsToInstaller(regularEnemy, bossEnemy, regularUnit, upgradedUnit);
            AssetDatabase.SaveAssets();
            Debug.Log(
                $"Config assets ready.\n" +
                $"- {RegularEnemyConfig.DefaultAssetPath}\n" +
                $"- {BossEnemyConfig.DefaultAssetPath}\n" +
                $"- {RegularUnitConfig.DefaultAssetPath}\n" +
                $"- {UpgradedUnitConfig.DefaultAssetPath}");
        }

        public static RegularEnemyConfig EnsureRegularEnemyConfig()
        {
            var config = LoadOrCreate<RegularEnemyConfig>(RegularEnemyConfig.DefaultAssetPath);
            AssignEnemyPrefabIfMissing(config, "_prefab");
            EditorUtility.SetDirty(config);
            return config;
        }

        public static BossEnemyConfig EnsureBossEnemyConfig()
        {
            var config = LoadOrCreate<BossEnemyConfig>(BossEnemyConfig.DefaultAssetPath);
            AssignEnemyPrefabIfMissing(config, "_prefab");
            EditorUtility.SetDirty(config);
            return config;
        }

        public static RegularUnitConfig EnsureRegularUnitConfig()
        {
            var config = LoadOrCreate<RegularUnitConfig>(RegularUnitConfig.DefaultAssetPath);
            AssignUnitPrefabIfMissing(config, "_prefab");
            EditorUtility.SetDirty(config);
            return config;
        }

        public static UpgradedUnitConfig EnsureUpgradedUnitConfig()
        {
            var config = LoadOrCreate<UpgradedUnitConfig>(UpgradedUnitConfig.DefaultAssetPath);
            AssignUnitPrefabIfMissing(config, "_prefab");
            EditorUtility.SetDirty(config);
            return config;
        }

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var config = AssetDatabase.LoadAssetAtPath<T>(path);
            if (config != null)
                return config;

            config = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(config, path);
            return config;
        }

        static void AssignEnemyPrefabIfMissing(Object config, string propertyName)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<EnemyView>(OrcPrefabPath);
            AssignPrefabIfMissing(config, propertyName, prefab);
        }

        static void AssignUnitPrefabIfMissing(Object config, string propertyName)
        {
            var unit = AssetDatabase.LoadAssetAtPath<GameObject>(ArcherPrefabPath);
            if (unit == null)
            {
                var unitView = AssetDatabase.LoadAssetAtPath<UnitView>(ArcherPrefabPath);
                unit = unitView != null ? unitView.gameObject : null;
            }

            AssignPrefabIfMissing(config, propertyName, unit);
        }

        static void AssignPrefabIfMissing(Object config, string propertyName, Object prefab)
        {
            if (prefab == null)
                return;

            var so = new SerializedObject(config);
            if (so.FindProperty(propertyName).objectReferenceValue == null)
                so.FindProperty(propertyName).objectReferenceValue = prefab;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AssignConfigsToInstaller(
            RegularEnemyConfig regularEnemy,
            BossEnemyConfig bossEnemy,
            RegularUnitConfig regularUnit,
            UpgradedUnitConfig upgradedUnit)
        {
            var installer = Object.FindFirstObjectByType<GameInstaller>();
            if (installer == null)
                return;

            var so = new SerializedObject(installer);
            so.FindProperty("_regularEnemyConfig").objectReferenceValue = regularEnemy;
            so.FindProperty("_bossEnemyConfig").objectReferenceValue = bossEnemy;
            so.FindProperty("_regularUnitConfig").objectReferenceValue = regularUnit;
            so.FindProperty("_upgradedUnitConfig").objectReferenceValue = upgradedUnit;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(installer);
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
