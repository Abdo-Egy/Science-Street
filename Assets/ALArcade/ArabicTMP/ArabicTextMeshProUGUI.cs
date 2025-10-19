using TMPro;
using UnityEngine;

namespace ALArcade.ArabicTMP
{
    [AddComponentMenu("UI/Arabic TextMeshPro - Text (UI)", 11)]
    public class ArabicTextMeshProUGUI : TextMeshProUGUI
    {
        [Header("Arabic Settings")]
        [SerializeField] private string m_ArabicText = "";
        [SerializeField] private bool m_ShowTashkeel = true;
        [SerializeField] private bool m_PreserveNumbers = true;
        [SerializeField] private bool m_FixTags = true;
        [SerializeField] private bool m_ForceRTL = true;

        public string arabicText
        {
            get => m_ArabicText;
            set
            {
                if (m_ArabicText == value) return;
                m_ArabicText = value;
                UpdateDisplayText();
            }
        }

        public bool showTashkeel
        {
            get => m_ShowTashkeel;
            set
            {
                if (m_ShowTashkeel == value) return;
                m_ShowTashkeel = value;
                UpdateDisplayText();
            }
        }

        public bool preserveNumbers
        {
            get => m_PreserveNumbers;
            set
            {
                if (m_PreserveNumbers == value) return;
                m_PreserveNumbers = value;
                UpdateDisplayText();
            }
        }

        public bool fixTags
        {
            get => m_FixTags;
            set
            {
                if (m_FixTags == value) return;
                m_FixTags = value;
                UpdateDisplayText();
            }
        }

        public bool forceRTL
        {
            get => m_ForceRTL;
            set
            {
                if (m_ForceRTL == value) return;
                m_ForceRTL = value;
                UpdateDisplayText();
            }
        }

        private void UpdateDisplayText()
        {
            if (!string.IsNullOrEmpty(m_ArabicText))
            {
                text = ArabicSupport.Fix(m_ArabicText, m_ShowTashkeel, m_PreserveNumbers, m_FixTags, m_ForceRTL);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            // Set RTL properties
            isRightToLeftText = true;

            // Set default alignment for Arabic
            if (alignment == TextAlignmentOptions.TopLeft)
                alignment = TextAlignmentOptions.TopRight;
            else if (alignment == TextAlignmentOptions.Left)
                alignment = TextAlignmentOptions.Right;
            else if (alignment == TextAlignmentOptions.BottomLeft)
                alignment = TextAlignmentOptions.BottomRight;

            UpdateDisplayText();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            UpdateDisplayText();
        }
#endif
    }
}