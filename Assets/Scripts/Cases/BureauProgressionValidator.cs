#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LostAndFound.Cases
{
    // Catches invalid generated cases directly in Unity's Console, including
    // inaccessible answers and unsolvable room puzzles.
    [InitializeOnLoad]
    internal static class BureauProgressionValidator
    {
        static BureauProgressionValidator()
        {
            EditorApplication.delayCall += Validate;
        }

        private static void Validate()
        {
            try
            {
                IReadOnlyList<CaseDefinition> cases = CaseDatabase.Cases;
                if (cases.Count != CaseDatabase.TotalLevels)
                    Debug.LogError(
                        $"Bureau case data: expected {CaseDatabase.TotalLevels} cases, got {cases.Count}.");

                HashSet<string> ids = new HashSet<string>();

                for (int i = 0; i < cases.Count; i++)
                {
                    CaseDefinition entry = cases[i];
                    string prefix = $"Case #{i + 1}";

                    if (entry == null)
                    {
                        Debug.LogError(prefix + " is missing.");
                        continue;
                    }

                    if (!ids.Add(entry.Id))
                        Debug.LogError(prefix + ": duplicate identifier " + entry.Id);

                    if (entry.Levels == null || entry.Clues == null ||
                        entry.Clues.Length < 1 ||
                        entry.Levels.Length != entry.Clues.Length)
                        Debug.LogError(prefix + ": one clue per Match-3 level is required.");

                    if (entry.AnswerOptions == null ||
                        entry.AnswerOptions.Length != 4 ||
                        entry.CorrectAnswerIndex < 0 ||
                        entry.CorrectAnswerIndex >= entry.AnswerOptions.Length)
                        Debug.LogError(prefix + ": invalid four-choice answer.");

                    if (entry.RoomId == null)
                        continue;

                    if (entry.RoomId != "PhotoLab" &&
                        entry.RoomId != "ArchiveRoom" &&
                        entry.RoomId != "Workshop")
                        Debug.LogError(prefix + ": unknown room " + entry.RoomId);

                    if (!entry.HasMiniGame || entry.MiniGameAfterClue < 1 ||
                        entry.MiniGameAfterClue > entry.RequiredClues)
                    {
                        Debug.LogError(prefix + ": incomplete room-puzzle data.");
                        continue;
                    }

                    HashSet<int> indices = new HashSet<int>();
                    foreach (int index in entry.MiniGameCorrectOrder)
                    {
                        if (index < 0 || index >= entry.MiniGameCards.Length ||
                            !indices.Add(index))
                            Debug.LogError(prefix + ": invalid or duplicate puzzle card index.");
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("Bureau case database failed validation: " + exception);
            }
        }
    }
}
#endif
