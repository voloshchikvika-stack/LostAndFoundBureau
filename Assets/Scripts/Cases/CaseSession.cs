using UnityEngine;

namespace LostAndFound.Cases
{
    public static class CaseSession
    {
        private static bool initialized;
        private static int currentCaseIndex;
        private static int completedClues;
        private static int lastClueIndex = -1;
        private static bool introSeen;
        private static bool pendingCluePopup;
        private static bool allCasesCompleted;

        public static int CurrentCaseIndex => currentCaseIndex;
        public static int CompletedClues => completedClues;
        public static bool IntroSeen => introSeen;
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
            currentCaseIndex = 0;
            completedClues = 0;
            lastClueIndex = -1;
            introSeen = false;
            pendingCluePopup = false;
            allCasesCompleted = false;
        }

        public static void MarkIntroSeen()
        {
            EnsureInitialized();
            introSeen = true;
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
                return false;
            }

            clueText = currentCase.Clues[lastClueIndex];
            pendingCluePopup = false;
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
                return;
            }

            currentCaseIndex = nextIndex;
            completedClues = 0;
            lastClueIndex = -1;
            introSeen = false;
            pendingCluePopup = false;
        }

        public static void ResetAllProgress()
        {
            initialized = false;
            EnsureInitialized();
        }
    }
}
