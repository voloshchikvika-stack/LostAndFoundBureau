using UnityEngine;
using UnityEngine.SceneManagement;

namespace LostAndFound.Match3
{
    public static class Match3Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateBoardIfNeeded()
        {
            if (SceneManager.GetActiveScene().name != "Match3")
                return;

            if (Object.FindFirstObjectByType<Match3Board>() != null)
                return;

            var boardObject = new GameObject("Match3Board");
            boardObject.AddComponent<Match3Board>();
        }
    }
}
