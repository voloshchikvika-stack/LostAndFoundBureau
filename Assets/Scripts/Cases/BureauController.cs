using UnityEngine;
using UnityEngine.SceneManagement;

namespace LostAndFound.Cases
{
    public class BureauController : MonoBehaviour
    {
        private enum BureauMode
        {
            ClientIntro,
            Desk,
            CaseFile,
            Answers,
            Finished
        }

        private BureauMode mode;
        private CaseDefinition currentCase;

        private Texture2D officeBackground;
        private Texture2D clientImage;
        private Texture2D lostItemImage;
        private Texture2D caseFolderImage;

        private Texture2D wallTexture;
        private Texture2D wallDarkTexture;
        private Texture2D deskTexture;
        private Texture2D deskEdgeTexture;
        private Texture2D panelTexture;
        private Texture2D panelStrongTexture;
        private Texture2D speechBubbleTexture;
        private Texture2D speechPointerTexture;
        private Texture2D buttonTexture;
        private Texture2D buttonHoverTexture;
        private Texture2D folderTexture;
        private Texture2D folderPaperTexture;
        private Texture2D overlayTexture;
        private Texture2D goodTexture;
        private Texture2D badTexture;
        private Texture2D accentTexture;
        private Texture2D corkTexture;
        private Texture2D paperTexture;

        private bool cluePopupVisible;
        private float cluePopupUntil;

        private bool reactionVisible;
        private bool reactionWasCorrect;
        private int reactionStage;
        private float reactionUntil;
        private string reactionText;

        private void Start()
        {
            CaseSession.EnsureInitialized();
            BuildUiTextures();

            if (CaseSession.AllCasesCompleted)
            {
                mode = BureauMode.Finished;
                return;
            }

            LoadCurrentCaseAssets();

            if (CaseSession.TryConsumePendingClue(out _))
            {
                cluePopupVisible = true;
                cluePopupUntil = Time.realtimeSinceStartup + 2.0f;
                mode = BureauMode.Desk;
            }
            else
            {
                mode = CaseSession.IntroSeen ? BureauMode.Desk : BureauMode.ClientIntro;
            }
        }

        private void Update()
        {
            float now = Time.realtimeSinceStartup;

            if (cluePopupVisible && now >= cluePopupUntil)
                cluePopupVisible = false;

            if (!reactionVisible || now < reactionUntil)
                return;

            if (!reactionWasCorrect && reactionStage == 0)
            {
                reactionStage = 1;
                reactionText = currentCase.CorrectAnswerExplanation;
                reactionUntil = now + 3.5f;
                return;
            }

            reactionVisible = false;
            CaseSession.AdvanceCase();

            if (CaseSession.AllCasesCompleted)
            {
                currentCase = null;
                mode = BureauMode.Finished;
                return;
            }

            LoadCurrentCaseAssets();
            mode = BureauMode.ClientIntro;
        }

        private void LoadCurrentCaseAssets()
        {
            currentCase = CaseSession.CurrentCase;

            officeBackground = Resources.Load<Texture2D>("Bureau/Background");
            caseFolderImage = Resources.Load<Texture2D>("Bureau/CaseFolder");

            if (currentCase == null)
            {
                clientImage = null;
                lostItemImage = null;
                return;
            }

            clientImage = Resources.Load<Texture2D>($"Cases/{currentCase.Id}/Client");
            lostItemImage = Resources.Load<Texture2D>($"Cases/{currentCase.Id}/LostItem");
        }

        private void BuildUiTextures()
        {
            wallTexture = MakeTexture(new Color(0.46f, 0.52f, 0.47f, 1f));
            wallDarkTexture = MakeTexture(new Color(0.26f, 0.31f, 0.29f, 1f));
            deskTexture = MakeTexture(new Color(0.46f, 0.25f, 0.13f, 1f));
            deskEdgeTexture = MakeTexture(new Color(0.30f, 0.15f, 0.08f, 1f));
            panelTexture = MakeRoundedTexture(new Color(0.09f, 0.11f, 0.13f, 0.94f), 22);
            panelStrongTexture = MakeRoundedTexture(new Color(0.07f, 0.08f, 0.10f, 0.985f), 26);
            speechBubbleTexture = MakeRoundedTexture(new Color(0.965f, 0.925f, 0.82f, 1f), 30);
            speechPointerTexture = MakeTriangleTexture(new Color(0.965f, 0.925f, 0.82f, 1f));
            buttonTexture = MakeRoundedTexture(new Color(0.31f, 0.51f, 0.35f, 1f), 24);
            buttonHoverTexture = MakeRoundedTexture(new Color(0.38f, 0.61f, 0.41f, 1f), 24);
            folderTexture = MakeRoundedTexture(new Color(0.73f, 0.49f, 0.22f, 1f), 18);
            folderPaperTexture = MakeRoundedTexture(new Color(0.92f, 0.82f, 0.61f, 1f), 20);
            overlayTexture = MakeTexture(new Color(0.01f, 0.015f, 0.02f, 0.66f));
            goodTexture = MakeRoundedTexture(new Color(0.18f, 0.47f, 0.31f, 0.98f), 26);
            badTexture = MakeRoundedTexture(new Color(0.50f, 0.22f, 0.18f, 0.98f), 26);
            accentTexture = MakeTexture(new Color(0.77f, 0.64f, 0.36f, 1f));
            corkTexture = MakeTexture(new Color(0.58f, 0.38f, 0.22f, 1f));
            paperTexture = MakeTexture(new Color(0.92f, 0.88f, 0.76f, 1f));
        }

        private Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private Texture2D MakeRoundedTexture(Color color, int radius)
        {
            const int size = 96;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            float r = Mathf.Clamp(radius, 1, size / 2 - 1);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = 0f;
                    float dy = 0f;

                    if (x < r) dx = r - x;
                    else if (x > size - 1 - r) dx = x - (size - 1 - r);

                    if (y < r) dy = r - y;
                    else if (y > size - 1 - r) dy = y - (size - 1 - r);

                    bool inside = dx * dx + dy * dy <= r * r;
                    texture.SetPixel(x, y, inside ? color : new Color(0f, 0f, 0f, 0f));
                }
            }

            texture.Apply();
            return texture;
        }

        private Texture2D MakeTriangleTexture(Color color)
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                float halfWidth = (1f - (float)y / (size - 1)) * (size * 0.5f);
                float center = (size - 1) * 0.5f;

                for (int x = 0; x < size; x++)
                {
                    bool inside = Mathf.Abs(x - center) <= halfWidth;
                    texture.SetPixel(x, y, inside ? color : new Color(0f, 0f, 0f, 0f));
                }
            }

            texture.Apply();
            return texture;
        }

        private void OnGUI()
        {
            float scale = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
            scale = Mathf.Clamp(scale, 0.65f, 2.5f);

            DrawOffice(scale);

            if (mode == BureauMode.Finished)
            {
                DrawFinished(scale);
                return;
            }

            DrawClient(scale);

            switch (mode)
            {
                case BureauMode.ClientIntro:
                    DrawClientIntro(scale);
                    break;
                case BureauMode.Desk:
                    DrawDeskMode(scale);
                    break;
                case BureauMode.CaseFile:
                    DrawCaseFile(scale);
                    break;
                case BureauMode.Answers:
                    DrawAnswers(scale);
                    break;
            }

            if (cluePopupVisible)
                DrawClueReceivedPopup(scale);

            if (reactionVisible)
                DrawReaction(scale);
        }

        private Rect R(float x, float y, float w, float h, float scale)
        {
            return new Rect(x * scale, y * scale, w * scale, h * scale);
        }

        private GUIStyle LabelStyle(int size, FontStyle fontStyle, TextAnchor alignment, float scale, Color? color = null)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(size * scale),
                fontStyle = fontStyle,
                alignment = alignment,
                wordWrap = true,
                padding = new RectOffset(0, 0, 0, 0)
            };

            style.normal.textColor = color ?? Color.white;
            return style;
        }

        private GUIStyle ButtonStyle(int size, float scale)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(size * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                padding = new RectOffset(16, 16, 10, 10)
            };

            style.normal.textColor = Color.white;
            style.hover.textColor = Color.white;
            style.active.textColor = Color.white;
            style.normal.background = buttonTexture;
            style.hover.background = buttonHoverTexture;
            style.active.background = buttonHoverTexture;
            return style;
        }

        private void DrawOffice(float scale)
        {
            if (officeBackground != null)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), officeBackground, ScaleMode.ScaleAndCrop);
            }
            else
            {
                DrawFallbackDetectiveOffice(scale);
            }

            GUI.DrawTexture(R(0, 795, 1920, 285, scale), deskTexture);
            GUI.DrawTexture(R(0, 790, 1920, 18, scale), deskEdgeTexture);

            GUIStyle signStyle = LabelStyle(18, FontStyle.Bold, TextAnchor.MiddleCenter, scale, new Color(0.93f, 0.87f, 0.72f));
            GUI.DrawTexture(R(700, 24, 520, 50, scale), panelTexture);
            GUI.Label(R(720, 29, 480, 39, scale), "БЮРО ПОТЕРЯННЫХ ВЕЩЕЙ", signStyle);
        }

        private void DrawFallbackDetectiveOffice(float scale)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), wallTexture, ScaleMode.StretchToFill);

            GUI.DrawTexture(R(0, 0, 1920, 90, scale), wallDarkTexture);
            GUI.DrawTexture(R(80, 140, 410, 270, scale), corkTexture);
            GUI.DrawTexture(R(105, 165, 360, 220, scale), wallDarkTexture);

            GUIStyle boardTitle = LabelStyle(17, FontStyle.Bold, TextAnchor.UpperCenter, scale, new Color(0.93f, 0.87f, 0.72f));
            GUI.Label(R(145, 176, 280, 30, scale), "АКТИВНЫЕ ДЕЛА", boardTitle);

            GUI.DrawTexture(R(125, 225, 115, 90, scale), paperTexture);
            GUI.DrawTexture(R(275, 220, 145, 110, scale), paperTexture);
            GUI.DrawTexture(R(205, 340, 120, 45, scale), accentTexture);

            GUI.DrawTexture(R(1430, 150, 350, 30, scale), wallDarkTexture);
            GUI.DrawTexture(R(1470, 180, 290, 245, scale), panelTexture);
            GUI.DrawTexture(R(1500, 210, 105, 155, scale), paperTexture);
            GUI.DrawTexture(R(1625, 220, 95, 135, scale), paperTexture);

            GUI.DrawTexture(R(525, 150, 50, 470, scale), wallDarkTexture);
            GUI.DrawTexture(R(1350, 120, 50, 500, scale), wallDarkTexture);
        }

        private void DrawClient(float scale)
        {
            if (currentCase == null)
                return;

            Rect clientRect = R(110, 245, 520, 560, scale);

            if (clientImage != null)
            {
                GUI.DrawTexture(clientRect, clientImage, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GUI.DrawTexture(R(205, 330, 330, 430, scale), panelStrongTexture);

                GUIStyle initialStyle = LabelStyle(120, FontStyle.Bold, TextAnchor.MiddleCenter, scale, new Color(0.82f, 0.72f, 0.55f));
                GUIStyle hintStyle = LabelStyle(16, FontStyle.Normal, TextAnchor.MiddleCenter, scale, new Color(0.72f, 0.76f, 0.78f));

                string initial = string.IsNullOrEmpty(currentCase.ClientName) ? "?" : currentCase.ClientName.Substring(0, 1);
                GUI.Label(R(235, 385, 270, 190, scale), initial, initialStyle);
                GUI.Label(R(225, 570, 290, 80, scale), "Добавьте Client.png\nдля персонажа", hintStyle);
            }
        }

        private void DrawClientIntro(float scale)
        {
            DrawSpeechBubble(
                R(590, 125, 1120, 565, scale),
                R(560, 610, 90, 76, scale),
                scale);

            GUIStyle nameStyle = LabelStyle(24, FontStyle.Bold, TextAnchor.MiddleLeft, scale, new Color(0.25f, 0.18f, 0.13f));
            GUIStyle itemStyle = LabelStyle(17, FontStyle.Bold, TextAnchor.MiddleLeft, scale, new Color(0.47f, 0.31f, 0.16f));
            GUIStyle storyStyle = LabelStyle(20, FontStyle.Normal, TextAnchor.UpperLeft, scale, new Color(0.16f, 0.13f, 0.11f));
            GUIStyle buttonStyle = ButtonStyle(19, scale);

            GUI.Label(R(650, 160, 980, 36, scale), currentCase.ClientName, nameStyle);
            GUI.Label(R(650, 205, 980, 28, scale), $"Потеряно: {currentCase.LostItemName}", itemStyle);
            GUI.Label(R(650, 252, 995, 320, scale), currentCase.Story, storyStyle);

            if (GUI.Button(R(1205, 590, 420, 66, scale), "НАЧАТЬ ПОИСК", buttonStyle))
            {
                CaseSession.MarkIntroSeen();
                SceneManager.LoadScene("Match3");
            }
        }

        private void DrawSpeechBubble(Rect bubble, Rect pointer, float scale)
        {
            GUI.DrawTexture(bubble, speechBubbleTexture, ScaleMode.StretchToFill, true);
            GUI.DrawTexture(pointer, speechPointerTexture, ScaleMode.StretchToFill, true);
        }

        private void DrawDeskMode(float scale)
        {
            DrawSpeechBubble(
                R(610, 160, 960, 205, scale),
                R(575, 300, 80, 68, scale),
                scale);

            GUIStyle nameStyle = LabelStyle(22, FontStyle.Bold, TextAnchor.MiddleLeft, scale, new Color(0.25f, 0.18f, 0.13f));
            GUIStyle bodyStyle = LabelStyle(20, FontStyle.Normal, TextAnchor.MiddleLeft, scale, new Color(0.16f, 0.13f, 0.11f));

            GUI.Label(R(665, 185, 820, 34, scale), currentCase.ClientName, nameStyle);

            string text = CaseSession.HasAllClues
                ? "Похоже, у нас уже достаточно информации. Посмотрите дело на столе и попробуйте понять, где осталась вещь."
                : "Удалось что-нибудь узнать? Новая информация должна быть в деле на столе.";

            GUI.Label(R(665, 235, 830, 95, scale), text, bodyStyle);

            if (CaseSession.CompletedClues > 0)
                DrawInteractiveCaseFolder(scale);
        }

        private void DrawInteractiveCaseFolder(float scale)
        {
            Rect folderRect = R(1235, 810, 475, 220, scale);

            if (caseFolderImage != null)
            {
                GUI.DrawTexture(folderRect, caseFolderImage, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GUI.DrawTexture(folderRect, folderTexture, ScaleMode.StretchToFill, true);
                GUI.DrawTexture(R(1285, 842, 370, 135, scale), folderPaperTexture, ScaleMode.StretchToFill, true);

                GUIStyle folderTitle = LabelStyle(22, FontStyle.Bold, TextAnchor.MiddleCenter, scale, new Color(0.24f, 0.13f, 0.07f));
                GUIStyle folderSmall = LabelStyle(15, FontStyle.Bold, TextAnchor.MiddleCenter, scale, new Color(0.37f, 0.23f, 0.12f));

                GUI.Label(R(1300, 858, 340, 40, scale), $"ДЕЛО №{CaseSession.CurrentCaseIndex + 1:00}", folderTitle);
                GUI.Label(R(1300, 902, 340, 38, scale), currentCase.LostItemName, folderSmall);
                GUI.Label(
                    R(1300, 945, 340, 30, scale),
                    $"УЛИКИ {CaseSession.CompletedClues}/{currentCase.RequiredClues}",
                    folderSmall);
            }

            GUIStyle invisibleButton = new GUIStyle(GUI.skin.button);
            invisibleButton.normal.background = null;
            invisibleButton.hover.background = null;
            invisibleButton.active.background = null;
            invisibleButton.normal.textColor = new Color(0, 0, 0, 0);
            invisibleButton.hover.textColor = new Color(0, 0, 0, 0);
            invisibleButton.active.textColor = new Color(0, 0, 0, 0);

            if (GUI.Button(folderRect, "", invisibleButton))
                mode = BureauMode.CaseFile;
        }

        private void DrawCaseFile(float scale)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(R(225, 85, 1470, 900, scale), folderTexture, ScaleMode.StretchToFill, true);
            GUI.DrawTexture(R(285, 145, 1350, 770, scale), folderPaperTexture, ScaleMode.StretchToFill, true);

            GUIStyle titleStyle = LabelStyle(33, FontStyle.Bold, TextAnchor.MiddleLeft, scale, new Color(0.24f, 0.13f, 0.07f));
            GUIStyle subtitleStyle = LabelStyle(17, FontStyle.Bold, TextAnchor.MiddleLeft, scale, new Color(0.46f, 0.29f, 0.14f));
            GUIStyle clueStyle = LabelStyle(19, FontStyle.Normal, TextAnchor.UpperLeft, scale, new Color(0.20f, 0.15f, 0.11f));
            GUIStyle clueHeaderStyle = LabelStyle(17, FontStyle.Bold, TextAnchor.MiddleLeft, scale, new Color(0.42f, 0.24f, 0.10f));
            GUIStyle buttonStyle = ButtonStyle(18, scale);

            GUI.Label(R(345, 180, 850, 48, scale), $"ДЕЛО №{CaseSession.CurrentCaseIndex + 1:00}: {currentCase.LostItemName}", titleStyle);
            GUI.Label(R(345, 228, 850, 30, scale), $"Клиент: {currentCase.ClientName}", subtitleStyle);

            if (lostItemImage != null)
                GUI.DrawTexture(R(1260, 170, 280, 205, scale), lostItemImage, ScaleMode.ScaleToFit, true);

            float clueY = 305f;
            for (int i = 0; i < CaseSession.CompletedClues; i++)
            {
                string clue = CaseSession.GetCollectedClue(i);

                GUI.DrawTexture(R(345, clueY, 1210, 150, scale), paperTexture, ScaleMode.StretchToFill, true);
                GUI.Label(R(375, clueY + 12, 1130, 28, scale), $"УЛИКА {i + 1}", clueHeaderStyle);
                GUI.Label(R(375, clueY + 44, 1130, 92, scale), clue, clueStyle);
                clueY += 166f;
            }

            if (GUI.Button(R(345, 835, 245, 58, scale), "ЗАКРЫТЬ ДЕЛО", buttonStyle))
                mode = BureauMode.Desk;

            if (CaseSession.HasAllClues)
            {
                if (GUI.Button(R(1175, 835, 380, 58, scale), "СДЕЛАТЬ ВЫВОД", buttonStyle))
                    mode = BureauMode.Answers;
            }
            else
            {
                if (GUI.Button(R(1080, 835, 475, 58, scale), "ПРОДОЛЖИТЬ ПОИСК", buttonStyle))
                    SceneManager.LoadScene("Match3");
            }
        }

        private void DrawAnswers(float scale)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(R(310, 150, 1300, 710, scale), folderPaperTexture, ScaleMode.StretchToFill, true);

            GUIStyle titleStyle = LabelStyle(31, FontStyle.Bold, TextAnchor.MiddleCenter, scale, new Color(0.24f, 0.13f, 0.07f));
            GUIStyle bodyStyle = LabelStyle(18, FontStyle.Normal, TextAnchor.MiddleCenter, scale, new Color(0.35f, 0.25f, 0.17f));
            GUIStyle buttonStyle = ButtonStyle(20, scale);

            GUI.Label(R(390, 205, 1140, 50, scale), "Где осталась потерянная вещь?", titleStyle);
            GUI.Label(R(450, 270, 1020, 50, scale), "Сопоставьте полученные улики и выберите одну версию.", bodyStyle);

            for (int i = 0; i < currentCase.AnswerOptions.Length && i < 4; i++)
            {
                int col = i % 2;
                int row = i / 2;

                Rect rect = R(455 + col * 520, 385 + row * 150, 470, 100, scale);

                if (GUI.Button(rect, currentCase.AnswerOptions[i], buttonStyle))
                {
                    bool correct = i == currentCase.CorrectAnswerIndex;
                    StartReaction(correct);
                }
            }

            if (GUI.Button(R(770, 745, 380, 58, scale), "ВЕРНУТЬСЯ К ДЕЛУ", buttonStyle))
                mode = BureauMode.CaseFile;
        }

        private void StartReaction(bool correct)
        {
            reactionVisible = true;
            reactionWasCorrect = correct;
            reactionStage = 0;
            reactionText = correct ? currentCase.CorrectResponse : currentCase.WrongResponse;
            reactionUntil = Time.realtimeSinceStartup + (correct ? 4.0f : 2.8f);
        }

        private void DrawClueReceivedPopup(float scale)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(R(625, 390, 670, 215, scale), panelStrongTexture, ScaleMode.StretchToFill, true);

            GUIStyle title = LabelStyle(35, FontStyle.Bold, TextAnchor.MiddleCenter, scale, new Color(0.96f, 0.80f, 0.40f));
            GUIStyle body = LabelStyle(18, FontStyle.Normal, TextAnchor.MiddleCenter, scale, new Color(0.88f, 0.90f, 0.92f));

            GUI.Label(R(665, 425, 590, 52, scale), "УЛИКА ПОЛУЧЕНА", title);
            GUI.Label(R(700, 500, 520, 60, scale), "Она добавлена в дело на вашем столе.", body);
        }

        private void DrawReaction(float scale)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture, ScaleMode.StretchToFill);

            if (!reactionWasCorrect && reactionStage == 1)
            {
                GUI.DrawTexture(R(485, 355, 950, 340, scale), panelStrongTexture, ScaleMode.StretchToFill, true);

                GUIStyle titleStyle = LabelStyle(29, FontStyle.Bold, TextAnchor.MiddleCenter, scale, new Color(0.96f, 0.80f, 0.40f));
                GUIStyle bodyStyle = LabelStyle(20, FontStyle.Normal, TextAnchor.MiddleCenter, scale);

                GUI.Label(R(545, 390, 830, 55, scale), "ПРАВИЛЬНЫЙ ОТВЕТ", titleStyle);
                GUI.Label(R(565, 470, 790, 150, scale), reactionText, bodyStyle);
                return;
            }

            DrawSpeechBubble(
                R(590, 160, 1050, 360, scale),
                R(555, 440, 82, 70, scale),
                scale);

            GUIStyle title = LabelStyle(23, FontStyle.Bold, TextAnchor.MiddleLeft, scale, new Color(0.25f, 0.18f, 0.13f));
            GUIStyle body = LabelStyle(21, FontStyle.Normal, TextAnchor.UpperLeft, scale, new Color(0.16f, 0.13f, 0.11f));

            string heading = reactionWasCorrect
                ? $"{currentCase.ClientName}: Спасибо!"
                : $"{currentCase.ClientName}: Нет, это не то место...";

            GUI.Label(R(650, 195, 900, 38, scale), heading, title);
            GUI.Label(R(650, 255, 900, 190, scale), reactionText, body);
        }

        private void DrawFinished(float scale)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(R(520, 330, 880, 390, scale), panelStrongTexture, ScaleMode.StretchToFill, true);

            GUIStyle title = LabelStyle(34, FontStyle.Bold, TextAnchor.MiddleCenter, scale);
            GUIStyle body = LabelStyle(20, FontStyle.Normal, TextAnchor.MiddleCenter, scale);
            GUIStyle buttonStyle = ButtonStyle(18, scale);

            GUI.Label(R(580, 375, 760, 60, scale), "Все доступные дела разобраны", title);
            GUI.Label(
                R(610, 465, 700, 100, scale),
                "Прототип системы клиентов работает. Теперь можно добавлять новые истории, улики и уровни.",
                body);

            if (GUI.Button(R(760, 610, 400, 58, scale), "НАЧАТЬ ЗАНОВО", buttonStyle))
            {
                CaseSession.ResetAllProgress();
                LoadCurrentCaseAssets();
                mode = BureauMode.ClientIntro;
            }
        }
    }
}
