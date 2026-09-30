using UnityEngine;

namespace LostAndFound.Cases
{
    public static class CaseSession
    {
        private const string SavePrefix = "LostAndFound.v2.";
        private static bool initialized;
        private static int currentCaseIndex;
        private static int completedClues;
        private static int lastClueIndex = -1;
        private static bool introSeen;
        private static bool inquirySeen;
        private static bool miniGameCompleted;
        private static bool pendingCluePopup;
        private static bool allCasesCompleted;

        public static int CurrentCaseIndex => currentCaseIndex;
        public static int CompletedClues => completedClues;
        public static bool IntroSeen => introSeen;
        public static bool InquirySeen => inquirySeen;
        public static bool MiniGameCompleted => miniGameCompleted;
        public static bool MiniGameReady
        {
            get
            {
                CaseDefinition currentCase = CurrentCase;
                return currentCase != null && currentCase.HasMiniGame &&
                       completedClues >= currentCase.MiniGameAfterClue && !miniGameCompleted;
            }
        }
        public static bool AllCasesCompleted => allCasesCompleted;
        public static bool HasActiveCase => !allCasesCompleted && CurrentCase != null;

        public static CaseDefinition CurrentCase
        {
            get
            {
                EnsureInitialized();
                return CaseDatabase.GetCase(currentCaseIndex);
            }
        }

        public static Match3LevelDefinition CurrentLevel
        {
            get
            {
                CaseDefinition currentCase = CurrentCase;
                if (currentCase == null || currentCase.Levels == null || currentCase.Levels.Length == 0)
                    return null;

                int index = Mathf.Clamp(completedClues, 0, currentCase.Levels.Length - 1);
                return currentCase.Levels[index];
            }
        }

        public static bool HasAllClues
        {
            get
            {
                CaseDefinition currentCase = CurrentCase;
                return currentCase != null && completedClues >= currentCase.RequiredClues;
            }
        }

        public static void EnsureInitialized()
        {
            if (initialized)
                return;

            initialized = true;
            currentCaseIndex = Mathf.Clamp(
                PlayerPrefs.GetInt(SavePrefix + "CaseIndex", 0), 0,
                Mathf.Max(0, CaseDatabase.Cases.Count - 1));
            completedClues = PlayerPrefs.GetInt(SavePrefix + "Clues", 0);
            lastClueIndex = PlayerPrefs.GetInt(SavePrefix + "LastClue", -1);
            introSeen = PlayerPrefs.GetInt(SavePrefix + "Intro", 0) == 1;
            inquirySeen = PlayerPrefs.GetInt(SavePrefix + "Inquiry", 0) == 1;
            miniGameCompleted = PlayerPrefs.GetInt(SavePrefix + "MiniGame", 0) == 1;
            pendingCluePopup = PlayerPrefs.GetInt(SavePrefix + "Popup", 0) == 1;
            allCasesCompleted = PlayerPrefs.GetInt(SavePrefix + "Finished", 0) == 1;
        }

        private static void Save()
        {
            PlayerPrefs.SetInt(SavePrefix + "CaseIndex", currentCaseIndex);
            PlayerPrefs.SetInt(SavePrefix + "Clues", completedClues);
            PlayerPrefs.SetInt(SavePrefix + "LastClue", lastClueIndex);
            PlayerPrefs.SetInt(SavePrefix + "Intro", introSeen ? 1 : 0);
            PlayerPrefs.SetInt(SavePrefix + "Inquiry", inquirySeen ? 1 : 0);
            PlayerPrefs.SetInt(SavePrefix + "MiniGame", miniGameCompleted ? 1 : 0);
            PlayerPrefs.SetInt(SavePrefix + "Popup", pendingCluePopup ? 1 : 0);
            PlayerPrefs.SetInt(SavePrefix + "Finished", allCasesCompleted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void MarkInquirySeen()
        {
            EnsureInitialized();
            inquirySeen = true;
            Save();
        }

        public static void CompleteMiniGame()
        {
            EnsureInitialized();
            if (MiniGameReady)
            {
                miniGameCompleted = true;
                Save();
            }
        }

        public static void MarkIntroSeen()
        {
            EnsureInitialized();
            introSeen = true;
            Save();
        }

        public static void CompleteCurrentSearch()
        {
            EnsureInitialized();

            CaseDefinition currentCase = CurrentCase;
            if (currentCase == null || completedClues >= currentCase.RequiredClues)
                return;

            lastClueIndex = completedClues;
            completedClues++;
            pendingCluePopup = true;
            introSeen = true;
            Save();
        }

        public static bool TryConsumePendingClue(out string clueText)
        {
            EnsureInitialized();
            clueText = null;

            if (!pendingCluePopup)
                return false;

            CaseDefinition currentCase = CurrentCase;
            if (currentCase == null || lastClueIndex < 0 || lastClueIndex >= currentCase.Clues.Length)
            {
                pendingCluePopup = false;
                Save();
                return false;
            }

            clueText = currentCase.Clues[lastClueIndex];
            pendingCluePopup = false;
            Save();
            return true;
        }

        public static string GetCollectedClue(int index)
        {
            CaseDefinition currentCase = CurrentCase;
            if (currentCase == null || index < 0 || index >= completedClues || index >= currentCase.Clues.Length)
                return null;

            return currentCase.Clues[index];
        }

        public static void AdvanceCase()
        {
            EnsureInitialized();

            int nextIndex = currentCaseIndex + 1;
            if (nextIndex >= CaseDatabase.Cases.Count)
            {
                allCasesCompleted = true;
                Save();
                return;
            }

            currentCaseIndex = nextIndex;
            completedClues = 0;
            lastClueIndex = -1;
            introSeen = false;
            inquirySeen = false;
            miniGameCompleted = false;
            pendingCluePopup = false;
            Save();
        }

        public static void ResetAllProgress()
        {
            foreach (string suffix in new[]
            {
                "CaseIndex", "Clues", "LastClue", "Intro",
                "Inquiry", "MiniGame", "Popup", "Finished"
            })
            {
                PlayerPrefs.DeleteKey(SavePrefix + suffix);
            }

            BureauEconomy.ResetEverything();
            PlayerPrefs.Save();
            initialized = false;
            EnsureInitialized();
        }
    }
}
