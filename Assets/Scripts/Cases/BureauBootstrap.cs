using UnityEngine;
using UnityEngine.SceneManagement;

namespace LostAndFound.Cases
{
    public static class BureauBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateBureauIfNeeded()
        {
            if (SceneManager.GetActiveScene().name != "SampleScene")
                return;

            if (Object.FindFirstObjectByType<BureauController>() != null)
                return;

            GameObject bureauObject = new GameObject("BureauController");
            bureauObject.AddComponent<BureauController>();
        }
    }
}
