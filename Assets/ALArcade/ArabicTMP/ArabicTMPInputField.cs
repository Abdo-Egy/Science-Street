using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ALArcade.ArabicTMP
{
    [AddComponentMenu("UI/Arabic TextMeshPro - Input Field", 12)]
    public class ArabicTMPInputField : TMP_InputField
    {
        [SerializeField] private bool m_ShowTashkeel = true;
        [SerializeField] private bool m_PreserveNumbers = true;
        [SerializeField] private bool m_FixTags = true;
        [SerializeField] private bool m_ForceRTL = true; // FIXED: Default to true for better Arabic support

        private string m_OriginalText = "";
        private bool m_ProcessingInternalChange = false;

        // Shadow the base .text property
        public new string text
        {
            get => m_OriginalText;
            set
            {
                if (m_OriginalText == value)
                    return;

                m_OriginalText = value;
                m_ProcessingInternalChange = true;
                var processed = ArabicSupport.Fix(value, m_ShowTashkeel, m_PreserveNumbers, m_FixTags, m_ForceRTL);
                base.SetTextWithoutNotify(processed);
                m_ProcessingInternalChange = false;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (textComponent != null)
            {
                // FIXED: Ensure text component is properly set up for RTL
                textComponent.isRightToLeftText = true;
                textComponent.alignment = TextAlignmentOptions.Right;
            }

            // Ensure text viewport has correct layout for RTL
            if (textViewport != null)
            {
                // Adjust padding to make sure text appears correctly aligned
                textComponent.margin = new Vector4(0, 0, 5, 0); // Add right margin for RTL text
            }
        }

        protected override void Start()
        {
            base.Start();
            onValueChanged.AddListener(OnValueChangedHandler);

            if (!string.IsNullOrEmpty(m_OriginalText))
                text = m_OriginalText;
        }

        private void OnValueChangedHandler(string value)
        {
            // Only update our stored original if this wasn't triggered by our own setText
            if (!m_ProcessingInternalChange)
            {
                m_OriginalText = value;
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            // When selected, ensure caretPosition is at the proper end for RTL
            if (caretPosition == 0 && m_OriginalText.Length > 0)
            {
                caretPosition = 0; // For RTL, visual position 0 is at the right side
            }
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            // Ensure text is properly formatted on deselection
            text = m_OriginalText;
        }

        // Override LateUpdate to re-process text before each frame render
        protected override void LateUpdate()
        {
            base.LateUpdate();

            // Apply Arabic shaping if the text changed and we're not in the middle of our own change
            if (!m_ProcessingInternalChange)
            {
                var processed = ArabicSupport.Fix(m_OriginalText, m_ShowTashkeel, m_PreserveNumbers, m_FixTags, m_ForceRTL);
                if (base.text != processed)
                {
                    m_ProcessingInternalChange = true;
                    base.SetTextWithoutNotify(processed);
                    m_ProcessingInternalChange = false;
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!string.IsNullOrEmpty(m_OriginalText))
            {
                m_ProcessingInternalChange = true;
                var processed = ArabicSupport.Fix(m_OriginalText, m_ShowTashkeel, m_PreserveNumbers, m_FixTags, m_ForceRTL);
                base.SetTextWithoutNotify(processed);
                m_ProcessingInternalChange = false;
            }

            if (textComponent != null)
            {
                textComponent.isRightToLeftText = true;
                textComponent.alignment = TextAlignmentOptions.Right;
            }
        }
#endif
    }
}