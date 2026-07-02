using UnityEngine;

namespace MagicArcher.Gameplay.Combat
{
    public sealed class CombatSceneRefs : MonoBehaviour
    {
        [SerializeField] Transform _projectilesRoot;

        public Transform ProjectilesRoot => _projectilesRoot;
    }
}
