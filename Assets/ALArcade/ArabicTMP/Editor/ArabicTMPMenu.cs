#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ALArcade.ArabicTMP.Editor
{
    public static class ArabicTMPMenu
    {
        [MenuItem("GameObject/UI/Arabic Text - TextMeshPro", false, 2001)]
        static void CreateArabicTextUGUI(MenuCommand menuCommand)
        {
            // Create a new GameObject with ArabicTextMeshProUGUI
            GameObject go = new GameObject("Arabic Text (TMP)");
            go.AddComponent<RectTransform>();
            go.AddComponent<ArabicTextMeshProUGUI>().text = "أهلا بالعالم";  // Hello World in Arabic

            // Set alignment to Right
            go.GetComponent<ArabicTextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

            // Assign font asset if default Arabic font exists
            TMP_FontAsset arabicFont = GetDefaultArabicFont();
            if (arabicFont != null)
            {
                go.GetComponent<ArabicTextMeshProUGUI>().font = arabicFont;
            }

            // Register the creation in the Undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

            // Set parent
            PlaceUIElementRoot(go, menuCommand);

            // Select the newly created object
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/3D Object/Arabic Text - TextMeshPro", false, 2001)]
        static void CreateArabicTextWorld(MenuCommand menuCommand)
        {
            // Create a new GameObject with ArabicTextMeshPro
            GameObject go = new GameObject("Arabic Text (TMP)");
            go.AddComponent<ArabicTextMeshPro>().text = "أهلا بالعالم";  // Hello World in Arabic

            // Set alignment to Right
            go.GetComponent<ArabicTextMeshPro>().alignment = TextAlignmentOptions.Right;

            // Set default size for 3D text
            go.GetComponent<ArabicTextMeshPro>().fontSize = 36;

            // Assign font asset if default Arabic font exists
            TMP_FontAsset arabicFont = GetDefaultArabicFont();
            if (arabicFont != null)
            {
                go.GetComponent<ArabicTextMeshPro>().font = arabicFont;
            }

            // Register the creation in the Undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

            // Set parent
            PlaceGameObjectInScene(go, menuCommand);

            // Select the newly created object
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/UI/Arabic Input Field - TextMeshPro", false, 2051)]
        static void CreateArabicInputField(MenuCommand menuCommand)
        {
            // Create a GameObject with InputField with built-in UI components
            GameObject go = CreateInputFieldWithRTL();

            // Register the creation in the Undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

            // Set parent
            PlaceUIElementRoot(go, menuCommand);

            // Select the newly created object
            Selection.activeObject = go;
        }

        private static GameObject CreateInputFieldWithRTL()
        {
            // Create a new GameObject with ArabicTMPInputField
            GameObject go = new GameObject("Arabic Input Field (TMP)");
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160, 30);

            // Add the Input field component which in turn adds the needed sub components
            ArabicTMPInputField inputField = go.AddComponent<ArabicTMPInputField>();

            // Get reference to the TMP_InputField.textComponent
            GameObject textArea = new GameObject("Text Area");
            RectTransform textAreaRectTransform = textArea.AddComponent<RectTransform>();
            textAreaRectTransform.anchorMin = Vector2.zero;
            textAreaRectTransform.anchorMax = Vector2.one;
            textAreaRectTransform.sizeDelta = Vector2.zero;
            textAreaRectTransform.offsetMin = new Vector2(10, 6);
            textAreaRectTransform.offsetMax = new Vector2(-10, -7);
            textArea.transform.SetParent(go.transform, false);

            // Create text component
            GameObject textComponent = new GameObject("Text");
            RectTransform textRectTransform = textComponent.AddComponent<RectTransform>();
            textRectTransform.anchorMin = new Vector2(0, 0);
            textRectTransform.anchorMax = new Vector2(1, 1);
            textRectTransform.sizeDelta = Vector2.zero;
            textRectTransform.offsetMin = new Vector2(0, 0);
            textRectTransform.offsetMax = new Vector2(0, 0);
            textComponent.transform.SetParent(textArea.transform, false);

            // Instead of regular TMP_Text, use our ArabicTextMeshProUGUI
            ArabicTextMeshProUGUI text = textComponent.AddComponent<ArabicTextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Right;
            text.color = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);
            text.text = "";

            // Create placeholder for the text input
            GameObject placeholder = new GameObject("Placeholder");
            RectTransform placeholderRectTransform = placeholder.AddComponent<RectTransform>();
            placeholderRectTransform.anchorMin = Vector2.zero;
            placeholderRectTransform.anchorMax = Vector2.one;
            placeholderRectTransform.sizeDelta = Vector2.zero;
            placeholderRectTransform.offsetMin = Vector2.zero;
            placeholderRectTransform.offsetMax = Vector2.zero;
            placeholder.transform.SetParent(textArea.transform, false);

            // Again use ArabicTextMeshProUGUI for the placeholder
            ArabicTextMeshProUGUI placeholderText = placeholder.AddComponent<ArabicTextMeshProUGUI>();
            placeholderText.alignment = TextAlignmentOptions.Right;
            placeholderText.text = "أدخل النص...";  // "Enter text..." in Arabic
            placeholderText.enableWordWrapping = false;
            placeholderText.extraPadding = true;

            Color placeholderColor = new Color(0f, 0f, 0f, 0.5f);
            placeholderText.color = placeholderColor;

            // Make the placeholder text slightly smaller
            placeholderText.fontSize = text.fontSize - 0.75f;

            // Set references on the InputField
            inputField.textComponent = text;
            inputField.placeholder = placeholderText;
            inputField.fontAsset = text.font;

            // Assign Arabic font if available
            TMP_FontAsset arabicFont = GetDefaultArabicFont();
            if (arabicFont != null)
            {
                text.font = arabicFont;
                placeholderText.font = arabicFont;
                inputField.fontAsset = arabicFont;
            }

            return go;
        }

        // Helper function to get default Arabic font, if available
        private static TMP_FontAsset GetDefaultArabicFont()
        {
            // Look for common Arabic fonts in project
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);

                // Look for fonts with common Arabic font names
                if (font != null)
                {
                    string fontName = font.name.ToLowerInvariant();
                    if (fontName.Contains("arabic") || fontName.Contains("amiri") ||
                        fontName.Contains("cairo") || fontName.Contains("scheherazade") ||
                        fontName.Contains("dubai") || fontName.Contains("noto") && fontName.Contains("arabic"))
                    {
                        return font;
                    }
                }
            }
            return null;
        }

        // Common code for placing UI elements in hierarchy
        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;

            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = GetOrCreateCanvasGameObject();
            }

            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
            element.name = uniqueName;
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
            Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            GameObjectUtility.SetParentAndAlign(element, parent);

            Selection.activeGameObject = element;
        }

        // Get or create canvas for UI elements
        private static GameObject GetOrCreateCanvasGameObject()
        {
            GameObject selectedGo = Selection.activeGameObject;

            // Try to find canvas in selected hierarchy
            Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
            {
                return canvas.gameObject;
            }

            // No canvas in hierarchy, try to find any canvas
            canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null && canvas.gameObject.activeInHierarchy)
            {
                return canvas.gameObject;
            }

            // No canvas exists, create one
            return CreateNewUI();
        }

        // Create a new UI with EventSystem
        private static GameObject CreateNewUI()
        {
            // Root canvas
            GameObject root = new GameObject("Canvas");
            root.layer = LayerMask.NameToLayer("UI");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();

            // Create EventSystem if needed
            CreateEventSystemIfNeeded();

            return root;
        }

        // Create EventSystem if none exists
        private static void CreateEventSystemIfNeeded()
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            }
        }

        // Place 3D objects in the scene
        private static void PlaceGameObjectInScene(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;

            if (parent == null)
            {
                // No context, place at scene root
                parent = null;
            }

            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent != null ? parent.transform : null, element.name);
            element.name = uniqueName;

            // Register undo
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);

            // Set parent
            if (parent != null)
            {
                Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
                GameObjectUtility.SetParentAndAlign(element, parent);
            }

            // Set selection to new object
            Selection.activeGameObject = element;
        }
    }
}
#endif