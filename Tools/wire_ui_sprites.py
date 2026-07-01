from pathlib import Path

SCENE = Path(
    r"A:\Unity\Azur\Azur-Test-Magic_Archer\Assets\Scenes\SampleScene.unity"
)

SPRITES = {
    "button": "a6759c4712ad5754a8aabadee46f4b40",
    "hand": "3d1004524f05fd94895cc29923bbad61",
    "square": "c50155422daa77d41bdb241e7be3d66b",
}


def sprite_ref(guid: str) -> str:
    return f"m_Sprite: {{fileID: 21300000, guid: {guid}, type: 3}}"


def patch_block(text: str, marker: str, patch_fn) -> str:
    start = text.find(marker)
    if start < 0:
        raise SystemExit(f"Marker not found: {marker}")
    end = text.find("\n--- !u!", start + 1)
    if end < 0:
        end = len(text)
    block = text[start:end]
    new_block = patch_fn(block)
    return text[:start] + new_block + text[end:]


def assign_sprite(block: str, guid: str) -> str:
    if "m_Sprite: {fileID: 0}" not in block:
        return block
    return block.replace("m_Sprite: {fileID: 0}", sprite_ref(guid), 1)


def main():
    text = SCENE.read_text(encoding="utf-8")

    patches = [
        ("--- !u!114 &721374659", lambda b: assign_sprite(b, SPRITES["button"])),
        ("--- !u!114 &1889423123", lambda b: assign_sprite(b, SPRITES["button"])),
        ("--- !u!114 &1648896859", lambda b: assign_sprite(b, SPRITES["hand"])),
        ("--- !u!114 &103324865", lambda b: assign_sprite(b, SPRITES["square"])),
        ("--- !u!114 &58761267", lambda b: assign_sprite(b, SPRITES["square"])),
    ]

    for marker, patch_fn in patches:
        text = patch_block(text, marker, patch_fn)

    text = text.replace(
        "  m_Children:\n  - {fileID: 103324864}\n  - {fileID: 1889423120}\n  - {fileID: 1648896858}",
        "  m_Children:\n  - {fileID: 103324864}\n  - {fileID: 1648896858}",
        1,
    )

    SCENE.write_text(text, encoding="utf-8")
    print("UI sprites wired in SampleScene.unity")


if __name__ == "__main__":
    main()
