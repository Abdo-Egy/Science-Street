using AL_Arcade.DialogueSystem.Scripts;
using DG.Tweening;
using LiquidVolumeFX;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager Instance;
    [SerializeField] RectTransform Arrow;
    [SerializeField] float SpeedArrow;
    [Header("liquidVolume")]
    [SerializeField] LiquidVolume CopperWater;
    [SerializeField] LiquidVolume ZincWater;

    [SerializeField] List<ItemPosition> Items = new List<ItemPosition>();

    private void Awake()
    {
        Instance = this;
    }

    public void CheckIfItemsInCorrectPosition(ItemPosition item)
    {
        if (Items.All(home => home.doneWithAction))
        {
            Arrow.DORotate(new Vector3(0, 0, -30f), SpeedArrow).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
        }
        else
        {
            Arrow.DORotate(new Vector3(0, 0, 0), SpeedArrow).SetEase(Ease.Linear).OnComplete(() => DOTween.Pause(Arrow));
        }
        ExpStep(item);
    }

    private void ExpStep(ItemPosition item)
    {

    }

    private void OnDestroy()
    {
        DOTween.Kill(Arrow);
    }
    public void RealoadScene()
    {
        GameContextBuilder.Instance.ClearActions();
        GameContextBuilder.Instance.ClearObjective();
        SceneManager.LoadScene(0);
    }
    public void CloseApp ()
    {
        Application.Quit();
    }
}
