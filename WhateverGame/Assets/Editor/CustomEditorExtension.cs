using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class CustomEditorExtension : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GridManager gridManager = (GridManager)target;

        if (GUILayout.Button("Generate grid units"))
        {
            gridManager.GenerateGrid();
            EditorUtility.SetDirty(gridManager);
            var so = new SerializedObject(gridManager);
            so.ApplyModifiedProperties();
            DrawDefaultInspector();
        }

        if (GUILayout.Button("Clear grid"))
        {
            gridManager.ClearGridData();
            EditorUtility.SetDirty(gridManager);
            var so = new SerializedObject(gridManager);
            so.ApplyModifiedProperties();
            DrawDefaultInspector();
        }
    }
}

