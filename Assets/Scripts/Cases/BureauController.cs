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

        private Texture2D wallTexture;
        private Texture2D deskTexture;
        private Texture2D panelTexture;
        private Texture2D panelStrongTexture;
        private Texture2D buttonTexture;
        private Texture2D buttonHoverTexture;
        private Texture2D folderTexture;
        private Texture2D overlayTexture;
        private Texture2D goodTexture;
        private Texture2D badTexture;

        private bool cluePopupVisible;
        private string latestClueText;
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

            if (CaseSession.TryConsumePendingClue(out string clue))
            {
                latestClueText = clue;
                cluePopupVisible = true;
                cluePopupUntil = Time.realtimeSinceStartup + 2.2f;
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
                reactionUntil = now + 3.4f;
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
            wallTexture = MakeTexture(new Color(0.48f, 0.39f, 0.30f, 1f));
            deskTexture = MakeTexture(new Color(0.24f, 0.13f, 0.075f, 1f));
            panelTexture = MakeTexture(new Color(0.08f, 0.095f, 0.12f, 0.95f));
            panelStrongTexture = MakeTexture(new Color(0.12f, 0.14f, 0.18f, 0.98f));
            buttonTexture = MakeTexture(new Color(0.26f, 0.52f, 0.39f, 1f));
            buttonHoverTexture = MakeTexture(new Color(0.32f, 0.62f, 0.46f, 1f));
            folderTexture = MakeTexture(new Color(0.74f, 0.54f, 0.27f, 1f));
            overlayTexture = MakeTexture(new Color(0.01f, 0.015f, 0.025f, 0.72f));
            goodTexture = MakeTexture(new Color(0.16f, 0.48f, 0.31f, 0.98f));
            badTexture = MakeTexture(new Color(0.50f, 0.20f, 0.18f, 0.98f));
        }

        private Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
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
                wordWrap = true
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
                padding = new RectOffset(14, 14, 8, 8)
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
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), wallTexture, ScaleMode.StretchToFill);
                GUI.DrawTexture(
                    new Rect(0, Screen.height * 0.69f, Screen.width, Screen.height * 0.31f),
                    deskTexture,
                    ScaleMode.StretchToFill);
            }

            GUIStyle header = LabelStyle(20, FontStyle.Bold, TextAnchor.MiddleCenter, scale, new Color(0.95f, 0.90f, 0.80f));
            GUI.DrawTexture(R(650, 18, 620, 54, scale), panelTexture);
            GUI.Label(R(670, 24, 580, 42, scale), "БЮРО ПОТЕРЯННЫХ ВЕЩЕЙ", header);
        }

        private void DrawClient(float scale)
        {
            if (currentCase == null)
                return;

            Rect clientRect = R(705, 125, 510, 510, scale);

            if (clientImage != null)
            {
                GUI.DrawTexture(clientRect, clientImage, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GUI.DrawTexture(clientRect, panelStrongTexture, ScaleMode.StretchToFill);
                GUIStyle initialStyle = LabelStyle(112, FontStyle.Bold, TextAnchor.MiddleCenter, scale, new Color(0.82f, 0.74f, 0.61f));
                string initial = string.IsNullOrEmpty(currentCase.ClientName) ? "?" : currentCase.ClientName.Substring(0, 1);
                GUI.Label(clientRect, initial, initialStyle);
            }

            GUIStyle nameStyle = LabelStyle(27, FontStyle.Bold, TextAnchor.MiddleCenter, scale);
            GUI.DrawTexture(R(790, 590, 340, 58, scale), panelTexture);
            GUI.Label(R(805, 596, 310, 44, scale), currentCase.ClientName, nameStyle);
        }

        private void DrawClientIntro(float scale)
        {
            GUI.DrawTexture(R(245, 680, 1430, 310, scale), panelTexture);

            GUIStyle itemStyle = LabelStyle(18, FontStyle.Bold, TextAnchor.MiddleLeft, scale, new Color(0.88f, 0.75f, 0.48f));
            GUIStyle storyStyle = LabelStyle(20, FontStyle.Normal, TextAnchor.UpperLeft, scale);
            GUIStyle buttonStyle = ButtonStyle(20, scale);

            GUI.Label(R(290, 700, 1340, 34, scale), $"Потеряно: {currentCase.LostItemName}", itemStyle);
            GUI.Label(R(290, 742, 1340, 165, scale), currentCase.Story, storyStyle);

            if (GUI.Button(R(760, 918, 400, 58, scale), "НАЧАТЬ ПОИСК", buttonStyle))
            {
                CaseSession.MarkIntroSeen();
                SceneManager.LoadScene("Match3");
            }
        }

        private void DrawDeskMode(float scale)
        {
            GUIStyle hintStyle = LabelStyle(18, FontStyle.Normal, TextAnchor.MiddleCenter, scale, new Color(0.96f, 0.93f, 0.86f));
            GUI.DrawTexture(R(590, 655, 740, 58, scale), panelTexture);
            GUI.Label(R(610, 661, 700, 44, scale), "На столе лежит дело. Откройте его и изучите полученные улики.", hintStyle);

            if (CaseSession.CompletedClues <= 0)
                return;

            GUIStyle folderStyle = ButtonStyle(22, scale);
            folderStyle.normal.background = folderTexture;
            folderStyle.hover.background = folderTexture;
            folderStyle.active.background = folderTexture;
            folderStyle.normal.textColor = new Color(0.19f, 0.11f, 0.055f);
            folderStyle.hover.textColor = folderStyle.normal.textColor;
            folderStyle.active.textColor = folderStyle.normal.textColor;

            string label =
                $"ДЕЛО №{CaseSession.CurrentCaseIndex + 1:00}\n" +
                $"{currentCase.LostItemName}\n" +
                $"Улики: {CaseSession.CompletedClues}/{currentCase.RequiredClues}";

            if (GUI.Button(R(755, 755, 410, 190, scale), label, folderStyle))
                mode = BureauMode.CaseFile;
        }

        private void DrawCaseFile(float scale)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(R(300, 120, 1320, 820, scale), panelStrongTexture);

            GUIStyle titleStyle = LabelStyle(34, FontStyle.Bold, TextAnchor.MiddleLeft, scale);
            GUIStyle subtitleStyle = LabelStyle(18, FontStyle.Bold, TextAnchor.MiddleLeft, scale, new Color(0.88f, 0.75f, 0.48f));
            GUIStyle clueStyle = LabelStyle(20, FontStyle.Normal, TextAnchor.UpperLeft, scale);
            GUIStyle buttonStyle = ButtonStyle(19, scale);

            GUI.Label(R(360, 155, 900, 48, scale), $"Дело: {currentCase.LostItemName}", titleStyle);
            GUI.Label(R(360, 205, 900, 34, scale), $"Клиент: {currentCase.ClientName}", subtitleStyle);

            if (lostItemImage != null)
                GUI.DrawTexture(R(1260, 150, 260, 210, scale), lostItemImage, ScaleMode.ScaleToFit, true);

            float clueY = 275f;
            for (int i = 0; i < CaseSession.CompletedClues; i++)
            {
                string clue = CaseSession.GetCollectedClue(i);
                GUI.DrawTexture(R(360, clueY, 1200, 145, scale), panelTexture);
                GUI.Label(R(390, clueY + 16, 1135, 112, scale), $"УЛИКА {i + 1}\n{clue}", clueStyle);
                clueY += 162f;
            }

            if (GUI.Button(R(360, 850, 250, 56, scale), "ЗАКРЫТЬ ДЕЛО", buttonStyle))
                mode = BureauMode.Desk;

            if (CaseSession.HasAllClues)
            {
                if (GUI.Button(R(1180, 850, 380, 56, scale), "СДЕЛАТЬ ВЫВОД", buttonStyle))
                    mode = BureauMode.Answers;
            }
            else
            {
                if (GUI.Button(R(1110, 850, 450, 56, scale), "ПРОДОЛЖИТЬ ПОИСК", buttonStyle))
                    SceneManager.LoadScene("Match3");
            }
        }

        private void DrawAnswers(float scale)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(R(360, 190, 1200, 650, scale), panelStrongTexture);

            GUIStyle titleStyle = LabelStyle(30, FontStyle.Bold, TextAnchor.MiddleCenter, scale);
            GUIStyle bodyStyle = LabelStyle(19, FontStyle.Normal, TextAnchor.MiddleCenter, scale, new Color(0.82f, 0.84f, 0.88f));
            GUIStyle buttonStyle = ButtonStyle(20, scale);

            GUI.Label(R(430, 230, 1060, 52, scale), "Где осталась потерянная вещь?", titleStyle);
            GUI.Label(R(470, 290, 980, 58, scale), "Используйте собранные улики и выберите одну версию.", bodyStyle);

            for (int i = 0; i < currentCase.AnswerOptions.Length && i < 4; i++)
            {
                int col = i % 2;
                int row = i / 2;

                Rect rect = R(470 + col * 510, 405 + row * 150, 460, 100, scale);

                if (GUI.Button(rect, currentCase.AnswerOptions[i], buttonStyle))
                {
                    bool correct = i == currentCase.CorrectAnswerIndex;
                    StartReaction(correct);
                }
            }

            if (GUI.Button(R(770, 745, 380, 56, scale), "ВЕРНУТЬСЯ К ДЕЛУ", buttonStyle))
                mode = BureauMode.CaseFile;
        }

        private void StartReaction(bool correct)
        {
            reactionVisible = true;
            reactionWasCorrect = correct;
            reactionStage = 0;
            reactionText = correct ? currentCase.CorrectResponse : currentCase.WrongResponse;
            reactionUntil = Time.realtimeSinceStartup + (correct ? 3.8f : 2.8f);
        }

        private void DrawClueReceivedPopup(float scale)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(R(570, 385, 780, 260, scale), panelStrongTexture);

            GUIStyle title = LabelStyle(36, FontStyle.Bold, TextAnchor.MiddleCenter, scale, new Color(0.96f, 0.79f, 0.38f));
            GUIStyle body = LabelStyle(19, FontStyle.Normal, TextAnchor.MiddleCenter, scale);

            GUI.Label(R(610, 420, 700, 60, scale), "УЛИКА ПОЛУЧЕНА", title);
            GUI.Label(R(640, 500, 640, 90, scale), "Новая улика добавлена в дело на столе.", body);
        }

        private void DrawReaction(float scale)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture, ScaleMode.StretchToFill);

            Texture2D cardTexture = reactionWasCorrect ? goodTexture : (reactionStage == 0 ? badTexture : panelStrongTexture);
            GUI.DrawTexture(R(485, 355, 950, 340, scale), cardTexture);

            GUIStyle titleStyle = LabelStyle(30, FontStyle.Bold, TextAnchor.MiddleCenter, scale);
            GUIStyle bodyStyle = LabelStyle(21, FontStyle.Normal, TextAnchor.MiddleCenter, scale);

            string title;
            if (reactionWasCorrect)
                title = $"{currentCase.ClientName}: Спасибо!";
            else if (reactionStage == 0)
                title = $"{currentCase.ClientName}: Кажется, это не то место...";
            else
                title = "РАЗБОР ДЕЛА";

            GUI.Label(R(545, 390, 830, 56, scale), title, titleStyle);
            GUI.Label(R(570, 465, 780, 160, scale), reactionText, bodyStyle);
        }

        private void DrawFinished(float scale)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(R(520, 330, 880, 390, scale), panelStrongTexture);

            GUIStyle title = LabelStyle(34, FontStyle.Bold, TextAnchor.MiddleCenter, scale);
            GUIStyle body = LabelStyle(20, FontStyle.Normal, TextAnchor.MiddleCenter, scale);
            GUIStyle buttonStyle = ButtonStyle(18, scale);

            GUI.Label(R(580, 375, 760, 60, scale), "Все доступные дела разобраны", title);
            GUI.Label(
                R(610, 465, 700, 100, scale),
                "Система клиентов уже работает. Теперь можно добавлять новые истории, улики, уровни и изображения.",
                body);

            if (GUI.Button(R(760, 610, 400, 58, scale), "НАЧАТЬ ПРОТОТИП ЗАНОВО", buttonStyle))
            {
                CaseSession.ResetAllProgress();
                LoadCurrentCaseAssets();
                mode = BureauMode.ClientIntro;
            }
        }
    }
}
