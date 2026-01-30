using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExampleToolWindow : EditorWindow
{
   /* [MenuItem("Tools/Examples/Tool Window")]
    public static void Open() => GetWindow<ExampleToolWindow>("Example Tool");

    public GameObject go;
    public SO_Screen so_Screen;
    
    public SceneAsset sceneAsset;

    private void OnGUI()
    {
        GUILayout.Label("Tool Window", EditorStyles.boldLabel);
        


        so_Screen = (SO_Screen)EditorGUILayout.ObjectField(
            "Target SO_Screen",
            so_Screen,
            typeof(SO_Screen),
            false // true = autorise les objets de la scène
        );
        
        go = (GameObject)EditorGUILayout.ObjectField(
            "Target GameObject",
            go,
            typeof(GameObject),
            false // true = autorise les objets de la scène
        );
        
        using (new EditorGUI.DisabledScope(so_Screen == null || go == null))
        {
            if (GUILayout.Button("Load"))
            {
                Undo.RecordObject(so_Screen, "Assign Prefab To SO_Screen");
                so_Screen.prefab = go;

                EditorUtility.SetDirty(so_Screen);   // important pour sauvegarder l'asset

                AssetDatabase.SaveAssets();        // optionnel: force la sauvegarde
                Debug.Log($"Assigné {go.name} dans {so_Screen.name}");
            }
        }

        GUISceneAssets();
    }

    private void GUISceneAssets()
    {
        GUILayout.Space(10);
        
        sceneAsset = (SceneAsset)EditorGUILayout.ObjectField(
            "Target sceneAsset",
            sceneAsset,
            typeof(SceneAsset),
            false // true = autorise les objets de la scène
        );
        
        using (new EditorGUI.DisabledScope(sceneAsset == null))
        {
            if (GUILayout.Button("Analyze Scene"))
            {
                AnalyzeScene();
            }
        }
    }
    private void AnalyzeScene()
    {
        string targetPath = AssetDatabase.GetAssetPath(sceneAsset);
        if (string.IsNullOrEmpty(targetPath))
            return;

        // Optionnel: demander de sauvegarder la scène courante
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        // On garde la scène active actuelle
        Scene previousActive = SceneManager.GetActiveScene();

        // On ouvre la scène à analyser en additif (ne remplace pas ton contexte)
        Scene loaded = EditorSceneManager.OpenScene(targetPath, OpenSceneMode.Single);
        SceneManager.SetActiveScene(loaded);

        try
        {
            var screens = GameObject.FindObjectsOfType<Page>(true);
            Debug.Log($"Trouvé {screens.Length} Screen dans la scène {loaded.name}");

            string folder = "Assets/GeneratedScreens";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets", "GeneratedScreens");

            AssetDatabase.StartAssetEditing(); // 🔥 gros gain si beaucoup de prefabs
            foreach (var screen in screens)
            {
                var go = screen.gameObject;

                // Si noms identiques -> collisions. (optionnel) sécuriser le nom:
                // string safeName = go.name.Replace("/", "_");
                string prefabPath = $"{folder}/{go.name}.prefab";

                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            }
            AssetDatabase.StopAssetEditing();

            AssetDatabase.SaveAssets(); // suffit généralement
            // AssetDatabase.Refresh(); // évite si possible (souvent inutile)
        }
        finally
        {
            // On restaure la scène active
            if (previousActive.IsValid())
                SceneManager.SetActiveScene(previousActive);

            // On ferme la scène analysée (sans la sauvegarder)
            EditorSceneManager.CloseScene(loaded, true);
        }
    }*/

}
