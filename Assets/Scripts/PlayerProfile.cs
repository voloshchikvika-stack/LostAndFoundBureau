using UnityEngine;

namespace LostAndFound.UI
{
    /// <summary>Local player profile. No network account or personal login.</summary>
    public static class PlayerProfile
    {
        private const string CreatedKey = "LostAndFound.Profile.Created.v1";
        private const string NameKey = "LostAndFound.Profile.Name.v1";
        private const string AvatarKey = "LostAndFound.Profile.Avatar.v1";

        public static bool IsCreated => PlayerPrefs.GetInt(CreatedKey, 0) == 1;
        public static string Name => PlayerPrefs.GetString(NameKey, "");
        public static int Avatar => Mathf.Clamp(PlayerPrefs.GetInt(AvatarKey, 0), 0, 5);

        public static bool TryCreate(string name, int avatar)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            name = name.Trim();
            if (name.Length > 18 || avatar < 0 || avatar > 5)
                return false;

            PlayerPrefs.SetString(NameKey, name);
            PlayerPrefs.SetInt(AvatarKey, avatar);
            // Write the completion marker last so unfinished profiles retry.
            PlayerPrefs.SetInt(CreatedKey, 1);
            PlayerPrefs.Save();
            return true;
        }

#if UNITY_EDITOR
        // For testing only. Deliberately does not delete game progress.
        public static void DebugClearProfile()
        {
            PlayerPrefs.DeleteKey(CreatedKey);
            PlayerPrefs.DeleteKey(NameKey);
            PlayerPrefs.DeleteKey(AvatarKey);
            PlayerPrefs.Save();
        }
#endif
    }
}
