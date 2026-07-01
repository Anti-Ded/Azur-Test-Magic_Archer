using MagicArcher.Gameplay.Enemies;
using MagicArcher.Gameplay.Units;
using MagicArcher.UI;
using UnityEditor;
using UnityEngine;

namespace MagicArcher.Editor
{
    public static class RebuildCombatUnitPrefabsMenu
    {
        const string ArcherPrefabPath = CombatPrefabPaths.Archer;
        const string OrcPrefabPath = CombatPrefabPaths.Orc;
        const string ElfArcherGlb = "Assets/Third Party/AssetsFotTestTask/Models/ElfArcher/ElfArcher.glb";
        const string ElfArcherBowGlb = "Assets/Third Party/AssetsFotTestTask/Models/ElfArcher/ElfArcher_Bow.glb";
        const string OrcGlb = "Assets/Third Party/AssetsFotTestTask/Models/Orc/Orc.glb";
        const string OrcAxeGlb = "Assets/Third Party/AssetsFotTestTask/Models/Orc/Orc_Axe.glb";
        const string ElfArcherMaterialPath = "Assets/Third Party/AssetsFotTestTask/Models/ElfArcher/Elf Archer.mat";
        const string ElfArcherBowMaterialPath = "Assets/Third Party/AssetsFotTestTask/Models/ElfArcher/Elf Archer Bow.mat";
        const string OrcMaterialPath = "Assets/Third Party/AssetsFotTestTask/Models/Orc/Materials/Merida&Orc.mat";
        const string GltfImporterGuid = "715df9372183c47e389bb6e19fbc3b52";

        static readonly string[] GlbPaths =
        {
            ElfArcherGlb,
            ElfArcherBowGlb,
            OrcGlb,
            OrcAxeGlb
        };

        [MenuItem("Magic Archer/Fix GLB And Rebuild Unit Prefabs")]
        public static void FixGlbAndRebuildUnitPrefabs()
        {
            if (!EditorUtility.DisplayDialog(
                    "Rebuild Unit Prefabs",
                    "Полностью пересоздаст префабы Archer и Orc из GLB.\n" +
                    "Анимации и ручные настройки префабов будут потеряны.\n\n" +
                    "Аниматоры НЕ перенастраиваются автоматически — запусти " +
                    "\"Setup Unit Animators\" отдельно, только если нужно.",
                    "Пересоздать",
                    "Отмена"))
                return;

            if (!EnsureGltFastInstalled())
                return;

            FixGlbMetaFiles();
            ReimportGlbs();

            if (!CanLoadModel(ElfArcherGlb) || !CanLoadModel(OrcGlb))
            {
                Debug.LogError(
                    "GLB models still cannot be loaded. Wait for Unity to finish importing, " +
                    "then run Magic Archer → Fix GLB And Rebuild Unit Prefabs again.");
                return;
            }

            RebuildArcherPrefab();
            RebuildOrcPrefab();
            SetupCombatSceneMenu.RepairCombatPrefabReferences();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("GLB models and Archer/Orc prefabs rebuilt. Animations were not modified.");
        }

        static bool EnsureGltFastInstalled()
        {
            var packagePath = "Packages/com.unity.cloud.gltfast/package.json";
            if (System.IO.File.Exists(packagePath))
                return true;

            Debug.LogError(
                "Package com.unity.cloud.gltfast is missing. It was added to Packages/manifest.json. " +
                "Return to Unity, wait for Package Manager to finish, then run this menu again.");
            return false;
        }

        static void FixGlbMetaFiles()
        {
            foreach (var path in GlbPaths)
            {
                if (!System.IO.File.Exists(path))
                    continue;

                var metaPath = path + ".meta";
                if (!System.IO.File.Exists(metaPath))
                    continue;

                var meta = System.IO.File.ReadAllText(metaPath);
                if (meta.Contains("guid: ") && meta.Contains("ScriptedImporter:") &&
                    meta.Contains(GltfImporterGuid))
                    continue;

                var guidStart = meta.IndexOf("guid: ", System.StringComparison.Ordinal);
                if (guidStart < 0)
                    continue;

                var guidLineEnd = meta.IndexOf('\n', guidStart);
                var guidLine = meta.Substring(guidStart, guidLineEnd - guidStart).TrimEnd('\r');

                var fixedMeta =
                    "fileFormatVersion: 2\n" +
                    guidLine + "\n" +
                    "ScriptedImporter:\n" +
                    "  internalIDToNameTable: []\n" +
                    "  externalObjects: {}\n" +
                    "  serializedVersion: 2\n" +
                    "  userData: \n" +
                    "  assetBundleName: \n" +
                    "  assetBundleVariant: \n" +
                    "  script: {fileID: 11500000, guid: " + GltfImporterGuid + ", type: 3}\n" +
                    "  editorImportSettings:\n" +
                    "    generateSecondaryUVSet: 0\n" +
                    "  importSettings:\n" +
                    "    nodeNameMethod: 1\n" +
                    "    animationMethod: 2\n" +
                    "    generateMipMaps: 1\n" +
                    "    texturesReadable: 0\n" +
                    "    defaultMinFilterMode: 9729\n" +
                    "    defaultMagFilterMode: 9729\n" +
                    "    anisotropicFilterLevel: 1\n" +
                    "  instantiationSettings:\n" +
                    "    mask: -1\n" +
                    "    layer: 0\n" +
                    "    skinUpdateWhenOffscreen: 1\n" +
                    "    lightIntensityFactor: 1\n" +
                    "    sceneObjectCreation: 2\n" +
                    "  assetDependencies: []\n" +
                    "  reportItems: []\n";

                System.IO.File.WriteAllText(metaPath, fixedMeta);
                Debug.Log($"GLB importer fixed: {path}");
            }
        }

        static void ReimportGlbs()
        {
            foreach (var path in GlbPaths)
            {
                if (!System.IO.File.Exists(path))
                {
                    Debug.LogWarning($"GLB not found, skipped: {path}");
                    continue;
                }

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        static bool CanLoadModel(string glbPath)
        {
            return LoadModelRoot(glbPath) != null;
        }

        static GameObject LoadModelRoot(string glbPath)
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
            if (root != null)
                return root;

            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(glbPath))
            {
                if (asset is GameObject go && go.transform.parent == null)
                    return go;
            }

            return null;
        }

        static GameObject InstantiateEmbeddedModel(string glbPath, Transform parent)
        {
            var asset = LoadModelRoot(glbPath);
            if (asset == null)
            {
                Debug.LogError($"Failed to load model: {glbPath}");
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            if (instance == null)
                instance = Object.Instantiate(asset, parent);

            PrefabUtility.UnpackPrefabInstance(
                instance,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);

            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            return instance;
        }

        static void RebuildArcherPrefab()
        {
            var root = new GameObject("Archer");
            try
            {
                var shooter = root.AddComponent<ArcherShooter>();
                var health = root.AddComponent<UnitHealth>();
                var unit = root.AddComponent<UnitView>();
                root.AddComponent<UnitHealthBarProvider>();

                var presenter = root.AddComponent<HealthBarPresenter>();
                presenter.SetProfile(HealthBarProfile.ArcherDefault);

                var muzzle = CreateChild(root.transform, "Muzzle", new Vector3(0f, 1.3f, 0.4f));
                var healthAnchor = CreateChild(root.transform, "HealthBarAnchor", new Vector3(0f, 1.8f, 0f));

                var model = InstantiateEmbeddedModel(ElfArcherGlb, root.transform);
                if (model == null)
                    return;

                model.name = "ElfArcher";
                ApplyMaterial(model, ElfArcherMaterialPath);

                var hand = FindBone(
                    model.transform,
                    "RightHand",
                    "mixamorig:RightHand",
                    "Hand_R",
                    "hand_r",
                    "Hand.R");

                var bowParent = hand != null ? hand : model.transform;
                var bow = InstantiateEmbeddedModel(ElfArcherBowGlb, bowParent);
                if (bow != null)
                {
                    bow.name = "ElfArcher_Bow";
                    bow.transform.localPosition = hand != null ? new Vector3(0f, 0.078f, 0f) : Vector3.zero;
                    bow.transform.localRotation = hand != null
                        ? Quaternion.Euler(0f, -90f, -180f)
                        : Quaternion.identity;
                    bow.transform.localScale = Vector3.one * 0.5f;
                    ApplyMaterial(bow, ElfArcherBowMaterialPath);
                }

                var animator = model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
                animator.applyRootMotion = false;

                Wire(unit, "_shooter", shooter);
                Wire(unit, "_health", health);
                Wire(shooter, "_muzzle", muzzle);
                Wire(shooter, "_animator", animator);
                Wire(presenter, "_anchor", healthAnchor);

                SavePrefab(root, ArcherPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static void RebuildOrcPrefab()
        {
            var root = new GameObject("Orc");
            try
            {
                var health = root.AddComponent<EnemyHealth>();
                var motor = root.AddComponent<EnemyMotor>();
                var view = root.AddComponent<EnemyView>();
                var presenter = root.AddComponent<HealthBarPresenter>();
                presenter.SetProfile(HealthBarProfile.OrcDefault);

                var aimPoint = CreateChild(root.transform, "AimPoint", new Vector3(0f, 1.2f, 0f));
                var healthAnchor = CreateChild(root.transform, "HealthBarAnchor", new Vector3(0f, 1.6f, 0f));

                var model = InstantiateEmbeddedModel(OrcGlb, root.transform);
                if (model == null)
                    return;

                model.name = "Orc";
                ApplyMaterial(model, OrcMaterialPath);

                var hand = FindBone(
                    model.transform,
                    "RightHand",
                    "mixamorig:RightHand",
                    "Hand_R",
                    "hand_r");

                var axeParent = hand != null ? hand : model.transform;
                var axe = InstantiateEmbeddedModel(OrcAxeGlb, axeParent);
                if (axe != null)
                {
                    axe.name = "Orc_Axe";
                    axe.transform.localPosition = hand != null
                        ? new Vector3(-1.478f, 0.737f, 0.16f)
                        : Vector3.zero;
                    axe.transform.localRotation = hand != null
                        ? Quaternion.Euler(-90f, 90f, 90f)
                        : Quaternion.identity;
                    axe.transform.localScale = Vector3.one * 3f;
                    ApplyMaterial(axe, OrcMaterialPath);
                }

                var animator = model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
                animator.applyRootMotion = false;

                Wire(view, "_health", health);
                Wire(view, "_motor", motor);
                Wire(health, "_aimPoint", aimPoint);
                Wire(presenter, "_anchor", healthAnchor);

                SavePrefab(root, OrcPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static Transform CreateChild(Transform parent, string name, Vector3 localPosition)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = localPosition;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
            return child;
        }

        static Transform FindBone(Transform root, params string[] names)
        {
            foreach (var name in names)
            {
                var direct = root.Find(name);
                if (direct != null)
                    return direct;
            }

            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                foreach (var name in names)
                {
                    if (transform.name.Contains(name, System.StringComparison.OrdinalIgnoreCase))
                        return transform;
                }
            }

            return null;
        }

        static void ApplyMaterial(GameObject target, string materialPath)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
                return;

            foreach (var renderer in target.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial = material;
        }

        static void Wire(Object target, string propertyName, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propertyName).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SavePrefab(GameObject root, string path)
        {
            EnsureFolder("Assets/_Source/Prefabs");
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Debug.Log($"Prefab saved: {path}");
        }

        static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            var parts = folderPath.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);

                current = next;
            }
        }
    }
}
