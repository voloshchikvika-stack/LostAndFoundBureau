using System.Collections.Generic;
using System.IO;
using LostAndFound.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LostAndFound.EditorTools
{
    public static class LoadingSceneInstaller
    {
        private const string LoadingPath = "Assets/Scenes/LoadingScene.unity";
        private const string BureauPath = "Assets/Scenes/SampleScene.unity";
        private const string Match3Path = "Assets/Scenes/Match3.unity";

        [MenuItem("LostAndFound/Install Loading Screen")]
        public static void Install()
        {
            if (!File.Exists(BureauPath))
            {
                EditorUtility.DisplayDialog("Loading Screen", "Missing " + BureauPath, "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string previousScene = SceneManager.GetActiveScene().path;

            if (!File.Exists(LoadingPath))
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                GameObject go = new GameObject("BureauLoadingScreen");
                go.AddComponent<BureauLoadingScreen>();
                EditorSceneManager.SaveScene(scene, LoadingPath);
            }

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(LoadingPath, true),
                new EditorBuildSettingsScene(BureauPath, true),
                new EditorBuildSettingsScene(Match3Path, true)
            };

            // Preserve any other scenes already configured in the project.
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path != LoadingPath && scene.path != BureauPath && scene.path != Match3Path)
                    scenes.Add(scene);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            if (!string.IsNullOrEmpty(previousScene) && File.Exists(previousScene))
                EditorSceneManager.OpenScene(previousScene);
            else
                EditorSceneManager.OpenScene(LoadingPath);

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Loading Screen",
                "Installed! Open Assets/Scenes/LoadingScene.unity and press Play to preview. " +
                "LoadingScene is now first in Build Settings.", "OK");
        }
    }
}
