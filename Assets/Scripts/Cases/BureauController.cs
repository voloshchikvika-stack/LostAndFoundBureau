using UnityEngine;
using UnityEngine.SceneManagement;

namespace LostAndFound.Cases
{
    public class BureauController : MonoBehaviour
    {
        private enum BureauMode
        {
            ClientIntro,
            CaseDetails,
            Desk,
            CaseFile,
            Answers,
            Inquiry,
            MiniGame,
            Room,
            Shop,
            Archive,
            ResetConfirm,
            Finished
        }

        private BureauMode mode;
        private BureauMode previousMode;
        private CaseDefinition currentCase;
        private int miniGameStep;
        private bool[] miniGamePicked = new bool[0];
        private string miniGameFeedback = "";
        private int archiveDetailIndex = -1;
        private int lastReward;
        private string activeRoomId;
        private BureauMode roomReturnMode;
        private bool temporaryRoomAccess;
        private int[] assemblySlots = { -1, -1, -1 };
        private int selectedFragment = -1;

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
        private Font uiFont;

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
            miniGameStep = 0;
            miniGamePicked = new bool[currentCase.MiniGameCards == null ? 0 : currentCase.MiniGameCards.Length];
            miniGameFeedback = "";
        }

        private void BuildUiTextures()
        {
            wallTexture = MakeTexture(new Color(0.46f, 0.52f, 0.47f, 1f));
            wallDarkTexture = MakeTexture(new Color(0.26f, 0.31f, 0.29f, 1f));
            deskTexture = MakeTexture(new Color(0.46f, 0.25f, 0.13f, 1f));
            deskEdgeTexture = MakeTexture(new Color(0.30f, 0.15f, 0.08f, 1f));
            panelTexture = MakeRoundedTexture(new Color(0.09f, 0.11f, 0.13f, 0.94f), 22);
            panelStrongTexture = MakeRoundedTexture(new Color(0.07f, 0.08f, 0.10f, 0.985f), 26);
            speechBubbleTexture = MakeRoundedTexture(new Color(0.975f, 0.935f, 0.84f, 1f), 24);
            speechPointerTexture = MakeTriangleTexture(new Color(0.975f, 0.935f, 0.84f, 1f));
            buttonTexture = MakeRoundedTexture(new Color(0.49f, 0.31f, 0.22f, 1f), 22);
            buttonHoverTexture = MakeRoundedTexture(new Color(0.59f, 0.39f, 0.28f, 1f), 22);
            folderTexture = MakeRoundedTexture(new Color(0.73f, 0.49f, 0.22f, 1f), 18);
            folderPaperTexture = MakeRoundedTexture(new Color(0.92f, 0.82f, 0.61f, 1f), 20);
            overlayTexture = MakeTexture(new Color(0.01f, 0.015f, 0.02f, 0.66f));
            goodTexture = MakeRoundedTexture(new Color(0.18f, 0.47f, 0.31f, 0.98f), 26);
            badTexture = MakeRoundedTexture(new Color(0.50f, 0.22f, 0.18f, 0.98f), 26);
            accentTexture = MakeTexture(new Color(0.77f, 0.64f, 0.36f, 1f));
            corkTexture = MakeTexture(new Color(0.58f, 0.38f, 0.22f, 1f));
            paperTexture = MakeTexture(new Color(0.92f, 0.88f, 0.76f, 1f));

            uiFont = Font.CreateDynamicFontFromOSFont(
                new[] { "Segoe UI", "Arial" },
                32);
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

            if (mode == BureauMode.Room || mode == BureauMode.MiniGame)
            {
                GUI.enabled = !cluePopupVisible && !reactionVisible;
                DrawRoomScene(scale);
                GUI.enabled = true;
                return;
            }

            DrawOffice(scale);
            bool canInteract = !cluePopupVisible && !reactionVisible;
            GUI.enabled = canInteract;
            DrawTopBar(scale);

            if (mode == BureauMode.Finished)
            {
                DrawFinished(scale);
                GUI.enabled = true;
                return;
            }

            DrawClient(scale);

            // The reaction is its own screen, not a layer on top of the answer sheet.
            if (reactionVisible)
            {
                GUI.enabled = true;
                DrawReaction(scale);
                return;
            }

            switch (mode)
            {
                case BureauMode.ClientIntro:
                    DrawClientIntro(scale);
                    break;
                case BureauMode.CaseDetails:
                    DrawCaseDetails(scale);
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
                case BureauMode.Inquiry:
                    DrawInquiry(scale);
                    break;
                case BureauMode.MiniGame:
                    DrawMiniGame(scale);
                    break;
                case BureauMode.Shop:
                    DrawShop(scale);
                    break;
                case BureauMode.Archive:
                    DrawArchive(scale);
                    break;
                case BureauMode.ResetConfirm:
                    DrawResetConfirm(scale);
                    break;
            }

            GUI.enabled = true;

            if (cluePopupVisible)
                DrawClueReceivedPopup(scale);

        }

        private Rect R(float x, float y, float w, float h, float scale)
        {
            return new Rect(x * scale, y * scale, w * scale, h * scale);
        }

        private GUIStyle LabelStyle(int size, FontStyle fontStyle, TextAnchor alignment, float scale, Color? color = null)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                font = uiFont,
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
                font = uiFont,
                fontSize = Mathf.RoundToInt(size * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                padding = new RectOffset(16, 16, 10, 10),
                border = new RectOffset(22, 22, 22, 22)
            };

            style.normal.textColor = new Color(1f, 0.96f, 0.89f);
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
                GUI.DrawTexture(R(0, 795, 1920, 285, scale), deskTexture);
                GUI.DrawTexture(R(0, 790, 1920, 18, scale), deskEdgeTexture);
            }

            if (officeBackground == null)
            {
                GUIStyle signStyle = LabelStyle(18, FontStyle.Bold, TextAnchor.MiddleCenter, scale, new Color(0.93f, 0.87f, 0.72f));
                DrawNineSlice(R(700, 24, 520, 50, scale), panelTexture, 20);
                GUI.Label(R(720, 29, 480, 39, scale), "БЮРО ПОТЕРЯННЫХ ВЕЩЕЙ", signStyle);
            }
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

            Rect clientRect = R(140, 235, 420, 515, scale);

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
            DrawCaseFolderOnDesk(scale, false);
            DrawSpeechBubble(
                R(560, 120, 1160, 515, scale),
                R(535, 525, 82, 70, scale),
                scale);

            Color darkCoffee = new Color(0.24f, 0.16f, 0.12f);
            Color mediumCoffee = new Color(0.49f, 0.32f, 0.22f);

            GUIStyle nameStyle = LabelStyle(25, FontStyle.Bold, TextAnchor.MiddleLeft, scale, darkCoffee);
            GUIStyle itemStyle = LabelStyle(16, FontStyle.Bold, TextAnchor.MiddleRight, scale, mediumCoffee);
            GUIStyle introStyle = LabelStyle(24, FontStyle.Normal, TextAnchor.UpperLeft, scale, darkCoffee);
            GUIStyle helperStyle = LabelStyle(16, FontStyle.Normal, TextAnchor.MiddleLeft, scale, mediumCoffee);
            GUIStyle buttonStyle = ButtonStyle(20, scale);

            GUI.Label(R(625, 163, 375, 42, scale), currentCase.ClientName, nameStyle);
            GUI.Label(R(980, 168, 660, 32, scale),
                currentCase.LostItemName.ToUpperInvariant(), itemStyle);
            GUI.DrawTexture(R(625, 215, 1030, 2, scale), accentTexture);

            GUI.Label(R(625, 257, 715, 195, scale), currentCase.ShortIntro, introStyle);
            DrawLostItemPreview(scale, 1410, 249, 210, 190);

            GUI.Label(R(625, 480, 900, 30, scale),
                "Можно сразу начать поиск или узнать подробности.", helperStyle);

            if (GUI.Button(R(775, 542, 340, 64, scale), "НАЧАТЬ ПОИСК", buttonStyle))
                BeginSearch();

            if (GUI.Button(R(1160, 542, 340, 64, scale), "ПОДРОБНЕЕ", buttonStyle))
                mode = BureauMode.CaseDetails;
        }

        private void DrawCaseDetails(float scale)
        {
            DrawCaseFolderOnDesk(scale, false);
            DrawSpeechBubble(
                R(560, 120, 1160, 515, scale),
                R(535, 525, 82, 70, scale),
                scale);

            Color darkCoffee = new Color(0.24f, 0.16f, 0.12f);
            Color mediumCoffee = new Color(0.49f, 0.32f, 0.22f);
            GUIStyle heading = LabelStyle(24, FontStyle.Bold, TextAnchor.MiddleLeft, scale, darkCoffee);
            GUIStyle story = LabelStyle(19, FontStyle.Normal, TextAnchor.UpperLeft, scale, darkCoffee);
            GUIStyle button = ButtonStyle(17, scale);

            GUI.Label(R(625, 158, 850, 44, scale), currentCase.ClientName + " · Подробности", heading);
            GUI.DrawTexture(R(625, 209, 1030, 2, scale), accentTexture);
            GUI.Label(R(625, 236, 1010, 280, scale), currentCase.Story, story);

            if (GUI.Button(R(665, 541, 255, 62, scale), "НАЗАД", button))
                mode = BureauMode.ClientIntro;

            if (!string.IsNullOrEmpty(currentCase.InquiryQuestion))
            {
                if (GUI.Button(R(962, 541, 295, 62, scale), "ЗАДАТЬ ВОПРОС", button))
                    mode = BureauMode.Inquiry;
            }

            if (GUI.Button(R(1300, 541, 320, 62, scale), "НАЧАТЬ ПОИСК", button))
                BeginSearch();
        }

        private void DrawLostItemPreview(float scale, float x, float y, float width, float height)
        {
            Rect preview = R(x, y, width, height, scale);
            DrawNineSlice(preview, folderPaperTexture, 20);

            if (lostItemImage != null)
            {
                GUI.DrawTexture(
                    R(x + 14, y + 12, width - 28, height - 24, scale),
                    lostItemImage, ScaleMode.ScaleToFit, true);
            }
            else
            {
                Color ink = new Color(0.46f, 0.32f, 0.23f);
                GUIStyle placeholder = LabelStyle(16, FontStyle.Bold, TextAnchor.MiddleCenter, scale, ink);
                GUI.Label(R(x + 20, y + 20, width - 40, height - 40, scale),
                    currentCase.LostItemName, placeholder);
            }
        }

        private void BeginSearch()
        {
            CaseSession.MarkIntroSeen();
            SceneManager.LoadScene("Match3");
        }

        private void DrawInquiry(float scale)
        {
            DrawCaseFolderOnDesk(scale, false);
            DrawSpeechBubble(R(575, 155, 1100, 440, scale), R(540, 500, 82, 70, scale), scale);

            Color darkCoffee = new Color(0.23f, 0.15f, 0.11f);
            GUIStyle heading = LabelStyle(24, FontStyle.Bold, TextAnchor.MiddleLeft, scale, darkCoffee);
            GUIStyle body = LabelStyle(20, FontStyle.Normal, TextAnchor.UpperLeft, scale, darkCoffee);
            GUIStyle button = ButtonStyle(18, scale);

            GUI.Label(R(640, 187, 960, 48, scale), currentCase.InquiryQuestion, heading);
            GUI.Label(R(640, 265, 960, 195, scale), currentCase.InquiryAnswer, body);

            if (GUI.Button(R(905, 497, 390, 64, scale), "ВЕРНУТЬСЯ", button))
            {
                CaseSession.MarkInquirySeen();
                mode = BureauMode.CaseDetails;
            }
        }

        private void DrawSpeechBubble(Rect bubble, Rect pointer, float scale)
        {
            DrawNineSlice(bubble, speechBubbleTexture, 24);
            GUI.DrawTexture(pointer, speechPointerTexture, ScaleMode.StretchToFill, true);
        }

        private void DrawNineSlice(Rect rect, Texture2D texture, int border)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(border, border, border, border),
                padding = new RectOffset(0, 0, 0, 0)
            };

            style.normal.background = texture;
            GUI.Box(rect, GUIContent.none, style);
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

            int responseIndex = CaseSession.CompletedClues - 1;
            bool hasResponse = currentCase.ClueResponses != null &&
                               responseIndex >= 0 && responseIndex < currentCase.ClueResponses.Length;
            string text = CaseSession.MiniGameCompleted
                ? currentCase.RoomId == "PhotoLab"
                    ? "На кадре из трамвая видна важная деталь! Давайте сравним " +
                      "улику из поиска и результаты фотолаборатории в папке дела."
                    : "Новое исследование завершено! Сравним находку из Match-3 " +
                      "с результатом работы в отделе — обе улики уже в папке."
                : hasResponse
                    ? currentCase.ClueResponses[responseIndex]
                    : "Удалось что-нибудь узнать? Новая информация должна быть в деле на столе.";

            GUI.Label(R(665, 235, 830, 95, scale), text, bodyStyle);

            // The active case is always on the desk, even before the first clue.
            DrawCaseFolderOnDesk(scale, true);
        }

        private void DrawCaseFolderOnDesk(float scale, bool interactive)
        {
            // Keep the case high enough on the desk to be clearly visible on every aspect ratio.
            Rect folderRect = R(690, 690, 540, 245, scale);
            Rect shadowRect = R(705, 708, 540, 245, scale);

            GUI.DrawTexture(shadowRect, overlayTexture, ScaleMode.StretchToFill, true);

            // Always draw a visible folder base. The optional PNG sits on top of it.
            DrawNineSlice(folderRect, folderTexture, 18);

            if (caseFolderImage != null)
            {
                GUI.DrawTexture(R(705, 700, 510, 210, scale),
                    caseFolderImage, ScaleMode.ScaleToFit, true);
            }

            GUIStyle folderTitle = LabelStyle(
                22, FontStyle.Bold, TextAnchor.MiddleCenter, scale,
                new Color(0.24f, 0.13f, 0.07f));
            GUIStyle folderSmall = LabelStyle(
                14, FontStyle.Bold, TextAnchor.MiddleCenter, scale,
                new Color(0.37f, 0.23f, 0.12f));

            DrawNineSlice(R(790, 735, 340, 112, scale), folderPaperTexture, 18);
            GUI.Label(R(810, 748, 300, 34, scale),
                $"ДЕЛО №{CaseSession.CurrentCaseIndex + 1:00}", folderTitle);
            GUI.Label(R(810, 787, 300, 28, scale),
                currentCase.LostItemName, folderSmall);

            GUI.Label(R(810, 818, 300, 25, scale),
                $"УЛИКИ {CaseSession.CollectedEvidenceCount}/{CaseSession.RequiredEvidenceCount}", folderSmall);

            if (!interactive)
                return;

            GUIStyle openStyle = ButtonStyle(16, scale);
            if (GUI.Button(R(790, 865, 340, 52, scale), "ОТКРЫТЬ ДЕЛО", openStyle))
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

            if (CaseSession.CollectedEvidenceCount == 0)
            {
                GUIStyle emptyStyle = LabelStyle(
                    20, FontStyle.Normal, TextAnchor.MiddleCenter, scale,
                    new Color(0.38f, 0.27f, 0.18f));

                GUI.Label(
                    R(420, 405, 1080, 100, scale),
                    "Улик пока нет. Начните поиск, чтобы добавить первую улику в дело.",
                    emptyStyle);
            }

            for (int i = 0; i < CaseSession.CollectedEvidenceCount; i++)
            {
                string clue = CaseSession.GetCollectedEvidenceText(i);
                string source = CaseSession.GetCollectedEvidenceSource(i);

                GUI.DrawTexture(R(345, clueY, 1210, 150, scale), paperTexture, ScaleMode.StretchToFill, true);
                GUI.Label(R(375, clueY + 12, 1130, 28, scale),
                    $"УЛИКА {i + 1} · {source}", clueHeaderStyle);
                GUI.Label(R(375, clueY + 44, 1130, 92, scale), clue, clueStyle);
                clueY += 166f;
            }

            if (GUI.Button(R(345, 835, 245, 58, scale), "ЗАКРЫТЬ ДЕЛО", buttonStyle))
                mode = BureauMode.Desk;

            if (CaseSession.MiniGameReady)
            {
                if (GUI.Button(R(960, 835, 595, 58, scale),
                    "ПЕРЕЙТИ: " + RoomName(currentCase.RoomId).ToUpperInvariant(), buttonStyle))
                    StartMiniGame();
            }
            else if (CaseSession.HasAllClues)
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
            if (reactionVisible)
                return;

            lastReward = BureauEconomy.CompleteCase(currentCase.Id, correct);
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
            if (!reactionWasCorrect && reactionStage == 1)
            {
                // Only the final explanation uses a darkened modal background.
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height),
                    overlayTexture, ScaleMode.StretchToFill);
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
            if (lastReward > 0)
                GUI.Label(R(650, 445, 900, 38, scale),
                    $"+{lastReward} монет", title);
        }

        private void DrawTopBar(float scale)
        {
            if (mode != BureauMode.ClientIntro && mode != BureauMode.Desk &&
                mode != BureauMode.Finished)
                return;

            GUIStyle info = LabelStyle(20, FontStyle.Bold, TextAnchor.MiddleCenter,
                scale, new Color(0.25f, 0.17f, 0.11f));
            GUIStyle button = ButtonStyle(17, scale);

            DrawNineSlice(R(1315, 20, 150, 58, scale), speechBubbleTexture, 24);
            GUI.Label(R(1330, 29, 120, 38, scale), $"{BureauEconomy.Coins} ◈", info);

            if (GUI.Button(R(1485, 20, 175, 58, scale), "ОТДЕЛЫ", button))
                OpenOverlay(BureauMode.Shop);

            if (GUI.Button(R(1678, 20, 200, 58, scale), "АРХИВ", button))
                OpenOverlay(BureauMode.Archive);

            if (GUI.Button(R(1678, 90, 200, 52, scale), "НОВАЯ ИГРА", button))
                OpenOverlay(BureauMode.ResetConfirm);
        }

        private void OpenOverlay(BureauMode next)
        {
            previousMode = mode;
            mode = next;
        }

        private void CloseOverlay()
        {
            mode = previousMode;
        }

        private void DrawResetConfirm(float scale)
        {
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height),
                overlayTexture, ScaleMode.StretchToFill);

            DrawNineSlice(R(545, 330, 830, 390, scale), folderPaperTexture, 20);

            Color ink = new Color(0.27f, 0.17f, 0.11f);
            GUIStyle title = LabelStyle(31, FontStyle.Bold, TextAnchor.MiddleCenter, scale, ink);
            GUIStyle body = LabelStyle(19, FontStyle.Normal, TextAnchor.MiddleCenter, scale, ink);
            GUIStyle button = ButtonStyle(18, scale);

            GUI.Label(R(600, 375, 720, 55, scale), "НАЧАТЬ ИГРУ ЗАНОВО?", title);
            GUI.Label(
                R(625, 455, 670, 105, scale),
                "Будут сброшены дела, улики, монеты и покупки в бюро. Игра снова начнётся с первого клиента.",
                body);

            if (GUI.Button(R(650, 610, 280, 62, scale), "ОТМЕНА", button))
                CloseOverlay();

            if (GUI.Button(R(990, 610, 280, 62, scale), "СБРОСИТЬ", button))
            {
                CaseSession.ResetAllProgress();
                SceneManager.LoadScene("SampleScene");
            }
        }

        private void DrawPurchasedDecor(float scale)
        {
            var positions = new[]
            {
                R(555, 814, 135, 125, scale),
                R(1180, 806, 120, 155, scale),
                R(1480, 585, 150, 175, scale),
                R(345, 785, 160, 135, scale)
            };

            int index = 0;
            foreach (BureauUpgrade upgrade in BureauEconomy.Upgrades)
            {
                if (BureauEconomy.Owns(upgrade.Id))
                {
                    Rect position = positions[index];
                    Texture2D decorImage = Resources.Load<Texture2D>("Bureau/Decor/" + upgrade.Id);

                    if (decorImage != null)
                    {
                        GUI.DrawTexture(position, decorImage, ScaleMode.ScaleToFit, true);
                    }
                    else
                    {
                        DrawNineSlice(position, folderPaperTexture, 20);
                        GUIStyle label = LabelStyle(13, FontStyle.Bold, TextAnchor.MiddleCenter,
                            scale, new Color(0.40f, 0.25f, 0.16f));
                        GUI.Label(new Rect(position.x + 8f * scale, position.y + 6f * scale,
                            position.width - 16f * scale, position.height - 12f * scale),
                            upgrade.Name, label);
                    }
                }
                index++;
            }
        }

        private void DrawShop(float scale)
        {
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height),
                overlayTexture, ScaleMode.StretchToFill);
            DrawNineSlice(R(270, 100, 1380, 860, scale), folderPaperTexture, 20);

            Color ink = new Color(0.27f, 0.17f, 0.11f);
            GUIStyle title = LabelStyle(34, FontStyle.Bold, TextAnchor.MiddleLeft, scale, ink);
            GUIStyle body = LabelStyle(18, FontStyle.Normal, TextAnchor.MiddleLeft, scale, ink);
            GUIStyle name = LabelStyle(23, FontStyle.Bold, TextAnchor.MiddleLeft, scale, ink);
            GUIStyle button = ButtonStyle(17, scale);

            GUI.Label(R(335, 138, 750, 52, scale), "РАЗВИТИЕ БЮРО", title);
            GUI.Label(R(1170, 140, 350, 50, scale), $"Монеты: {BureauEconomy.Coins}", name);
            GUI.Label(R(335, 204, 1120, 40, scale),
                "Открывайте отделы, чтобы исследовать снимки, документы и предметы.", body);

            int i = 0;
            foreach (BureauUpgrade upgrade in BureauEconomy.Upgrades)
            {
                float y = 270f + i * 190f;
                DrawNineSlice(R(325, y, 1260, 177, scale), speechBubbleTexture, 20);
                Texture2D icon = Resources.Load<Texture2D>(
                    "Bureau/Rooms/" + upgrade.Id + "/Icon");

                if (icon != null)
                    GUI.DrawTexture(R(345, y + 13, 158, 149, scale),
                        icon, ScaleMode.ScaleToFit, true);
                else
                    DrawNineSlice(R(350, y + 20, 142, 130, scale), folderTexture, 18);

                GUI.Label(R(530, y + 20, 665, 43, scale), upgrade.Name, name);
                GUI.Label(R(530, y + 74, 645, 83, scale), upgrade.Description, body);

                bool owned = BureauEconomy.Owns(upgrade.Id);
                GUI.enabled = owned || BureauEconomy.Coins >= upgrade.Price;
                string caption = owned ? "ПОСЕТИТЬ" :
                    $"ОТКРЫТЬ · {upgrade.Price} МОНЕТ";

                if (GUI.Button(R(1220, y + 53, 315, 67, scale), caption, button))
                {
                    if (owned || BureauEconomy.TryBuy(upgrade))
                    {
                        activeRoomId = upgrade.Id;
                        roomReturnMode = BureauMode.Shop;
                        temporaryRoomAccess = false;
                        mode = BureauMode.Room;
                        GUI.enabled = true;
                        return;
                    }
                }

                GUI.enabled = true;
                i++;
            }

            if (GUI.Button(R(760, 861, 400, 68, scale), "ВЕРНУТЬСЯ", button))
                CloseOverlay();
        }

        private static string RoomName(string roomId)
        {
            switch (roomId)
            {
                case "PhotoLab": return "Фотолаборатория";
                case "ArchiveRoom": return "Архив документов";
                case "Workshop": return "Мастерская находок";
                default: return "Отдел расследований";
            }
        }

        private void DrawRoomScene(float scale)
        {
            Texture2D background = Resources.Load<Texture2D>(
                "Bureau/Rooms/" + activeRoomId + "/Background");

            if (background != null)
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height),
                    background, ScaleMode.ScaleAndCrop, true);
            else
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height),
                    wallTexture, ScaleMode.StretchToFill);

            Color ink = new Color(0.27f, 0.17f, 0.11f);
            GUIStyle heading = LabelStyle(31, FontStyle.Bold,
                TextAnchor.MiddleCenter, scale, ink);
            GUIStyle body = LabelStyle(19, FontStyle.Normal,
                TextAnchor.MiddleCenter, scale, ink);
            GUIStyle button = ButtonStyle(18, scale);

            DrawNineSlice(R(615, 24, 690, 76, scale), speechBubbleTexture, 24);
            GUI.Label(R(645, 33, 630, 53, scale), RoomName(activeRoomId), heading);

            if (GUI.Button(R(42, 33, 245, 62, scale), "← НАЗАД", button))
            {
                mode = roomReturnMode;
                return;
            }

            if (mode == BureauMode.Room)
            {
                DrawNineSlice(R(475, 700, 970, 280, scale),
                    folderPaperTexture, 22);

                bool neededForCase = currentCase != null &&
                    CaseSession.MiniGameReady &&
                    currentCase.RoomId == activeRoomId;

                GUI.Label(R(530, 727, 860, 57, scale),
                    neededForCase ? "НОВОЕ ЗАДАНИЕ ПО ДЕЛУ" : "ОТДЕЛ ОТКРЫТ",
                    heading);

                GUI.Label(R(530, 794, 860, 78, scale),
                    neededForCase
                        ? $"Для дела «{currentCase.LostItemName}» здесь появилось новое исследование."
                        : "Помещение доступно в вашем бюро. " +
                          "Задания клиентов будут появляться здесь по мере расследований.",
                    body);

                if (neededForCase &&
                    GUI.Button(R(760, 889, 400, 65, scale),
                        "НАЧАТЬ ИССЛЕДОВАНИЕ", button))
                    StartMiniGame();

                return;
            }

            if (!BureauEconomy.Owns(activeRoomId) && !temporaryRoomAccess)
            {
                DrawNineSlice(R(445, 645, 1030, 355, scale),
                    folderPaperTexture, 22);
                GUI.Label(R(495, 682, 930, 57, scale),
                    "ОТКРЫТЬ ОТДЕЛ", heading);
                GUI.Label(R(520, 748, 880, 75, scale),
                    "Это помещение понадобится для дела " +
                    currentCase.ClientName + ". Можно купить его " +
                    "навсегда или пройти текущий сюжет по временному пропуску.",
                    body);

                foreach (BureauUpgrade upgrade in BureauEconomy.Upgrades)
                {
                    if (upgrade.Id != activeRoomId)
                        continue;

                    GUI.enabled = BureauEconomy.Coins >= upgrade.Price;
                    if (GUI.Button(R(520, 859, 425, 73, scale),
                        $"ОТКРЫТЬ · {upgrade.Price} МОНЕТ", button))
                        BureauEconomy.TryBuy(upgrade);
                    GUI.enabled = true;
                    break;
                }

                if (GUI.Button(R(980, 859, 425, 73, scale),
                    "ПО ДЕЛУ · БЕСПЛАТНО", button))
                    temporaryRoomAccess = true;

                return;
            }

            DrawMiniGame(scale);
        }

        private void DrawArchive(float scale)
        {
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), overlayTexture);
            DrawNineSlice(R(280, 115, 1360, 860, scale), folderPaperTexture, 20);
            Color ink = new Color(0.28f, 0.18f, 0.12f);
            GUIStyle title = LabelStyle(33, FontStyle.Bold, TextAnchor.MiddleLeft, scale, ink);
            GUIStyle body = LabelStyle(18, FontStyle.Normal, TextAnchor.UpperLeft, scale, ink);
            GUIStyle item = LabelStyle(20, FontStyle.Bold, TextAnchor.MiddleLeft, scale, ink);
            GUIStyle button = ButtonStyle(17, scale);

            GUI.Label(R(350, 155, 900, 58, scale), "АРХИВ РАССЛЕДОВАНИЙ", title);
            GUI.Label(R(350, 213, 1150, 43, scale),
                "Здесь сохраняются решения завершённых дел.", body);

            for (int i = 0; i < CaseDatabase.Cases.Count; i++)
            {
                CaseDefinition entry = CaseDatabase.GetCase(i);
                int result = BureauEconomy.CaseResult(entry.Id);
                string outcome = result == 1 ? "НАЙДЕНО" : (result == -1 ? "НЕ НАЙДЕНО" : "НЕ ЗАВЕРШЕНО");
                string label = $"ДЕЛО №{i + 1:00}: {entry.LostItemName}     {outcome}";
                if (GUI.Button(R(350, 277 + i * 105, 1220, 82, scale), label, button))
                    archiveDetailIndex = i;
            }

            if (archiveDetailIndex >= 0 && archiveDetailIndex < CaseDatabase.Cases.Count)
            {
                CaseDefinition selected = CaseDatabase.GetCase(archiveDetailIndex);
                int result = BureauEconomy.CaseResult(selected.Id);
                if (result != 0)
                {
                    GUI.Label(R(365, 525, 1110, 48, scale),
                        $"Клиент: {selected.ClientName}", item);
                    GUI.Label(R(365, 587, 1110, 180, scale),
                        result == 1 ? selected.CorrectResponse :
                        selected.CorrectAnswerExplanation, body);
                }
                else
                {
                    GUI.Label(R(365, 525, 1110, 120, scale),
                        "Материалы этого дела откроются после завершения расследования.", body);
                }
            }

            if (GUI.Button(R(760, 865, 400, 67, scale), "ВЕРНУТЬСЯ", button))
                CloseOverlay();
        }

        private void StartMiniGame()
        {
            if (currentCase == null || !CaseSession.MiniGameReady)
                return;

            activeRoomId = currentCase.RoomId;
            roomReturnMode = BureauMode.CaseFile;
            temporaryRoomAccess = false;
            mode = BureauMode.MiniGame;
            miniGameStep = 0;
            miniGameFeedback = "";
            miniGamePicked = new bool[currentCase.MiniGameCards.Length];
            selectedFragment = -1;
            assemblySlots = new int[currentCase.MiniGameCards.Length];
            for (int i = 0; i < assemblySlots.Length; i++)
                assemblySlots[i] = -1;
        }

        private void DrawMiniGame(float scale)
        {
            // Display the puzzle on a work card while leaving the upper part of
            // the photo lab, archive or workshop illustration visible.
            DrawNineSlice(R(360, 352, 1200, 675, scale),
                folderPaperTexture, 22);

            Color ink = new Color(0.28f, 0.18f, 0.12f);
            Color accent = new Color(0.50f, 0.34f, 0.23f);
            GUIStyle heading = LabelStyle(27, FontStyle.Bold,
                TextAnchor.MiddleCenter, scale, ink);
            GUIStyle body = LabelStyle(18, FontStyle.Normal,
                TextAnchor.MiddleCenter, scale, ink);
            GUIStyle progress = LabelStyle(16, FontStyle.Bold,
                TextAnchor.MiddleCenter, scale, accent);
            GUIStyle button = ButtonStyle(17, scale);

            GUI.Label(R(405, 378, 1110, 55, scale),
                currentCase.MiniGameTitle, heading);
            GUI.Label(R(430, 448, 1060, 67, scale),
                currentCase.MiniGameInstruction, body);

            bool solved = CaseSession.MiniGameCompleted;

            if (!solved && currentCase.MiniGameMode == "Spot")
            {
                DrawPhotoPuzzle(scale, button, body);
            }
            else if (!solved && currentCase.MiniGameMode == "Assembly")
            {
                DrawRestorationPuzzle(scale, button, body);
            }
            else if (!solved)
            {
                GUI.Label(R(595, 516, 730, 34, scale),
                    $"Выбрано по порядку: {miniGameStep}/{miniGamePicked.Length}", progress);

                for (int i = 0; i < currentCase.MiniGameCards.Length; i++)
                {
                    bool picked = miniGamePicked[i];
                    GUI.enabled = !picked;

                    if (GUI.Button(R(470, 561 + i * 94, 980, 77, scale),
                        picked ? "✓ " + currentCase.MiniGameCards[i] :
                            currentCase.MiniGameCards[i], button))
                    {
                        if (i == currentCase.MiniGameCorrectOrder[miniGameStep])
                        {
                            miniGamePicked[i] = true;
                            miniGameStep++;

                            if (miniGameStep == miniGamePicked.Length)
                            {
                                CaseSession.CompleteMiniGame();
                                miniGameFeedback = currentCase.MiniGameResult;
                            }
                            else
                                miniGameFeedback = "Верно! Выберите следующий фрагмент.";
                        }
                        else
                        {
                            miniGameStep = 0;
                            miniGamePicked = new bool[currentCase.MiniGameCards.Length];
                            miniGameFeedback = "Порядок неверный. Попробуйте ещё раз.";
                        }
                    }

                    GUI.enabled = true;
                }

                GUI.Label(R(520, 860, 880, 55, scale), miniGameFeedback, body);
            }
            else
            {
                GUI.Label(R(480, 558, 960, 70, scale),
                    "УЛИКА ДОБАВЛЕНА В ДЕЛО", heading);
                GUI.Label(R(460, 650, 1000, 138, scale),
                    currentCase.MiniGameResult, body);
            }

            if (GUI.Button(R(755, 940, 410, 59, scale),
                solved ? "ПРОЧИТАТЬ УЛИКИ В ДЕЛЕ" : "ЗАКРЫТЬ ЗАДАНИЕ", button))
                mode = BureauMode.CaseFile;
        }

        private void DrawPhotoPuzzle(float scale, GUIStyle button, GUIStyle body)
        {
            for (int i = 0; i < currentCase.MiniGameCards.Length; i++)
            {
                if (GUI.Button(R(470, 555 + i * 100, 980, 82, scale),
                    currentCase.MiniGameCards[i], button))
                {
                    if (i == currentCase.MiniGameCorrectOrder[0])
                    {
                        CaseSession.CompleteMiniGame();
                        miniGameFeedback = currentCase.MiniGameResult;
                    }
                    else
                        miniGameFeedback = "Этот кадр не объясняет обрыв ремешка. " +
                            "Поищите снимок, где виден момент потери.";
                }
            }

            GUI.Label(R(510, 860, 900, 52, scale), miniGameFeedback, body);
        }

        private void DrawRestorationPuzzle(float scale, GUIStyle button, GUIStyle body)
        {
            Color ink = new Color(0.38f, 0.25f, 0.17f);
            GUIStyle hint = LabelStyle(16, FontStyle.Bold,
                TextAnchor.MiddleCenter, scale, ink);

            GUI.Label(R(490, 516, 940, 33, scale),
                "1. Выберите фрагмент в верхнем ряду. " +
                "2. Поместите его в нужное место в нижнем ряду.", hint);

            for (int i = 0; i < currentCase.MiniGameCards.Length; i++)
            {
                string label = (selectedFragment == i ? "● " : "") +
                    currentCase.MiniGameCards[i];

                if (GUI.Button(R(412 + i * 371, 566, 346, 88, scale), label, button))
                    selectedFragment = i;
            }

            for (int slot = 0; slot < assemblySlots.Length; slot++)
            {
                int item = assemblySlots[slot];
                string label = item < 0
                    ? $"МЕСТО {slot + 1} · ПУСТО"
                    : $"{slot + 1}. {currentCase.MiniGameCards[item]}";

                if (GUI.Button(R(412 + slot * 371, 680, 346, 92, scale), label, button))
                {
                    if (selectedFragment >= 0)
                    {
                        for (int n = 0; n < assemblySlots.Length; n++)
                        {
                            if (assemblySlots[n] == selectedFragment)
                                assemblySlots[n] = -1;
                        }

                        assemblySlots[slot] = selectedFragment;
                        selectedFragment = -1;

                        bool complete = true;
                        bool correct = true;

                        for (int n = 0; n < assemblySlots.Length; n++)
                        {
                            complete &= assemblySlots[n] >= 0;
                            correct &= assemblySlots[n] == currentCase.MiniGameCorrectOrder[n];
                        }

                        if (complete && correct)
                        {
                            CaseSession.CompleteMiniGame();
                            miniGameFeedback = currentCase.MiniGameResult;
                        }
                        else if (complete)
                            miniGameFeedback = "Фрагменты пока не сложились в адрес. " +
                                "Выберите фрагмент и поменяйте его место.";
                        else
                            miniGameFeedback = "Продолжайте восстанавливать расписку.";
                    }
                    else if (item >= 0)
                    {
                        // Take a fragment back out to change the arrangement.
                        selectedFragment = item;
                        assemblySlots[slot] = -1;
                    }
                }
            }

            GUI.Label(R(510, 823, 900, 94, scale), miniGameFeedback, body);
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
