using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using LiquidVolumeFX;

public class Spoon : MonoBehaviour, ItemAction
{
    [SerializeField] float AnimationDuraction;
    [SerializeField] int MaxLoops;
    [SerializeField] float Maxrotation;
    [SerializeField] LoopType LoopType;
    [SerializeField] GameObject AnotherSpoon;
    [SerializeField] LiquidVolume liquidVolume;
    public void StartAction()
    {
        TweenLiquidVolume(.2f);
        transform.DORotate(new Vector3(-12.59f, Maxrotation, 90), AnimationDuraction)
            .SetLoops(MaxLoops).OnComplete( () =>{ GetComponent<ItemPosition>().ReturnToHome(); ChangerSpoon(); });
    }
    public void ChangerSpoon()
    {
        StartCoroutine(Enable());
    }
    IEnumerator Enable()
    {
        yield return new WaitForSeconds(1.5f);
        AnotherSpoon.SetActive(true);
        this.gameObject.SetActive(false);
    }
    public void StopAction()
    {
        DOTween.KillAll();
        TweenLiquidVolume(0);
    }
    void TweenLiquidVolume(float Value)
    {
        DOTween.To(() => liquidVolume.turbulence2, x => liquidVolume.turbulence2 = x, Value, .5f);
    }
}
