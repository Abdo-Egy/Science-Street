using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Sulphate : MonoBehaviour,ItemAction
{
    [SerializeField] float AnimationDuration;
    [SerializeField] float MaxRotation;
    [SerializeField] int Loops;
    public void StartAction()
    {
        transform.DORotate(new Vector3(transform.rotation.x, transform.rotation.y, MaxRotation),
            AnimationDuration).SetLoops(Loops, LoopType.Yoyo).OnComplete(() => GetComponent<ItemPosition>().ReturnToHome());
    }
    public void StopAction()
    {
        DOTween.KillAll();
    }

}
