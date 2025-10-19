using UnityEngine;

[CreateAssetMenu(fileName = "DialogueSequence", menuName = "Dialogue/Sequence")]
public class DialogueSequence : ScriptableObject
{
    public string sequenceName;
    public DialogueMessageBase firstMessage;
    public bool pauseGameDuringDialogue = true;
}