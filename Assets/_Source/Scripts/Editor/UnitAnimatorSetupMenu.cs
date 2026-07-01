using System.Collections.Generic;
using System.IO;
using MagicArcher.Gameplay.Units;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MagicArcher.Editor
{
    public static class UnitAnimatorSetupMenu
    {
        const string AnimationsFolder = "Assets/_Source/Animations";

        static readonly UnitAnimEntry[] Units =
        {
            new UnitAnimEntry(
                CombatPrefabPaths.Archer,
                "Assets/Third Party/AssetsFotTestTask/Models/ElfArcher/ElfArcher.glb",
                $"{AnimationsFolder}/Archer.controller",
                $"{AnimationsFolder}/ArcherAvatar.asset",
                true),
            new UnitAnimEntry(
                CombatPrefabPaths.Orc,
                "Assets/Third Party/AssetsFotTestTask/Models/Orc/Orc.glb",
                $"{AnimationsFolder}/Orc.controller",
                $"{AnimationsFolder}/OrcAvatar.asset",
                false,
                "Assets/Third Party/AssetsFotTestTask/Models/Orc"),
            new UnitAnimEntry(
                CombatPrefabPaths.Merida,
                "Assets/Third Party/AssetsFotTestTask/Models/Merida/Merida.glb",
                $"{AnimationsFolder}/Merida.controller",
                $"{AnimationsFolder}/MeridaAvatar.asset",
                true)
        };

        [MenuItem("Magic Archer/Setup Unit Animators")]
        public static void SetupUnitAnimators()
        {
            if (!EditorUtility.DisplayDialog(
                    "Setup Unit Animators",
                    "Настроит Animator только на префабах без контроллера.\n" +
                    "Существующие Animator Controller НЕ пересоздаются.\n" +
                    "Walk.anim и уже настроенные префабы не трогаются.",
                    "Продолжить",
                    "Отмена"))
                return;

            EnsureFolder(AnimationsFolder);

            foreach (var unit in Units)
                SetupUnit(unit);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Unit animators and avatars configured.");
        }

        static void SetupUnit(UnitAnimEntry unit)
        {
            if (!File.Exists(unit.PrefabPath))
            {
                Debug.LogWarning($"Prefab not found, skipped: {unit.PrefabPath}");
                return;
            }

            if (!File.Exists(unit.GlbPath))
            {
                Debug.LogWarning($"Model not found, skipped: {unit.GlbPath}");
                return;
            }

            var clips = LoadAnimationClips(unit.GlbPath, unit.ExternalAnimationsFolder);
            if (clips.Count == 0)
            {
                Debug.LogWarning(
                    $"No animation clips found in {unit.GlbPath}. " +
                    "Make sure the GLB is imported (glTFast) and contains animations.");
                return;
            }

            var controller = GetOrCreateController(unit.ControllerPath, clips);
            if (controller == null)
            {
                Debug.LogWarning($"Failed to create animator controller for {unit.PrefabPath}");
                return;
            }

            var prefabRoot = PrefabUtility.LoadPrefabContents(unit.PrefabPath);
            try
            {
                var animator = prefabRoot.GetComponentInChildren<Animator>(true);
                if (animator == null)
                {
                    Debug.LogWarning($"Animator not found on prefab: {unit.PrefabPath}");
                    return;
                }

                if (animator.runtimeAnimatorController != null && animator.avatar != null)
                {
                    Debug.Log($"Animator already configured, skipped: {unit.PrefabPath}");
                    return;
                }

                var avatar = GetOrCreateAvatarAsset(unit, animator.gameObject);
                if (avatar == null)
                {
                    Debug.LogWarning($"Failed to create avatar for {unit.PrefabPath}");
                    return;
                }

                animator.avatar = avatar;
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;

                if (unit.WireArcherShooter)
                {
                    var shooter = prefabRoot.GetComponent<ArcherShooter>();
                    if (shooter != null)
                        SetSerializedReference(shooter, "_animator", animator);
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, unit.PrefabPath);
                Debug.Log($"Animator configured: {unit.PrefabPath} (avatar: {unit.AvatarPath})");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        static Avatar GetOrCreateAvatarAsset(UnitAnimEntry unit, GameObject animatorObject)
        {
            var glbAvatar = LoadAvatar(unit.GlbPath);
            if (glbAvatar != null)
                return glbAvatar;

            var existing = AssetDatabase.LoadAssetAtPath<Avatar>(unit.AvatarPath);
            if (existing != null)
                AssetDatabase.DeleteAsset(unit.AvatarPath);

            var skeletonRootName = FindSkeletonRootName(animatorObject);
            if (string.IsNullOrEmpty(skeletonRootName))
            {
                Debug.LogWarning($"Skeleton root not found on {animatorObject.name}");
                return null;
            }

            var avatar = AvatarBuilder.BuildGenericAvatar(animatorObject, skeletonRootName);
            if (!avatar.isValid)
            {
                Debug.LogWarning(
                    $"Invalid avatar for {animatorObject.name} (skeleton root: {skeletonRootName}).");
                return null;
            }

            AssetDatabase.CreateAsset(avatar, unit.AvatarPath);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<Avatar>(unit.AvatarPath);
        }

        static string FindSkeletonRootName(GameObject animatorObject)
        {
            var armature = animatorObject.transform.Find("Armature");
            if (armature != null)
                return armature.name;

            var renderer = animatorObject.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (renderer == null)
                return null;

            if (renderer.rootBone != null)
            {
                var skeletonRoot = renderer.rootBone;
                while (skeletonRoot.parent != null && skeletonRoot.parent != animatorObject.transform)
                    skeletonRoot = skeletonRoot.parent;

                return skeletonRoot.name;
            }

            if (renderer.bones == null || renderer.bones.Length == 0)
                return null;

            var bone = renderer.bones[0];
            while (bone != null && bone.parent != null && bone.parent != animatorObject.transform)
                bone = bone.parent;

            return bone != null ? bone.name : null;
        }

        static Dictionary<string, AnimationClip> LoadAnimationClips(
            string glbPath,
            string externalAnimationsFolder = null)
        {
            var clips = new Dictionary<string, AnimationClip>();
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(glbPath))
            {
                if (asset is not AnimationClip clip || clip.name.StartsWith("__"))
                    continue;

                clips[clip.name] = clip;
            }

            if (string.IsNullOrEmpty(externalAnimationsFolder))
                return clips;

            var folderGuids = AssetDatabase.FindAssets("t:AnimationClip", new[] { externalAnimationsFolder });
            foreach (var guid in folderGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null || clip.name.StartsWith("__"))
                    continue;

                clips[clip.name] = clip;
            }

            return clips;
        }

        static Avatar LoadAvatar(string glbPath)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(glbPath))
            {
                if (asset is Avatar avatar)
                    return avatar;
            }

            return null;
        }

        static AnimatorController GetOrCreateController(
            string controllerPath,
            Dictionary<string, AnimationClip> clips)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (existing != null)
            {
                Debug.Log($"Animator controller already exists, kept as-is: {controllerPath}");
                return existing;
            }

            return CreateController(controllerPath, clips);
        }

        static AnimatorController CreateController(
            string controllerPath,
            Dictionary<string, AnimationClip> clips)
        {
            EnsureFolder(Path.GetDirectoryName(controllerPath)?.Replace('\\', '/'));

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            var stateMachine = controller.layers[0].stateMachine;
            var idleClip = FindClip(clips, "Idle") ?? FindClip(clips, "Walk");
            var attackClip = FindClip(clips, "Attack");
            var deathClip = FindClip(clips, "Death");
            var walkClip = FindClip(clips, "Walk");
            var victoryClip = FindClip(clips, "Victory");

            if (victoryClip != null)
                controller.AddParameter("Victory", AnimatorControllerParameterType.Trigger);

            if (idleClip == null)
            {
                Debug.LogWarning($"Idle/Walk clip missing for {controllerPath}");
                return null;
            }

            var idleState = stateMachine.AddState(walkClip != null ? "Walk" : "Idle");
            idleState.motion = walkClip ?? idleClip;
            stateMachine.defaultState = idleState;

            if (walkClip != null && walkClip != idleClip)
            {
                var legacyIdleState = stateMachine.AddState("Idle");
                legacyIdleState.motion = idleClip;
            }

            if (attackClip != null)
            {
                var attackState = stateMachine.AddState("Attack");
                attackState.motion = attackClip;

                var toAttack = idleState.AddTransition(attackState);
                toAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
                toAttack.hasExitTime = false;
                toAttack.duration = 0.05f;

                var toIdle = attackState.AddTransition(idleState);
                toIdle.hasExitTime = true;
                toIdle.exitTime = 0.92f;
                toIdle.duration = 0.1f;
            }

            if (deathClip != null)
            {
                var deathState = stateMachine.AddState("Death");
                deathState.motion = deathClip;

                var toDeath = stateMachine.AddAnyStateTransition(deathState);
                toDeath.AddCondition(AnimatorConditionMode.If, 0f, "Death");
                toDeath.hasExitTime = false;
                toDeath.duration = 0.05f;
                toDeath.canTransitionToSelf = false;
            }

            if (victoryClip != null)
            {
                var victoryState = stateMachine.AddState("Victory");
                victoryState.motion = victoryClip;

                var toVictory = stateMachine.AddAnyStateTransition(victoryState);
                toVictory.AddCondition(AnimatorConditionMode.If, 0f, "Victory");
                toVictory.hasExitTime = false;
                toVictory.duration = 0.1f;
                toVictory.canTransitionToSelf = false;
            }

            EditorUtility.SetDirty(controller);
            return controller;
        }

        static AnimationClip FindClip(Dictionary<string, AnimationClip> clips, string clipName)
        {
            if (clips.TryGetValue(clipName, out var exact))
                return exact;

            foreach (var pair in clips)
            {
                if (pair.Key.Equals(clipName, System.StringComparison.OrdinalIgnoreCase))
                    return pair.Value;
            }

            return null;
        }

        static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            var parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            var name = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, name);
        }

        static void SetSerializedReference(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
                return;

            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        readonly struct UnitAnimEntry
        {
            public readonly string PrefabPath;
            public readonly string GlbPath;
            public readonly string ControllerPath;
            public readonly string AvatarPath;
            public readonly bool WireArcherShooter;
            public readonly string ExternalAnimationsFolder;

            public UnitAnimEntry(
                string prefabPath,
                string glbPath,
                string controllerPath,
                string avatarPath,
                bool wireArcherShooter,
                string externalAnimationsFolder = null)
            {
                PrefabPath = prefabPath;
                GlbPath = glbPath;
                ControllerPath = controllerPath;
                AvatarPath = avatarPath;
                WireArcherShooter = wireArcherShooter;
                ExternalAnimationsFolder = externalAnimationsFolder;
            }
        }
    }
}
