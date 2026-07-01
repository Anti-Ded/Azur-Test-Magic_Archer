using MagicArcher.Gameplay.Grid;
using UnityEditor;
using UnityEngine;

namespace MagicArcher.Editor
{
    [CustomEditor(typeof(GridBoardView))]
    public sealed class GridBoardViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8f);

            var view = (GridBoardView)target;

            if (GUILayout.Button("Rebuild Grid", GUILayout.Height(28f)))
                view.RebuildForEditor();

            EditorGUILayout.HelpBox(
                "Подкрути Cell Size / Cell Scale / Y Offset и нажми Rebuild Grid. " +
                "Cell Size должен совпадать с Level Root → Cell Size для логики размещения юнитов.",
                MessageType.Info);
        }
    }
}
