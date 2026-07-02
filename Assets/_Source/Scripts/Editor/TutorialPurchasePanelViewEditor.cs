using MagicArcher.UI;
using UnityEditor;
using UnityEngine;

namespace MagicArcher.Editor
{
    [CustomEditor(typeof(TutorialPurchasePanelView))]
    public sealed class TutorialPurchasePanelViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8f);

            var view = (TutorialPurchasePanelView)target;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Get Horizontal Values", GUILayout.Height(28f)))
                view.GetHorizontalValues();
            if (GUILayout.Button("Get Vertical Values", GUILayout.Height(28f)))
                view.GetVerticalValues();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Расставь Hero A, Hero B и Buy Button в нужной ориентации, затем нажми соответствующую кнопку — " +
                "текущие RectTransform значения сохранятся в Horizontal/Vertical Layout.",
                MessageType.Info);
        }
    }
}
