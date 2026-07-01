using MagicArcher.Core.Config;
using MagicArcher.Core.Audio;
using MagicArcher.Gameplay.Level;
using MagicArcher.Gameplay.Combat;
using MagicArcher.Gameplay.Economy;
using MagicArcher.Gameplay.Flow;
using MagicArcher.Gameplay.Units;
using MagicArcher.Installers;
using MagicArcher.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MagicArcher.Editor
{
    public static class SetupCombatSceneMenu
    {
        const string ArcherPrefabPath = CombatPrefabPaths.Archer;
        const string OrcPrefabPath = CombatPrefabPaths.Orc;
        const string ProjectilePrefabPath = "Assets/_Source/Prefabs/Projectiles/Projectile.prefab";
        const string HealthBarPrefabPath = "Assets/_Source/Prefabs/UI/HealthBarUi.prefab";
        const string CoinFlyPrefabPath = "Assets/_Source/Prefabs/UI/CoinFlyVfx.prefab";
        const string CoinSpritePath = "Assets/Third Party/AssetsFotTestTask/Sprites/sprite_currency_gold.png";
        const string ButtonSpritePath = "Assets/Third Party/AssetsFotTestTask/Sprites/Button.png";
        const string HandSpritePath = "Assets/Third Party/AssetsFotTestTask/Sprites/Hand.png";
        const string DimmerSpritePath = "Assets/Third Party/AssetsFotTestTask/Sprites/square.png";
        const string SoundsRoot = "Assets/Third Party/AssetsFotTestTask/Sounds";

        [MenuItem("Magic Archer/Create Health Bar Prefab")]
        public static void CreateHealthBarPrefabMenu()
        {
            var prefab = EnsureHealthBarPrefab(forceRecreate: false);
            if (prefab != null)
            {
                EnsureHealthBarUiSettings(prefab);
                Debug.Log($"Health bar prefab ready at {HealthBarPrefabPath}");
            }
        }

        [MenuItem("Magic Archer/Wire UI Sprites")]
        public static void WireUiSprites()
        {
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Wire UI Sprites: Canvas not found.");
                return;
            }

            var canvasRect = canvas.GetComponent<RectTransform>();
            var coinTarget = canvasRect.Find("CoinHudTarget") as RectTransform;
            if (coinTarget != null)
                SetupCoinTarget(coinTarget);

            var coinFlyPool = canvasRect.Find("CoinFlyPool") as RectTransform;
            if (coinFlyPool == null)
                coinFlyPool = SetupCoinFlyPool(canvasRect);

            var combatRefs = Object.FindFirstObjectByType<CombatSceneRefs>();
            if (combatRefs != null)
            {
                var coinFlyPrefab = AssetDatabase.LoadAssetAtPath<CoinFlyVfx>(CoinFlyPrefabPath);
                var refsSo = new SerializedObject(combatRefs);
                refsSo.FindProperty("_coinFlyPrefab").objectReferenceValue = coinFlyPrefab;
                refsSo.FindProperty("_coinFlyPoolRoot").objectReferenceValue = coinFlyPool;
                refsSo.ApplyModifiedPropertiesWithoutUndo();
            }

            var buyButton = canvasRect.Find("BuyUnitButton") as RectTransform;
            if (buyButton != null)
            {
                var background = GetOrAdd<Image>(buyButton.gameObject);
                background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath);
                background.type = Image.Type.Sliced;
                background.color = Color.white;
                var button = GetOrAdd<Button>(buyButton.gameObject);
                button.targetGraphic = background;
            }

            var overlayRect = canvasRect.Find("TutorialOverlay") as RectTransform;
            if (overlayRect != null)
            {
                RemoveNestedUiChild(overlayRect, "BuyUnitButton");
                SetupTutorialOverlay(overlayRect);
            }

            var endgameRect = canvasRect.Find("EndgameOverlay") as RectTransform;
            if (endgameRect != null)
                SetupEndgameOverlay(endgameRect);

            var buyRect = canvasRect.Find("BuyUnitButton") as RectTransform;
            var overlayView = canvas.GetComponentInChildren<TutorialOverlayView>(true);
            EnsureOrientationLayout(canvas.gameObject, coinTarget, buyRect, overlayView);
            EnsureGameplayCameraLayout();
            EnsureConfigAssetsMenu.EnsureRegularEnemyConfig();
            EnsureConfigAssetsMenu.EnsureBossEnemyConfig();
            EnsureConfigAssetsMenu.EnsureRegularUnitConfig();
            EnsureConfigAssetsMenu.EnsureUpgradedUnitConfig();

            EditorUtility.SetDirty(canvas);
            Debug.Log("UI sprites wired. Save scene.");
        }

        [MenuItem("Magic Archer/Repair Combat Prefab References")]
        public static void RepairCombatPrefabReferences()
        {
            var combatSystems = GameObject.Find("CombatSystems");
            if (combatSystems == null)
            {
                Debug.LogError("CombatSystems object not found in scene.");
                return;
            }

            var refs = combatSystems.GetComponent<CombatSceneRefs>();
            if (refs == null)
            {
                Debug.LogError("CombatSceneRefs not found on CombatSystems.");
                return;
            }

            var projectilesRoot = combatSystems.transform.Find("Projectiles");
            var coinTarget = GameObject.Find("CoinHudTarget")?.GetComponent<RectTransform>();
            var coinFlyPool = GameObject.Find("CoinFlyPool")?.GetComponent<RectTransform>();
            var coinFlyPrefab = AssetDatabase.LoadAssetAtPath<CoinFlyVfx>(CoinFlyPrefabPath);

            AssetDatabase.ImportAsset(ArcherPrefabPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(OrcPrefabPath, ImportAssetOptions.ForceUpdate);

            if (coinTarget != null && projectilesRoot != null)
                AssignRefs(refs, projectilesRoot, coinTarget, coinFlyPool, coinFlyPrefab);
            else
            {
                var so = new SerializedObject(refs);
                so.FindProperty("_archerPrefab").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<UnitView>(ArcherPrefabPath);
                so.FindProperty("_orcPrefab").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<MagicArcher.Gameplay.Enemies.EnemyView>(OrcPrefabPath);
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EnsureConfigAssetsMenu.EnsureRegularEnemyConfig();
            EnsureConfigAssetsMenu.EnsureBossEnemyConfig();
            EnsureConfigAssetsMenu.EnsureRegularUnitConfig();
            EnsureConfigAssetsMenu.EnsureUpgradedUnitConfig();
            AssignEnemyConfigsToSceneInstaller();

            EditorUtility.SetDirty(combatSystems);
            Debug.Log("Combat prefab references repaired. Save scene.");
        }

        static void AssignEnemyConfigsToSceneInstaller()
        {
            var regular = AssetDatabase.LoadAssetAtPath<RegularEnemyConfig>(RegularEnemyConfig.DefaultAssetPath);
            var boss = AssetDatabase.LoadAssetAtPath<BossEnemyConfig>(BossEnemyConfig.DefaultAssetPath);
            var regularUnit = AssetDatabase.LoadAssetAtPath<RegularUnitConfig>(RegularUnitConfig.DefaultAssetPath);
            var upgradedUnit = AssetDatabase.LoadAssetAtPath<UpgradedUnitConfig>(UpgradedUnitConfig.DefaultAssetPath);
            if (regular == null || boss == null || regularUnit == null || upgradedUnit == null)
                return;

            var installer = Object.FindFirstObjectByType<GameInstaller>();
            if (installer == null)
                return;

            var so = new SerializedObject(installer);
            so.FindProperty("_regularEnemyConfig").objectReferenceValue = regular;
            so.FindProperty("_bossEnemyConfig").objectReferenceValue = boss;
            so.FindProperty("_regularUnitConfig").objectReferenceValue = regularUnit;
            so.FindProperty("_upgradedUnitConfig").objectReferenceValue = upgradedUnit;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(installer);
        }

        [MenuItem("Magic Archer/Setup Combat Scene")]
        public static void SetupScene()
        {
            var combatSystems = GetOrCreate("CombatSystems");
            var refs = GetOrAdd<CombatSceneRefs>(combatSystems);
            GetOrAdd<IntroCombatDirector>(combatSystems);
            GetOrAdd<EnemyWaveController>(combatSystems);
            GetOrAdd<CombatPhaseScheduler>(combatSystems);
            GetOrAdd<TutorialPurchaseDirector>(combatSystems);
            GetOrAdd<PostPurchaseCombatDirector>(combatSystems);
            GetOrAdd<TutorialMergeDirector>(combatSystems);
            GetOrAdd<MainLoopDirector>(combatSystems);
            GetOrAdd<UnitDragController>(combatSystems);
            GetOrAdd<MergeVfx>(combatSystems);
            SetupAudioHost(combatSystems);

            var projectilesRoot = GetOrCreateChild(combatSystems.transform, "Projectiles");

            var canvas = GetOrCreateCanvas();
            var canvasRect = canvas.GetComponent<RectTransform>();
            var coinTarget = GetOrCreateUiChild(canvasRect, "CoinHudTarget");
            SetupCoinTarget(coinTarget);

            var coinFlyPool = SetupCoinFlyPool(canvasRect);
            var coinFlyPrefab = AssetDatabase.LoadAssetAtPath<CoinFlyVfx>(CoinFlyPrefabPath);

            var hudRect = GetOrCreateUiChild(canvasRect, "CurrencyHud");
            var hud = GetOrAdd<CurrencyHud>(hudRect.gameObject);
            SetupCurrencyHud(hud, coinTarget);

            var buyButton = SetupBuyButton(canvasRect);
            var overlayRect = GetOrCreateUiChild(canvasRect, "TutorialOverlay");
            StretchFullScreen(overlayRect);
            RemoveNestedUiChild(overlayRect, "BuyUnitButton");
            var overlay = SetupTutorialOverlay(overlayRect);
            var endgameOverlay = SetupEndgameOverlay(canvasRect);
            var ctaFeedback = SetupCtaFeedback(canvasRect);
            var healthBarPrefab = EnsureHealthBarPrefab();
            EnsureHealthBarUiSettings(healthBarPrefab);
            EnsureConfigAssetsMenu.EnsureRegularEnemyConfig();
            EnsureConfigAssetsMenu.EnsureBossEnemyConfig();
            EnsureConfigAssetsMenu.EnsureRegularUnitConfig();
            EnsureConfigAssetsMenu.EnsureUpgradedUnitConfig();
            var healthBarsRoot = GetOrCreateUiChild(canvasRect, "HealthBars");
            StretchFullScreen(healthBarsRoot);
            SetupCombatUiRefs(canvas.gameObject, overlay, buyButton, endgameOverlay, ctaFeedback, healthBarsRoot, healthBarPrefab);
            EnsureOrientationLayout(canvas.gameObject, coinTarget, buyButton.transform as RectTransform, overlay);
            EnsureGameplayCameraLayout();

            EnsureEventSystem();

            AssignRefs(refs, projectilesRoot.transform, coinTarget, coinFlyPool, coinFlyPrefab);

            AssignEnemyConfigsToSceneInstaller();

            EditorUtility.SetDirty(combatSystems);
            EditorUtility.SetDirty(canvas);
            Debug.Log("Combat scene objects created. Save scene.");
        }

        static void AssignRefs(
            CombatSceneRefs refs,
            Transform projectilesRoot,
            RectTransform coinTarget,
            RectTransform coinFlyPool,
            CoinFlyVfx coinFlyPrefab)
        {
            var so = new SerializedObject(refs);
            so.FindProperty("_archerPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<UnitView>(ArcherPrefabPath);
            so.FindProperty("_orcPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<MagicArcher.Gameplay.Enemies.EnemyView>(OrcPrefabPath);
            so.FindProperty("_projectilePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ProjectileView>(ProjectilePrefabPath);
            so.FindProperty("_projectilesRoot").objectReferenceValue = projectilesRoot;
            so.FindProperty("_coinHudTarget").objectReferenceValue = coinTarget;
            so.FindProperty("_coinFlyPrefab").objectReferenceValue = coinFlyPrefab;
            so.FindProperty("_coinFlyPoolRoot").objectReferenceValue = coinFlyPool;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static RectTransform SetupCoinFlyPool(RectTransform canvasRect)
        {
            var poolRect = GetOrCreateUiChild(canvasRect, "CoinFlyPool");
            StretchFullScreen(poolRect);
            return poolRect;
        }

        static CtaFeedbackView SetupCtaFeedback(RectTransform canvasRect)
        {
            var rect = GetOrCreateUiChild(canvasRect, "CtaFeedback");
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -24f);
            rect.sizeDelta = new Vector2(240f, 88f);

            var background = GetOrAdd<Image>(rect.gameObject);
            background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath);
            background.type = Image.Type.Sliced;
            background.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);
            background.raycastTarget = false;

            var labelRect = GetOrCreateUiChild(rect, "Label");
            StretchFullScreen(labelRect);
            var label = GetOrAdd<Text>(labelRect.gameObject);
            label.text = "CTA BUTTON CLICKED";
            label.fontSize = 20;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var view = GetOrAdd<CtaFeedbackView>(rect.gameObject);
            var so = new SerializedObject(view);
            so.FindProperty("_panel").objectReferenceValue = rect;
            so.FindProperty("_label").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();

            rect.gameObject.SetActive(false);
            return view;
        }

        static BuyUnitButtonView SetupBuyButton(RectTransform canvasRect)
        {
            var buttonRect = GetOrCreateUiChild(canvasRect, "BuyUnitButton");
            buttonRect.anchorMin = new Vector2(1f, 0.5f);
            buttonRect.anchorMax = new Vector2(1f, 0.5f);
            buttonRect.pivot = new Vector2(1f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(-40f, 0f);
            buttonRect.sizeDelta = new Vector2(140f, 140f);

            var background = GetOrAdd<Image>(buttonRect.gameObject);
            background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath);
            background.type = Image.Type.Sliced;
            background.color = Color.white;

            var button = GetOrAdd<Button>(buttonRect.gameObject);
            button.targetGraphic = background;

            var titleRect = GetOrCreateUiChild(buttonRect, "Title");
            titleRect.anchorMin = new Vector2(0.5f, 0.65f);
            titleRect.anchorMax = new Vector2(0.5f, 0.65f);
            titleRect.sizeDelta = new Vector2(120f, 36f);
            var title = GetOrAdd<Text>(titleRect.gameObject);
            title.text = "HIRE";
            title.fontSize = 22;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = Color.white;
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var costRect = GetOrCreateUiChild(buttonRect, "Cost");
            costRect.anchorMin = new Vector2(0.5f, 0.3f);
            costRect.anchorMax = new Vector2(0.5f, 0.3f);
            costRect.sizeDelta = new Vector2(80f, 40f);
            var cost = GetOrAdd<Text>(costRect.gameObject);
            cost.text = "10";
            cost.fontSize = 30;
            cost.alignment = TextAnchor.MiddleCenter;
            cost.color = Color.white;
            cost.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var buyView = GetOrAdd<BuyUnitButtonView>(buttonRect.gameObject);
            var so = new SerializedObject(buyView);
            so.FindProperty("_button").objectReferenceValue = button;
            so.FindProperty("_costLabel").objectReferenceValue = cost;
            so.FindProperty("_titleLabel").objectReferenceValue = title;
            so.ApplyModifiedPropertiesWithoutUndo();

            buttonRect.gameObject.SetActive(false);
            return buyView;
        }

        static TutorialOverlayView SetupTutorialOverlay(RectTransform overlayRect)
        {
            StretchFullScreen(overlayRect);

            var dimmerRect = GetOrCreateUiChild(overlayRect, "Dimmer");
            StretchFullScreen(dimmerRect);
            var dimmer = GetOrAdd<Image>(dimmerRect.gameObject);
            dimmer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DimmerSpritePath);
            dimmer.color = new Color(0f, 0f, 0f, 0.55f);
            dimmer.raycastTarget = true;

            var handRect = GetOrCreateUiChild(overlayRect, "Hand");
            handRect.sizeDelta = new Vector2(96f, 96f);
            var hand = GetOrAdd<Image>(handRect.gameObject);
            hand.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HandSpritePath);
            hand.raycastTarget = false;
            if (hand.sprite != null)
                hand.SetNativeSize();

            var overlay = GetOrAdd<TutorialOverlayView>(overlayRect.gameObject);
            var so = new SerializedObject(overlay);
            so.FindProperty("_dimmer").objectReferenceValue = dimmer;
            so.FindProperty("_hand").objectReferenceValue = handRect;
            so.ApplyModifiedPropertiesWithoutUndo();

            dimmerRect.SetSiblingIndex(0);
            handRect.SetAsLastSibling();

            overlayRect.gameObject.SetActive(false);
            return overlay;
        }

        static void EnsureOrientationLayout(
            GameObject canvas,
            RectTransform coinHudTarget,
            RectTransform buyUnitButton,
            TutorialOverlayView tutorialOverlay)
        {
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0f;
            }

            var layout = GetOrAdd<OrientationLayoutController>(canvas);
            var so = new SerializedObject(layout);
            so.FindProperty("_canvasScaler").objectReferenceValue = scaler;
            so.FindProperty("_coinHudTarget").objectReferenceValue = coinHudTarget;
            so.FindProperty("_buyUnitButton").objectReferenceValue = buyUnitButton;
            so.FindProperty("_tutorialOverlay").objectReferenceValue = tutorialOverlay;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void EnsureGameplayCameraLayout()
        {
            var camera = Object.FindFirstObjectByType<Camera>();
            if (camera == null)
                return;

            GetOrAdd<GameplayCameraLayout>(camera.gameObject);
        }

        static void SetupCombatUiRefs(
            GameObject canvas,
            TutorialOverlayView overlay,
            BuyUnitButtonView buyButton,
            EndgameOverlayView endgameOverlay,
            CtaFeedbackView ctaFeedback,
            RectTransform healthBarsRoot,
            HealthBarUiView healthBarPrefab)
        {
            var refs = GetOrAdd<CombatUiRefs>(canvas);
            var so = new SerializedObject(refs);
            so.FindProperty("_tutorialOverlay").objectReferenceValue = overlay;
            so.FindProperty("_buyUnitButton").objectReferenceValue = buyButton;
            so.FindProperty("_endgameOverlay").objectReferenceValue = endgameOverlay;
            so.FindProperty("_ctaFeedback").objectReferenceValue = ctaFeedback;
            so.FindProperty("_healthBarsRoot").objectReferenceValue = healthBarsRoot;
            so.FindProperty("_healthBarPrefab").objectReferenceValue = healthBarPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        const string HealthBarSettingsPath = "Assets/_Source/Resources/HealthBarUiSettings.asset";

        static void EnsureHealthBarUiSettings(HealthBarUiView prefab)
        {
            var folder = "Assets/_Source/Resources";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Source"))
                    AssetDatabase.CreateFolder("Assets", "_Source");
                AssetDatabase.CreateFolder("Assets/_Source", "Resources");
            }

            var settings = AssetDatabase.LoadAssetAtPath<HealthBarUiSettings>(HealthBarSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<HealthBarUiSettings>();
                AssetDatabase.CreateAsset(settings, HealthBarSettingsPath);
            }

            var so = new SerializedObject(settings);
            so.FindProperty("_prefab").objectReferenceValue = prefab;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        static HealthBarUiView EnsureHealthBarPrefab(bool forceRecreate = false)
        {
            if (!forceRecreate)
            {
                var existing = AssetDatabase.LoadAssetAtPath<HealthBarUiView>(HealthBarPrefabPath);
                if (existing != null)
                    return existing;
            }

            var root = new GameObject("HealthBarUi", typeof(RectTransform));
            var rootRect = (RectTransform)root.transform;
            rootRect.sizeDelta = new Vector2(72f, 10f);

            var backRect = GetOrCreateUiChild(rootRect, "Back");
            StretchFullScreen(backRect);
            var back = GetOrAdd<Image>(backRect.gameObject);
            back.color = new Color(0.05f, 0.24f, 0.06f, 0.95f);
            back.raycastTarget = false;

            var fillRect = GetOrCreateUiChild(rootRect, "Fill");
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(1f, 1f);
            fillRect.offsetMax = new Vector2(-1f, -1f);
            var fill = GetOrAdd<Image>(fillRect.gameObject);
            fill.color = new Color(0.2f, 0.95f, 0.25f, 1f);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 1f;
            fill.raycastTarget = false;

            var view = root.AddComponent<HealthBarUiView>();
            var viewSo = new SerializedObject(view);
            viewSo.FindProperty("_back").objectReferenceValue = back;
            viewSo.FindProperty("_fill").objectReferenceValue = fill;
            viewSo.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, HealthBarPrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<HealthBarUiView>(HealthBarPrefabPath);
        }

        static void SetupAudioHost(GameObject combatSystems)
        {
            var host = GetOrAdd<AudioHost>(combatSystems);
            var so = new SerializedObject(host);
            so.FindProperty("_music").objectReferenceValue = LoadAudio("music_loop_01.mp3");
            so.FindProperty("_orcHit").objectReferenceValue = LoadAudio("orc_hit.mp3");
            so.FindProperty("_coins").objectReferenceValue = LoadAudio("coins.mp3");
            so.FindProperty("_merge").objectReferenceValue = LoadAudio("merge.mp3");
            so.FindProperty("_unitBuy").objectReferenceValue = LoadAudio("unit_buy.mp3");
            so.FindProperty("_victory").objectReferenceValue = LoadAudio("Victory.mp3");
            so.FindProperty("_defeat").objectReferenceValue = LoadAudio("Defeat.mp3");
            so.FindProperty("_bowReleases").arraySize = 3;
            so.FindProperty("_bowReleases").GetArrayElementAtIndex(0).objectReferenceValue = LoadAudio("bow_release_01.mp3");
            so.FindProperty("_bowReleases").GetArrayElementAtIndex(1).objectReferenceValue = LoadAudio("bow_release_02.mp3");
            so.FindProperty("_bowReleases").GetArrayElementAtIndex(2).objectReferenceValue = LoadAudio("bow_release_03.mp3");
            so.FindProperty("_orcDeaths").arraySize = 3;
            so.FindProperty("_orcDeaths").GetArrayElementAtIndex(0).objectReferenceValue = LoadAudio("orc_death_01.mp3");
            so.FindProperty("_orcDeaths").GetArrayElementAtIndex(1).objectReferenceValue = LoadAudio("orc_death_02.mp3");
            so.FindProperty("_orcDeaths").GetArrayElementAtIndex(2).objectReferenceValue = LoadAudio("orc_death_03.mp3");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static AudioClip LoadAudio(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>($"{SoundsRoot}/{fileName}");
        }

        static EndgameOverlayView SetupEndgameOverlay(RectTransform canvasRect)
        {
            var overlayRect = GetOrCreateUiChild(canvasRect, "EndgameOverlay");
            StretchFullScreen(overlayRect);

            var dimmerRect = GetOrCreateUiChild(overlayRect, "Dimmer");
            StretchFullScreen(dimmerRect);
            var dimmer = GetOrAdd<Image>(dimmerRect.gameObject);
            dimmer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DimmerSpritePath);
            dimmer.color = new Color(0f, 0f, 0f, 0.7f);
            dimmer.raycastTarget = false;

            var titleRect = GetOrCreateUiChild(overlayRect, "Title");
            titleRect.anchorMin = new Vector2(0.5f, 0.58f);
            titleRect.anchorMax = new Vector2(0.5f, 0.58f);
            titleRect.sizeDelta = new Vector2(640f, 96f);
            var title = GetOrAdd<Text>(titleRect.gameObject);
            title.text = "VICTORY";
            title.fontSize = 64;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = Color.white;
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var hintRect = GetOrCreateUiChild(overlayRect, "Hint");
            hintRect.anchorMin = new Vector2(0.5f, 0.42f);
            hintRect.anchorMax = new Vector2(0.5f, 0.42f);
            hintRect.sizeDelta = new Vector2(480f, 48f);
            var hint = GetOrAdd<Text>(hintRect.gameObject);
            hint.text = "Tap to continue";
            hint.fontSize = 28;
            hint.alignment = TextAnchor.MiddleCenter;
            hint.color = new Color(1f, 1f, 1f, 0.85f);
            hint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var overlay = GetOrAdd<EndgameOverlayView>(overlayRect.gameObject);
            var so = new SerializedObject(overlay);
            so.FindProperty("_root").objectReferenceValue = overlayRect.gameObject;
            so.FindProperty("_titleLabel").objectReferenceValue = title;
            so.FindProperty("_hintLabel").objectReferenceValue = hint;
            so.ApplyModifiedPropertiesWithoutUndo();

            overlayRect.gameObject.SetActive(false);
            return overlay;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
                return;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        static void StretchFullScreen(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void SetupCurrencyHud(CurrencyHud hud, RectTransform coinTarget)
        {
            var label = coinTarget.GetComponentInChildren<Text>();
            if (label == null)
                return;

            var so = new SerializedObject(hud);
            so.FindProperty("_coinsLabel").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetupCoinTarget(RectTransform coinTarget)
        {
            coinTarget.anchorMin = new Vector2(1f, 1f);
            coinTarget.anchorMax = new Vector2(1f, 1f);
            coinTarget.pivot = new Vector2(1f, 1f);
            coinTarget.anchoredPosition = new Vector2(-40f, -40f);
            coinTarget.sizeDelta = new Vector2(160f, 60f);

            var layout = GetOrAdd<HorizontalLayoutGroup>(coinTarget.gameObject);
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.spacing = 8f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            var iconRect = GetOrCreateUiChild(coinTarget, "Icon");
            var icon = GetOrAdd<Image>(iconRect.gameObject);
            icon.sprite = LoadOrCreateCoinSprite();
            if (icon.sprite != null)
                icon.SetNativeSize();

            var textRect = GetOrCreateUiChild(coinTarget, "Value");
            RemoveLegacyTmp(textRect.gameObject);
            var text = GetOrAdd<Text>(textRect.gameObject);
            text.text = "0";
            text.fontSize = 28;
            text.alignment = TextAnchor.MiddleRight;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        static void RemoveLegacyTmp(GameObject go)
        {
            var tmpType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            if (tmpType == null)
                return;

            var tmp = go.GetComponent(tmpType);
            if (tmp != null)
                Object.DestroyImmediate(tmp);
        }

        static void RemoveNestedUiChild(RectTransform parent, string name)
        {
            var child = parent.Find(name);
            if (child == null)
                return;

            Object.DestroyImmediate(child.gameObject);
        }

        static RectTransform GetOrCreateUiChild(RectTransform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                if (child is RectTransform existingRect)
                    return existingRect;

                Object.DestroyImmediate(child.gameObject);
            }

            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        static Sprite LoadOrCreateCoinSprite()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CoinSpritePath);
            if (sprite != null)
                return sprite;

            var importer = AssetImporter.GetAtPath(CoinSpritePath) as TextureImporter;
            if (importer == null)
                return null;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(CoinSpritePath);
        }

        static GameObject GetOrCreateCanvas()
        {
            var existing = Object.FindFirstObjectByType<Canvas>();
            if (existing != null)
                return existing.gameObject;

            var canvasGo = new GameObject("UI", typeof(RectTransform));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();
            return canvasGo;
        }

        static GameObject GetOrCreate(string name)
        {
            var existing = GameObject.Find(name);
            return existing != null ? existing : new GameObject(name);
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

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }
    }
}
