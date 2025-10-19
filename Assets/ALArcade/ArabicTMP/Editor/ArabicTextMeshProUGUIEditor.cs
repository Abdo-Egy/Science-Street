#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;
using ALArcade.ArabicTMP;

namespace ALArcade.ArabicTMP.Editor {
    [CustomEditor(typeof(ArabicTextMeshProUGUI)), CanEditMultipleObjects]
    public class ArabicTextMeshProUGUIEditor : TMPro.EditorUtilities.TMP_EditorPanelUI {
        // SerializedProperties for Arabic text component
        SerializedProperty m_ArabicTextProp;
        SerializedProperty m_ShowTashkeelProp;
        SerializedProperty m_PreserveNumbersProp;
        SerializedProperty m_FixTagsProp;
        SerializedProperty m_ForceRTLProp;

        protected override void OnEnable() {
            base.OnEnable();
            
            // Get references to serialized properties
            m_ArabicTextProp = serializedObject.FindProperty("m_ArabicText");
            m_ShowTashkeelProp = serializedObject.FindProperty("m_ShowTashkeel");
            m_PreserveNumbersProp = serializedObject.FindProperty("m_PreserveNumbers");
            m_FixTagsProp = serializedObject.FindProperty("m_FixTags");
            m_ForceRTLProp = serializedObject.FindProperty("m_ForceRTL");
        }

        public override void OnInspectorGUI() {
            if (target == null)
                return;
                
            serializedObject.Update();

            // Draw Arabic text input field
            EditorGUILayout.LabelField("Arabic Text", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            
            string newText = EditorGUILayout.TextArea(m_ArabicTextProp.stringValue, GUILayout.MinHeight(80));
            if (EditorGUI.EndChangeCheck()) {
                m_ArabicTextProp.stringValue = newText;
                
                // Ensure immediate update in editor
                foreach (var obj in targets) {
                    if (obj is ArabicTextMeshProUGUI arabicText) {
                        arabicText.arabicText = newText;
                    }
                }
            }
            
            // Draw options section
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Arabic Options", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(m_ShowTashkeelProp, new GUIContent("Show Diacritics (Tashkeel)", "Display Arabic diacritic marks"));
            EditorGUILayout.PropertyField(m_PreserveNumbersProp, new GUIContent("Preserve Numbers", "Keep Western numerals (123) as-is"));
            EditorGUILayout.PropertyField(m_FixTagsProp, new GUIContent("Fix Rich Text Tags", "Process rich text tags like <color> properly"));
            EditorGUILayout.PropertyField(m_ForceRTLProp, new GUIContent("Force RTL", "Force right-to-left rendering even if text starts with Latin characters"));
            
            EditorGUILayout.Space();
            
            // Draw default TMP properties
            EditorGUILayout.LabelField("TextMeshPro Settings", EditorStyles.boldLabel);
            
            // Hide the original text property since we're using our custom one
            SerializedProperty prop = serializedObject.FindProperty("m_text");
            if (prop != null) {
                // Make the TMP text field read-only to prevent editing it directly
                GUI.enabled = false;
                EditorGUILayout.PropertyField(prop, new GUIContent("Processed Text (Read Only)"));
                GUI.enabled = true;
            }
            
            DrawMainSettings();
            DrawExtraSettings();
            
            serializedObject.ApplyModifiedProperties();
            
            // Show helpful notes for first-time users
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Enter Arabic text in the 'Arabic Text' field above. It will be automatically shaped and displayed correctly.", MessageType.Info);
        }
    }
}
#endif