using UnityEngine;
using UnityEngine.SceneManagement;

namespace LostAndFound.Cases
{
    public static class BureauBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoaded()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateOnInitialScene()
        {
            CreateBureauIfNeeded(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CreateBureauIfNeeded(scene);
        }

        private static void CreateBureauIfNeeded(Scene scene)
        {
            if (scene.name != "SampleScene")
                return;

            if (Object.FindFirstObjectByType<BureauController>() != null)
                return;

            GameObject bureauObject = new GameObject("BureauController");
            bureauObject.AddComponent<BureauController>();
        }
    }
}
