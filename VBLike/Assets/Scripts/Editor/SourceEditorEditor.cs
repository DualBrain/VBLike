using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SourceEditor))]
public class SourceEditorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
