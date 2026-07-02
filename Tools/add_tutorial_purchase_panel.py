from pathlib import Path

PREFAB = Path(
    r"A:\Unity\Azur\Azur-Test-Magic_Archer\Assets\_Source\Prefabs\UI\Canvas UI.prefab"
)

CANVAS_RECT = "4399128645009501436"
COMBAT_UI_REFS = "1253967950416773989"

SPRITE_SQUARE = "c50155422daa77d41bdb241e7be3d66b"
SPRITE_BACK = "6d2985e989a11894e9e6f58358da0eb7"
SPRITE_LIGHT_CARD = "4c1435d997feb2e44a068eacff6b7864"
SPRITE_PANEL_ARCHER = "520e9178f40cc5947998aa806753e4a9"
SPRITE_BUTTON = "a6759c4712ad5754a8aabadee46f4b40"
SPRITE_COIN = "965d4be75fa8cea4da80bd66e11c6bec"
GRAYSCALE_MAT = "df93ca558ec690f448f1678b9d2ea2e5"
SCRIPT_PANEL = "c8e4f1a23b5d64789a0c1e2f3d4b5a6c"
SCRIPT_IMAGE = "fe87c0e1cc204ed48ad3b37840f39efc"
SCRIPT_BUTTON = "4e29b1a8efbd4b44bb3f3716e73f07ff"
SCRIPT_BUY = "15d819c08859c1442ab32b423d83ab11"
SCRIPT_TEXT = "5f7201a12d95ffc409449d95f23cf332"

PANEL_ROOT = "9011111111111111101"
PANEL_RECT = "9011111111111111102"
PANEL_VIEW = "9011111111111111103"
DIMMER_GO = "9011111111111111104"
DIMMER_RECT = "9011111111111111105"
DIMMER_CR = "9011111111111111106"
DIMMER_IMG = "9011111111111111107"
CONTENT_GO = "9011111111111111108"
CONTENT_RECT = "9011111111111111109"
BG_IMG = "9011111111111111110"
HERO_A_GO = "9011111111111111111"
HERO_A_RECT = "9011111111111111112"
HERO_A_CR = "9011111111111111113"
HERO_A_IMG = "9011111111111111114"
HERO_B_GO = "9011111111111111115"
HERO_B_RECT = "9011111111111111116"
HERO_B_CR = "9011111111111111117"
HERO_B_IMG = "9011111111111111118"
BUY_GO = "9011111111111111119"
BUY_RECT = "9011111111111111120"
BUY_CR = "9011111111111111121"
BUY_BG = "9011111111111111122"
BUY_BTN = "9011111111111111123"
BUY_VIEW = "9011111111111111124"
BUY_ICON_GO = "9011111111111111125"
BUY_ICON_RECT = "9011111111111111126"
BUY_ICON_CR = "9011111111111111127"
BUY_ICON_IMG = "9011111111111111128"
BUY_COST_GO = "9011111111111111129"
BUY_COST_RECT = "9011111111111111130"
BUY_COST_CR = "9011111111111111131"
BUY_COST_TEXT = "9011111111111111132"


def image_block(file_id, go_id, rect_id, cr_id, img_id, father, name, sprite, size, pos, material=None):
    mat_line = f"  m_Material: {{fileID: 2100000, guid: {material}, type: 2}}\n" if material else "  m_Material: {fileID: 0}\n"
    return f"""--- !u!1 &{go_id}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {rect_id}}}
  - component: {{fileID: {cr_id}}}
  - component: {{fileID: {img_id}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{rect_id}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {father}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: {pos[0]}, y: {pos[1]}}}
  m_SizeDelta: {{x: {size[0]}, y: {size[1]}}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{cr_id}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_CullTransparentMesh: 1
--- !u!114 &{img_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {SCRIPT_IMAGE}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
{mat_line}  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: 21300000, guid: {sprite}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 1
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
"""


def main():
    text = PREFAB.read_text(encoding="utf-8")
    if PANEL_VIEW in text:
        print("TutorialPurchasePanel already present")
        return

    canvas_children_marker = f"  m_Children:\n  - {{fileID: 9080563170440248517}}"
    if canvas_children_marker not in text:
        raise SystemExit("Canvas children marker not found")

    text = text.replace(
        canvas_children_marker,
        canvas_children_marker + f"\n  - {{fileID: {PANEL_RECT}}}",
        1,
    )

    combat_refs_marker = "  _healthBarPrefab:"
    if combat_refs_marker not in text:
        raise SystemExit("CombatUiRefs marker not found")

    text = text.replace(
        combat_refs_marker,
        f"  _tutorialPurchasePanel: {{fileID: {PANEL_VIEW}}}\n  _healthBarPrefab:",
        1,
    )

    block = f"""--- !u!1 &{PANEL_ROOT}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {PANEL_RECT}}}
  - component: {{fileID: {PANEL_VIEW}}}
  m_Layer: 0
  m_Name: TutorialPurchasePanel
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 0
--- !u!224 &{PANEL_RECT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {PANEL_ROOT}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {DIMMER_RECT}}}
  - {{fileID: {CONTENT_RECT}}}
  m_Father: {{fileID: {CANVAS_RECT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!114 &{PANEL_VIEW}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {PANEL_ROOT}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {SCRIPT_PANEL}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  _dimmer: {{fileID: {DIMMER_IMG}}}
  _panelBackground: {{fileID: {BG_IMG}}}
  _heroImageA: {{fileID: {HERO_A_IMG}}}
  _heroImageB: {{fileID: {HERO_B_IMG}}}
  _buyButton: {{fileID: {BUY_VIEW}}}
--- !u!1 &{DIMMER_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {DIMMER_RECT}}}
  - component: {{fileID: {DIMMER_CR}}}
  - component: {{fileID: {DIMMER_IMG}}}
  m_Layer: 0
  m_Name: Dimmer
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{DIMMER_RECT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {DIMMER_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {PANEL_RECT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{DIMMER_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {DIMMER_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{DIMMER_IMG}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {DIMMER_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {SCRIPT_IMAGE}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0, g: 0, b: 0, a: 0.72}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: 21300000, guid: {SPRITE_SQUARE}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!1 &{CONTENT_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {CONTENT_RECT}}}
  m_Layer: 0
  m_Name: Content
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{CONTENT_RECT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {CONTENT_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: 9011111111111111133}}
  - {{fileID: {HERO_A_RECT}}}
  - {{fileID: {HERO_B_RECT}}}
  - {{fileID: {BUY_RECT}}}
  m_Father: {{fileID: {PANEL_RECT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 720, y: 820}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!1 &9011111111111111133
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 9011111111111111134}}
  - component: {{fileID: 9011111111111111135}}
  - component: {{fileID: {BG_IMG}}}
  m_Layer: 0
  m_Name: PanelBackground
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &9011111111111111134
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 9011111111111111133}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {CONTENT_RECT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: 0, y: 40}}
  m_SizeDelta: {{x: 680, y: 520}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &9011111111111111135
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 9011111111111111133}}
  m_CullTransparentMesh: 1
--- !u!114 &{BG_IMG}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 9011111111111111133}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {SCRIPT_IMAGE}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: 21300000, guid: {SPRITE_BACK}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
{image_block('', HERO_A_GO, HERO_A_RECT, HERO_A_CR, HERO_A_IMG, CONTENT_RECT, 'HeroImageA', SPRITE_PANEL_ARCHER, (220, 280), (-130, 70))}
{image_block('', HERO_B_GO, HERO_B_RECT, HERO_B_CR, HERO_B_IMG, CONTENT_RECT, 'HeroImageB', SPRITE_PANEL_ARCHER, (220, 280), (130, 70))}
--- !u!1 &{BUY_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {BUY_RECT}}}
  - component: {{fileID: {BUY_CR}}}
  - component: {{fileID: {BUY_BG}}}
  - component: {{fileID: {BUY_BTN}}}
  - component: {{fileID: {BUY_VIEW}}}
  m_Layer: 0
  m_Name: TutorialBuyButton
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{BUY_RECT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {BUY_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {BUY_ICON_RECT}}}
  - {{fileID: {BUY_COST_RECT}}}
  m_Father: {{fileID: {CONTENT_RECT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: 0, y: -250}}
  m_SizeDelta: {{x: 320, y: 120}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{BUY_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {BUY_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{BUY_BG}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {BUY_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {SCRIPT_IMAGE}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 2100000, guid: {GRAYSCALE_MAT}, type: 2}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: 21300000, guid: {SPRITE_BUTTON}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &{BUY_BTN}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {BUY_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {SCRIPT_BUTTON}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Navigation:
    m_Mode: 3
    m_WrapAround: 0
    m_SelectOnUp: {{fileID: 0}}
    m_SelectOnDown: {{fileID: 0}}
    m_SelectOnLeft: {{fileID: 0}}
    m_SelectOnRight: {{fileID: 0}}
  m_Transition: 0
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_PressedColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 1}}
    m_SelectedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_DisabledColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 0.5019608}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_SpriteState:
    m_HighlightedSprite: {{fileID: 0}}
    m_PressedSprite: {{fileID: 0}}
    m_SelectedSprite: {{fileID: 0}}
    m_DisabledSprite: {{fileID: 0}}
  m_AnimationTriggers:
    m_NormalTrigger: Normal
    m_HighlightedTrigger: Highlighted
    m_PressedTrigger: Pressed
    m_SelectedTrigger: Selected
    m_DisabledTrigger: Disabled
  m_Interactable: 1
  m_TargetGraphic: {{fileID: {BUY_BG}}}
  m_OnClick:
    m_PersistentCalls:
      m_Calls: []
--- !u!114 &{BUY_VIEW}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {BUY_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {SCRIPT_BUY}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  _button: {{fileID: {BUY_BTN}}}
  _background: {{fileID: {BUY_BG}}}
  _icon: {{fileID: {BUY_ICON_IMG}}}
  _costLabel: {{fileID: {BUY_COST_TEXT}}}
  _titleLabel: {{fileID: 0}}
  _enabledTextColor: {{r: 1, g: 1, b: 1, a: 1}}
  _disabledTextColor: {{r: 0.35, g: 0.35, b: 0.35, a: 1}}
  _enabledEffectAmount: 0
  _disabledEffectAmount: 1
  _enabledBrightness: 1
  _disabledBrightness: 0.75
{image_block('', BUY_ICON_GO, BUY_ICON_RECT, BUY_ICON_CR, BUY_ICON_IMG, BUY_RECT, 'Icon', SPRITE_COIN, (40, 40), (-34, 4), GRAYSCALE_MAT)}
--- !u!1 &{BUY_COST_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {BUY_COST_RECT}}}
  - component: {{fileID: {BUY_COST_CR}}}
  - component: {{fileID: {BUY_COST_TEXT}}}
  m_Layer: 0
  m_Name: Cost
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{BUY_COST_RECT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {BUY_COST_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {BUY_RECT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: 28, y: 0}}
  m_SizeDelta: {{x: 120, y: 60}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{BUY_COST_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {BUY_COST_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{BUY_COST_TEXT}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {BUY_COST_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {SCRIPT_TEXT}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_FontData:
    m_Font: {{fileID: 10102, guid: 0000000000000000e000000000000000, type: 0}}
    m_FontSize: 42
    m_FontStyle: 1
    m_BestFit: 0
    m_MinSize: 10
    m_MaxSize: 42
    m_Alignment: 4
    m_AlignByGeometry: 0
    m_RichText: 1
    m_HorizontalOverflow: 0
    m_VerticalOverflow: 0
    m_LineSpacing: 1
  m_Text: 10
"""

    text = block + text
    PREFAB.write_text(text, encoding="utf-8")
    print("TutorialPurchasePanel added")


if __name__ == "__main__":
    main()
