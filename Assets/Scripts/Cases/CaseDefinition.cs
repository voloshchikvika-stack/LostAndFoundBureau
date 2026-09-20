namespace LostAndFound.Cases
{
    public class Match3LevelDefinition
    {
        public int Moves { get; }
        public int TargetType { get; }
        public int TargetCount { get; }
        public bool BombsEnabled { get; }
        public bool PlanesEnabled { get; }
        public bool RocketsEnabled { get; }

        public Match3LevelDefinition(
            int moves,
            int targetType,
            int targetCount,
            bool bombsEnabled = false,
            bool planesEnabled = false,
            bool rocketsEnabled = false)
        {
            Moves = moves;
            TargetType = targetType;
            TargetCount = targetCount;
            BombsEnabled = bombsEnabled;
            PlanesEnabled = planesEnabled;
            RocketsEnabled = rocketsEnabled;
        }
    }

    public class CaseDefinition
    {
        public string Id { get; }
        public string ClientName { get; }
        public string LostItemName { get; }
        public string Story { get; }
        public string[] Clues { get; }
        public Match3LevelDefinition[] Levels { get; }
        public string[] AnswerOptions { get; }
        public int CorrectAnswerIndex { get; }
        public string CorrectResponse { get; }
        public string WrongResponse { get; }
        public string CorrectAnswerExplanation { get; }

        public int RequiredClues => Clues == null ? 0 : Clues.Length;

        public CaseDefinition(
            string id,
            string clientName,
            string lostItemName,
            string story,
            string[] clues,
            Match3LevelDefinition[] levels,
            string[] answerOptions,
            int correctAnswerIndex,
            string correctResponse,
            string wrongResponse,
            string correctAnswerExplanation)
        {
            Id = id;
            ClientName = clientName;
            LostItemName = lostItemName;
            Story = story;
            Clues = clues;
            Levels = levels;
            AnswerOptions = answerOptions;
            CorrectAnswerIndex = correctAnswerIndex;
            CorrectResponse = correctResponse;
            WrongResponse = wrongResponse;
            CorrectAnswerExplanation = correctAnswerExplanation;
        }
    }
}
