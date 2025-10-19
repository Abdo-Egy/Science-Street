using UnityEngine;
using DG.Tweening;

public class PopupUI : MonoBehaviour
{
    [Header("Popup Settings")]
    public float duration = 0.4f;
    public Ease easeType = Ease.OutBack;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        // Reset to zero scale first
        transform.localScale = Vector3.zero;

        // Animate back to original
        transform.DOScale(originalScale, duration).SetEase(easeType);
    }
}