using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LostAndFound.Match3
{
    public class Match3Board : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField] private int width = 8;
        [SerializeField] private int height = 8;
        [SerializeField] private float cellSize = 1.05f;

        [Header("Level Goal")]
        [SerializeField] private int maxMoves = 20;
        [SerializeField] private int targetType = 0;
        [SerializeField] private int targetCount = 15;

        [Header("Timing")]
        [SerializeField] private float swapDuration = 0.16f;
        [SerializeField] private float fallDuration = 0.18f;
        [SerializeField] private float clearDelay = 0.12f;

        private Match3Piece[,] pieces;
        private Match3Piece selectedPiece;
        private Sprite[] pieceSprites;
        private Texture2D[] pieceTextures;
        private Camera mainCamera;
        private bool inputLocked;
        private bool gameEnded;
        private bool levelWon;
        private int score;
        private int moves;
        private int collectedTarget;

        private Texture2D hudCardTexture;
        private Texture2D hudCardStrongTexture;
        private Texture2D overlayTexture;
        private Texture2D resultCardTexture;
        private Texture2D boardPanelTexture;
        private GameObject runtimeBackground;
        private GameObject runtimeBoardPanel;

        private readonly string[] pieceNames =
        {
            "Следы",
            "Ключ",
            "Карта",
            "Записка",
            "Бирка",
            "Лупа"
        };

        private readonly Color[] palette =
        {
            new Color(0.95f, 0.35f, 0.42f),
            new Color(0.31f, 0.66f, 0.96f),
            new Color(0.42f, 0.82f, 0.53f),
            new Color(0.98f, 0.73f, 0.28f),
            new Color(0.67f, 0.46f, 0.93f),
            new Color(0.96f, 0.49f, 0.77f)
        };

        private float StartX => -(width - 1) * cellSize * 0.5f;
        private float StartY => -(height - 1) * cellSize * 0.5f;
        private int MovesLeft => Mathf.Max(0, maxMoves - moves);

        private void Start()
        {
            pieces = new Match3Piece[width, height];
            targetType = Mathf.Clamp(targetType, 0, palette.Length - 1);

            DisablePrototypeCanvas();
            ConfigureCamera();
            CreateRuntimeVisuals();
            CreateBoardWithoutStartingMatches();
        }

        private void Update()
        {
            if (inputLocked || gameEnded)
                return;

            if (TryGetPointerDown(out Vector2 screenPosition))
                HandlePointer(screenPosition);
        }

        private void DisablePrototypeCanvas()
        {
            GameObject prototypeCanvas = GameObject.Find("Canvas");
            if (prototypeCanvas != null)
                prototypeCanvas.SetActive(false);
        }

        private void CreateRuntimeVisuals()
        {
            pieceSprites = new Sprite[palette.Length];
            pieceTextures = new Texture2D[palette.Length];

            for (int i = 0; i < palette.Length; i++)
                pieceSprites[i] = Match3VisualFactory.CreateTokenSprite(palette[i], i, out pieceTextures[i]);

            hudCardTexture = Match3VisualFactory.CreateRoundedTexture(
                new Color(0.10f, 0.15f, 0.22f, 0.94f), 64, 16);

            hudCardStrongTexture = Match3VisualFactory.CreateRoundedTexture(
                new Color(0.16f, 0.23f, 0.32f, 0.98f), 64, 16);

            resultCardTexture = Match3VisualFactory.CreateRoundedTexture(
                new Color(0.10f, 0.15f, 0.22f, 0.98f), 96, 24);

            overlayTexture = Match3VisualFactory.CreateSolidTexture(
                new Color(0.01f, 0.02f, 0.035f, 0.68f));

            boardPanelTexture = Match3VisualFactory.CreateSolidTexture(
                new Color(0.03f, 0.05f, 0.08f, 0.52f));

            CreateBackground();
            CreateBoardPanel();
        }

        private void CreateBackground()
        {
            runtimeBackground = new GameObject("RuntimeBackground");
            SpriteRenderer renderer = runtimeBackground.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = -100;

            Sprite customBackground = Resources.Load<Sprite>("Match3/Background");
            Sprite backgroundSprite;

            if (customBackground != null)
            {
                backgroundSprite = customBackground;
            }
            else
            {
                backgroundSprite = Match3VisualFactory.CreateGradientBackgroundSprite(out _);
            }

            renderer.sprite = backgroundSprite;
            runtimeBackground.transform.position = new Vector3(0f, 0f, 5f);

            float visibleHeight = mainCamera.orthographicSize * 2f;
            float visibleWidth = visibleHeight * mainCamera.aspect;

            Vector2 spriteSize = backgroundSprite.bounds.size;
            float scale = Mathf.Max(
                visibleWidth / Mathf.Max(0.01f, spriteSize.x),
                visibleHeight / Mathf.Max(0.01f, spriteSize.y));

            runtimeBackground.transform.localScale = Vector3.one * scale;
        }

        private void CreateBoardPanel()
        {
            runtimeBoardPanel = new GameObject("BoardPanel");
            SpriteRenderer renderer = runtimeBoardPanel.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = -10;

            Sprite panelSprite = Sprite.Create(
                boardPanelTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);

            renderer.sprite = panelSprite;
            runtimeBoardPanel.transform.position = new Vector3(0f, 0f, 1f);
            runtimeBoardPanel.transform.localScale = new Vector3(
                width * cellSize + 0.7f,
                height * cellSize + 0.7f,
                1f);
        }

        private bool TryGetPointerDown(out Vector2 screenPosition)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }

        private void HandlePointer(Vector2 screenPosition)
        {
            if (mainCamera == null)
                return;

            Vector3 world = mainCamera.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, -mainCamera.transform.position.z));

            int column = Mathf.RoundToInt((world.x - StartX) / cellSize);
            int row = Mathf.RoundToInt((world.y - StartY) / cellSize);

            if (!IsInside(column, row))
                return;

            Match3Piece clicked = pieces[column, row];
            if (clicked == null)
                return;

            if (selectedPiece == null)
            {
                SelectPiece(clicked);
                return;
            }

            if (clicked == selectedPiece)
            {
                DeselectCurrent();
                return;
            }

            if (AreAdjacent(selectedPiece, clicked))
            {
                StartCoroutine(TrySwap(selectedPiece, clicked));
                return;
            }

            SelectPiece(clicked);
        }

        private void SelectPiece(Match3Piece piece)
        {
            if (selectedPiece != null)
                selectedPiece.SetSelected(false);

            selectedPiece = piece;
            selectedPiece.SetSelected(true);
        }

        private void DeselectCurrent()
        {
            if (selectedPiece != null)
                selectedPiece.SetSelected(false);

            selectedPiece = null;
        }

        private bool AreAdjacent(Match3Piece a, Match3Piece b)
        {
            int distance = Mathf.Abs(a.Column - b.Column) + Mathf.Abs(a.Row - b.Row);
            return distance == 1;
        }

        private IEnumerator TrySwap(Match3Piece first, Match3Piece second)
        {
            inputLocked = true;
            DeselectCurrent();

            yield return SwapAndAnimate(first, second, swapDuration);

            HashSet<Match3Piece> matches = FindAllMatches();

            if (matches.Count == 0)
            {
                yield return SwapAndAnimate(first, second, swapDuration);
                inputLocked = false;
                yield break;
            }

            moves++;
            yield return ResolveBoard(matches);
            CheckEndConditions();
            inputLocked = false;
        }

        private IEnumerator ResolveBoard(HashSet<Match3Piece> matches)
        {
            while (matches.Count > 0)
            {
                ClearMatches(matches);
                yield return new WaitForSeconds(clearDelay);

                yield return CollapseColumns();
                yield return RefillBoard();

                matches = FindAllMatches();
            }
        }

        private IEnumerator SwapAndAnimate(Match3Piece a, Match3Piece b, float duration)
        {
            int aColumn = a.Column;
            int aRow = a.Row;
            int bColumn = b.Column;
            int bRow = b.Row;

            pieces[aColumn, aRow] = b;
            pieces[bColumn, bRow] = a;

            a.SetCoordinates(bColumn, bRow);
            b.SetCoordinates(aColumn, aRow);

            StartCoroutine(a.MoveTo(WorldPosition(a.Column, a.Row), duration));
            StartCoroutine(b.MoveTo(WorldPosition(b.Column, b.Row), duration));

            yield return new WaitForSeconds(duration);
        }

        private void ClearMatches(HashSet<Match3Piece> matches)
        {
            foreach (Match3Piece piece in matches)
            {
                if (piece == null)
                    continue;

                int column = piece.Column;
                int row = piece.Row;

                if (IsInside(column, row) && pieces[column, row] == piece)
                    pieces[column, row] = null;

                score += 100;

                if (piece.Type == targetType)
                    collectedTarget++;

                Destroy(piece.gameObject);
            }
        }

        private void CheckEndConditions()
        {
            if (collectedTarget >= targetCount)
            {
                levelWon = true;
                gameEnded = true;
                return;
            }

            if (moves >= maxMoves)
            {
                levelWon = false;
                gameEnded = true;
            }
        }

        private IEnumerator CollapseColumns()
        {
            bool movedAny = false;

            for (int column = 0; column < width; column++)
            {
                int targetRow = 0;

                for (int row = 0; row < height; row++)
                {
                    Match3Piece piece = pieces[column, row];
                    if (piece == null)
                        continue;

                    if (row != targetRow)
                    {
                        pieces[column, targetRow] = piece;
                        pieces[column, row] = null;

                        piece.SetCoordinates(column, targetRow);
                        StartCoroutine(piece.MoveTo(WorldPosition(column, targetRow), fallDuration));
                        movedAny = true;
                    }

                    targetRow++;
                }
            }

            if (movedAny)
                yield return new WaitForSeconds(fallDuration);
        }

        private IEnumerator RefillBoard()
        {
            bool createdAny = false;

            for (int column = 0; column < width; column++)
            {
                int missing = 0;

                for (int row = 0; row < height; row++)
                {
                    if (pieces[column, row] != null)
                        continue;

                    int type = Random.Range(0, palette.Length);
                    Vector3 target = WorldPosition(column, row);
                    Vector3 spawn = target + Vector3.up * cellSize * (height + missing + 1);

                    Match3Piece piece = CreatePiece(type, column, row, spawn);
                    pieces[column, row] = piece;

                    StartCoroutine(piece.MoveTo(target, fallDuration));
                    missing++;
                    createdAny = true;
                }
            }

            if (createdAny)
                yield return new WaitForSeconds(fallDuration);
        }

        private HashSet<Match3Piece> FindAllMatches()
        {
            HashSet<Match3Piece> result = new HashSet<Match3Piece>();

            for (int row = 0; row < height; row++)
            {
                int runStart = 0;

                while (runStart < width)
                {
                    Match3Piece startPiece = pieces[runStart, row];
                    if (startPiece == null)
                    {
                        runStart++;
                        continue;
                    }

                    int runEnd = runStart + 1;
                    while (runEnd < width &&
                           pieces[runEnd, row] != null &&
                           pieces[runEnd, row].Type == startPiece.Type)
                    {
                        runEnd++;
                    }

                    if (runEnd - runStart >= 3)
                    {
                        for (int column = runStart; column < runEnd; column++)
                            result.Add(pieces[column, row]);
                    }

                    runStart = runEnd;
                }
            }

            for (int column = 0; column < width; column++)
            {
                int runStart = 0;

                while (runStart < height)
                {
                    Match3Piece startPiece = pieces[column, runStart];
                    if (startPiece == null)
                    {
                        runStart++;
                        continue;
                    }

                    int runEnd = runStart + 1;
                    while (runEnd < height &&
                           pieces[column, runEnd] != null &&
                           pieces[column, runEnd].Type == startPiece.Type)
                    {
                        runEnd++;
                    }

                    if (runEnd - runStart >= 3)
                    {
                        for (int row = runStart; row < runEnd; row++)
                            result.Add(pieces[column, row]);
                    }

                    runStart = runEnd;
                }
            }

            return result;
        }

        private void CreateBoardWithoutStartingMatches()
        {
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    int type = ChooseTypeWithoutImmediateMatch(column, row);
                    Match3Piece piece = CreatePiece(type, column, row, WorldPosition(column, row));
                    pieces[column, row] = piece;
                }
            }
        }

        private int ChooseTypeWithoutImmediateMatch(int column, int row)
        {
            List<int> candidates = new List<int>();

            for (int type = 0; type < palette.Length; type++)
            {
                bool horizontalMatch =
                    column >= 2 &&
                    pieces[column - 1, row] != null &&
                    pieces[column - 2, row] != null &&
                    pieces[column - 1, row].Type == type &&
                    pieces[column - 2, row].Type == type;

                bool verticalMatch =
                    row >= 2 &&
                    pieces[column, row - 1] != null &&
                    pieces[column, row - 2] != null &&
                    pieces[column, row - 1].Type == type &&
                    pieces[column, row - 2].Type == type;

                if (!horizontalMatch && !verticalMatch)
                    candidates.Add(type);
            }

            return candidates[Random.Range(0, candidates.Count)];
        }

        private Match3Piece CreatePiece(int type, int column, int row, Vector3 position)
        {
            GameObject pieceObject = new GameObject($"Piece_{column}_{row}");
            pieceObject.transform.SetParent(transform);
            pieceObject.transform.position = position;

            Match3Piece piece = pieceObject.AddComponent<Match3Piece>();
            piece.Initialize(type, column, row, pieceSprites[type], Color.white, cellSize * 0.86f);

            return piece;
        }

        private Vector3 WorldPosition(int column, int row)
        {
            return new Vector3(
                StartX + column * cellSize,
                StartY + row * cellSize,
                0f);
        }

        private bool IsInside(int column, int row)
        {
            return column >= 0 && column < width && row >= 0 && row < height;
        }

        private void ConfigureCamera()
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            mainCamera.orthographic = true;
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);
            mainCamera.orthographicSize = height * cellSize * 0.66f;
            mainCamera.backgroundColor = new Color(0.035f, 0.055f, 0.085f);
        }

        private void OnGUI()
        {
            if (hudCardTexture == null || pieceTextures == null)
                return;

            float scale = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
            scale = Mathf.Clamp(scale, 0.65f, 2.5f);

            GUIStyle titleStyle = CreateLabelStyle(17, FontStyle.Normal, TextAnchor.UpperCenter, scale);
            titleStyle.normal.textColor = new Color(0.75f, 0.80f, 0.88f);

            GUIStyle valueStyle = CreateLabelStyle(34, FontStyle.Bold, TextAnchor.MiddleCenter, scale);
            GUIStyle goalValueStyle = CreateLabelStyle(26, FontStyle.Bold, TextAnchor.MiddleLeft, scale);
            GUIStyle goalTitleStyle = CreateLabelStyle(15, FontStyle.Normal, TextAnchor.MiddleLeft, scale);
            goalTitleStyle.normal.textColor = new Color(0.75f, 0.80f, 0.88f);

            float cardHeight = 92f * scale;
            float movesWidth = 190f * scale;
            float goalWidth = 330f * scale;
            float scoreWidth = 190f * scale;
            float gap = 16f * scale;
            float totalWidth = movesWidth + goalWidth + scoreWidth + gap * 2f;
            float startX = (Screen.width - totalWidth) * 0.5f;
            float y = 22f * scale;

            Rect movesRect = new Rect(startX, y, movesWidth, cardHeight);
            Rect goalRect = new Rect(startX + movesWidth + gap, y, goalWidth, cardHeight);
            Rect scoreRect = new Rect(startX + movesWidth + gap + goalWidth + gap, y, scoreWidth, cardHeight);

            DrawCard(movesRect, hudCardTexture);
            DrawCard(goalRect, hudCardStrongTexture);
            DrawCard(scoreRect, hudCardTexture);

            GUI.Label(new Rect(movesRect.x, movesRect.y + 7f * scale, movesRect.width, 22f * scale), "ХОДЫ", titleStyle);
            GUI.Label(new Rect(movesRect.x, movesRect.y + 26f * scale, movesRect.width, 55f * scale), MovesLeft.ToString(), valueStyle);

            float iconSize = 58f * scale;
            Rect iconRect = new Rect(goalRect.x + 18f * scale, goalRect.y + 17f * scale, iconSize, iconSize);
            GUI.DrawTexture(iconRect, pieceTextures[targetType], ScaleMode.ScaleToFit, true);

            GUI.Label(
                new Rect(goalRect.x + 88f * scale, goalRect.y + 16f * scale, goalRect.width - 102f * scale, 25f * scale),
                $"ЦЕЛЬ · {pieceNames[targetType]}",
                goalTitleStyle);

            GUI.Label(
                new Rect(goalRect.x + 88f * scale, goalRect.y + 36f * scale, goalRect.width - 102f * scale, 42f * scale),
                $"{Mathf.Min(collectedTarget, targetCount)} / {targetCount}",
                goalValueStyle);

            GUI.Label(new Rect(scoreRect.x, scoreRect.y + 7f * scale, scoreRect.width, 22f * scale), "СЧЁТ", titleStyle);
            GUI.Label(new Rect(scoreRect.x, scoreRect.y + 26f * scale, scoreRect.width, 55f * scale), score.ToString(), valueStyle);

            if (gameEnded)
                DrawResultOverlay(scale);
        }

        private GUIStyle CreateLabelStyle(int fontSize, FontStyle fontStyle, TextAnchor alignment, float scale)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(fontSize * scale),
                fontStyle = fontStyle,
                alignment = alignment
            };

            style.normal.textColor = Color.white;
            return style;
        }

        private void DrawCard(Rect rect, Texture2D texture)
        {
            GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
        }

        private void DrawResultOverlay(float scale)
        {
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), overlayTexture, ScaleMode.StretchToFill);

            float width = 600f * scale;
            float height = 250f * scale;
            Rect card = new Rect(
                Screen.width * 0.5f - width * 0.5f,
                Screen.height * 0.5f - height * 0.5f,
                width,
                height);

            DrawCard(card, resultCardTexture);

            GUIStyle badgeStyle = CreateLabelStyle(17, FontStyle.Bold, TextAnchor.MiddleCenter, scale);
            badgeStyle.normal.textColor = levelWon
                ? new Color(0.55f, 0.94f, 0.68f)
                : new Color(1f, 0.66f, 0.55f);

            GUIStyle headingStyle = CreateLabelStyle(34, FontStyle.Bold, TextAnchor.MiddleCenter, scale);
            GUIStyle bodyStyle = CreateLabelStyle(19, FontStyle.Normal, TextAnchor.MiddleCenter, scale);
            bodyStyle.normal.textColor = new Color(0.78f, 0.82f, 0.88f);

            string badge = levelWon ? "ДЕЛО ПРОДВИНУЛОСЬ" : "ПОПРОБУЕМ ЕЩЁ РАЗ";
            string heading = levelWon ? "Улика получена" : "Ходы закончились";
            string body = levelWon
                ? "Следы привели нас к новой зацепке."
                : "Нужно собрать цель до окончания ходов.";

            GUI.Label(new Rect(card.x, card.y + 30f * scale, card.width, 28f * scale), badge, badgeStyle);
            GUI.Label(new Rect(card.x + 20f * scale, card.y + 72f * scale, card.width - 40f * scale, 55f * scale), heading, headingStyle);
            GUI.Label(new Rect(card.x + 36f * scale, card.y + 142f * scale, card.width - 72f * scale, 54f * scale), body, bodyStyle);
        }
    }
}
