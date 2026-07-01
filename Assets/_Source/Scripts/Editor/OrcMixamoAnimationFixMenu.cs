using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MagicArcher.Editor
{
    public static class OrcMixamoAnimationFixMenu
    {
        const string OrcAnimationsFolder = "Assets/Third Party/AssetsFotTestTask/Models/Orc";
        const string OrcControllerPath = "Assets/_Source/Animations/Orc.controller";
        const string ReferenceClipPath = "Assets/Third Party/AssetsFotTestTask/Models/Orc/Attack.anim";
        const string WalkAnimPath = "Assets/Third Party/AssetsFotTestTask/Models/Orc/Walk.anim";
        const string WalkFbxPath = "Assets/Third Party/AssetsFotTestTask/Models/Orc/Walking.fbx";
        const string OrcPrefabPath = CombatPrefabPaths.Orc;
        const string HipsBonePath = "Armature/mixamorig:Hips";
        const string ArmaturePath = "Armature";
        const string SkeletonRootPrefix = "Armature/";
        const float ArmatureScale = 0.23f;

        static readonly string[] ClipNames = { "Walk", "Attack", "Death", "Idle" };

        static readonly string[] RotationProperties =
        {
            "localRotation.x",
            "localRotation.y",
            "localRotation.z",
            "localRotation.w"
        };

        static readonly string[] PositionProperties =
        {
            "localPosition.x",
            "localPosition.y",
            "localPosition.z"
        };

        static readonly string[] ScaleProperties =
        {
            "localScale.x",
            "localScale.y",
            "localScale.z"
        };

        [MenuItem("Magic Archer/Fix Orc Walk Animation")]
        public static void FixOrcWalkAnimation()
        {
            if (!EditorUtility.DisplayDialog(
                    "Fix Orc Walk Animation",
                    "Поправит только Walk.anim: поворот Armature, масштаб и высоту Hips.\n\n" +
                    "Attack/Death/Idle не изменяются.",
                    "Продолжить",
                    "Отмена"))
                return;

            FixOrcWalkAnimationInternal();
        }

        public static void FixOrcWalkAnimationInternal()
        {
            var walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(WalkAnimPath);
            if (walkClip == null)
            {
                Debug.LogError($"Walk clip not found: {WalkAnimPath}");
                return;
            }

            if (AlignWalkClipToAttack(walkClip))
            {
                EditorUtility.SetDirty(walkClip);
                AssetDatabase.SaveAssets();
                Debug.Log("Orc walk animation fixed.");
            }
            else
            {
                Debug.Log("Orc walk animation already aligned.");
            }
        }

        [MenuItem("Magic Archer/Fix Orc Mixamo Animations")]
        public static void FixOrcMixamoAnimations()
        {
            if (!EditorUtility.DisplayDialog(
                    "Fix Orc Mixamo Animations",
                    "Поправит Attack/Death клипы и перепривяжет Orc.controller.\n\n" +
                    "Walk.anim НЕ изменяется.",
                    "Продолжить",
                    "Отмена"))
                return;

            var fixedCount = 0;
            foreach (var clipName in ClipNames)
            {
                if (clipName is "Idle" or "Walk")
                    continue;

                var clipPath = $"{OrcAnimationsFolder}/{clipName}.anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null)
                {
                    Debug.LogWarning($"Clip not found: {clipPath}");
                    continue;
                }

                if (FixClip(clip))
                {
                    EditorUtility.SetDirty(clip);
                    fixedCount++;
                    Debug.Log($"Fixed animation clip: {clipPath}");
                }
            }

            WireOrcController();
            AssetDatabase.SaveAssets();
            Debug.Log($"Orc Mixamo animation fix complete. Updated {fixedCount} clip(s).");
        }

        static void EnsureWalkClipFromFbx()
        {
            var walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(WalkAnimPath);
            if (walkClip != null && HasHipsRotation(walkClip))
                return;

            var sourceClip = LoadAnimationClipFromModel(WalkFbxPath);
            if (sourceClip == null)
            {
                Debug.LogWarning($"Could not import walk animation from: {WalkFbxPath}");
                return;
            }

            var recreated = Object.Instantiate(sourceClip);
            recreated.name = "Walk";

            if (walkClip != null)
                AssetDatabase.DeleteAsset(WalkAnimPath);

            AssetDatabase.CreateAsset(recreated, WalkAnimPath);
            AssetDatabase.ImportAsset(WalkAnimPath);
            Debug.Log("Recreated Walk.anim from Walking.fbx");
        }

        static AnimationClip LoadAnimationClipFromModel(string modelPath)
        {
            return AssetDatabase
                .LoadAllAssetsAtPath(modelPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(clip => !clip.name.StartsWith("__preview__"));
        }

        static bool HasHipsRotation(AnimationClip clip)
        {
            foreach (var property in RotationProperties)
            {
                var binding = EditorCurveBinding.FloatCurve(HipsBonePath, typeof(Transform), property);
                if (AnimationUtility.GetEditorCurve(clip, binding) != null)
                    return true;
            }

            return false;
        }

        static bool FixClip(AnimationClip clip)
        {
            var changed = RemapPathsAndStripRootMotion(clip);

            if (clip.name == "Walk")
                changed |= AlignWalkClipToAttack(clip);

            return changed;
        }

        static bool RemapPathsAndStripRootMotion(AnimationClip clip)
        {
            var changed = false;
            var bindings = AnimationUtility.GetCurveBindings(clip);
            var objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);

            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (ShouldRemoveRootCurve(binding))
                {
                    AnimationUtility.SetEditorCurve(clip, binding, null);
                    changed = true;
                    continue;
                }

                var fixedPath = FixBonePath(binding.path);
                if (fixedPath == binding.path)
                    continue;

                AnimationUtility.SetEditorCurve(clip, binding, null);
                var fixedBinding = binding;
                fixedBinding.path = fixedPath;
                AnimationUtility.SetEditorCurve(clip, fixedBinding, curve);
                changed = true;
            }

            foreach (var binding in objectBindings)
            {
                var curve = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                if (ShouldRemoveRootCurve(binding))
                {
                    AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
                    changed = true;
                    continue;
                }

                var fixedPath = FixBonePath(binding.path);
                if (fixedPath == binding.path)
                    continue;

                AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
                var fixedBinding = binding;
                fixedBinding.path = fixedPath;
                AnimationUtility.SetObjectReferenceCurve(clip, fixedBinding, curve);
                changed = true;
            }

            return changed;
        }

        static readonly Quaternion WalkArmatureRotation = Quaternion.identity;

        static bool AlignWalkClipToAttack(AnimationClip walkClip)
        {
            var changed = false;
            changed |= FixWalkArmatureRoot(walkClip);
            changed |= AlignWalkHipsFromReference(walkClip);
            changed |= ConfigureWalkClipSettings(walkClip);
            changed |= ClearGenericRootTransformFlag(WalkAnimPath);
            return changed;
        }

        static bool AlignWalkHipsFromReference(AnimationClip walkClip)
        {
            var referenceClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ReferenceClipPath);
            if (referenceClip == null)
                return AlignWalkHipsHeight(walkClip);

            var changed = false;

            if (TryGetFirstRotation(referenceClip, HipsBonePath, out var referenceRotation) &&
                TryGetFirstRotation(walkClip, HipsBonePath, out var walkRestRotation))
            {
                changed |= RetargetBoneRotation(
                    walkClip,
                    HipsBonePath,
                    referenceRotation,
                    walkRestRotation);
            }

            if (TryGetFirstPosition(referenceClip, HipsBonePath, out var referencePosition) &&
                TryGetFirstPosition(walkClip, HipsBonePath, out var walkRestPosition))
            {
                changed |= RetargetBonePosition(
                    walkClip,
                    HipsBonePath,
                    referencePosition,
                    walkRestPosition);
            }

            return changed;
        }

        static bool ClearGenericRootTransformFlag(string clipPath)
        {
            if (!System.IO.File.Exists(clipPath))
                return false;

            var text = System.IO.File.ReadAllText(clipPath);
            if (!text.Contains("m_HasGenericRootTransform: 1"))
                return false;

            text = text.Replace("m_HasGenericRootTransform: 1", "m_HasGenericRootTransform: 0");
            System.IO.File.WriteAllText(clipPath, text);
            AssetDatabase.ImportAsset(clipPath, ImportAssetOptions.ForceUpdate);
            return true;
        }

        static bool FixWalkArmatureRoot(AnimationClip walkClip)
        {
            var referenceClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{OrcAnimationsFolder}/Idle.anim");
            var changed = false;
            changed |= RemoveTransformCurves(walkClip, ArmaturePath);

            if (referenceClip != null && TryGetFirstRotation(referenceClip, ArmaturePath, out var referenceRotation))
                changed |= EnsureConstantRotation(walkClip, ArmaturePath, referenceRotation);
            else
                changed |= EnsureConstantRotation(walkClip, ArmaturePath, WalkArmatureRotation);

            changed |= EnsureArmatureScaleCurve(walkClip);
            return changed;
        }

        static bool AlignWalkHipsHeight(AnimationClip walkClip)
        {
            if (!TryGetFirstPosition(walkClip, HipsBonePath, out var walkRestPosition))
            {
                Debug.LogWarning("Could not read Hips position from Walk.anim");
                return false;
            }

            var bindHipsPosition = GetBindPoseHipsPosition();
            var targetHipsPosition = Quaternion.Inverse(WalkArmatureRotation) * bindHipsPosition;
            var delta = targetHipsPosition - walkRestPosition;
            if (delta.sqrMagnitude <= 0.000001f)
                return false;

            return OffsetBonePosition(walkClip, HipsBonePath, delta);
        }

        static bool OffsetBonePosition(AnimationClip clip, string bonePath, Vector3 delta)
        {
            var curves = GetTransformCurves(clip, bonePath, PositionProperties);
            if (curves[0] == null || curves[0].length == 0)
                return false;

            for (var i = 0; i < curves[0].length; i++)
            {
                var keyX = curves[0].keys[i];
                var keyY = curves[1].keys[i];
                var keyZ = curves[2].keys[i];

                keyX.value += delta.x;
                keyY.value += delta.y;
                keyZ.value += delta.z;

                curves[0].MoveKey(i, keyX);
                curves[1].MoveKey(i, keyY);
                curves[2].MoveKey(i, keyZ);
            }

            SetTransformCurves(clip, bonePath, PositionProperties, curves);
            return true;
        }

        static Vector3 GetBindPoseHipsPosition()
        {
            var hips = FindOrcHipsTransform();
            return hips != null ? hips.localPosition : Vector3.zero;
        }

        static Transform FindOrcHipsTransform()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(OrcPrefabPath);
            if (prefab == null)
                return null;

            foreach (var transform in prefab.GetComponentsInChildren<Transform>(true))
            {
                if (transform.name == "mixamorig:Hips")
                    return transform;
            }

            return null;
        }

        static bool RemoveTransformCurves(AnimationClip clip, string bonePath)
        {
            var changed = false;
            foreach (var property in RotationProperties)
            {
                var binding = EditorCurveBinding.FloatCurve(bonePath, typeof(Transform), property);
                if (AnimationUtility.GetEditorCurve(clip, binding) == null)
                    continue;

                AnimationUtility.SetEditorCurve(clip, binding, null);
                changed = true;
            }

            foreach (var property in PositionProperties)
            {
                var binding = EditorCurveBinding.FloatCurve(bonePath, typeof(Transform), property);
                if (AnimationUtility.GetEditorCurve(clip, binding) == null)
                    continue;

                AnimationUtility.SetEditorCurve(clip, binding, null);
                changed = true;
            }

            foreach (var property in ScaleProperties)
            {
                var binding = EditorCurveBinding.FloatCurve(bonePath, typeof(Transform), property);
                if (AnimationUtility.GetEditorCurve(clip, binding) == null)
                    continue;

                AnimationUtility.SetEditorCurve(clip, binding, null);
                changed = true;
            }

            return changed;
        }

        static bool EnsureConstantRotation(AnimationClip clip, string bonePath, Quaternion rotation)
        {
            var times = GetClipKeyframeTimes(clip);
            if (times.Count == 0)
                times = new List<float> { 0f, clip.length > 0f ? clip.length : 1f };

            var curves = new AnimationCurve[RotationProperties.Length];
            for (var i = 0; i < curves.Length; i++)
                curves[i] = new AnimationCurve();

            var components = new[] { rotation.x, rotation.y, rotation.z, rotation.w };
            for (var i = 0; i < RotationProperties.Length; i++)
            {
                foreach (var time in times)
                    curves[i].AddKey(time, components[i]);
            }

            SetTransformCurves(clip, bonePath, RotationProperties, curves);
            return true;
        }

        static bool EnsureArmatureScaleCurve(AnimationClip walkClip)
        {
            var times = GetClipKeyframeTimes(walkClip);
            if (times.Count == 0)
                return false;

            var scale = new Vector3(ArmatureScale, ArmatureScale, ArmatureScale);
            return EnsureConstantTransformCurves(walkClip, ArmaturePath, scale, ScaleProperties, times);
        }

        static bool CopyConstantTransformCurves(
            AnimationClip sourceClip,
            AnimationClip targetClip,
            string bonePath,
            string[] properties)
        {
            if (!TryGetFirstVector(sourceClip, bonePath, properties, out var value))
                return false;

            return EnsureConstantTransformCurves(targetClip, bonePath, value, properties);
        }

        static bool EnsureConstantTransformCurves(
            AnimationClip clip,
            string bonePath,
            Vector3 value,
            string[] properties,
            List<float> times = null)
        {
            times ??= GetClipKeyframeTimes(clip);
            if (times.Count == 0)
                return false;

            var curves = new AnimationCurve[properties.Length];
            for (var i = 0; i < curves.Length; i++)
                curves[i] = new AnimationCurve();

            var components = new[] { value.x, value.y, value.z };
            for (var i = 0; i < properties.Length; i++)
            {
                foreach (var time in times)
                    curves[i].AddKey(time, components[i]);
            }

            SetTransformCurves(clip, bonePath, properties, curves);
            return true;
        }

        static List<float> GetClipKeyframeTimes(AnimationClip clip)
        {
            var times = new HashSet<float>();
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve == null)
                    continue;

                foreach (var key in curve.keys)
                    times.Add(key.time);
            }

            if (times.Count == 0)
            {
                times.Add(1f / 60f);
                times.Add(clip.length);
            }

            return times.OrderBy(time => time).ToList();
        }

        static bool TryGetFirstVector(
            AnimationClip clip,
            string bonePath,
            string[] properties,
            out Vector3 value)
        {
            value = Vector3.zero;
            var curves = GetTransformCurves(clip, bonePath, properties);
            if (curves[0] == null || curves[0].length == 0)
                return false;

            value = new Vector3(
                curves[0].keys[0].value,
                curves[1].keys[0].value,
                curves[2].keys[0].value);
            return true;
        }

        static bool ConfigureWalkClipSettings(AnimationClip walkClip)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(walkClip);
            var changed = false;

            if (!settings.loopTime)
            {
                settings.loopTime = true;
                changed = true;
            }

            if (settings.keepOriginalPositionY)
            {
                settings.keepOriginalPositionY = false;
                changed = true;
            }

            if (changed)
                AnimationUtility.SetAnimationClipSettings(walkClip, settings);

            return changed;
        }

        static bool RetargetBonePosition(
            AnimationClip clip,
            string bonePath,
            Vector3 referencePosition,
            Vector3 sourceRestPosition)
        {
            var curves = GetTransformCurves(clip, bonePath, PositionProperties);
            if (curves[0] == null || curves[0].length == 0)
                return false;

            var delta = referencePosition - sourceRestPosition;
            for (var i = 0; i < curves[0].length; i++)
            {
                var keyX = curves[0].keys[i];
                var keyY = curves[1].keys[i];
                var keyZ = curves[2].keys[i];

                keyX.value += delta.x;
                keyY.value += delta.y;
                keyZ.value += delta.z;

                curves[0].MoveKey(i, keyX);
                curves[1].MoveKey(i, keyY);
                curves[2].MoveKey(i, keyZ);
            }

            SetTransformCurves(clip, bonePath, PositionProperties, curves);
            return true;
        }

        static bool RetargetBoneRotation(
            AnimationClip clip,
            string bonePath,
            Quaternion referenceRotation,
            Quaternion sourceRestRotation)
        {
            var curves = GetTransformCurves(clip, bonePath, RotationProperties);
            if (curves[0] == null || curves[0].length == 0)
                return false;

            var inverseSourceRest = Quaternion.Inverse(NormalizeQuaternion(sourceRestRotation));
            for (var i = 0; i < curves[0].length; i++)
            {
                var time = curves[0].keys[i].time;
                var sourceRotation = ReadQuaternion(curves, time);
                var delta = NormalizeQuaternion(inverseSourceRest * sourceRotation);
                var fixedRotation = NormalizeQuaternion(referenceRotation * delta);
                WriteQuaternion(curves, i, fixedRotation);
            }

            SetTransformCurves(clip, bonePath, RotationProperties, curves);
            return true;
        }

        static bool TryGetFirstRotation(AnimationClip clip, string bonePath, out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            var curves = GetTransformCurves(clip, bonePath, RotationProperties);
            if (curves[0] == null || curves[0].length == 0)
                return false;

            rotation = ReadQuaternion(curves, curves[0].keys[0].time);
            return true;
        }

        static bool TryGetFirstPosition(AnimationClip clip, string bonePath, out Vector3 position)
        {
            position = Vector3.zero;
            var curves = GetTransformCurves(clip, bonePath, PositionProperties);
            if (curves[0] == null || curves[0].length == 0)
                return false;

            position = new Vector3(
                curves[0].keys[0].value,
                curves[1].keys[0].value,
                curves[2].keys[0].value);
            return true;
        }

        static AnimationCurve[] GetTransformCurves(AnimationClip clip, string bonePath, string[] properties)
        {
            var curves = new AnimationCurve[properties.Length];
            for (var i = 0; i < properties.Length; i++)
            {
                var binding = EditorCurveBinding.FloatCurve(bonePath, typeof(Transform), properties[i]);
                curves[i] = AnimationUtility.GetEditorCurve(clip, binding);
            }

            return curves;
        }

        static void SetTransformCurves(
            AnimationClip clip,
            string bonePath,
            string[] properties,
            AnimationCurve[] curves)
        {
            for (var i = 0; i < properties.Length; i++)
            {
                var binding = EditorCurveBinding.FloatCurve(bonePath, typeof(Transform), properties[i]);
                AnimationUtility.SetEditorCurve(clip, binding, curves[i]);
            }
        }

        static Quaternion ReadQuaternion(AnimationCurve[] curves, float time)
        {
            return NormalizeQuaternion(new Quaternion(
                curves[0].Evaluate(time),
                curves[1].Evaluate(time),
                curves[2].Evaluate(time),
                curves[3].Evaluate(time)));
        }

        static void WriteQuaternion(AnimationCurve[] curves, int keyIndex, Quaternion rotation)
        {
            var keyX = curves[0].keys[keyIndex];
            var keyY = curves[1].keys[keyIndex];
            var keyZ = curves[2].keys[keyIndex];
            var keyW = curves[3].keys[keyIndex];

            keyX.value = rotation.x;
            keyY.value = rotation.y;
            keyZ.value = rotation.z;
            keyW.value = rotation.w;

            curves[0].MoveKey(keyIndex, keyX);
            curves[1].MoveKey(keyIndex, keyY);
            curves[2].MoveKey(keyIndex, keyZ);
            curves[3].MoveKey(keyIndex, keyW);
        }

        static Quaternion NormalizeQuaternion(Quaternion rotation)
        {
            var magnitude = Mathf.Sqrt(
                rotation.x * rotation.x +
                rotation.y * rotation.y +
                rotation.z * rotation.z +
                rotation.w * rotation.w);

            if (magnitude <= Mathf.Epsilon)
                return Quaternion.identity;

            rotation.x /= magnitude;
            rotation.y /= magnitude;
            rotation.z /= magnitude;
            rotation.w /= magnitude;
            return rotation;
        }

        static bool ShouldRemoveRootCurve(EditorCurveBinding binding)
        {
            return string.IsNullOrEmpty(binding.path);
        }

        static string FixBonePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (path.StartsWith(SkeletonRootPrefix))
                return path;

            if (path == "Armature")
                return path;

            if (path.StartsWith("mixamorig:"))
                return SkeletonRootPrefix + path;

            return path;
        }

        static void WireOrcController()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(OrcControllerPath);
            if (controller == null)
            {
                Debug.LogWarning($"Animator controller not found: {OrcControllerPath}");
                return;
            }

            var clips = new Dictionary<string, AnimationClip>();
            foreach (var clipName in ClipNames)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{OrcAnimationsFolder}/{clipName}.anim");
                if (clip != null)
                    clips[clipName] = clip;
            }

            foreach (var layer in controller.layers)
                AssignClips(layer.stateMachine, clips);

            EditorUtility.SetDirty(controller);
        }

        static void AssignClips(AnimatorStateMachine stateMachine, Dictionary<string, AnimationClip> clips)
        {
            foreach (var child in stateMachine.states)
            {
                if (clips.TryGetValue(child.state.name, out var clip))
                    child.state.motion = clip;
            }

            foreach (var childMachine in stateMachine.stateMachines)
                AssignClips(childMachine.stateMachine, clips);
        }

        [MenuItem("Magic Archer/Fix Orc Mixamo Animations", true)]
        static bool FixOrcMixamoAnimationsValidate()
        {
            return !Application.isPlaying;
        }

        [MenuItem("Magic Archer/Fix Orc Walk Animation", true)]
        static bool FixOrcWalkAnimationValidate()
        {
            return !Application.isPlaying;
        }
    }
}
