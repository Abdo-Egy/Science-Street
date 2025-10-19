using System.Collections.Generic;
using UnityEngine;



// Base class for dialogue messages
    public abstract class DialogueMessageBase : ScriptableObject
    {
        [Header("Character Information")] public Sprite characterSprite;
        public string characterName;

        [Header("Message Content")] [TextArea(3, 5)]
        public string messageText;

        public AudioClip voiceClip;

        [Header("Dialogue Flow")] public List<DialogueReply> replies = new List<DialogueReply>();
        public DialogueMessageBase nextMessage; // Used when there are no replies
    }




