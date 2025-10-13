using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
public class FadeAnimation : MonoBehaviour
{
    [SerializeField] Material Material;
    [SerializeField] float FadeSpeed = .5f;
    private void OnEnable()
    {
        Color c = Material.color;
        c.a = 0f;
        Material.color = c;
        Material.DOFade(.35f, FadeSpeed).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
    }
    private void OnDisable()
    {
        DOTween.Kill(Material);
    }
}
