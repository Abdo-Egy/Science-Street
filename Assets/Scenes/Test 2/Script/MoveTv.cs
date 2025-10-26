using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; 

public class MoveTv : MonoBehaviour
{
    [SerializeField] Vector3 TargetPosition;
    [SerializeField] Vector3 TargetRotation;
    [SerializeField] float Duration = 1f;
    [SerializeField] Ease EaseType;

    void Start()
    {
        transform.DOMove(TargetPosition, Duration).SetEase(EaseType).SetLoops(-1,LoopType.Yoyo);
        transform.DORotate(TargetRotation, Duration).SetEase(EaseType).SetLoops(-1, LoopType.Yoyo);
    }

}
