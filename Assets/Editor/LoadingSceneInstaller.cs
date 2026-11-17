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
        private const string ProfilePath = "Assets/Scenes/ProfileSetup.unity";
        private const string BureauPath = "Assets/Scenes/SampleScene.unity";
        private const string Match3Path = "Assets/Scenes/Match3.unity";

        [MenuItem("LostAndFound/Install Loading Screen")]
        public static void Install()
        {
            if (!File.Exists(BureauPath))
            {
                EditorUtility.DisplayDialog("First Run", "Missing " + BureauPath, "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string previousScene = SceneManager.GetActiveScene().path;
            EnsureScene<BureauLoadingScreen>(LoadingPath, "BureauLoadingScreen");
            EnsureScene<PlayerProfileSetup>(ProfilePath, "PlayerProfileSetup");

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(LoadingPath, true),
                new EditorBuildSettingsScene(ProfilePath, true),
                new EditorBuildSettingsScene(BureauPath, true),
                new EditorBuildSettingsScene(Match3Path, true)
            };
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                if (scene.path != LoadingPath && scene.path != ProfilePath &&
                    scene.path != BureauPath && scene.path != Match3Path)
                    scenes.Add(scene);

            EditorBuildSettings.scenes = scenes.ToArray();
            if (!string.IsNullOrEmpty(previousScene) && File.Exists(previousScene))
                EditorSceneManager.OpenScene(previousScene);
            else
                EditorSceneManager.OpenScene(LoadingPath);

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("First Run",
                "Installed LoadingScene and ProfileSetup. Open LoadingScene and press Play. " +
                "Only new players will see the profile screen.", "OK");
        }

        private static void EnsureScene<T>(string path, string objectName) where T : Component
        {
            if (File.Exists(path))
            {
                Scene existing = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                if (Object.FindFirstObjectByType<T>() == null)
                {
                    new GameObject(objectName).AddComponent<T>();
                    EditorSceneManager.MarkSceneDirty(existing);
                    EditorSceneManager.SaveScene(existing);
                }
                return;
            }
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject(objectName).AddComponent<T>();
            EditorSceneManager.SaveScene(scene, path);
        }

        [MenuItem("LostAndFound/Test/Show Profile Setup Again")]
        private static void TestShowProfileSetup()
        {
            if (EditorUtility.DisplayDialog("Profile test",
                "Show the profile form again on the next startup? " +
                "Existing case progress and reputation will be preserved.", "Reset profile only", "Cancel"))
            {
                PlayerProfile.DebugClearProfile();
                EditorUtility.DisplayDialog("Profile test",
                    "Open LoadingScene and press Play to create a new profile.", "OK");
            }
        }
    }
}
