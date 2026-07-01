import re
from pathlib import Path

ORC_FOLDER = Path(
    r"A:\Unity\Azur\Azur-Test-Magic_Archer\Assets\Third Party\AssetsFotTestTask\Models\Orc"
)
WALK_PATH = ORC_FOLDER / "Walk.anim"
ATTACK_PATH = ORC_FOLDER / "Attack.anim"
HIPS_PATH = "Armature/mixamorig:Hips"
ARMATURE_PATH = "Armature"
START_TIME = 0.041666668
ARMATURE_SCALE = (0.23, 0.23, 0.23)
ARMATURE_ROTATION = (-8.1460335e-08, -0.0, 0.0, 1.0)
ARMATURE_POSITION = (-0.0, 0.0, -0.0)


def qnorm(q):
    x, y, z, w = q
    n = (x * x + y * y + z * z + w * w) ** 0.5
    if n < 1e-8:
        return (0.0, 0.0, 0.0, 1.0)
    return (x / n, y / n, z / n, w / n)


def qmul(a, b):
    ax, ay, az, aw = a
    bx, by, bz, bw = b
    return (
        aw * bx + ax * bw + ay * bz - az * by,
        aw * by - ax * bz + ay * bw + az * bx,
        aw * bz + ax * by - ay * bx + az * bw,
        aw * bw - ax * bx - ay * by - az * bz,
    )


def qinv(q):
    x, y, z, w = q
    n = x * x + y * y + z * z + w * w
    return (-x / n, -y / n, -z / n, w / n)


def find_rotation_block(text, bone_path):
    pattern = (
        r"  m_RotationCurves:\n"
        r"(  - curve:\n      serializedVersion: 2\n      m_Curve:\n"
        r"(?:      - serializedVersion: 3\n.*?\n)*?"
        r"      m_PreInfinity: 2\n      m_PostInfinity: 2\n      m_RotationOrder: 4\n"
        rf"    path: {re.escape(bone_path)})"
    )
    match = re.search(pattern, text, re.S)
    if not match:
        return None
    return match.start(1), match.end(1), match.group(1)


def read_quaternions(block):
    return [
        tuple(map(float, match))
        for match in re.findall(
            r"value: \{x: ([^,]+), y: ([^,]+), z: ([^,]+), w: ([^}]+)\}", block
        )
    ]


def replace_quaternions(block, values):
    index = 0

    def repl(match):
        nonlocal index
        x, y, z, w = values[index]
        index += 1
        return f"value: {{x: {x:.8g}, y: {y:.8g}, z: {z:.8g}, w: {w:.8g}}}"

    return re.sub(
        r"value: \{x: ([^,]+), y: ([^,]+), z: ([^,]+), w: ([^}]+)\}",
        repl,
        block,
    )


def rebase_quaternions(values, reference, source_rest):
    inv_source_rest = qinv(qnorm(source_rest))
    reference = qnorm(reference)
    return [
        qnorm(qmul(reference, qmul(inv_source_rest, qnorm(value))))
        for value in values
    ]


def build_vector_curve(times, vector, bone_path):
    x, y, z = vector
    lines = [
        "  - curve:",
        "      serializedVersion: 2",
        "      m_Curve:",
    ]
    for time in times:
        lines.extend(
            [
                "      - serializedVersion: 3",
                f"        time: {time}",
                f"        value: {{x: {x:.8g}, y: {y:.8g}, z: {z:.8g}}}",
                "        inSlope: {x: 0, y: 0, z: 0}",
                "        outSlope: {x: 0, y: 0, z: 0}",
                "        tangentMode: 0",
                "        weightedMode: 0",
                "        inWeight: {x: 0.33333334, y: 0.33333334, z: 0.33333334}",
                "        outWeight: {x: 0.33333334, y: 0.33333334, z: 0.33333334}",
            ]
        )
    lines.extend(
        [
            "      m_PreInfinity: 2",
            "      m_PostInfinity: 2",
            "      m_RotationOrder: 4",
            f"    path: {bone_path}",
        ]
    )
    return "\n".join(lines)


def build_quaternion_curve(times, rotation, bone_path):
    x, y, z, w = rotation
    lines = [
        "  - curve:",
        "      serializedVersion: 2",
        "      m_Curve:",
    ]
    for time in times:
        lines.extend(
            [
                "      - serializedVersion: 3",
                f"        time: {time}",
                f"        value: {{x: {x:.8g}, y: {y:.8g}, z: {z:.8g}, w: {w:.8g}}}",
                "        inSlope: {x: 0, y: 0, z: 0, w: 0}",
                "        outSlope: {x: 0, y: 0, z: 0, w: 0}",
                "        tangentMode: 0",
                "        weightedMode: 0",
                "        inWeight: {x: 0.33333334, y: 0.33333334, z: 0.33333334, w: 0.33333334}",
                "        outWeight: {x: 0.33333334, y: 0.33333334, z: 0.33333334, w: 0.33333334}",
            ]
        )
    lines.extend(
        [
            "      m_PreInfinity: 2",
            "      m_PostInfinity: 2",
            "      m_RotationOrder: 4",
            f"    path: {bone_path}",
        ]
    )
    return "\n".join(lines)


def read_attack_reference():
    attack = ATTACK_PATH.read_text(encoding="utf-8")
    hips_rot = find_rotation_block(attack, HIPS_PATH)
    hips_pos = re.search(
        r"  m_PositionCurves:\n(  - curve:.*?    path: Armature/mixamorig:Hips\n)",
        attack,
        re.S,
    )
    if not hips_rot or not hips_pos:
        raise SystemExit("Attack reference curves not found")
    ref_rotation = read_quaternions(hips_rot[2])[0]
    ref_position = tuple(
        map(
            float,
            re.findall(
                r"value: \{x: ([^,]+), y: ([^,]+), z: ([^}]+)\}",
                hips_pos.group(1),
            )[0],
        )
    )
    return ref_rotation, ref_position


def rebuild_tail(stop_time, ref_position):
    times = [START_TIME, stop_time]
    return f"""  m_CompressedRotationCurves: []
  m_EulerCurves: []
  m_PositionCurves:
{build_vector_curve(times, ref_position, HIPS_PATH)}
{build_vector_curve(times, ARMATURE_POSITION, ARMATURE_PATH)}
  m_ScaleCurves:
{build_vector_curve(times, ARMATURE_SCALE, ARMATURE_PATH)}
  m_FloatCurves: []
  m_PPtrCurves: []
  m_SampleRate: 60
  m_WrapMode: 0
  m_Bounds:
    m_Center: {{x: 0, y: 0, z: 0}}
    m_Extent: {{x: 0, y: 0, z: 0}}
  m_ClipBindingConstant:
    genericBindings: []
    pptrCurveMapping: []
  m_AnimationClipSettings:
    serializedVersion: 2
    m_AdditiveReferencePoseClip: {{fileID: 0}}
    m_AdditiveReferencePoseTime: 0
    m_StartTime: 0
    m_StopTime: {stop_time}
    m_OrientationOffsetY: 0
    m_Level: 0
    m_CycleOffset: 0
    m_HasAdditiveReferencePose: 0
    m_LoopTime: 1
    m_LoopBlend: 0
    m_LoopBlendOrientation: 0
    m_LoopBlendPositionY: 0
    m_LoopBlendPositionXZ: 0
    m_KeepOriginalOrientation: 0
    m_KeepOriginalPositionY: 1
    m_KeepOriginalPositionXZ: 0
    m_HeightFromFeet: 0
    m_Mirror: 0
  m_EditorCurves: []
  m_EulerEditorCurves: []
  m_HasGenericRootTransform: 0
  m_HasMotionFloatCurves: 0
  m_Events: []
"""


def main():
    text = WALK_PATH.read_text(encoding="utf-8")
    ref_rotation, ref_position = read_attack_reference()
    stop_time = float(re.search(r"m_StopTime: ([^\n]+)", text).group(1))

    hips_rot = find_rotation_block(text, HIPS_PATH)
    if not hips_rot:
        raise SystemExit("Walk hips rotation block not found")

    walk_values = read_quaternions(hips_rot[2])
    fixed_values = rebase_quaternions(walk_values, ref_rotation, walk_values[0])
    text = (
        text[: hips_rot[0]]
        + replace_quaternions(hips_rot[2], fixed_values)
        + text[hips_rot[1] :]
    )

    marker = "  m_CompressedRotationCurves:"
    marker_index = text.find(marker)
    if marker_index < 0:
        raise SystemExit("Compressed rotation marker not found")

    armature_rot_block = build_quaternion_curve(
        [START_TIME, stop_time], ARMATURE_ROTATION, ARMATURE_PATH
    )
    text = text[:marker_index] + armature_rot_block + "\n" + rebuild_tail(stop_time, ref_position)

    WALK_PATH.write_text(text, encoding="utf-8")
    print(
        "Walk.anim rebuilt:",
        f"hips={ref_position}",
        f"armature scale={ARMATURE_SCALE}",
        f"stop={stop_time}",
    )


if __name__ == "__main__":
    main()
