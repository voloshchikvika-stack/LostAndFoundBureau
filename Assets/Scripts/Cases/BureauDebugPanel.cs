#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.SceneManagement;
using LostAndFound.Match3;

namespace LostAndFound.Cases
{
    // This panel is intentionally excluded from regular (non-development)
    // builds. It is created automatically in the Bureau and Match3 scenes.
    public sealed class BureauDebugPanel : MonoBehaviour
    {
        private const float Margin = 12f;
        private const float PanelWidth = 360f;

        private bool expanded = true;
        private bool resetConfirmation;
        private Texture2D panelBackground;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void SpawnOnInitialScene()
        {
            EnsurePanel(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsurePanel(scene);
        }

        private static void EnsurePanel(Scene scene)
        {
            if (scene.name != "SampleScene" && scene.name != "Match3")
                return;

            if (Object.FindFirstObjectByType<BureauDebugPanel>() != null)
                return;

            GameObject panelObject = new GameObject("TEST ONLY · Case and Match3 controls");
            panelObject.AddComponent<BureauDebugPanel>();
        }

        private void Awake()
        {
            panelBackground = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            panelBackground.SetPixel(0, 0, new Color(0.14f, 0.12f, 0.11f, 0.94f));
            panelBackground.Apply();
        }

        private void OnDestroy()
        {
            if (panelBackground != null)
                Destroy(panelBackground);
        }

        private static float Scale()
        {
            return Mathf.Clamp(
                Mathf.Min(Screen.width / 1920f, Screen.height / 1080f),
                0.70f, 1.30f);
        }

        private Rect AreaRect(float scale)
        {
            bool inMatch3 = SceneManager.GetActiveScene().name == "Match3";
            float height = !expanded ? 52f :
                resetConfirmation ? 196f :
                inMatch3 ? 280f : 230f;

            return new Rect(Margin, Margin, PanelWidth * scale, height * scale);
        }

        public static bool IsPointerOverPanel(Vector2 screenPosition)
        {
            BureauDebugPanel panel = Object.FindFirstObjectByType<BureauDebugPanel>();
            if (panel == null)
                return false;

            Rect bounds = panel.AreaRect(Scale());
            Vector2 guiPosition = new Vector2(
                screenPosition.x, Screen.height - screenPosition.y);
            return bounds.Contains(guiPosition);
        }

        private Rect At(float x, float y, float width, float height, float scale)
        {
            return new Rect(
                Margin + x * scale, Margin + y * scale,
                width * scale, height * scale);
        }

        private void PrepareStyles(float scale)
        {
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(12, Mathf.RoundToInt(18 * scale)),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(1f, 0.88f, 0.62f);

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(11, Mathf.RoundToInt(14 * scale)),
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
            labelStyle.normal.textColor = Color.white;

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Max(11, Mathf.RoundToInt(16 * scale)),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
        }

        private void OnGUI()
        {
            if (SceneManager.GetActiveScene().name != "SampleScene" &&
                SceneManager.GetActiveScene().name != "Match3")
                return;

            float scale = Scale();
            PrepareStyles(scale);

            int previousDepth = GUI.depth;
            GUI.depth = -2000;

            GUI.DrawTexture(AreaRect(scale), panelBackground);
            string title = expanded ? "▲ ТЕСТОВЫЕ КНОПКИ" : "▼ ТЕСТ";
            if (GUI.Button(At(10, 9, 340, 37, scale), title, buttonStyle))
            {
                expanded = !expanded;
                resetConfirmation = false;
            }

            if (expanded)
            {
                if (resetConfirmation)
                    DrawResetConfirmation(scale);
                else
                    DrawMainPanel(scale);
            }

            GUI.depth = previousDepth;
        }

        private void DrawMainPanel(float scale)
        {
            GUI.Label(At(16, 51, 325, 27, scale),
                "Перейти к началу дела:", labelStyle);

            DrawCaseButton(0, 14, 81, scale);
            DrawCaseButton(1, 184, 81, scale);
            DrawCaseButton(2, 14, 126, scale);
            DrawCaseButton(3, 184, 126, scale);

            if (GUI.Button(At(14, 178, 332, 41, scale),
                "НОВАЯ ИГРА · ПОЛНЫЙ СБРОС", buttonStyle))
            {
                resetConfirmation = true;
            }

            if (SceneManager.GetActiveScene().name == "Match3")
            {
                if (GUI.Button(At(14, 226, 332, 41, scale),
                    "ПРОПУСТИТЬ MATCH-3 + УЛИКА", buttonStyle))
                {
                    Match3Board board = Object.FindFirstObjectByType<Match3Board>();
                    if (board != null)
                        board.DebugSkipAndGrantClue();
                }
            }
        }

        private void DrawCaseButton(int caseIndex, float x, float y, float scale)
        {
            CaseDefinition entry = CaseDatabase.GetCase(caseIndex);
            if (entry == null)
                return;

            string caption = CaseSession.CurrentCaseIndex == caseIndex
                ? entry.ClientName + " ✓"
                : entry.ClientName;

            if (GUI.Button(At(x, y, 162, 38, scale), caption, buttonStyle))
            {
                if (CaseSession.DebugSelectCase(caseIndex))
                    SceneManager.LoadScene("SampleScene");
            }
        }

        private void DrawResetConfirmation(float scale)
        {
            GUI.Label(At(17, 53, 324, 59, scale),
                "Стереть дела, все улики, монеты\nи купленные отделы?",
                labelStyle);

            if (GUI.Button(At(14, 130, 155, 48, scale),
                "ОТМЕНА", buttonStyle))
                resetConfirmation = false;

            if (GUI.Button(At(183, 130, 163, 48, scale),
                "СБРОСИТЬ", buttonStyle))
            {
                CaseSession.ResetAllProgress();
                SceneManager.LoadScene("SampleScene");
            }
        }
    }
}
#endif
