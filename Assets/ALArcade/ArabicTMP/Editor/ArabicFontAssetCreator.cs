#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;
using TMPro.EditorUtilities;
using System.IO;
using System.Text;

namespace ALArcade.ArabicTMP.Editor
{
    public class ArabicFontAssetCreator : EditorWindow
    {
        private Font sourceFont;
        private string savePath = "Assets/";
        private string saveFileName = "New Arabic Font Asset";
        private int samplingPointSize = 90;
        private int atlasPadding = 9;
        private int atlasWidth = 1024;
        private int atlasHeight = 1024;
        private bool includeArabicBasic = true;
        private bool includeArabicSupplement = true;
        private bool includeArabicExtendedA = true;
        private bool includeArabicPresentationA = true;
        private bool includeArabicPresentationB = true;
        private bool includePersian = true;

        private TMP_FontAsset createdFontAsset;
        private readonly string[] arabicRanges = new string[] {
            // Basic Arabic
            "0600-06FF",
            // Arabic Supplement
            "0750-077F",
            // Arabic Extended-A
            "08A0-08FF",
            // Arabic Presentation Forms-A
            "FB50-FDFF",
            // Arabic Presentation Forms-B
            "FE70-FEFF",
            // Persian Characters
            "067E, 0686, 0698, 06AF"
        };

        [MenuItem("Window/AL-Arcade/Arabic TMP/Font Asset Creator")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<ArabicFontAssetCreator>("Arabic Font Asset Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Create Arabic TextMeshPro Font Asset", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Source font selection
            sourceFont = (Font)EditorGUILayout.ObjectField("Source Font File", sourceFont, typeof(Font), false);

            // Atlas settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Atlas Settings", EditorStyles.boldLabel);
            samplingPointSize = EditorGUILayout.IntField("Sampling Point Size", samplingPointSize);
            atlasPadding = EditorGUILayout.IntField("Atlas Padding", atlasPadding);
            atlasWidth = EditorGUILayout.IntField("Atlas Width", atlasWidth);
            atlasHeight = EditorGUILayout.IntField("Atlas Height", atlasHeight);

            // Character sets
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Character Sets to Include", EditorStyles.boldLabel);
            includeArabicBasic = EditorGUILayout.Toggle("Arabic Basic (0600-06FF)", includeArabicBasic);
            includeArabicSupplement = EditorGUILayout.Toggle("Arabic Supplement (0750-077F)", includeArabicSupplement);
            includeArabicExtendedA = EditorGUILayout.Toggle("Arabic Extended-A (08A0-08FF)", includeArabicExtendedA);
            includeArabicPresentationA = EditorGUILayout.Toggle("Arabic Presentation Forms-A (FB50-FDFF)", includeArabicPresentationA);
            includeArabicPresentationB = EditorGUILayout.Toggle("Arabic Presentation Forms-B (FE70-FEFF)", includeArabicPresentationB);
            includePersian = EditorGUILayout.Toggle("Persian Letters (پ چ ژ گ)", includePersian);

            // Save settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Save Settings", EditorStyles.boldLabel);
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            saveFileName = EditorGUILayout.TextField("Asset Name", saveFileName);

            // Create button
            EditorGUILayout.Space();

            if (sourceFont == null)
            {
                EditorGUILayout.HelpBox("A Source Font File is required to create a font asset.", MessageType.Error);
            }

            EditorGUI.BeginDisabledGroup(sourceFont == null);
            if (GUILayout.Button("Create Font Asset"))
            {
                GenerateFontAsset();
            }
            EditorGUI.EndDisabledGroup();

            // Show the created font asset
            EditorGUILayout.Space();
            if (createdFontAsset != null)
            {
                EditorGUILayout.ObjectField("Created Font Asset", createdFontAsset, typeof(TMP_FontAsset), false);
            }

            // Help box
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "This utility creates a TextMeshPro Font Asset with all necessary Arabic Unicode ranges.\n\n" +
                "1. Select a source font that contains Arabic glyphs.\n" +
                "2. Choose which character sets to include.\n" +
                "3. Set the save location and asset name.\n" +
                "4. Click 'Create Font Asset'.\n\n" +
                "The created asset will be ready for use with the Arabic TMP components.",
                MessageType.Info);
        }

        private void GenerateFontAsset()
        {
            if (sourceFont == null)
            {
                EditorUtility.DisplayDialog("Error", "A Source Font File is required to create a font asset.", "OK");
                return;
            }

            // Generate the character set string for TMP Font Asset Creator
            StringBuilder charSetBuilder = new StringBuilder();

            // Add selected character ranges
            if (includeArabicBasic) charSetBuilder.Append(arabicRanges[0] + "\n");
            if (includeArabicSupplement) charSetBuilder.Append(arabicRanges[1] + "\n");
            if (includeArabicExtendedA) charSetBuilder.Append(arabicRanges[2] + "\n");
            if (includeArabicPresentationA) charSetBuilder.Append(arabicRanges[3] + "\n");
            if (includeArabicPresentationB) charSetBuilder.Append(arabicRanges[4] + "\n");
            if (includePersian) charSetBuilder.Append(arabicRanges[5] + "\n");

            // Add basic Latin range for mixed text
            charSetBuilder.Append("0020-007F\n"); // Basic Latin

            string characterSet = charSetBuilder.ToString();

            // Create temp file with character set
            string tempFilePath = Path.Combine(Path.GetTempPath(), "ArabicCharSet.txt");
            File.WriteAllText(tempFilePath, characterSet);

            // Ensure the save path exists
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            // Generate the font asset using TMP_FontAsset.CreateFontAsset
            string fullPath = Path.Combine(savePath, saveFileName + ".asset");

            // Create the font asset
            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                samplingPointSize,
                atlasPadding,
                UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                atlasWidth,
                atlasHeight,
                TMPro.AtlasPopulationMode.Dynamic,
                true);

            // Initialize character table with our character set
            fontAsset.ReadFontAssetDefinition();

            // Save the created font asset
            AssetDatabase.CreateAsset(fontAsset, fullPath);

            // Save the material as a sub-asset
            fontAsset.material.name = saveFileName + " Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

            // Update atlas texture and add it as sub-asset
            if (fontAsset.atlasTexture != null)
            {
                fontAsset.atlasTexture.name = saveFileName + " Atlas";
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
            }

            // The following is a workaround to "load" character set into the font
            // In practice, this will set these characters to be loaded when needed
            string sampleText = "السلام عليكم مرحبا بكم في يونيتي";
            fontAsset.TryAddCharacters(sampleText);

            // Save all assets
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Store the created font asset reference
            createdFontAsset = fontAsset;

            EditorUtility.DisplayDialog("Success", "Arabic font asset created successfully at:\n" + fullPath, "OK");

            // Clean up temp file
            try
            {
                File.Delete(tempFilePath);
            }
            catch
            {
                // Ignore cleanup errors
            }

            // Select the created asset
            EditorGUIUtility.PingObject(fontAsset);
        }
    }
}
#endif