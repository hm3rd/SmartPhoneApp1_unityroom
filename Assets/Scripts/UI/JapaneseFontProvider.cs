using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// WebGLにも同梱される日本語Fontを、シーン配置済み／自動生成のLegacy Textへ共通適用する。
/// Assets/Resources/Fonts/JapaneseFont.ttf（または.otf）を配置して使用する。
/// </summary>
public static class JapaneseFontProvider
{
    private const string ResourcePath = "Fonts/JapaneseFont";
    private static Font japaneseFont;
    private static bool loadAttempted;
    private static bool warningLogged;
    private static TMP_FontAsset japaneseTmpFont;

    public static Font Font
    {
        get
        {
            if (!loadAttempted)
            {
                loadAttempted = true;
                japaneseFont = Resources.Load<Font>(ResourcePath);
            }

            if (japaneseFont == null && !warningLogged)
            {
                warningLogged = true;
                Debug.LogWarning(
                    "WebGL日本語フォントが未設定です。" +
                    "Assets/Resources/Fonts/JapaneseFont.ttf を配置してください。");
            }
            return japaneseFont;
        }
    }

    public static TMP_FontAsset TMPFont
    {
        get
        {
            if (japaneseTmpFont == null && Font != null)
            {
                japaneseTmpFont = TMP_FontAsset.CreateFontAsset(
                    Font,
                    90,
                    9,
                    GlyphRenderMode.SDFAA,
                    1024,
                    1024,
                    AtlasPopulationMode.Dynamic);
                japaneseTmpFont.name = "JapaneseFont Runtime SDF";
                japaneseTmpFont.isMultiAtlasTexturesEnabled = true;

                // 後からInstantiateされるTMPテキストも日本語へフォールバックできるようにする。
                if (TMP_Settings.fallbackFontAssets != null &&
                    !TMP_Settings.fallbackFontAssets.Contains(japaneseTmpFont))
                {
                    TMP_Settings.fallbackFontAssets.Add(japaneseTmpFont);
                }
            }
            return japaneseTmpFont;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        _ = TMPFont;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToScene(scene);
    }

    public static void Apply(Text text)
    {
        if (text == null) return;
        Font font = Font ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null) text.font = font;
    }

    public static void Apply(TMP_Text text)
    {
        if (text == null) return;
        TMP_FontAsset font = TMPFont;
        if (font != null) text.font = font;
    }

    private static void ApplyToScene(Scene scene)
    {
        Font font = Font;
        if (font == null) return;

        Text[] texts = Resources.FindObjectsOfTypeAll<Text>();
        foreach (Text text in texts)
        {
            if (text != null && text.gameObject.scene == scene)
            {
                text.font = font;
            }
        }


        TMP_Text[] tmpTexts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach (TMP_Text text in tmpTexts)
        {
            if (text != null && text.gameObject.scene == scene)
            {
                text.font = TMPFont;
            }
        }
    }
}
