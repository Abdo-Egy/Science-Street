using TMPro;
using UnityEngine;

namespace ALArcade.ArabicTMP
{
    /// <summary>
    /// Arabic-enabled TextMeshPro component for 3D/World space
    /// </summary>
    [AddComponentMenu("Text/Arabic TextMeshPro - Text", 10)]
    public class ArabicTextMeshPro : TextMeshPro
    {
        // Original (editable) Arabic text
        [SerializeField] private string m_ArabicText;

        // Plugin options
        [Tooltip("Show Arabic diacritics (tashkeel)")]
        [SerializeField] private bool m_ShowTashkeel = true;

        [Tooltip("Keep Western digits (123) as-is instead of converting to Eastern Arabic")]
        [SerializeField] private bool m_PreserveNumbers = true;

        [Tooltip("Process rich text tags (color, bold, etc.)")]
        [SerializeField] private bool m_FixTags = true;

        [Tooltip("Force RTL even if text starts with Latin letters")]
        [SerializeField] private bool m_ForceRTL = true; // FIXED: Default to true for better Arabic support

        /// <summary>
        /// The original Arabic text (not fixed for rendering)
        /// </summary>
        public string arabicText
        {
            get { return m_ArabicText; }
            set
            {
                if (m_ArabicText == value)
                    return;

                m_ArabicText = value;

                // Apply Arabic text processing and update TMP text
                text = ArabicSupport.Fix(m_ArabicText, m_ShowTashkeel, m_PreserveNumbers, m_FixTags, m_ForceRTL);
            }
        }

        /// <summary>
        /// Show or hide Arabic diacritics (tashkeel)
        /// </summary>
        public bool showTashkeel
        {
            get { return m_ShowTashkeel; }
            set
            {
                if (m_ShowTashkeel == value)
                    return;

                m_ShowTashkeel = value;

                // Re-process text with new setting
                if (!string.IsNullOrEmpty(m_ArabicText))
                    text = ArabicSupport.Fix(m_ArabicText, m_ShowTashkeel, m_PreserveNumbers, m_FixTags, m_ForceRTL);
            }
        }

        /// <summary>
        /// Preserve Western digits (123) as-is
        /// </summary>
        public bool preserveNumbers
        {
            get { return m_PreserveNumbers; }
            set
            {
                if (m_PreserveNumbers == value)
                    return;

                m_PreserveNumbers = value;

                // Re-process text with new setting
                if (!string.IsNullOrEmpty(m_ArabicText))
                    text = ArabicSupport.Fix(m_ArabicText, m_ShowTashkeel, m_PreserveNumbers, m_FixTags, m_ForceRTL);
            }
        }

        /// <summary>
        /// Process rich text tags (color, bold, etc.)
        /// </summary>
        public bool fixTags
        {
            get { return m_FixTags; }
            set
            {
                if (m_FixTags == value)
                    return;

                m_FixTags = value;

                // Re-process text with new setting
                if (!string.IsNullOrEmpty(m_ArabicText))
                    text = ArabicSupport.Fix(m_ArabicText, m_ShowTashkeel, m_PreserveNumbers, m_FixTags, m_ForceRTL);
            }
        }

        /// <summary>
        /// Force RTL even if text starts with Latin letters
        /// </summary>
        public bool forceRTL
        {
            get { return m_ForceRTL; }
            set
            {
                if (m_ForceRTL == value)
                    return;

                m_ForceRTL = value;

                // Re-process text with new setting
                if (!string.IsNullOrEmpty(m_ArabicText))
                    text = ArabicSupport.Fix(m_ArabicText, m_ShowTashkeel, m_PreserveNumbers, m_FixTags, m_ForceRTL);
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            // Update displayed text whenever properties change in the editor
            if (!string.IsNullOrEmpty(m_ArabicText))
            {
                text = ArabicSupport.Fix(m_ArabicText, m_ShowTashkeel, m_PreserveNumbers, m_FixTags, m_ForceRTL);
            }

            // Set alignment to right by default for Arabic text
            if (alignment == TextAlignmentOptions.TopLeft ||
                alignment == TextAlignmentOptions.Left ||
                alignment == TextAlignmentOptions.BottomLeft)
            {
                alignment = TextAlignmentOptions.Right;
            }
        }
#endif

        protected override void Awake()
        {
            base.Awake();

            // Initialize with proper alignment for RTL text
            if (alignment == TextAlignmentOptions.TopLeft)
                alignment = TextAlignmentOptions.TopRight;
            else if (alignment == TextAlignmentOptions.Left)
                alignment = TextAlignmentOptions.Right;
            else if (alignment == TextAlignmentOptions.BottomLeft)
                alignment = TextAlignmentOptions.BottomRight;

            // Set RTL flag (for TMP's own RTL awareness)
            isRightToLeftText = true;

            // Initial text processing
            if (!string.IsNullOrEmpty(m_ArabicText))
            {
                text = ArabicSupport.Fix(m_ArabicText, m_ShowTashkeel, m_PreserveNumbers, m_FixTags, m_ForceRTL);
            }
        }
    }
}