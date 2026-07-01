"""Retarget Walk.anim hips to Attack rest pose."""
from __future__ import annotations

import math
import re
from pathlib import Path

WALK_PATH = Path(
    r"A:\Unity\Azur\Azur-Test-Magic_Archer\Assets\Third Party\AssetsFotTestTask\Models\Orc\Walk.anim"
)

REFERENCE_HIPS_ROT = (0.045587327, 0.4262758, -0.045654263, 0.90228957)
WALK_REST_HIPS_ROT = (0.74977607, 0.29127428, 0.23660514, 0.5449892)
REFERENCE_HIPS_POS = (0.051700518, 3.9425952, 0.069730945)
WALK_REST_HIPS_POS = (-0.0009305403, 0.00067109725, 0.03614212)

ARMATURE_ROT_BLOCK = """  - curve:
      serializedVersion: 2
      m_Curve:
      - serializedVersion: 3
        time: 0
        value: {x: -8.1460335e-08, y: -0, z: 0, w: 1}
        inSlope: {x: Infinity, y: Infinity, z: Infinity, w: Infinity}
        outSlope: {x: 0, y: 0, z: 0, w: 0}
        tangentMode: 0
        weightedMode: 0
        inWeight: {x: 0, y: 0, z: 0, w: 0}
        outWeight: {x: 0, y: 0, z: 0, w: 0}
      - serializedVersion: 3
        time: 1.4000001
        value: {x: -8.1460335e-08, y: -0, z: 0, w: 1}
        inSlope: {x: Infinity, y: Infinity, z: Infinity, w: Infinity}
        outSlope: {x: 0, y: 0, z: 0, w: 0}
        tangentMode: 0
        weightedMode: 0
        inWeight: {x: 0, y: 0, z: 0, w: 0}
        outWeight: {x: 0, y: 0, z: 0, w: 0}
      m_PreInfinity: 2
      m_PostInfinity: 2
      m_RotationOrder: 4
    path: Armature
"""


def normalize(q: tuple[float, float, float, float]) -> tuple[float, float, float, float]:
    x, y, z, w = q
    m = math.sqrt(x * x + y * y + z * z + w * w)
    if m <= 1e-8:
        return 0.0, 0.0, 0.0, 1.0
    return x / m, y / m, z / m, w / m


def quat_mult(
    a: tuple[float, float, float, float],
    b: tuple[float, float, float, float],
) -> tuple[float, float, float, float]:
    ax, ay, az, aw = a
    bx, by, bz, bw = b
    return (
        aw * bx + ax * bw + ay * bz - az * by,
        aw * by - ax * bz + ay * bw + az * bx,
        aw * bz + ax * by - ay * bx + az * bw,
        aw * bw - ax * bx - ay * by - az * bz,
    )


def quat_inverse(q: tuple[float, float, float, float]) -> tuple[float, float, float, float]:
    x, y, z, w = normalize(q)
    return -x, -y, -z, w


def retarget_rotation(
    source: tuple[float, float, float, float],
    reference: tuple[float, float, float, float],
    source_rest: tuple[float, float, float, float],
) -> tuple[float, float, float, float]:
    inv_rest = quat_inverse(source_rest)
    delta = normalize(quat_mult(inv_rest, normalize(source)))
    return normalize(quat_mult(normalize(reference), delta))


def format_float(value: float) -> str:
    text = f"{value:.9g}"
    if "e" not in text and "." not in text:
        text += ".0"
    return text


def format_quat(q: tuple[float, float, float, float]) -> str:
    return (
        f"{{x: {format_float(q[0])}, y: {format_float(q[1])}, "
        f"z: {format_float(q[2])}, w: {format_float(q[3])}}}"
    )


def format_vec3(v: tuple[float, float, float]) -> str:
    return (
        f"{{x: {format_float(v[0])}, y: {format_float(v[1])}, "
        f"z: {format_float(v[2])}}}"
    )


def patch_quat_values(block: str) -> str:
    pattern = re.compile(
        r"value: \{x: ([^,]+), y: ([^,]+), z: ([^,]+), w: ([^}]+)\}"
    )

    def repl(match: re.Match[str]) -> str:
        source = tuple(float(group) for group in match.groups())
        fixed = retarget_rotation(source, REFERENCE_HIPS_ROT, WALK_REST_HIPS_ROT)
        return f"value: {format_quat(fixed)}"

    return pattern.sub(repl, block)


def patch_vec3_values(block: str, delta: tuple[float, float, float]) -> str:
    pattern = re.compile(r"value: \{x: ([^,]+), y: ([^,]+), z: ([^}]+)\}")

    def repl(match: re.Match[str]) -> str:
        x, y, z = (float(group) for group in match.groups())
        fixed = (x + delta[0], y + delta[1], z + delta[2])
        return f"value: {format_vec3(fixed)}"

    return pattern.sub(repl, block)


def extract_curve_block(section: str, path_marker: str, end_marker: str) -> tuple[str, str, str]:
    path_index = section.find(path_marker)
    if path_index < 0:
        raise RuntimeError(f"Missing path marker: {path_marker!r}")

    block_start = section.rfind("  - curve:", 0, path_index)
    if block_start < 0:
        raise RuntimeError(f"Missing curve block start for: {path_marker!r}")

    end_index = section.find(end_marker, path_index + len(path_marker))
    if end_index < 0:
        end_index = len(section)

    block_end = section.rfind("  - curve:", block_start + 1, end_index)
    if block_end < 0:
        block_end = end_index

    return section[:block_start], section[block_start:block_end], section[block_end:]


def ensure_armature_rotation(section: str) -> str:
    if re.search(r"^    path: Armature\s*$", section, re.MULTILINE):
        return section
    return section + ARMATURE_ROT_BLOCK


def ensure_scale_curve(section: str) -> str:
    if "    path: Armature\n  m_FloatCurves:" in section:
        return section

    scale_block = """  - curve:
      serializedVersion: 2
      m_Curve:
      - serializedVersion: 3
        time: 0
        value: {x: 0.23, y: 0.23, z: 0.23}
        inSlope: {x: Infinity, y: Infinity, z: Infinity}
        outSlope: {x: 0, y: 0, z: 0}
        tangentMode: 0
        weightedMode: 0
        inWeight: {x: 0, y: 0, z: 0}
        outWeight: {x: 0, y: 0, z: 0}
      - serializedVersion: 3
        time: 1.4000001
        value: {x: 0.23, y: 0.23, z: 0.23}
        inSlope: {x: Infinity, y: Infinity, z: Infinity}
        outSlope: {x: 0, y: 0, z: 0}
        tangentMode: 0
        weightedMode: 0
        inWeight: {x: 0, y: 0, z: 0}
        outWeight: {x: 0, y: 0, z: 0}
      m_PreInfinity: 2
      m_PostInfinity: 2
      m_RotationOrder: 4
    path: Armature
"""

    if "  m_ScaleCurves: []" in section:
        return section.replace("  m_ScaleCurves: []", "  m_ScaleCurves:\n" + scale_block, 1)

    return section


def patch_settings(text: str) -> str:
    text = text.replace("m_HasGenericRootTransform: 1", "m_HasGenericRootTransform: 0")
    text = text.replace("    m_LoopTime: 0\n", "    m_LoopTime: 1\n")
    text = text.replace("    m_KeepOriginalPositionY: 1\n", "    m_KeepOriginalPositionY: 0\n")
    return text


def main() -> None:
    text = WALK_PATH.read_text(encoding="utf-8")

    rot_start = text.find("  m_RotationCurves:\n")
    rot_end = text.find("  m_CompressedRotationCurves:", rot_start)
    rot_section = text[rot_start:rot_end]

    prefix, hips_rot_block, rot_tail = extract_curve_block(
        rot_section,
        "    path: Armature/mixamorig:Hips\n",
        "    path: Armature/mixamorig:Hips/mixamorig:Spine\n",
    )
    rot_section = prefix + patch_quat_values(hips_rot_block) + rot_tail
    rot_section = ensure_armature_rotation(rot_section)
    text = text[:rot_start] + rot_section + text[rot_end:]

    pos_start = text.find("  m_PositionCurves:\n")
    pos_end = text.find("  m_ScaleCurves:", pos_start)
    pos_section = text[pos_start:pos_end]

    prefix, hips_pos_block, pos_tail = extract_curve_block(
        pos_section,
        "    path: Armature/mixamorig:Hips\n",
        "  m_ScaleCurves:",
    )
    delta = tuple(r - w for r, w in zip(REFERENCE_HIPS_POS, WALK_REST_HIPS_POS))
    pos_section = prefix + patch_vec3_values(hips_pos_block, delta) + pos_tail
    text = text[:pos_start] + pos_section + text[pos_end:]

    scale_start = text.find("  m_ScaleCurves:")
    scale_end = text.find("  m_FloatCurves:", scale_start)
    scale_section = text[scale_start:scale_end]
    scale_section = ensure_scale_curve(scale_section)
    text = text[:scale_start] + scale_section + text[scale_end:]

    text = patch_settings(text)
    WALK_PATH.write_text(text, encoding="utf-8")
    print("Walk.anim hips retargeted to Attack rest pose.")


if __name__ == "__main__":
    main()
