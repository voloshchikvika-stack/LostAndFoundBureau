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
        public bool ColorClearEnabled { get; }

        public Match3LevelDefinition(
            int moves,
            int targetType,
            int targetCount,
            bool bombsEnabled = false,
            bool planesEnabled = false,
            bool rocketsEnabled = false,
            bool colorClearEnabled = false)
        {
            Moves = moves;
            TargetType = targetType;
            TargetCount = targetCount;
            BombsEnabled = bombsEnabled;
            PlanesEnabled = planesEnabled;
            RocketsEnabled = rocketsEnabled;
            ColorClearEnabled = colorClearEnabled;
        }
    }

    public class CaseDefinition
    {
        public string Id { get; }
        public string ClientName { get; }
        public string LostItemName { get; }
        public string Story { get; }
        public string ShortIntro { get; }
        public string[] Clues { get; }
        public Match3LevelDefinition[] Levels { get; }
        public string[] AnswerOptions { get; }
        public int CorrectAnswerIndex { get; }
        public string CorrectResponse { get; }
        public string WrongResponse { get; }
        public string CorrectAnswerExplanation { get; }
        public string InquiryQuestion { get; }
        public string InquiryAnswer { get; }
        public string[] ClueResponses { get; }
        public int MiniGameAfterClue { get; }
        public string MiniGameTitle { get; }
        public string MiniGameInstruction { get; }
        public string[] MiniGameCards { get; }
        public int[] MiniGameCorrectOrder { get; }
        public string MiniGameResult { get; }
        public string RoomId { get; }
        public string MiniGameMode { get; }

        // Photo analysis requires one valid answer index; reconstruction and
        // chronology puzzles require a full permutation of their fragments.
        public bool HasMiniGame => MiniGameAfterClue > 0 && MiniGameCards != null &&
                                   MiniGameCards.Length > 0 && MiniGameCorrectOrder != null &&
                                   (MiniGameMode == "Spot" || MiniGameMode == "Compare" ||
                                    MiniGameMode == "Focus" || MiniGameMode == "Catalog" ||
                                    MiniGameMode == "CrossCheck" || MiniGameMode == "Repair" ||
                                    MiniGameMode == "Pair"
                                       ? MiniGameCorrectOrder.Length == 1 &&
                                         MiniGameCorrectOrder[0] >= 0 &&
                                         MiniGameCorrectOrder[0] < MiniGameCards.Length
                                       : MiniGameCorrectOrder.Length == MiniGameCards.Length);

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
            string correctAnswerExplanation,
            string inquiryQuestion = null,
            string inquiryAnswer = null,
            string[] clueResponses = null,
            int miniGameAfterClue = 0,
            string miniGameTitle = null,
            string miniGameInstruction = null,
            string[] miniGameCards = null,
            int[] miniGameCorrectOrder = null,
            string miniGameResult = null,
            string shortIntro = null,
            string roomId = null,
            string miniGameMode = "Sequence")
        {
            Id = id;
            ClientName = clientName;
            LostItemName = lostItemName;
            Story = story;
            ShortIntro = string.IsNullOrEmpty(shortIntro) ? story : shortIntro;
            Clues = clues;
            Levels = levels;
            AnswerOptions = answerOptions;
            CorrectAnswerIndex = correctAnswerIndex;
            CorrectResponse = correctResponse;
            WrongResponse = wrongResponse;
            CorrectAnswerExplanation = correctAnswerExplanation;
            InquiryQuestion = inquiryQuestion;
            InquiryAnswer = inquiryAnswer;
            ClueResponses = clueResponses;
            MiniGameAfterClue = miniGameAfterClue;
            MiniGameTitle = miniGameTitle;
            MiniGameInstruction = miniGameInstruction;
            MiniGameCards = miniGameCards;
            MiniGameCorrectOrder = miniGameCorrectOrder;
            MiniGameResult = miniGameResult;
            RoomId = roomId;
            MiniGameMode = miniGameMode;
        }
    }
}
