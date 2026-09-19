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
        private Sprite pieceSprite;
        private Camera mainCamera;
        private bool inputLocked;
        private bool gameEnded;
        private bool levelWon;
        private int score;
        private int moves;
        private int collectedTarget;

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
            pieceSprite = CreateSquareSprite();

            targetType = Mathf.Clamp(targetType, 0, palette.Length - 1);
            ConfigureCamera();
            CreateBoardWithoutStartingMatches();
        }

        private void Update()
        {
            if (inputLocked || gameEnded)
                return;

            if (TryGetPointerDown(out Vector2 screenPosition))
                HandlePointer(screenPosition);
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
            piece.Initialize(type, column, row, pieceSprite, palette[type], cellSize * 0.86f);

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
            mainCamera.orthographicSize = height * cellSize * 0.65f;
            mainCamera.backgroundColor = new Color(0.08f, 0.10f, 0.16f);
        }

        private Sprite CreateSquareSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.name = "Match3RuntimeSprite";
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
        }

        private void OnGUI()
        {
            float uiScale = Mathf.Max(1f, Screen.height / 1080f);

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(26 * uiScale),
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            GUIStyle infoStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(18 * uiScale),
                normal = { textColor = new Color(0.9f, 0.92f, 0.96f) }
            };

            GUIStyle resultStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(34 * uiScale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            float x = 20 * uiScale;
            float y = 18 * uiScale;
            float line = 34 * uiScale;

            GUI.Label(new Rect(x, y, 700 * uiScale, 48 * uiScale),
                "Бюро потерянных вещей — Match-3", titleStyle);

            GUI.Label(new Rect(x, y + line * 1.4f, 350 * uiScale, 32 * uiScale),
                $"Счёт: {score}", infoStyle);

            GUI.Label(new Rect(x, y + line * 2.3f, 350 * uiScale, 32 * uiScale),
                $"Ходы: {MovesLeft}/{maxMoves}", infoStyle);

            Color oldColor = GUI.color;
            GUI.color = palette[targetType];
            GUI.Box(new Rect(x, y + line * 3.4f, 28 * uiScale, 28 * uiScale), GUIContent.none);
            GUI.color = oldColor;

            GUI.Label(new Rect(x + 38 * uiScale, y + line * 3.25f, 500 * uiScale, 34 * uiScale),
                $"Цель: собрать {targetCount} красных — {Mathf.Min(collectedTarget, targetCount)}/{targetCount}",
                infoStyle);

            GUI.Label(new Rect(x, Screen.height - 54 * uiScale, 820 * uiScale, 34 * uiScale),
                "Нажми на две соседние фишки. Совпадения 3+ исчезают.", infoStyle);

            if (!gameEnded)
                return;

            string message = levelWon
                ? "ДЕЛО ПРОДВИНУЛОСЬ! УЛИКА ПОЛУЧЕНА"
                : "ХОДЫ ЗАКОНЧИЛИСЬ";

            GUI.Box(new Rect(
                Screen.width * 0.5f - 360 * uiScale,
                Screen.height * 0.5f - 70 * uiScale,
                720 * uiScale,
                140 * uiScale), GUIContent.none);

            GUI.Label(new Rect(
                Screen.width * 0.5f - 340 * uiScale,
                Screen.height * 0.5f - 52 * uiScale,
                680 * uiScale,
                104 * uiScale), message, resultStyle);
        }
    }
}
