using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LasenersmSpoon : MonoBehaviour, ItemAction
{
    [SerializeField] GameObject Powder;
    [SerializeField] GameObject CopperLasenersmSpoon;
    [SerializeField] float PlayerAnimationAfter;
    [SerializeField] float AnimationDuraction;
    [Header("Step 1 scooping some powder")]
    [SerializeField] Vector3 MaxRotateToScoopPowder;
    [SerializeField] Vector3 MoveBeakerPosition;
    [SerializeField] Vector3 RotateToPlacePowderInBeaker;
    [SerializeField] Vector3 MovePowderPosition;
    [SerializeField] float MovePowderPositionDuraction;

    public void StartAction()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(PlayerAnimationAfter);

        sequence.Append(transform.DORotate(MaxRotateToScoopPowder, AnimationDuraction));
        sequence.AppendCallback(() =>
        {
            Powder.SetActive(true);
            GetComponent<ItemPosition>().enabled = false;
        });
        sequence.Append(transform.DOMove(MoveBeakerPosition, AnimationDuraction));
        sequence.Append(transform.DORotate(RotateToPlacePowderInBeaker, AnimationDuraction));
        sequence.JoinCallback(() =>
        {
            Powder.transform.position = new Vector3(Powder.transform.position.x, Powder.transform.position.y, -0.966f);
            Powder.transform.DOMove(MovePowderPosition, MovePowderPositionDuraction);
        }).OnComplete(() =>Powder.SetActive(false));
        sequence.AppendInterval(.1f);
        sequence.AppendCallback(() =>
        {
            GetComponent<ItemPosition>().enabled = true;
            GetComponent<ItemPosition>().ReturnToHome();
        });



    }
    public void ChangerLasenersmSpoon()
    {
        StartCoroutine(Enable());
    }
    IEnumerator Enable()
    {
        yield return new WaitForSeconds(1.5f);
        this.gameObject.SetActive(false);
        CopperLasenersmSpoon.SetActive(true);
    }
    private void Update()
    {

    }
    public void StopAction()
    {
    }
}
