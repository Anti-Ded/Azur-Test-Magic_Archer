using System;

namespace MagicArcher.Core.Config
{
    public sealed class UnitConfigCatalog
    {
        readonly UnitConfigBase[] _configs;

        public UnitConfigCatalog(params UnitConfigBase[] configs)
        {
            _configs = configs ?? Array.Empty<UnitConfigBase>();
        }

        public UnitConfigBase ResolveMergeResult(UnitConfigBase first, UnitConfigBase second)
        {
            if (first == null || second == null)
                return null;

            for (var i = 0; i < _configs.Length; i++)
            {
                var config = _configs[i];
                if (config != null && config.MatchesMergeIngredients(first, second))
                    return config;
            }

            return null;
        }

        public bool IsMergeIngredient(UnitConfigBase config)
        {
            if (config == null)
                return false;

            for (var i = 0; i < _configs.Length; i++)
            {
                var recipe = _configs[i];
                if (recipe != null && recipe.ContainsMergeChild(config))
                    return true;
            }

            return false;
        }
    }
}
