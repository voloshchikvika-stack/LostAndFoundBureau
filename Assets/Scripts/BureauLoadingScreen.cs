using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LostAndFound.UI
{
    /// <summary>
    /// First scene of the player build. Loads the existing bureau asynchronously.
    /// Optional background: Assets/Resources/Loading/BureauStreet.png
    /// (Texture Type: Default, readable is not required).
    /// </summary>
    public sealed class BureauLoadingScreen : MonoBehaviour
    {
        private const string DestinationScene = "SampleScene";
        private const float MinimumVisibleSeconds = 1.2f;
        private AsyncOperation operation;
        private float progress;
        private string error;
        private Texture2D background;

        private void Awake()
        {
            // The loading scene is intentionally UI-only. Unity still needs a
            // rendering camera, otherwise Game View displays "No cameras rendering".
            // Create it automatically so already-installed scenes are repaired too.
            if (GetComponentInChildren<Camera>() == null)
            {
                GameObject cameraObject = new GameObject("LoadingCamera");
                cameraObject.transform.SetParent(transform, false);
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.16f, 0.11f, 0.09f);
                camera.cullingMask = 0;
                camera.orthographic = true;
                camera.depth = -100;
            }

            background = Resources.Load<Texture2D>("Loading/BureauStreet");
        }

        private IEnumerator Start()
        {
            // Let Unity render the loading screen before starting the next scene.
            yield return null;

            if (!Application.CanStreamedLevelBeLoaded(DestinationScene))
            {
                error = "Scene SampleScene is not in Build Settings.";
                yield break;
            }

            operation = SceneManager.LoadSceneAsync(DestinationScene, LoadSceneMode.Single);
            if (operation == null)
            {
                error = "Could not start scene loading.";
                yield break;
            }

            operation.allowSceneActivation = false;
            float started = Time.realtimeSinceStartup;

            while (operation.progress < 0.9f)
            {
                progress = Mathf.Max(progress, Mathf.Clamp01(operation.progress / 0.9f));
                yield return null;
            }

            progress = 1f;
            while (Time.realtimeSinceStartup - started < MinimumVisibleSeconds)
                yield return null;

            operation.allowSceneActivation = true;
        }

        private static void RectColor(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private static GUIStyle TextStyle(int fontSize, FontStyle weight, TextAnchor align, Color color)
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = weight,
                alignment = align,
                normal = { textColor = color },
                wordWrap = true
            };
        }

        private void OnGUI()
        {
            float w = Screen.width;
            float h = Screen.height;
            if (w <= 0 || h <= 0) return;

            if (background != null)
            {
                GUI.DrawTexture(new Rect(0, 0, w, h), background, ScaleMode.ScaleAndCrop);
            }
            else
            {
                DrawTemporaryStreet(w, h);
            }

            // Legible overlay for any photo, including a brightly lit exterior.
            RectColor(new Rect(0, h * 0.77f, w, h * 0.23f), new Color(0.13f, 0.085f, 0.055f, 0.80f));

            float titleSize = Mathf.Clamp(Mathf.Min(w * 0.041f, h * 0.083f), 22f, 57f);
            GUI.Label(new Rect(w * 0.06f, h * 0.035f, w * 0.88f, h * 0.19f),
                "БЮРО ПОТЕРЯННЫХ ВЕЩЕЙ",
                TextStyle(Mathf.RoundToInt(titleSize), FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.81f)));

            float barW = Mathf.Min(w * 0.72f, 850f);
            float barH = Mathf.Clamp(h * 0.032f, 14f, 35f);
            Rect bar = new Rect((w - barW) / 2f, h * 0.89f, barW, barH);
            RectColor(bar, new Color(0.24f, 0.14f, 0.09f));
            float inset = Mathf.Max(3f, barH * 0.12f);
            float fillWidth = Mathf.Max(0, (bar.width - inset * 2f) * progress);
            if (fillWidth > 0)
                RectColor(new Rect(bar.x + inset, bar.y + inset, fillWidth, bar.height - inset * 2f),
                    new Color(0.94f, 0.71f, 0.37f));

            float textSize = Mathf.Clamp(h * 0.035f, 15f, 32f);
            string message = error ?? "Открываем бюро...";
            GUI.Label(new Rect(w * 0.08f, h * 0.805f, w * 0.84f, h * 0.07f),
                message, TextStyle(Mathf.RoundToInt(textSize), FontStyle.Bold,
                    TextAnchor.MiddleCenter, Color.white));
        }

        private static void DrawTemporaryStreet(float w, float h)
        {
            RectColor(new Rect(0, 0, w, h), new Color(0.65f, 0.75f, 0.77f));
            RectColor(new Rect(0, h * 0.70f, w, h * 0.30f), new Color(0.40f, 0.39f, 0.34f));
            RectColor(new Rect(w * 0.19f, h * 0.18f, w * 0.62f, h * 0.55f),
                new Color(0.61f, 0.41f, 0.28f));
            RectColor(new Rect(w * 0.17f, h * 0.155f, w * 0.66f, h * 0.044f),
                new Color(0.32f, 0.23f, 0.19f));
            for (int i = 0; i < 4; i++)
            {
                float x = w * (0.245f + i * 0.145f);
                RectColor(new Rect(x, h * 0.27f, w * 0.095f, h * 0.19f),
                    new Color(0.25f, 0.20f, 0.15f));
                RectColor(new Rect(x + w * 0.009f, h * 0.285f, w * 0.077f, h * 0.16f),
                    new Color(1f, 0.77f, 0.44f));
            }

            RectColor(new Rect(w * 0.44f, h * 0.51f, w * 0.12f, h * 0.22f),
                new Color(0.25f, 0.16f, 0.12f));
            RectColor(new Rect(w * 0.455f, h * 0.525f, w * 0.09f, h * 0.13f),
                new Color(0.96f, 0.71f, 0.42f));
            RectColor(new Rect(w * 0.31f, h * 0.49f, w * 0.38f, h * 0.065f),
                new Color(0.24f, 0.16f, 0.12f));
            GUI.Label(new Rect(w * 0.31f, h * 0.49f, w * 0.38f, h * 0.065f),
                "БЮРО ПОТЕРЯННЫХ ВЕЩЕЙ",
                TextStyle(Mathf.Max(11, Mathf.RoundToInt(Mathf.Min(w * 0.017f, h * 0.028f))),
                    FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.87f, 0.59f)));
        }
    }
}
