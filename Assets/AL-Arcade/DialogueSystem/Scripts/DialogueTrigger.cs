
using UnityEngine;
namespace AL_Arcade.DialogueSystem.Scripts
{
   public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueSequence dialogueSequence;
    public DialogueMessageBase singleMessage;
    
    [Header("Trigger Settings")]
    public bool triggerOnStart = false;
    public bool triggerOnCollision = false;
    public bool triggerOnInteract = true;
    public KeyCode interactKey = KeyCode.E;
    
    [Header("Visual Feedback")]
    public GameObject interactionPrompt;
    
    private bool playerInRange = false;
    private bool hasTriggered = false;
    
    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
        
        if (triggerOnStart)
            TriggerDialogue();
    }
    
    void Update()
    {
        if (triggerOnInteract && playerInRange && Input.GetKeyDown(interactKey))
        {
            TriggerDialogue();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            if (interactionPrompt != null && !hasTriggered)
                interactionPrompt.SetActive(true);
            
            if (triggerOnCollision)
                TriggerDialogue();
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }
    
    public void TriggerDialogue()
    {
        if (hasTriggered && !triggerOnInteract) return;
        
        if (DialogueManager.Instance != null)
        {
            if (dialogueSequence != null)
                DialogueManager.Instance.StartDialogue(dialogueSequence);
            else if (singleMessage != null)
                DialogueManager.Instance.StartDialogue(singleMessage);
        }
        
        hasTriggered = true;
        
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
}
}