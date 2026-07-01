# Magic Archer — Test Task

Unity 6000.0.70f1 playable prototype based on the Azur Games test assignment (Magic Archer / Merge Heroes core).

## Requirements

- Unity **6000.0.70f1**
- Target platform: **WebGL** (scene runs in Editor as well)

## Quick Start

1. Open project in Unity 6000.0.70f1
2. Open scene `Assets/Scenes/SampleScene.unity`
3. Run editor menus in order:
   - **Magic Archer → Setup Level Prefab**
   - **Magic Archer → Create Combat Prefabs**
   - **Magic Archer → Setup Combat Scene**
4. Ensure scene has:
   - `SceneContext` + `GameInstaller` in Mono Installers
   - `GameBootstrap` on scene
   - `Level` prefab instance
5. **Tools → Demigiant → Setup DOTween** (first time only)
6. Press **Play**

## Gameplay Flow

1. Intro combat — archer kills first orc (+10 coins)
2. Tutorial purchase — buy second archer for 10 coins
3. Post-purchase combat — 2 more orcs
4. Tutorial merge — drag one archer onto another
5. Main loop — buy archers (15, +5 per buy), fight orcs + boss
6. **Victory** — boss killed → tap triggers `CTA` in console
7. **Defeat** — enemy reaches the grid → units die → tap triggers `CTA`

## Architecture

- **Zenject (Extenject)** — DI, embedded in `Packages/com.svermeulen.extenject`
- **State machine** — `IntroCombat → TutorialPurchase → PostPurchaseCombat → TutorialMerge → MainLoop → Victory/Defeat`
- **Composition** — separate components for units, enemies, grid, economy, combat, UI
- **Coroutines + DOTween** — delays and VFX (no UniTask)

## Project Structure

```
Assets/
  _Source/           # Game code, prefabs, materials, scenes
  Third Party/       # Provided test assets (models, sprites, sounds)
  Plugins/Demigiant/ # DOTween
Packages/
  com.svermeulen.extenject/
```

## Notes

- CTA is a stub: `Debug.Log("CTA")` on click after win/lose
- Character models may fall back to placeholders if GLB import fails — run **Create Combat Prefabs** again after fixing imports
- Hand tutorial uses `Assets/Third Party/AssetsFotTestTask/Sprites/Hand.png`

## License / Assets

Third-party assets are provided in `Assets/Third Party/AssetsFotTestTask/` for the test task only.
