using UnityEngine;
using UnityEngine.SceneManagement;

namespace LostAndFound.UI
{
    /// <summary>
    /// First-run profile screen, intentionally separate from the playable bureau.
    /// Uses IMGUI to match the game's existing UI implementation.
    /// </summary>
    public sealed class PlayerProfileSetup : MonoBehaviour
    {
        private const string BureauScene = "SampleScene";
        private static readonly string[] AvatarNames =
        {
            "Следопыт", "Исследователь", "Детектив",
            "Архивариус", "Искатель", "Знаток"
        };

        private static readonly Color[] CoatColors =
        {
            new Color(.23f,.39f,.31f), new Color(.49f,.27f,.21f),
            new Color(.28f,.32f,.51f), new Color(.57f,.39f,.22f),
            new Color(.35f,.36f,.31f), new Color(.47f,.31f,.47f)
        };
        private static readonly Color[] HairColors =
        {
            new Color(.22f,.14f,.10f), new Color(.66f,.37f,.17f),
            new Color(.18f,.17f,.15f), new Color(.46f,.29f,.17f),
            new Color(.61f,.44f,.27f), new Color(.30f,.18f,.16f)
        };
        private readonly Texture2D[] portraits = new Texture2D[6];
        private Texture2D background;
        private string playerName = "";
        private int selectedAvatar;
        private string validation = "";
        private bool entering;

        private void Awake()
        {
            if (GetComponentInChildren<Camera>() == null)
            {
                GameObject cam = new GameObject("ProfileCamera");
                cam.transform.SetParent(transform, false);
                Camera camera = cam.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.19f,.14f,.11f);
                camera.cullingMask = 0;
                camera.orthographic = true;
            }

            background = Resources.Load<Texture2D>("Loading/BureauStreet");
            for (int i = 0; i < portraits.Length; i++)
                portraits[i] = BuildPortrait(i);
        }

        private void Start()
        {
            if (PlayerProfile.IsCreated)
            {
                SceneManager.LoadScene(BureauScene);
                return;
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < portraits.Length; i++)
                if (portraits[i] != null) Destroy(portraits[i]);
        }

        private static void Fill(Texture2D t, Color color)
        {
            Color[] pixels = new Color[t.width * t.height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            t.SetPixels(pixels);
        }

        private static void Ellipse(Texture2D t, float cx, float cy, float rx, float ry, Color color)
        {
            int left = Mathf.Max(0, Mathf.FloorToInt(cx - rx));
            int right = Mathf.Min(t.width - 1, Mathf.CeilToInt(cx + rx));
            int bottom = Mathf.Max(0, Mathf.FloorToInt(cy - ry));
            int top = Mathf.Min(t.height - 1, Mathf.CeilToInt(cy + ry));
            for (int y = bottom; y <= top; y++)
                for (int x = left; x <= right; x++)
                    if (Mathf.Pow((x - cx) / rx, 2) + Mathf.Pow((y - cy) / ry, 2) <= 1)
                        t.SetPixel(x, y, color);
        }

        private static Texture2D BuildPortrait(int id)
        {
            const int size = 128;
            Texture2D t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            t.name = "DetectiveAvatar_" + id;
            t.filterMode = FilterMode.Bilinear;
            Color back = Color.Lerp(CoatColors[id], new Color(.91f,.79f,.58f), .66f);
            Fill(t, back);
            Ellipse(t, 64, 5, 56, 42, CoatColors[id]);
            Color skin = id == 2 || id == 5
                ? new Color(.73f,.48f,.34f)
                : new Color(.95f,.72f,.54f);
            Ellipse(t, 64, 60, 30, 37, HairColors[id]);
            Ellipse(t, 64, 55, 24, 31, skin);
            if (id % 2 == 0)
            {
                Ellipse(t, 57, 82, 34, 18, HairColors[id]);
                Ellipse(t, 40, 67, 9, 27, HairColors[id]);
            }
            else
            {
                Ellipse(t, 65, 88, 30, 15, HairColors[id]);
                Ellipse(t, 85, 69, 8, 22, HairColors[id]);
            }
            Color eye = new Color(.25f,.19f,.16f);
            Ellipse(t, 55, 55, 2.8f, 2.8f, eye);
            Ellipse(t, 73, 55, 2.8f, 2.8f, eye);
            Ellipse(t, 64, 40, 8, 2, new Color(.62f,.34f,.30f));
            // Shirt collar and scarf.
            Ellipse(t, 64, 18, 15, 10, new Color(.94f,.88f,.75f));
            t.Apply(false);
            return t;
        }

        private static GUIStyle Style(int size, FontStyle weight, TextAnchor align, Color color)
        {
            GUIStyle result = new GUIStyle(GUI.skin.label);
            result.fontSize = size;
            result.fontStyle = weight;
            result.alignment = align;
            result.normal.textColor = color;
            result.wordWrap = true;
            return result;
        }

        private static void Block(Rect r, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private void OnGUI()
        {
            float w = Screen.width;
            float h = Screen.height;
            if (w < 1 || h < 1) return;
            float scale = Mathf.Min(w / 1920f, h / 1080f);
            Rect safe = Screen.safeArea;
            // Center the panel with comfortable margins on all phone sizes.
            float panelWidth = Mathf.Min(1460f * scale, safe.width * .96f);
            float panelHeight = Mathf.Min(950f * scale, safe.height * .96f);
            float x = (w - panelWidth) * .5f;
            float y = (h - panelHeight) * .5f;
            if (background != null)
                GUI.DrawTexture(new Rect(0, 0, w, h), background, ScaleMode.ScaleAndCrop);
            else
                Block(new Rect(0, 0, w, h), new Color(.39f,.27f,.19f));

            Block(new Rect(0, 0, w, h), new Color(.11f,.07f,.05f,.72f));
            Block(new Rect(x, y, panelWidth, panelHeight), new Color(.96f,.90f,.79f));
            Block(new Rect(x + 10 * scale, y + 10 * scale, panelWidth - 20 * scale, 14 * scale),
                new Color(.58f,.36f,.23f));

            Rect R(float px, float py, float pw, float ph)
            {
                return new Rect(x + px * panelWidth / 1460f, y + py * panelHeight / 950f,
                    pw * panelWidth / 1460f, ph * panelHeight / 950f);
            }
            int Font(float value) { return Mathf.Max(11, Mathf.RoundToInt(value * scale)); }
            Color ink = new Color(.27f,.18f,.13f);

            GUI.Label(R(70, 44, 1320, 75), "ДОБРО ПОЖАЛОВАТЬ В БЮРО!",
                Style(Font(53), FontStyle.Bold, TextAnchor.MiddleCenter, ink));
            GUI.Label(R(85, 121, 1290, 67),
                "Как к вам обращаться? Выберите имя и своего персонажа.",
                Style(Font(29), FontStyle.Normal, TextAnchor.MiddleCenter, ink));
            GUI.Label(R(175, 206, 1110, 43), "ВАШЕ ИМЯ",
                Style(Font(28), FontStyle.Bold, TextAnchor.MiddleLeft, ink));

            GUI.SetNextControlName("PlayerName");
            GUIStyle input = new GUIStyle(GUI.skin.textField)
            {
                fontSize = Font(35),
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(15, 12, 8, 8)
            };
            playerName = GUI.TextField(R(175, 258, 1110, 76), playerName, 18, input);
            GUI.Label(R(175, 354, 1110, 49), "ВЫБЕРИТЕ АВАТАР",
                Style(Font(28), FontStyle.Bold, TextAnchor.MiddleLeft, ink));

            float cardW = 186, cardH = 245, gap = 20;
            float startX = (1460f - (cardW * 6 + gap * 5)) / 2f;
            for (int i = 0; i < 6; i++)
            {
                Rect card = R(startX + i * (cardW + gap), 421, cardW, cardH);
                Block(card, selectedAvatar == i
                    ? new Color(.77f,.62f,.38f)
                    : new Color(.88f,.80f,.67f));
                Rect picture = R(startX + i * (cardW + gap) + 16, 436, 154, 156);
                GUI.DrawTexture(picture, portraits[i]);
                GUI.Label(R(startX + i * (cardW + gap), 602, cardW, 49),
                    AvatarNames[i], Style(Font(19), FontStyle.Bold, TextAnchor.MiddleCenter, ink));
                if (GUI.Button(card, GUIContent.none, GUIStyle.none))
                    selectedAvatar = i;
            }

            GUI.Label(R(100, 696, 1260, 47), validation,
                Style(Font(24), FontStyle.Bold, TextAnchor.MiddleCenter, new Color(.70f,.20f,.14f)));
            Rect buttonRect = R(395, 761, 670, 106);
            Block(buttonRect, new Color(.22f,.46f,.32f));
            GUI.Label(buttonRect, "НАЧАТЬ РАССЛЕДОВАНИЕ",
                Style(Font(34), FontStyle.Bold, TextAnchor.MiddleCenter, Color.white));
            if (!entering && GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
            {
                if (string.IsNullOrWhiteSpace(playerName))
                {
                    validation = "Введите имя, чтобы продолжить";
                }
                else if (PlayerProfile.TryCreate(playerName, selectedAvatar))
                {
                    entering = true;
                    SceneManager.LoadScene(BureauScene);
                }
            }
        }
    }
}
