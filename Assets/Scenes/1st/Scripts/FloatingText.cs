using UnityEngine;
using DG.Tweening;
public class FloatingText : MonoBehaviour
{
    [SerializeField] RectTransform RectTransform;
    [SerializeField] CanvasGroup CanvasGroup;
    [SerializeField] float Duraction;
    [SerializeField] float MaxHight;

    [SerializeField] Ease Ease;
    void Start()
    {
        //RectTransform.DOAnchorPosY(-MaxHight, Duraction).SetLoops(-1,LoopType.Yoyo).SetEase(Ease);
        CanvasGroup.DOFade(0, Duraction).SetLoops(-1, LoopType.Yoyo).SetEase(Ease);
    }
    private void OnDestroy()
    {
        DOTween.Kill(CanvasGroup);
    }
}
