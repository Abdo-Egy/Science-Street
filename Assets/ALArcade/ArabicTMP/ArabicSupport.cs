using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ALArcade.ArabicTMP
{
    /// <summary>
    /// AL-Arcade Ultimate Arabic Support for Unity TextMeshPro
    /// FINAL VERSION - All issues fixed
    /// </summary>
    public static class ArabicSupport
    {

        #region Unicode Ranges

        // Arabic Unicode blocks
        private const int ARABIC_BASIC_START = 0x0600;
        private const int ARABIC_BASIC_END = 0x06FF;
        private const int ARABIC_SUPPLEMENT_START = 0x0750;
        private const int ARABIC_SUPPLEMENT_END = 0x077F;
        private const int ARABIC_EXTENDED_A_START = 0x08A0;
        private const int ARABIC_EXTENDED_A_END = 0x08FF;
        private const int ARABIC_PRESENTATION_A_START = 0xFB50;
        private const int ARABIC_PRESENTATION_A_END = 0xFDFF;
        private const int ARABIC_PRESENTATION_B_START = 0xFE70;
        private const int ARABIC_PRESENTATION_B_END = 0xFEFF;

        // Special control characters
        private const char ZWJ = (char)0x200D;
        private const char ZWNJ = (char)0x200C;
        private const char RLM = (char)0x200F;
        private const char LRM = (char)0x200E;
        private const char ALM = (char)0x061C;

        #endregion

        #region Enums

        private enum LetterForm
        {
            Isolated = 0,
            Initial = 1,
            Medial = 2,
            Final = 3
        }

        private enum ConnectionType
        {
            None,
            Right,
            Dual,
            Transparent
        }

        private enum TextDirection
        {
            LTR,
            RTL,
            Neutral
        }

        #endregion

        #region Character Mappings

        private static readonly Dictionary<char, char[]> ArabicGlyphMap = new Dictionary<char, char[]>() {
            // Basic Arabic Letters
            { 'ا', new char[] { 'ا', 'ا', '\uFE8E', '\uFE8E' } },
            { 'ب', new char[] { 'ب', '\uFE91', '\uFE92', '\uFE90' } },
            { 'ت', new char[] { 'ت', '\uFE97', '\uFE98', '\uFE96' } },
            { 'ث', new char[] { 'ث', '\uFE9B', '\uFE9C', '\uFE9A' } },
            { 'ج', new char[] { 'ج', '\uFE9F', '\uFEA0', '\uFE9E' } },
            { 'ح', new char[] { 'ح', '\uFEA3', '\uFEA4', '\uFEA2' } },
            { 'خ', new char[] { 'خ', '\uFEA7', '\uFEA8', '\uFEA6' } },
            { 'د', new char[] { 'د', 'د', '\uFEAA', '\uFEAA' } },
            { 'ذ', new char[] { 'ذ', 'ذ', '\uFEAC', '\uFEAC' } },
            { 'ر', new char[] { 'ر', 'ر', '\uFEAE', '\uFEAE' } },
            { 'ز', new char[] { 'ز', 'ز', '\uFEB0', '\uFEB0' } },
            { 'س', new char[] { 'س', '\uFEB3', '\uFEB4', '\uFEB2' } },
            { 'ش', new char[] { 'ش', '\uFEB7', '\uFEB8', '\uFEB6' } },
            { 'ص', new char[] { 'ص', '\uFEBB', '\uFEBC', '\uFEBA' } },
            { 'ض', new char[] { 'ض', '\uFEBF', '\uFEC0', '\uFEBE' } },
            { 'ط', new char[] { 'ط', '\uFEC3', '\uFEC4', '\uFEC2' } },
            { 'ظ', new char[] { 'ظ', '\uFEC7', '\uFEC8', '\uFEC6' } },
            { 'ع', new char[] { 'ع', '\uFECB', '\uFECC', '\uFECA' } },
            { 'غ', new char[] { 'غ', '\uFECF', '\uFED0', '\uFECE' } },
            { 'ف', new char[] { 'ف', '\uFED3', '\uFED4', '\uFED2' } },
            { 'ق', new char[] { 'ق', '\uFED7', '\uFED8', '\uFED6' } },
            { 'ك', new char[] { 'ك', '\uFEDB', '\uFEDC', '\uFEDA' } },
            { 'ل', new char[] { 'ل', '\uFEDF', '\uFEE0', '\uFEDE' } },
            { 'م', new char[] { 'م', '\uFEE3', '\uFEE4', '\uFEE2' } },
            { 'ن', new char[] { 'ن', '\uFEE7', '\uFEE8', '\uFEE6' } },
            { 'ه', new char[] { 'ه', '\uFEEB', '\uFEEC', '\uFEEA' } },
            { 'و', new char[] { 'و', 'و', '\uFEEE', '\uFEEE' } },
            { 'ي', new char[] { 'ي', '\uFEF3', '\uFEF4', '\uFEF2' } },
            { 'ى', new char[] { 'ى', 'ى', '\uFEF0', '\uFEF0' } },
            { 'ة', new char[] { 'ة', 'ة', '\uFE94', '\uFE94' } },
            { 'ء', new char[] { 'ء', 'ء', 'ء', 'ء' } },
            
            // Alef variants
            { 'آ', new char[] { 'آ', 'آ', '\uFE82', '\uFE82' } },
            { 'أ', new char[] { 'أ', 'أ', '\uFE84', '\uFE84' } },
            { 'إ', new char[] { 'إ', 'إ', '\uFE88', '\uFE88' } },
            { 'ؤ', new char[] { 'ؤ', 'ؤ', '\uFE86', '\uFE86' } },
            { 'ئ', new char[] { 'ئ', '\uFE8B', '\uFE8C', '\uFE8A' } },
            
            // Persian/Urdu letters
            { 'پ', new char[] { 'پ', '\uFB58', '\uFB59', '\uFB57' } },
            { 'چ', new char[] { 'چ', '\uFB7C', '\uFB7D', '\uFB7B' } },
            { 'ژ', new char[] { 'ژ', 'ژ', '\uFB8B', '\uFB8B' } },
            { 'گ', new char[] { 'گ', '\uFB94', '\uFB95', '\uFB93' } },
            { 'ک', new char[] { 'ک', '\uFB90', '\uFB91', '\uFB8F' } },
            { 'ی', new char[] { 'ی', '\uFBFE', '\uFBFF', '\uFBFD' } }
        };

        private static readonly Dictionary<string, char[]> LamAlefLigatures = new Dictionary<string, char[]>() {
            { "لا", new char[] { '\uFEFB', '\uFEFC' } },
            { "لأ", new char[] { '\uFEF7', '\uFEF8' } },
            { "لإ", new char[] { '\uFEF9', '\uFEFA' } },
            { "لآ", new char[] { '\uFEF5', '\uFEF6' } }
        };

        // FIXED: Lam-Alef ligatures also don't connect forward
        private static readonly HashSet<char> RightConnectingOnly = new HashSet<char> {
            'ا', 'آ', 'أ', 'إ', 'ى',    // Alef family
            'د', 'ذ',                    // Dal family
            'ر', 'ز', 'ژ',              // Reh family
            'و', 'ؤ',                    // Waw family
            'ة',                         // Teh Marbuta
            'ء',                         // Hamza
            // CRITICAL FIX: Add Lam-Alef ligatures
            '\uFEFB', '\uFEFC',         // لا ligatures
            '\uFEF7', '\uFEF8',         // لأ ligatures
            '\uFEF9', '\uFEFA',         // لإ ligatures
            '\uFEF5', '\uFEF6'          // لآ ligatures
        };

        private static readonly HashSet<char> Diacritics = new HashSet<char> {
            '\u064B', '\u064C', '\u064D', '\u064E', '\u064F', '\u0650', '\u0651', '\u0652',
            '\u0653', '\u0654', '\u0655', '\u0656', '\u0657', '\u0658', '\u0659', '\u065A',
            '\u065B', '\u065C', '\u065D', '\u065E', '\u065F', '\u0670'
        };

        #endregion

        #region Public API

        public static string Fix(string originalText, bool showTashkeel = true, bool preserveNumbers = true,
                                bool fixTags = true, bool forceRTL = true)
        {

            if (string.IsNullOrEmpty(originalText))
            {
                return originalText;
            }

            try
            {
                List<TagInfo> tags = new List<TagInfo>();
                string textWithoutTags = originalText;

                if (fixTags)
                {
                    textWithoutTags = ExtractTMPTags(originalText, tags);
                }

                bool containsArabic = ContainsArabicCharacters(textWithoutTags);

                if (!containsArabic && !forceRTL)
                {
                    return originalText;
                }

                string processedText = ProcessArabicText(textWithoutTags, showTashkeel, preserveNumbers, forceRTL);

                if (fixTags && tags.Count > 0)
                {
                    processedText = ReinsertTMPTags(processedText, tags);
                }

                return processedText;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AL-Arcade Arabic] Error processing text: {ex.Message}");
                return originalText;
            }
        }

        public static void LogFontRecommendations()
        {
            Debug.Log("[AL-Arcade Arabic] 🎯 RECOMMENDED ARABIC FONTS FOR UNITY:\n" +
                     "✅ Noto Sans Arabic (Google Fonts - Free & Excellent)\n" +
                     "✅ Amiri (Free - Perfect for traditional Arabic)\n" +
                     "✅ Cairo (Google Fonts - Modern & Clean)\n" +
                     "✅ Almarai (Google Fonts - Great for UI)\n" +
                     "Download from: fonts.google.com");
        }

        #endregion

        #region Core Processing

        private static string ProcessArabicText(string text, bool showTashkeel, bool preserveNumbers, bool forceRTL)
        {
            if (string.IsNullOrEmpty(text)) return text;

            List<CharInfo> chars = AnalyzeCharacters(text);

            if (!showTashkeel)
            {
                chars.RemoveAll(c => c.IsDiacritic);
            }

            if (!preserveNumbers)
            {
                ConvertToArabicNumerals(chars);
            }

            ProcessLamAlefLigatures(chars);
            ApplyArabicShaping(chars);
            string result = HandleBidirectionalText(chars, forceRTL);

            return result;
        }

        private static List<CharInfo> AnalyzeCharacters(string text)
        {
            List<CharInfo> chars = new List<CharInfo>(text.Length);

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                CharInfo charInfo = new CharInfo
                {
                    OriginalChar = c,
                    ProcessedChar = c,
                    IsArabic = IsArabicChar(c),
                    IsDiacritic = IsDiacritic(c),
                    IsLatin = IsLatinChar(c),
                    IsDigit = char.IsDigit(c),
                    IsWhitespace = char.IsWhiteSpace(c),
                    IsPunctuation = char.IsPunctuation(c),
                    ConnectionType = GetConnectionType(c),
                    Direction = GetCharDirection(c)
                };
                chars.Add(charInfo);
            }

            return chars;
        }

        private static void ProcessLamAlefLigatures(List<CharInfo> chars)
        {
            for (int i = 0; i < chars.Count - 1; i++)
            {
                if (chars[i].OriginalChar == 'ل')
                {
                    int alefIndex = FindNextNonDiacriticIndex(chars, i);
                    if (alefIndex != -1)
                    {
                        char alefChar = chars[alefIndex].OriginalChar;
                        string lamAlefKey = "ل" + alefChar;

                        if (LamAlefLigatures.ContainsKey(lamAlefKey))
                        {
                            bool isConnectedToPrevious = CanConnectToPrevious(chars, i);
                            char ligature = LamAlefLigatures[lamAlefKey][isConnectedToPrevious ? 1 : 0];

                            chars[i].ProcessedChar = ligature;
                            chars[i].IsLigature = true;
                            // CRITICAL FIX: Mark ligature as right-connecting only
                            chars[i].ConnectionType = ConnectionType.Right;

                            chars[alefIndex].ShouldRemove = true;
                        }
                    }
                }
            }

            chars.RemoveAll(c => c.ShouldRemove);
        }

        private static void ApplyArabicShaping(List<CharInfo> chars)
        {
            for (int i = 0; i < chars.Count; i++)
            {
                CharInfo charInfo = chars[i];

                if (!charInfo.IsArabic || charInfo.IsDiacritic || charInfo.IsLigature)
                {
                    continue;
                }

                char baseChar = charInfo.OriginalChar;
                if (ArabicGlyphMap.ContainsKey(baseChar))
                {
                    LetterForm form = DetermineLetterForm(chars, i);
                    charInfo.ProcessedChar = ArabicGlyphMap[baseChar][(int)form];
                }
            }
        }

        // COMPLETELY FIXED: Bidirectional text handling
        private static string HandleBidirectionalText(List<CharInfo> chars, bool forceRTL)
        {
            if (chars.Count == 0) return "";

            List<TextRun> runs = CreateDirectionalRuns(chars);
            bool isOverallRTL = forceRTL || IsPrimarilyArabic(chars);

            StringBuilder result = new StringBuilder();

            if (isOverallRTL)
            {
                // RTL Context: Reverse run order, but NEVER reverse English internally
                for (int i = runs.Count - 1; i >= 0; i--)
                {
                    TextRun run = runs[i];

                    if (run.IsRTL)
                    {
                        // Arabic: Keep in logical order (already shaped correctly)
                        foreach (var charInfo in run.Characters)
                        {
                            result.Append(charInfo.ProcessedChar);
                        }
                    }
                    else
                    {
                        // CRITICAL FIX: English in RTL context - NEVER reverse
                        foreach (var charInfo in run.Characters)
                        {
                            result.Append(charInfo.ProcessedChar);
                        }
                    }
                }
            }
            else
            {
                // LTR Context
                foreach (var run in runs)
                {
                    if (run.IsRTL)
                    {
                        // Arabic in LTR context - reverse
                        for (int i = run.Characters.Count - 1; i >= 0; i--)
                        {
                            result.Append(run.Characters[i].ProcessedChar);
                        }
                    }
                    else
                    {
                        // English in LTR context - keep normal
                        foreach (var charInfo in run.Characters)
                        {
                            result.Append(charInfo.ProcessedChar);
                        }
                    }
                }
            }

            return result.ToString();
        }

        // IMPROVED: Better run creation
        private static List<TextRun> CreateDirectionalRuns(List<CharInfo> chars)
        {
            List<TextRun> runs = new List<TextRun>();
            if (chars.Count == 0) return runs;

            TextRun currentRun = null;

            for (int i = 0; i < chars.Count; i++)
            {
                CharInfo charInfo = chars[i];

                bool isRTL = charInfo.Direction == TextDirection.RTL;
                bool isLTR = charInfo.Direction == TextDirection.LTR;
                bool isNeutral = charInfo.Direction == TextDirection.Neutral;

                if (currentRun == null)
                {
                    currentRun = new TextRun
                    {
                        IsRTL = isRTL,
                        Characters = new List<CharInfo> { charInfo }
                    };
                }
                else if (isNeutral)
                {
                    // Neutral characters join current run
                    currentRun.Characters.Add(charInfo);
                }
                else if (isRTL == currentRun.IsRTL)
                {
                    // Same direction
                    currentRun.Characters.Add(charInfo);
                }
                else
                {
                    // Direction change
                    runs.Add(currentRun);
                    currentRun = new TextRun
                    {
                        IsRTL = isRTL,
                        Characters = new List<CharInfo> { charInfo }
                    };
                }
            }

            if (currentRun != null)
            {
                runs.Add(currentRun);
            }

            return runs;
        }

        #endregion

        #region Helper Methods

        private static LetterForm DetermineLetterForm(List<CharInfo> chars, int index)
        {
            bool canConnectToPrevious = CanConnectToPrevious(chars, index);
            bool canConnectToNext = CanConnectToNext(chars, index);

            if (canConnectToPrevious && canConnectToNext)
            {
                return LetterForm.Medial;
            }
            else if (canConnectToPrevious)
            {
                return LetterForm.Final;
            }
            else if (canConnectToNext)
            {
                return LetterForm.Initial;
            }
            else
            {
                return LetterForm.Isolated;
            }
        }

        private static bool CanConnectToPrevious(List<CharInfo> chars, int index)
        {
            if (index <= 0) return false;

            for (int i = index - 1; i >= 0; i--)
            {
                if (chars[i].IsDiacritic) continue;

                // Check original character AND processed character for right-connecting-only
                bool isRightConnectingOnly = RightConnectingOnly.Contains(chars[i].OriginalChar) ||
                                           RightConnectingOnly.Contains(chars[i].ProcessedChar);

                return chars[i].IsArabic &&
                       chars[i].ConnectionType != ConnectionType.None &&
                       !isRightConnectingOnly;
            }
            return false;
        }

        private static bool CanConnectToNext(List<CharInfo> chars, int index)
        {
            if (index >= chars.Count - 1) return false;

            // CRITICAL FIX: Check both original and processed character
            bool isRightConnectingOnly = RightConnectingOnly.Contains(chars[index].OriginalChar) ||
                                       RightConnectingOnly.Contains(chars[index].ProcessedChar);

            if (isRightConnectingOnly)
            {
                return false;
            }

            for (int i = index + 1; i < chars.Count; i++)
            {
                if (chars[i].IsDiacritic) continue;

                return chars[i].IsArabic && chars[i].ConnectionType != ConnectionType.None;
            }
            return false;
        }

        private static int FindNextNonDiacriticIndex(List<CharInfo> chars, int startIndex)
        {
            for (int i = startIndex + 1; i < chars.Count; i++)
            {
                if (!chars[i].IsDiacritic)
                {
                    return i;
                }
            }
            return -1;
        }

        private static ConnectionType GetConnectionType(char c)
        {
            if (IsDiacritic(c)) return ConnectionType.Transparent;
            if (!IsArabicChar(c)) return ConnectionType.None;
            if (c == 'ء') return ConnectionType.None;
            if (RightConnectingOnly.Contains(c)) return ConnectionType.Right;
            return ConnectionType.Dual;
        }

        private static TextDirection GetCharDirection(char c)
        {
            if (IsArabicChar(c)) return TextDirection.RTL;
            if (IsLatinChar(c)) return TextDirection.LTR;
            return TextDirection.Neutral;
        }

        private static bool IsPrimarilyArabic(List<CharInfo> chars)
        {
            int arabicCount = 0;
            int totalCount = 0;

            foreach (var c in chars)
            {
                if (!c.IsWhitespace && !c.IsDiacritic)
                {
                    totalCount++;
                    if (c.IsArabic) arabicCount++;
                }
            }

            return totalCount > 0 && (arabicCount * 2 > totalCount);
        }

        private static void ConvertToArabicNumerals(List<CharInfo> chars)
        {
            char[] westernNumerals = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            char[] arabicNumerals = { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };

            for (int i = 0; i < chars.Count; i++)
            {
                if (chars[i].IsDigit)
                {
                    for (int j = 0; j < westernNumerals.Length; j++)
                    {
                        if (chars[i].OriginalChar == westernNumerals[j])
                        {
                            chars[i].ProcessedChar = arabicNumerals[j];
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Character Classification

        private static bool ContainsArabicCharacters(string text)
        {
            foreach (char c in text)
            {
                if (IsArabicChar(c)) return true;
            }
            return false;
        }

        private static bool IsArabicChar(char c)
        {
            int code = (int)c;
            return (code >= ARABIC_BASIC_START && code <= ARABIC_BASIC_END) ||
                   (code >= ARABIC_SUPPLEMENT_START && code <= ARABIC_SUPPLEMENT_END) ||
                   (code >= ARABIC_EXTENDED_A_START && code <= ARABIC_EXTENDED_A_END) ||
                   (code >= ARABIC_PRESENTATION_A_START && code <= ARABIC_PRESENTATION_A_END) ||
                   (code >= ARABIC_PRESENTATION_B_START && code <= ARABIC_PRESENTATION_B_END);
        }

        private static bool IsLatinChar(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }

        private static bool IsDiacritic(char c)
        {
            return Diacritics.Contains(c);
        }

        #endregion

        #region TextMeshPro Tag Handling

        private static string ExtractTMPTags(string text, List<TagInfo> tags)
        {
            StringBuilder result = new StringBuilder();
            bool inTag = false;
            StringBuilder tagBuilder = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '<' && !inTag)
                {
                    inTag = true;
                    tagBuilder.Clear();
                    tagBuilder.Append(c);
                    continue;
                }

                if (inTag)
                {
                    tagBuilder.Append(c);
                    if (c == '>')
                    {
                        inTag = false;
                        tags.Add(new TagInfo
                        {
                            Tag = tagBuilder.ToString(),
                            Position = result.Length
                        });
                    }
                    continue;
                }

                result.Append(c);
            }

            if (inTag)
            {
                result.Append(tagBuilder);
            }

            return result.ToString();
        }

        private static string ReinsertTMPTags(string text, List<TagInfo> tags)
        {
            if (tags.Count == 0) return text;

            StringBuilder result = new StringBuilder(text);

            for (int i = tags.Count - 1; i >= 0; i--)
            {
                int pos = Math.Min(tags[i].Position, result.Length);
                result.Insert(pos, tags[i].Tag);
            }

            return result.ToString();
        }

        #endregion

        #region Data Classes

        private class CharInfo
        {
            public char OriginalChar;
            public char ProcessedChar;
            public bool IsArabic;
            public bool IsDiacritic;
            public bool IsLatin;
            public bool IsDigit;
            public bool IsWhitespace;
            public bool IsPunctuation;
            public bool IsLigature;
            public bool ShouldRemove;
            public ConnectionType ConnectionType;
            public TextDirection Direction;
        }

        private class TextRun
        {
            public bool IsRTL;
            public List<CharInfo> Characters = new List<CharInfo>();
        }

        private class TagInfo
        {
            public string Tag;
            public int Position;
        }

        #endregion
    }
}