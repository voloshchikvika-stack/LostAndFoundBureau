using UnityEngine;

namespace LostAndFound.Mobile
{
    public static class MobileLayout
    {
        public const float DesignWidth = 1920f;
        public const float DesignHeight = 1080f;

        public static Rect SafeArea
        {
            get
            {
                Rect area = Screen.safeArea;
                if (area.width <= 1f || area.height <= 1f)
                    return new Rect(0f, 0f, Screen.width, Screen.height);

                return area;
            }
        }

        public static float Scale
        {
            get
            {
                Rect area = SafeArea;
                return Mathf.Clamp(
                    Mathf.Min(
                        area.width / DesignWidth,
                        area.height / DesignHeight),
                    0.45f,
                    2.5f);
            }
        }

        // GUI coordinates use a top-left origin, while Screen.safeArea uses
        // bottom-left coordinates.
        public static Rect SafeAreaGui
        {
            get
            {
                Rect area = SafeArea;
                return new Rect(
                    area.xMin,
                    Screen.height - area.yMax,
                    area.width,
                    area.height);
            }
        }

        public static Vector2 Offset
        {
            get
            {
                Rect area = SafeAreaGui;
                float scale = Scale;

                return new Vector2(
                    area.x + (area.width - DesignWidth * scale) * 0.5f,
                    area.y + (area.height - DesignHeight * scale) * 0.5f);
            }
        }

        public static Rect R(
            float x,
            float y,
            float width,
            float height)
        {
            float scale = Scale;
            Vector2 offset = Offset;

            return new Rect(
                offset.x + x * scale,
                offset.y + y * scale,
                width * scale,
                height * scale);
        }

        public static Vector2 ScreenToGui(Vector2 screenPosition)
        {
            return new Vector2(
                screenPosition.x,
                Screen.height - screenPosition.y);
        }

        public static float SwipeThreshold
        {
            get
            {
                // Scale from the 1920x1080 design reference, but keep a useful
                // minimum on low-resolution simulator profiles.
                return Mathf.Max(28f, 52f * Scale);
            }
        }
    }

    public static class MobileRuntime
    {
        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Configure()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            if (!Application.isMobilePlatform)
                return;

            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;

            Input.multiTouchEnabled = true;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}
