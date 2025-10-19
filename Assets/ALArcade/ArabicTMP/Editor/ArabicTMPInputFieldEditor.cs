#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;
using ALArcade.ArabicTMP;

namespace ALArcade.ArabicTMP.Editor {
    [CustomEditor(typeof(ArabicTMPInputField)), CanEditMultipleObjects]
    public class ArabicTMPInputFieldEditor : TMPro.EditorUtilities.TMP_InputFieldEditor {
        // SerializedProperties for Arabic input field options
        SerializedProperty m_ShowTashkeelProp;
        SerializedProperty m_PreserveNumbersProp;
        SerializedProperty m_FixTagsProp;
        SerializedProperty m_ForceRTLProp;

        protected override void OnEnable() {
            base.OnEnable();
            
            // Get references to serialized properties
            m_ShowTashkeelProp = serializedObject.FindProperty("m_ShowTashkeel");
            m_PreserveNumbersProp = serializedObject.FindProperty("m_PreserveNumbers");
            m_FixTagsProp = serializedObject.FindProperty("m_FixTags");
            m_ForceRTLProp = serializedObject.FindProperty("m_ForceRTL");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            
            // Draw the Arabic options before the base inspector
            EditorGUILayout.LabelField("Arabic Input Options", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(m_ShowTashkeelProp, new GUIContent("Show Diacritics (Tashkeel)", "Display Arabic diacritic marks"));
            EditorGUILayout.PropertyField(m_PreserveNumbersProp, new GUIContent("Preserve Numbers", "Keep Western numerals (123) as-is"));
            EditorGUILayout.PropertyField(m_FixTagsProp, new GUIContent("Fix Rich Text Tags", "Process rich text tags like <color> properly"));
            EditorGUILayout.PropertyField(m_ForceRTLProp, new GUIContent("Force RTL", "Force right-to-left rendering even if text starts with Latin characters"));
            
            EditorGUILayout.Space();
            
            // Call the base class's OnInspectorGUI
            base.OnInspectorGUI();
            
            serializedObject.ApplyModifiedProperties();
            
            // Show helpful notes for first-time users
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("This component works best with an ArabicTextMeshProUGUI as the text component.", MessageType.Info);
        }
    }
}
#endif