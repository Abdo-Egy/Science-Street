using System.Linq;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager Instance;
    [SerializeField] RectTransform Arrow;
    [SerializeField] float SpeedArrow;
    [HideInInspector] public List<ItemPosition> Items = new List<ItemPosition>();

    private void Awake()
    {
        Instance = this;
    }

    public void CheckIfItemsInCorrectPosition()
    {
        if (Items.All(home => home.inHome))
        {
            Arrow.DORotate(new Vector3(0, 0, -30f), SpeedArrow).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
        }
        else
        {
            Arrow.DORotate(new Vector3(0, 0, 0), SpeedArrow).SetEase(Ease.Linear).OnComplete(() => DOTween.Pause(Arrow));
        }
    }
    private void OnDestroy()
    {
        DOTween.Kill(Arrow);
    }
}
