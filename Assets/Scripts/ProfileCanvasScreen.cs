using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace LostAndFound.UI
{
    // Canvas-based onboarding; independent from the old IMGUI screen.
    public sealed class ProfileCanvasScreen : MonoBehaviour
    {
        private const string BureauScene = "SampleScene";
        private readonly Color cream = new Color(.985f, .942f, .863f);
        private readonly Color ink = new Color(.31f, .21f, .16f);
        private readonly Color green = new Color(.23f, .48f, .36f);
        private readonly Color gold = new Color(.80f, .60f, .35f);
        private Font font;
        private InputField nameField;
        private Text feedback;
        private Image[] cardBorders = new Image[3];
        private int chosen;
        private bool opening;
        private Sprite[] fallbackSprites;

        private void Awake()
        {
            // Opening the profile scene directly is supported for testing.
            EnsureCamera();
            if (PlayerProfile.IsCreated)
            {
                SceneManager.LoadScene(BureauScene);
                return;
            }

            BuildCanvas();
        }

        private static void EnsureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject root = new GameObject("ProfileCamera");
                cam = root.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(.20f, .14f, .10f);
                cam.cullingMask = 0;
            }
            if (UnityEngine.Object.FindFirstObjectByType<AudioListener>() == null)
                cam.gameObject.AddComponent<AudioListener>();
        }

        private void BuildCanvas()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Segoe UI" }, 32);

            GameObject canvasObject = new GameObject("Profile UI Canvas", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            EnsureEventSystem();

            RectTransform root = canvasObject.GetComponent<RectTransform>();
            Stretch(root);
            RawImage backdrop = AddRaw("Street backdrop", root);
            Stretch(backdrop.rectTransform);
            backdrop.texture = Resources.Load<Texture2D>("Loading/BureauStreet");
            backdrop.color = backdrop.texture != null ? Color.white : new Color(.42f, .30f, .22f);
            // A translucent dimmer keeps all input labels readable.
            Image shade = AddImage("Dim backdrop", root, new Color(.15f, .10f, .08f, .65f));
            Stretch(shade.rectTransform);

            Image panel = AddImage("Paper profile card", root, cream);
            SetRect(panel.rectTransform, 0, 0, 1250, 900);
            Image stripe = AddImage("Top gold ribbon", panel.rectTransform, gold);
            SetRect(stripe.rectTransform, 0, 420, 1250, 18);

            AddText("Title", panel.rectTransform, "Давайте познакомимся!", 60,
                new Vector2(0, 345), new Vector2(1160, 90), ink, FontStyle.Bold);
            AddText("Subtitle", panel.rectTransform, "Как вас зовут?", 34,
                new Vector2(0, 250), new Vector2(1050, 65), ink, FontStyle.Normal);

            Image nameBox = AddImage("Name box", panel.rectTransform, new Color(1f, .985f, .955f));
            SetRect(nameBox.rectTransform, 0, 135, 850, 100);
            Outline inputOutline = nameBox.gameObject.AddComponent<Outline>();
            inputOutline.effectColor = new Color(.76f, .58f, .37f);
            inputOutline.effectDistance = new Vector2(3, 3);
            nameField = nameBox.gameObject.AddComponent<InputField>();
            nameField.characterLimit = 18;
            nameField.contentType = InputField.ContentType.Standard;
            nameField.lineType = InputField.LineType.SingleLine;

            Text inputText = AddText("Player name", nameBox.rectTransform, "", 42,
                Vector2.zero, new Vector2(760, 90), ink, FontStyle.Normal);
            inputText.alignment = TextAnchor.MiddleLeft;
            nameField.textComponent = inputText;
            Text placeholder = AddText("Hint", nameBox.rectTransform, "Ваше имя", 38,
                Vector2.zero, new Vector2(760, 90), new Color(.61f,.52f,.44f), FontStyle.Italic);
            placeholder.alignment = TextAnchor.MiddleLeft;
            nameField.placeholder = placeholder;

            AddText("Choose avatar", panel.rectTransform, "Выберите значок профиля", 32,
                new Vector2(0, 30), new Vector2(1040, 60), ink, FontStyle.Normal);

            Sprite[] pieces = Resources.LoadAll<Sprite>("Match3/Pieces");
            Texture2D[] textures = Resources.LoadAll<Texture2D>("Match3/Pieces");
            for (int i = 0; i < 3; i++)
            {
                int avatarIndex = i;
                Image card = AddImage("Avatar " + (i + 1), panel.rectTransform,
                    i == 0 ? new Color(.92f,.77f,.53f) : new Color(.87f,.82f,.74f));
                cardBorders[i] = card;
                SetRect(card.rectTransform, (i - 1) * 260, -125, 215, 205);

                Image inner = AddImage("Icon background", card.rectTransform,
                    new Color(1f, .972f, .925f));
                SetRect(inner.rectTransform, 0, 0, 193, 183);
                Image piece = AddImage("Match 3 piece", inner.rectTransform, Color.white);
                SetRect(piece.rectTransform, 0, 0, 135, 135);
                piece.preserveAspect = true;
                if (i < pieces.Length && pieces[i] != null)
                    piece.sprite = pieces[i];
                else if (i < textures.Length && textures[i] != null)
                    piece.sprite = Sprite.Create(textures[i], new Rect(0,0,textures[i].width,textures[i].height),
                        new Vector2(.5f,.5f));
                else
                    piece.sprite = CreateFallback(i);

                Button choice = card.gameObject.AddComponent<Button>();
                choice.transition = Selectable.Transition.None;
                choice.onClick.AddListener(() => Choose(avatarIndex));
            }

            feedback = AddText("Validation message", panel.rectTransform, "", 28,
                new Vector2(0, -265), new Vector2(1080, 55), new Color(.72f,.27f,.22f), FontStyle.Bold);

            Image go = AddImage("Continue", panel.rectTransform, green);
            SetRect(go.rectTransform, 0, -355, 575, 102);
            AddText("Continue label", go.rectTransform, "ПРОДОЛЖИТЬ", 39,
                Vector2.zero, new Vector2(550, 95), Color.white, FontStyle.Bold);
            Button next = go.gameObject.AddComponent<Button>();
            next.onClick.AddListener(Continue);

            // Scale paper card down on mobile and small landscape previews.
            AspectRatioFitter fitter = panel.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.None;
            RectTransform panelRect = panel.rectTransform;
            panelRect.anchorMin = new Vector2(.5f, .5f);
            panelRect.anchorMax = new Vector2(.5f, .5f);
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;
            GameObject events = new GameObject("Profile EventSystem", typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            DontDestroyOnLoad(events);
        }

        private static Image AddImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static RawImage AddRaw(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RawImage>();
        }

        private Text AddText(string name, Transform parent, string value, int size,
            Vector2 center, Vector2 dimensions, Color color, FontStyle weight)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            Text t = go.GetComponent<Text>();
            t.font = font;
            t.text = value;
            t.fontSize = size;
            t.fontStyle = weight;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 14;
            t.resizeTextMaxSize = size;
            SetRect(t.rectTransform, center.x, center.y, dimensions.x, dimensions.y);
            return t;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(x,y);
            rect.sizeDelta = new Vector2(w,h);
        }

        private static Sprite CreateFallback(int index)
        {
            int n = 128;
            Texture2D tex = new Texture2D(n,n,TextureFormat.RGBA32,false);
            Color color = new Color[] {
                new Color(.92f,.56f,.30f),
                new Color(.41f,.66f,.78f),
                new Color(.67f,.51f,.77f)
            }[index];
            for(int y=0;y<n;y++)
            for(int x=0;x<n;x++)
            {
                float dx = (x-63.5f)/57f, dy=(y-63.5f)/57f;
                bool inside = Mathf.Abs(dx)+Mathf.Abs(dy)<1.1f;
                tex.SetPixel(x,y,inside ? color : Color.clear);
            }
            tex.Apply();
            return Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(.5f,.5f));
        }

        private void Choose(int i)
        {
            chosen = i;
            for(int n=0;n<cardBorders.Length;n++)
                cardBorders[n].color = n == i
                    ? new Color(.92f,.77f,.53f)
                    : new Color(.87f,.82f,.74f);
        }

        private void Continue()
        {
            if (opening) return;
            string name = nameField.text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                feedback.text = "Введите имя, чтобы продолжить";
                return;
            }
            if (!PlayerProfile.TryCreate(name, chosen))
            {
                feedback.text = "Проверьте имя (до 18 символов)";
                return;
            }
            opening = true;
            SceneManager.LoadScene(BureauScene);
        }
    }
}
