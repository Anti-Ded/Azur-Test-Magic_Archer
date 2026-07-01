from pathlib import Path

SCENE = Path(
    r"A:\Unity\Azur\Azur-Test-Magic_Archer\Assets\Scenes\SampleScene.unity"
)

ORIENTATION_GUID = "f3a8c2d14b5e4f29a7c6d8e91b2047fa"
CAMERA_LAYOUT_GUID = "8e4b1a9c7d2f4e6b9a3c5d7e1f2048ab"

ORIENTATION_ID = "2847193021"
CAMERA_LAYOUT_ID = "2847193022"


def main():
    text = SCENE.read_text(encoding="utf-8")

    if f"&{ORIENTATION_ID}" not in text:
        text = text.replace(
            "  m_Component:\n"
            "  - component: {fileID: 1940913862}\n"
            "  - component: {fileID: 1940913861}\n"
            "  - component: {fileID: 1940913860}\n"
            "  - component: {fileID: 1940913859}\n"
            "  - component: {fileID: 1940913863}\n"
            "  m_Layer: 0\n"
            "  m_Name: UI",
            "  m_Component:\n"
            "  - component: {fileID: 1940913862}\n"
            "  - component: {fileID: 1940913861}\n"
            "  - component: {fileID: 1940913860}\n"
            "  - component: {fileID: 1940913859}\n"
            "  - component: {fileID: 1940913863}\n"
            f"  - component: {{fileID: {ORIENTATION_ID}}}\n"
            "  m_Layer: 0\n"
            "  m_Name: UI",
            1,
        )

        orientation_block = f"""--- !u!114 &{ORIENTATION_ID}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1940913858}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {ORIENTATION_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  _canvasScaler: {{fileID: 1940913860}}
  _coinHudTarget: {{fileID: 1434881234}}
  _buyUnitButton: {{fileID: 721374657}}
  _tutorialOverlay: {{fileID: 840038601}}
  _coinHudLandscape:
    AnchorMin: {{x: 1, y: 1}}
    AnchorMax: {{x: 1, y: 1}}
    Pivot: {{x: 1, y: 1}}
    AnchoredPosition: {{x: -40, y: -40}}
    SizeDelta: {{x: 160, y: 60}}
  _coinHudPortrait:
    AnchorMin: {{x: 1, y: 1}}
    AnchorMax: {{x: 1, y: 1}}
    Pivot: {{x: 1, y: 1}}
    AnchoredPosition: {{x: -40, y: -40}}
    SizeDelta: {{x: 160, y: 60}}
  _buyButtonLandscape:
    AnchorMin: {{x: 1, y: 0.5}}
    AnchorMax: {{x: 1, y: 0.5}}
    Pivot: {{x: 1, y: 0.5}}
    AnchoredPosition: {{x: -40, y: 0}}
    SizeDelta: {{x: 140, y: 140}}
  _buyButtonPortrait:
    AnchorMin: {{x: 0.5, y: 0}}
    AnchorMax: {{x: 0.5, y: 0}}
    Pivot: {{x: 0.5, y: 0}}
    AnchoredPosition: {{x: 0, y: 48}}
    SizeDelta: {{x: 240, y: 88}}
"""
        insert_at = text.find("--- !u!1 &1943663487")
        text = text[:insert_at] + orientation_block + text[insert_at:]

    if f"&{CAMERA_LAYOUT_ID}" not in text:
        text = text.replace(
            "  m_Component:\n"
            "  - component: {fileID: 330585546}\n"
            "  - component: {fileID: 330585545}\n"
            "  - component: {fileID: 330585544}\n"
            "  - component: {fileID: 330585547}\n"
            "  m_Layer: 0\n"
            "  m_Name: Main Camera",
            "  m_Component:\n"
            "  - component: {fileID: 330585546}\n"
            "  - component: {fileID: 330585545}\n"
            "  - component: {fileID: 330585544}\n"
            "  - component: {fileID: 330585547}\n"
            f"  - component: {{fileID: {CAMERA_LAYOUT_ID}}}\n"
            "  m_Layer: 0\n"
            "  m_Name: Main Camera",
            1,
        )

        camera_block = f"""--- !u!114 &{CAMERA_LAYOUT_ID}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 330585543}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {CAMERA_LAYOUT_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  _camera: {{fileID: 330585545}}
  _landscapePosition: {{x: 0.58, y: 19.51, z: -18.1}}
  _landscapeEuler: {{x: 69.254, y: 0, z: 0}}
  _landscapeFov: 60
  _portraitPosition: {{x: 0.58, y: 21.5, z: -20.5}}
  _portraitEuler: {{x: 69.254, y: 0, z: 0}}
  _portraitFov: 64
"""
        insert_at = text.find("--- !u!1 &410087039")
        if insert_at < 0:
            insert_at = text.find("--- !u!1 &721374655")
        text = text[:insert_at] + camera_block + text[insert_at:]

    text = text.replace(
        "  m_ReferenceResolution: {x: 800, y: 600}",
        "  m_ReferenceResolution: {x: 1920, y: 1080}",
        1,
    )

    SCENE.write_text(text, encoding="utf-8")
    print("Orientation layout components added to SampleScene.unity")


if __name__ == "__main__":
    main()
