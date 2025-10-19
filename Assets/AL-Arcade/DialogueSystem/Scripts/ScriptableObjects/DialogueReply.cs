using UnityEngine;


    [CreateAssetMenu(fileName = "DialogueReply", menuName = "Dialogue/Reply")]
    public class DialogueReply : ScriptableObject
    {
        [TextArea(2, 3)] public string replyText;
        public AudioClip replyAudioClip;
        public DialogueMessageBase nextMessage;

        [Header("Optional Conditions")] public bool requiresCondition;
        public string conditionKey; // Can be used for quest/stat checks
    }

