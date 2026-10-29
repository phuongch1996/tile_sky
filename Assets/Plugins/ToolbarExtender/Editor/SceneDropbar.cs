using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;

[InitializeOnLoad]
public class SceneDropbar
{
    static SceneDropbar()
    {
        ToolbarExtender.RightToolbarGUI.Add(OnRight_ToolbarGUI);
    }

    static void OnRight_ToolbarGUI()
    {
        string currentSceneName = EditorSceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(currentSceneName))
        {
            currentSceneName = "No Scene";
        }

        if (EditorGUILayout.DropdownButton(
                new GUIContent(currentSceneName),
                FocusType.Passive,
                EditorStyles.toolbarDropDown,
                GUILayout.Width(120)
                ))
        {
            GenericMenu menu = new GenericMenu();

            //string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
            //if (sceneGUIDs.Length <= 0)
            //{
            //    Debug.LogWarning("No Scenes found in Assets/Scenes");
            //    return;
            //}
            //foreach (string guid in sceneGUIDs)
            //{
            //    string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            //    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath); // Lấy tên scene không có .unity
            //    menu.AddItem(new GUIContent(sceneName), false, () => LoadScene(scenePath));
            //}
            EditorBuildSettingsScene[] scenesInBuild = EditorBuildSettings.scenes;
            if (scenesInBuild.Length <= 0)
            {
                Debug.LogWarning("No Scenes found in Assets/Scenes");
                return;
            }
            foreach (EditorBuildSettingsScene scene in scenesInBuild)
            {
                Debug.Log(scene.path);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scene.path); // Lấy tên scene không có .unity
                Debug.Log(sceneName);
                menu.AddItem(new GUIContent(sceneName), false, () => LoadScene(scene.path));
            }

            Rect buttonRect = GUILayoutUtility.GetLastRect();
            Rect menuRect = new Rect(buttonRect.x, buttonRect.y + buttonRect.height, buttonRect.width, 20);
            menu.DropDown(menuRect);
        }
    }
    static void LoadScene(string scenePath)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
    }

    private static Texture2D MakeSolidTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
