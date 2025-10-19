using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UISequentialAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.4f;
    [SerializeField] private float delayBetweenElements = 0.1f;
    [SerializeField] private float moveDistance = 50f;
    [SerializeField] private Ease fadeInEase = Ease.OutQuart;
    [SerializeField] private Ease fadeOutEase = Ease.InQuart;
    
    // The sequence objects we'll use to control the animations
    private Sequence fadeInSequence;
    private Sequence fadeOutSequence;
    
    [Header("Animation List")]
    public List<RectTransform> uiElements;
    
    /// <summary>
    /// Fade in UI elements sequentially with an upward movement
    /// </summary>
    public void FadeInElementsSequentially()
    {
        // Kill previous sequences if they exist
        if (fadeInSequence != null && fadeInSequence.IsActive())
            fadeInSequence.Kill(true);
            
        if (fadeOutSequence != null && fadeOutSequence.IsActive())
            fadeOutSequence.Kill(true);
        
        // Create a new sequence
        fadeInSequence = DOTween.Sequence();
        
        for (int i = 0; i < uiElements.Count; i++)
        {
            RectTransform rt = uiElements[i];
            CanvasGroup canvasGroup = EnsureCanvasGroup(rt.gameObject);
            
            // Store original position
            Vector2 originalPosition = rt.anchoredPosition;
            
            // Set initial state
            canvasGroup.alpha = 0f;
            rt.anchoredPosition = originalPosition - new Vector2(0, moveDistance);
            
            // Create a sub-sequence for this element
            Sequence elementSequence = DOTween.Sequence();
            
            // Add fade and move tweens
            elementSequence.Append(canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase))
                          .Join(rt.DOAnchorPos(originalPosition, fadeInDuration).SetEase(fadeInEase));
            
            // Add to main sequence with appropriate delay
            fadeInSequence.Insert(i * delayBetweenElements, elementSequence);
        }
        
        fadeInSequence.Play();
    }
    
    /// <summary>
    /// Fade out UI elements sequentially with a downward movement
    /// </summary>
    public void FadeOutElementsSequentially()
    {
        // Kill previous sequences if they exist
        if (fadeOutSequence != null && fadeOutSequence.IsActive())
            fadeOutSequence.Kill(true);
            
        if (fadeInSequence != null && fadeInSequence.IsActive())
            fadeInSequence.Kill(true);
        
        // Create a new sequence
        fadeOutSequence = DOTween.Sequence();
        
        // For fade out, we often want to start from the last element for satisfying UX
        for (int i = uiElements.Count - 1; i >= 0; i--)
        {
            RectTransform rt = uiElements[i];
            CanvasGroup canvasGroup = EnsureCanvasGroup(rt.gameObject);
            
            // Store original position
            Vector2 originalPosition = rt.anchoredPosition;
            
            // Create a sub-sequence for this element
            Sequence elementSequence = DOTween.Sequence();
            
            // Add fade and move tweens
            elementSequence.Append(canvasGroup.DOFade(0f, fadeOutDuration).SetEase(fadeOutEase))
                          .Join(rt.DOAnchorPos(originalPosition - new Vector2(0, moveDistance), fadeOutDuration).SetEase(fadeOutEase));
            
            // Add to main sequence with appropriate delay
            fadeOutSequence.Insert((uiElements.Count - 1 - i) * delayBetweenElements, elementSequence);
        }
        
        fadeOutSequence.Play();
    }
    
    /// <summary>
    /// Ensures the GameObject has a CanvasGroup component
    /// </summary>
    private CanvasGroup EnsureCanvasGroup(GameObject obj)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = obj.AddComponent<CanvasGroup>();
        return canvasGroup;
    }
}