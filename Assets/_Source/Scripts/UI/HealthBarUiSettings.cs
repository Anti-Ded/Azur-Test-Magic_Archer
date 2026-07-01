using UnityEngine;

namespace MagicArcher.UI
{
    [CreateAssetMenu(fileName = "HealthBarUiSettings", menuName = "Magic Archer/Health Bar UI Settings")]
    public sealed class HealthBarUiSettings : ScriptableObject
    {
        const string DefaultResourcePath = "HealthBarUiSettings";

        [SerializeField] HealthBarUiView _prefab;

        public HealthBarUiView Prefab => _prefab;

        public static HealthBarUiSettings LoadDefault()
        {
            return Resources.Load<HealthBarUiSettings>(DefaultResourcePath);
        }
    }
}
