// NavBtnEditor.cs
// ➜ Place ce script dans un dossier "Editor/" (obligatoire).
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavBtn))]
public class NavBtnEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var actionProp = serializedObject.FindProperty("action");
        var targetIdProp = serializedObject.FindProperty("targetId");
        var disableWhileBusyProp = serializedObject.FindProperty("disableButtonWhileBusy");

        EditorGUILayout.PropertyField(actionProp);

        var actionEnum = (NavBtn.ActionType)actionProp.enumValueIndex;

        // N'affiche targetId que quand c'est utile
        if (actionEnum == NavBtn.ActionType.OpenTo || actionEnum == NavBtn.ActionType.OpenOverlay)
        {
            EditorGUILayout.PropertyField(targetIdProp);
            if (string.IsNullOrEmpty(targetIdProp.stringValue))
            {
                EditorGUILayout.HelpBox("Target Id requis pour OpenTo / OpenOverlay.", MessageType.Warning);
            }
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.PropertyField(disableWhileBusyProp);

        serializedObject.ApplyModifiedProperties();
    }
}