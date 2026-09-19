using UnityEngine;

namespace LostAndFound.Match3
{
    public static class Match3VisualFactory
    {
        private const int TokenSize = 96;

        public static Sprite CreateTokenSprite(Color background, int type, out Texture2D texture)
        {
            texture = new Texture2D(TokenSize, TokenSize, TextureFormat.RGBA32, false);
            texture.name = $"Match3Token_{type}";
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color32[] pixels = new Color32[TokenSize * TokenSize];
            Color border = Color.Lerp(background, Color.black, 0.25f);

            for (int y = 0; y < TokenSize; y++)
            {
                for (int x = 0; x < TokenSize; x++)
                {
                    bool outer = InsideRoundedRect(x, y, 4, 4, TokenSize - 8, TokenSize - 8, 20);
                    bool inner = InsideRoundedRect(x, y, 8, 8, TokenSize - 16, TokenSize - 16, 16);

                    if (!outer)
                    {
                        pixels[y * TokenSize + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    Color c = inner ? background : border;
                    float highlight = Mathf.Clamp01((y - 48f) / 48f) * 0.12f;
                    c = Color.Lerp(c, Color.white, highlight);
                    pixels[y * TokenSize + x] = c;
                }
            }

            texture.SetPixels32(pixels);
            DrawIcon(texture, type, new Color(1f, 0.97f, 0.9f, 1f));
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, TokenSize, TokenSize),
                new Vector2(0.5f, 0.5f),
                TokenSize);
        }

        public static Texture2D CreateRoundedTexture(Color color, int size = 64, int radius = 16)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color32[] pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = InsideRoundedRect(x, y, 0, 0, size, size, radius);
                    pixels[y * size + x] = inside ? (Color32)color : new Color32(0, 0, 0, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        public static Sprite CreateGradientBackgroundSprite(out Texture2D texture)
        {
            const int width = 16;
            const int height = 128;

            texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color top = new Color(0.14f, 0.20f, 0.28f, 1f);
            Color bottom = new Color(0.035f, 0.055f, 0.085f, 1f);

            for (int y = 0; y < height; y++)
            {
                float t = y / (float)(height - 1);
                Color row = Color.Lerp(bottom, top, t);

                for (int x = 0; x < width; x++)
                    texture.SetPixel(x, y, row);
            }

            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.5f),
                height);
        }

        public static Texture2D CreateSolidTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static bool InsideRoundedRect(int px, int py, int x, int y, int width, int height, int radius)
        {
            float cx = x + width * 0.5f;
            float cy = y + height * 0.5f;
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            float dx = Mathf.Max(Mathf.Abs(px + 0.5f - cx) - (halfW - radius), 0f);
            float dy = Mathf.Max(Mathf.Abs(py + 0.5f - cy) - (halfH - radius), 0f);

            return dx * dx + dy * dy <= radius * radius;
        }

        private static void DrawIcon(Texture2D texture, int type, Color color)
        {
            switch (type)
            {
                case 0:
                    DrawFootprints(texture, color);
                    break;
                case 1:
                    DrawKey(texture, color);
                    break;
                case 2:
                    DrawMap(texture, color);
                    break;
                case 3:
                    DrawNote(texture, color);
                    break;
                case 4:
                    DrawTag(texture, color);
                    break;
                default:
                    DrawMagnifier(texture, color);
                    break;
            }
        }

        private static void DrawFootprints(Texture2D t, Color c)
        {
            FillEllipse(t, 38, 55, 10, 17, c);
            FillEllipse(t, 58, 40, 10, 17, c);

            FillCircle(t, 30, 71, 4, c);
            FillCircle(t, 36, 74, 4, c);
            FillCircle(t, 43, 73, 4, c);

            FillCircle(t, 51, 56, 4, c);
            FillCircle(t, 58, 59, 4, c);
            FillCircle(t, 65, 57, 4, c);
        }

        private static void DrawKey(Texture2D t, Color c)
        {
            DrawCircleOutline(t, 36, 58, 13, 5, c);
            FillRect(t, 46, 54, 27, 8, c);
            FillRect(t, 66, 45, 7, 12, c);
            FillRect(t, 58, 48, 7, 9, c);
        }

        private static void DrawMap(Texture2D t, Color c)
        {
            DrawLine(t, 25, 28, 25, 70, 5, c);
            DrawLine(t, 47, 23, 47, 66, 5, c);
            DrawLine(t, 70, 27, 70, 70, 5, c);
            DrawLine(t, 25, 70, 47, 64, 5, c);
            DrawLine(t, 47, 64, 70, 70, 5, c);
            DrawLine(t, 25, 28, 47, 22, 5, c);
            DrawLine(t, 47, 22, 70, 28, 5, c);
            DrawLine(t, 33, 51, 42, 46, 4, c);
            DrawLine(t, 42, 46, 54, 52, 4, c);
            FillCircle(t, 58, 54, 4, c);
        }

        private static void DrawNote(Texture2D t, Color c)
        {
            FillRect(t, 27, 23, 43, 51, c);
            Color cut = new Color(0f, 0f, 0f, 0f);
            FillRect(t, 34, 57, 29, 4, cut);
            FillRect(t, 34, 47, 25, 4, cut);
            FillRect(t, 34, 37, 20, 4, cut);
        }

        private static void DrawTag(Texture2D t, Color c)
        {
            FillRect(t, 28, 29, 42, 40, c);
            FillCircle(t, 32, 49, 13, c);
            FillCircle(t, 32, 49, 5, new Color(0.35f, 0.35f, 0.35f, 1f));
            DrawLine(t, 67, 33, 76, 42, 5, c);
            DrawLine(t, 76, 42, 67, 51, 5, c);
        }

        private static void DrawMagnifier(Texture2D t, Color c)
        {
            DrawCircleOutline(t, 43, 56, 17, 6, c);
            DrawLine(t, 55, 43, 73, 25, 8, c);
        }

        private static void FillCircle(Texture2D t, int cx, int cy, int radius, Color c)
        {
            int r2 = radius * radius;

            for (int y = cy - radius; y <= cy + radius; y++)
            {
                for (int x = cx - radius; x <= cx + radius; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;

                    if (dx * dx + dy * dy <= r2)
                        SetPixelSafe(t, x, y, c);
                }
            }
        }

        private static void FillEllipse(Texture2D t, int cx, int cy, int rx, int ry, Color c)
        {
            for (int y = cy - ry; y <= cy + ry; y++)
            {
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    float dx = (x - cx) / (float)rx;
                    float dy = (y - cy) / (float)ry;

                    if (dx * dx + dy * dy <= 1f)
                        SetPixelSafe(t, x, y, c);
                }
            }
        }

        private static void DrawCircleOutline(Texture2D t, int cx, int cy, int radius, int thickness, Color c)
        {
            int outer2 = radius * radius;
            int inner = Mathf.Max(0, radius - thickness);
            int inner2 = inner * inner;

            for (int y = cy - radius; y <= cy + radius; y++)
            {
                for (int x = cx - radius; x <= cx + radius; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;
                    int d2 = dx * dx + dy * dy;

                    if (d2 <= outer2 && d2 >= inner2)
                        SetPixelSafe(t, x, y, c);
                }
            }
        }

        private static void FillRect(Texture2D t, int x, int y, int width, int height, Color c)
        {
            for (int py = y; py < y + height; py++)
            {
                for (int px = x; px < x + width; px++)
                    SetPixelSafe(t, px, py, c);
            }
        }

        private static void DrawLine(Texture2D t, int x0, int y0, int x1, int y1, int thickness, Color c)
        {
            int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));

            if (steps == 0)
            {
                FillCircle(t, x0, y0, Mathf.Max(1, thickness / 2), c);
                return;
            }

            for (int i = 0; i <= steps; i++)
            {
                float k = i / (float)steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, k));
                int y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, k));
                FillCircle(t, x, y, Mathf.Max(1, thickness / 2), c);
            }
        }

        private static void SetPixelSafe(Texture2D t, int x, int y, Color c)
        {
            if (x < 0 || x >= t.width || y < 0 || y >= t.height)
                return;

            t.SetPixel(x, y, c);
        }
    }
}
