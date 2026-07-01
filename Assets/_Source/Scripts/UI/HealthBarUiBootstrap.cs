using UnityEngine;

namespace MagicArcher.UI
{
    public static class HealthBarUiBootstrap
    {
        const string HealthBarsChildName = "HealthBars";

        public static void Configure(HealthBarUiService service, CombatUiRefs uiRefs, Canvas canvas)
        {
            if (service == null)
                return;

            if (canvas == null)
            {
                canvas = uiRefs != null
                    ? uiRefs.GetComponent<Canvas>() ?? uiRefs.GetComponentInParent<Canvas>()
                    : Object.FindFirstObjectByType<Canvas>();
            }

            if (canvas == null)
                return;

            var container = EnsureContainer(canvas, uiRefs?.HealthBarsRoot);
            var prefab = uiRefs?.HealthBarPrefab ?? HealthBarUiSettings.LoadDefault()?.Prefab;
            service.Configure(container, prefab, canvas);

            if (prefab == null)
            {
                Debug.LogWarning(
                    "HealthBarUi: prefab is missing. Run Magic Archer → Setup Combat Scene " +
                    "or place HealthBarUiSettings in Resources.");
            }
        }

        public static RectTransform EnsureContainer(Canvas canvas, RectTransform existingRoot)
        {
            if (existingRoot != null)
                return existingRoot;

            var existing = canvas.transform.Find(HealthBarsChildName) as RectTransform;
            if (existing != null)
                return existing;

            var go = new GameObject(HealthBarsChildName, typeof(RectTransform));
            var container = go.GetComponent<RectTransform>();
            container.SetParent(canvas.transform, false);
            StretchFullScreen(container);
            return container;
        }

        static void StretchFullScreen(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
