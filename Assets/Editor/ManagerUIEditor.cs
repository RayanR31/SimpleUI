// ManagerUIEditor.cs (dans un dossier Editor/)
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ManagerUI))]
public class ManagerUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Check Setup"))
        {
            Validate();
        }
    }

    private void Validate()
    {
        var pages = Object.FindObjectsOfType<Page>(true);
        var ids = new System.Collections.Generic.HashSet<string>();

        foreach (var p in pages)
        {
            var id = p.GetIdForValidation(); // ou expose une property Id
            if (!ids.Add(id))
                Debug.LogError($"Duplicate Page ID detected: '{id}'", p);
        }

        var btns = Object.FindObjectsOfType<NavBtn>(true);
        foreach (var b in btns)
        {
            if (b.RequiresTargetIdForValidation() && string.IsNullOrWhiteSpace(b.GetTargetIdForValidation()))
                Debug.LogWarning($"NavBtn '{b.name}' is missing a Target ID.", b);
        }

        Debug.Log("ManagerUI: Validation complete. Check Console for warnings/errors.");
    }
}
#endif