using UnityEngine;
using LostAndFound.Mobile;

namespace LostAndFound.Core
{
    [DefaultExecutionOrder(-1000)]
    public sealed class GameSettingsManager : MonoBehaviour
    {
        private const string MusicKey = "LostAndFound.Settings.Music";
        private const string SoundKey = "LostAndFound.Settings.Sound";
        private const string VibrationKey = "LostAndFound.Settings.Vibration";

        private static GameSettingsManager instance;

        private AudioSource musicSource;
        private AudioSource sfxSource;

        private AudioClip clickClip;
        private AudioClip matchClip;
        private AudioClip invalidClip;
        private AudioClip bonusClip;
        private AudioClip winClip;
        private AudioClip loseClip;
        private AudioClip puzzleClip;
        private AudioClip caseClip;

        private Texture2D panelTexture;
        private Texture2D cardTexture;
        private Texture2D buttonTexture;
        private Texture2D enabledTexture;
        private Texture2D disabledTexture;
        private Texture2D overlayTexture;
        private Font uiFont;

        private bool settingsOpen;
        private bool pendingMusic;
        private bool pendingSound;
        private bool pendingVibration;

        public static bool IsSettingsOpen =>
            instance != null && instance.settingsOpen;

        public static bool MusicEnabled =>
            PlayerPrefs.GetInt(MusicKey, 1) == 1;

        public static bool SoundEnabled =>
            PlayerPrefs.GetInt(SoundKey, 1) == 1;

        public static bool VibrationEnabled =>
            PlayerPrefs.GetInt(VibrationKey, 1) == 1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
                return;

            GameObject root = new GameObject("GameSettingsManager");
            DontDestroyOnLoad(root);
            instance = root.AddComponent<GameSettingsManager>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = 0.28f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.85f;

            BuildUi();
            BuildAudio();
            ApplySavedSettings();
        }

        private void BuildAudio()
        {
            AudioClip customMusic =
                Resources.Load<AudioClip>("Audio/Music/MainTheme");

            musicSource.clip = customMusic != null
                ? customMusic
                : CreateAmbientMusic();

            clickClip =
                Resources.Load<AudioClip>("Audio/SFX/Click") ??
                CreateTone("ui_click", 560f, 0.055f, 0.18f);

            matchClip =
                Resources.Load<AudioClip>("Audio/SFX/Match") ??
                CreateTone("match", 760f, 0.085f, 0.22f);

            invalidClip =
                Resources.Load<AudioClip>("Audio/SFX/Invalid") ??
                CreateSweep("invalid", 330f, 190f, 0.12f, 0.16f);

            bonusClip =
                Resources.Load<AudioClip>("Audio/SFX/Bonus") ??
                CreateSweep("bonus", 430f, 1080f, 0.20f, 0.25f);

            winClip =
                Resources.Load<AudioClip>("Audio/SFX/Win") ??
                CreateArpeggio("win",
                    new[] { 523.25f, 659.25f, 783.99f, 1046.5f },
                    0.12f, 0.28f);

            loseClip =
                Resources.Load<AudioClip>("Audio/SFX/Lose") ??
                CreateArpeggio("lose",
                    new[] { 392f, 329.63f, 261.63f },
                    0.16f, 0.20f);

            puzzleClip =
                Resources.Load<AudioClip>("Audio/SFX/Puzzle") ??
                CreateArpeggio("puzzle",
                    new[] { 440f, 554.37f, 659.25f },
                    0.10f, 0.22f);

            caseClip =
                Resources.Load<AudioClip>("Audio/SFX/Case") ??
                CreateArpeggio("case",
                    new[] { 392f, 523.25f, 659.25f, 783.99f },
                    0.11f, 0.24f);
        }

        private void BuildUi()
        {
            panelTexture = MakeRoundedTexture(
                new Color(0.98f, 0.92f, 0.79f, 1f), 22);
            cardTexture = MakeRoundedTexture(
                new Color(0.93f, 0.82f, 0.64f, 1f), 18);
            buttonTexture = MakeRoundedTexture(
                new Color(0.52f, 0.29f, 0.16f, 1f), 18);
            enabledTexture = MakeRoundedTexture(
                new Color(0.35f, 0.55f, 0.16f, 1f), 18);
            disabledTexture = MakeRoundedTexture(
                new Color(0.73f, 0.59f, 0.43f, 1f), 18);
            overlayTexture = MakeTexture(
                new Color(0f, 0f, 0f, 0.60f));

            uiFont = Font.CreateDynamicFontFromOSFont(
                new[] { "Segoe UI", "Arial" },
                32);
        }

        private void ApplySavedSettings()
        {
            if (musicSource != null)
            {
                if (MusicEnabled)
                {
                    if (musicSource.clip != null && !musicSource.isPlaying)
                        musicSource.Play();
                }
                else
                {
                    musicSource.Stop();
                }
            }

            if (!SoundEnabled && sfxSource != null)
                sfxSource.Stop();
        }

        private void ApplyPendingSettingsImmediately()
        {
            if (musicSource != null)
            {
                if (pendingMusic)
                {
                    if (musicSource.clip != null && !musicSource.isPlaying)
                        musicSource.Play();
                }
                else
                {
                    musicSource.Stop();
                }
            }

            if (!pendingSound && sfxSource != null)
                sfxSource.Stop();
        }

        private void OpenSettings()
        {
            pendingMusic = MusicEnabled;
            pendingSound = SoundEnabled;
            pendingVibration = VibrationEnabled;
            settingsOpen = true;
            PlayUiClick();
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetInt(MusicKey, pendingMusic ? 1 : 0);
            PlayerPrefs.SetInt(SoundKey, pendingSound ? 1 : 0);
            PlayerPrefs.SetInt(VibrationKey, pendingVibration ? 1 : 0);
            PlayerPrefs.Save();

            settingsOpen = false;
            ApplySavedSettings();

            if (SoundEnabled)
                PlayUiClick();
        }

        private void CloseWithoutSaving()
        {
            settingsOpen = false;

            // Restore the last saved state if the player closes with X.
            pendingMusic = MusicEnabled;
            pendingSound = SoundEnabled;
            pendingVibration = VibrationEnabled;
            ApplySavedSettings();

            if (SoundEnabled)
                PlayUiClick();
        }

        private void OnGUI()
        {
            GUI.depth = -1000;

            float scale = MobileLayout.Scale;
            Rect gearRect = MobileLayout.R(34, 952, 82, 82);

            if (!settingsOpen)
            {
                GUIStyle gearStyle = ButtonStyle(34, scale);
                if (GUI.Button(gearRect, "⚙", gearStyle))
                    OpenSettings();
                return;
            }

            GUI.DrawTexture(
                new Rect(0f, 0f, Screen.width, Screen.height),
                overlayTexture,
                ScaleMode.StretchToFill);

            Rect panel = MobileLayout.R(570, 155, 780, 760);
            DrawNineSlice(panel, panelTexture, 28);

            GUIStyle title = LabelStyle(
                34, FontStyle.Bold, TextAnchor.MiddleCenter,
                scale, new Color(0.30f, 0.17f, 0.10f));

            GUI.Label(
                MobileLayout.R(655, 200, 610, 64),
                "НАСТРОЙКИ",
                title);

            GUIStyle closeStyle = ButtonStyle(22, scale);
            if (GUI.Button(
                MobileLayout.R(1240, 190, 62, 62),
                "×",
                closeStyle))
            {
                CloseWithoutSaving();
                return;
            }

            DrawSettingRow(
                scale,
                320f,
                "МУЗЫКА",
                ref pendingMusic);

            DrawSettingRow(
                scale,
                455f,
                "ЗВУКИ",
                ref pendingSound);

            DrawSettingRow(
                scale,
                590f,
                "ВИБРАЦИЯ",
                ref pendingVibration);

            GUIStyle saveStyle = ButtonStyle(22, scale);
            if (GUI.Button(
                MobileLayout.R(760, 760, 400, 82),
                "СОХРАНИТЬ",
                saveStyle))
            {
                SaveSettings();
            }
        }

        private void DrawSettingRow(
            float scale,
            float y,
            string label,
            ref bool value)
        {
            Rect row = MobileLayout.R(665, y, 590, 96);
            DrawNineSlice(row, cardTexture, 18);

            GUIStyle labelStyle = LabelStyle(
                23, FontStyle.Bold, TextAnchor.MiddleLeft,
                scale, new Color(0.31f, 0.19f, 0.12f));

            GUI.Label(
                MobileLayout.R(705, y + 22, 260, 50),
                label,
                labelStyle);

            Rect toggle = MobileLayout.R(1010, y + 16, 205, 64);
            Texture2D background =
                value ? enabledTexture : disabledTexture;
            DrawNineSlice(toggle, background, 18);

            GUIStyle toggleStyle = LabelStyle(
                18, FontStyle.Bold, TextAnchor.MiddleCenter,
                scale, value ? Color.white : new Color(0.35f, 0.22f, 0.15f));

            GUI.Label(
                toggle,
                value ? "ВКЛ" : "ВЫКЛ",
                toggleStyle);

            GUIStyle invisible = new GUIStyle(GUIStyle.none);
            if (GUI.Button(toggle, GUIContent.none, invisible))
            {
                bool wasSoundOn = pendingSound;
                value = !value;

                // Preview every change immediately. Music stops/starts at once,
                // and disabling sounds cuts any currently playing SFX.
                ApplyPendingSettingsImmediately();

                // Do not play a click after the player has just turned sound off.
                if (pendingSound && wasSoundOn)
                    PlayUiClick();
            }
        }

        private GUIStyle LabelStyle(
            int size,
            FontStyle fontStyle,
            TextAnchor alignment,
            float scale,
            Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                font = uiFont,
                fontSize = Mathf.RoundToInt(size * scale),
                fontStyle = fontStyle,
                alignment = alignment,
                wordWrap = true
            };
            style.normal.textColor = color;
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
                border = new RectOffset(18, 18, 18, 18),
                padding = new RectOffset(8, 8, 6, 6)
            };
            style.normal.background = buttonTexture;
            style.hover.background = buttonTexture;
            style.active.background = buttonTexture;
            style.normal.textColor = new Color(1f, 0.95f, 0.84f);
            style.hover.textColor = Color.white;
            style.active.textColor = Color.white;
            return style;
        }

        private static void DrawNineSlice(
            Rect rect,
            Texture2D texture,
            int border)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(border, border, border, border),
                padding = new RectOffset(0, 0, 0, 0)
            };
            style.normal.background = texture;
            GUI.Box(rect, GUIContent.none, style);
        }

        public static void PlayUiClick()
        {
            Play(instance != null ? instance.clickClip : null, false);
        }

        public static void PlayMatch()
        {
            Play(instance != null ? instance.matchClip : null, false);
        }

        public static void PlayInvalid()
        {
            Play(instance != null ? instance.invalidClip : null, false);
        }

        public static void PlayBonus()
        {
            Play(instance != null ? instance.bonusClip : null, true);
        }

        public static void PlayWin()
        {
            Play(instance != null ? instance.winClip : null, true);
        }

        public static void PlayLose()
        {
            Play(instance != null ? instance.loseClip : null, true);
        }

        public static void PlayPuzzleSuccess()
        {
            Play(instance != null ? instance.puzzleClip : null, true);
        }

        public static void PlayCaseComplete()
        {
            Play(instance != null ? instance.caseClip : null, true);
        }

        private static void Play(AudioClip clip, bool haptic)
        {
            if (instance != null &&
                SoundEnabled &&
                clip != null &&
                instance.sfxSource != null)
            {
                instance.sfxSource.PlayOneShot(clip);
            }

            if (haptic)
                Vibrate();
        }

        public static void Vibrate()
        {
            if (!VibrationEnabled || !Application.isMobilePlatform)
                return;

            Handheld.Vibrate();
        }

        private static AudioClip CreateAmbientMusic()
        {
            const int sampleRate = 22050;
            const float duration = 12f;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            float[] chord = { 196f, 246.94f, 293.66f, 392f };
            float[] melody = {
                392f, 440f, 493.88f, 440f,
                349.23f, 392f, 440f, 329.63f
            };

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float value = 0f;

                for (int c = 0; c < chord.Length; c++)
                    value += Mathf.Sin(2f * Mathf.PI * chord[c] * t) * 0.010f;

                int noteIndex =
                    Mathf.FloorToInt(t / 1.5f) % melody.Length;
                float local = (t % 1.5f) / 1.5f;
                float envelope =
                    Mathf.Sin(Mathf.PI * Mathf.Clamp01(local));
                value +=
                    Mathf.Sin(2f * Mathf.PI * melody[noteIndex] * t) *
                    0.018f *
                    envelope;

                value +=
                    Mathf.Sin(2f * Mathf.PI * 98f * t) * 0.008f;

                samples[i] = value;
            }

            AudioClip clip = AudioClip.Create(
                "GeneratedDetectiveTheme",
                sampleCount,
                1,
                sampleRate,
                false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateTone(
            string name,
            float frequency,
            float duration,
            float amplitude)
        {
            const int sampleRate = 22050;
            int sampleCount =
                Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float normalized = i / (float)sampleCount;
                float envelope = 1f - normalized;
                samples[i] =
                    Mathf.Sin(2f * Mathf.PI * frequency * t) *
                    amplitude *
                    envelope;
            }

            AudioClip clip = AudioClip.Create(
                name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateSweep(
            string name,
            float startFrequency,
            float endFrequency,
            float duration,
            float amplitude)
        {
            const int sampleRate = 22050;
            int sampleCount =
                Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            float[] samples = new float[sampleCount];
            float phase = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float normalized = i / (float)sampleCount;
                float frequency = Mathf.Lerp(
                    startFrequency,
                    endFrequency,
                    normalized);

                phase += 2f * Mathf.PI * frequency / sampleRate;
                float envelope = 1f - normalized;
                samples[i] =
                    Mathf.Sin(phase) * amplitude * envelope;
            }

            AudioClip clip = AudioClip.Create(
                name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateArpeggio(
            string name,
            float[] frequencies,
            float noteDuration,
            float amplitude)
        {
            const int sampleRate = 22050;
            int samplesPerNote =
                Mathf.Max(1, Mathf.RoundToInt(sampleRate * noteDuration));
            int sampleCount = samplesPerNote * frequencies.Length;
            float[] samples = new float[sampleCount];

            for (int n = 0; n < frequencies.Length; n++)
            {
                for (int i = 0; i < samplesPerNote; i++)
                {
                    float t = i / (float)sampleRate;
                    float normalized = i / (float)samplesPerNote;
                    float envelope = 1f - normalized;

                    samples[n * samplesPerNote + i] =
                        Mathf.Sin(
                            2f * Mathf.PI *
                            frequencies[n] * t) *
                        amplitude *
                        envelope;
                }
            }

            AudioClip clip = AudioClip.Create(
                name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static Texture2D MakeTexture(Color color)
        {
            Texture2D texture =
                new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.SetPixels(new[] { color, color, color, color });
            texture.Apply();
            return texture;
        }

        private static Texture2D MakeRoundedTexture(
            Color color,
            int radius)
        {
            const int size = 64;
            Texture2D texture =
                new Texture2D(size, size, TextureFormat.RGBA32, false);

            Color clear =
                new Color(color.r, color.g, color.b, 0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = RoundedContains(
                        x + 0.5f,
                        y + 0.5f,
                        size,
                        size,
                        radius);

                    texture.SetPixel(x, y, inside ? color : clear);
                }
            }

            texture.Apply();
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }

        private static bool RoundedContains(
            float x,
            float y,
            float width,
            float height,
            float radius)
        {
            float clampedX =
                Mathf.Clamp(x, radius, width - radius);
            float clampedY =
                Mathf.Clamp(y, radius, height - radius);
            float dx = x - clampedX;
            float dy = y - clampedY;
            return dx * dx + dy * dy <= radius * radius;
        }
    }
}
