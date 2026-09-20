using UnityEngine;
using UnityEngine.SceneManagement;

namespace LostAndFound.Match3
{
    public static class Match3Bootstrap
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
            CreateBoardIfNeeded(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CreateBoardIfNeeded(scene);
        }

        private static void CreateBoardIfNeeded(Scene scene)
        {
            if (scene.name != "Match3")
                return;

            if (Object.FindFirstObjectByType<Match3Board>() != null)
                return;

            GameObject boardObject = new GameObject("Match3Board");
            boardObject.AddComponent<Match3Board>();
        }
    }
}
