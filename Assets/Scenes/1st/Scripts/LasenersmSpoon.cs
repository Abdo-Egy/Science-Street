using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LasenersmSpoon : MonoBehaviour, ItemAction
{
    [SerializeField] float PlayerAnimationAfter;
    [SerializeField] float AnimationDuraction;
    [Header("Step 1")] // Move Bottle Head to Target Position
    [SerializeField] Transform BottleHead;
    [SerializeField] Transform TargetBottleHead;
    [Header("Step 2")] // Move spoon to the bottle
    [SerializeField] Transform TargetBottle;
    [Header("Step 3")] // scooping some powder 
    [SerializeField] GameObject Powder;
    [SerializeField] Vector3 MaxRotateToScoopPowder;
    [Header("Step 4")] // move to the Beaker
    [SerializeField] Vector3 BeakerPosition;
    //[SerializeField] GameObject CopperLasenersmSpoon;
    [Header("Step 5")] // place powder in Beaker
    [SerializeField] Vector3 RotateToBeaker;
    [Header("Step 6")] // move powder to Beaker
    [SerializeField] Vector3 MovePowderPosition;

    //[Header("Step 1 scooping some powder")]
    //[SerializeField] Vector3 MaxRotateToScoopPowder;

    //[SerializeField] Vector3 MoveBeakerPosition;
    //[SerializeField] Vector3 RotateToPlacePowderInBeaker;
    //[SerializeField] Vector3 MovePowderPosition;
    [SerializeField] float MovePowderPositionDuraction;

    public void StartAction()
    {
        Vector3 startPos = BottleHead.position;
        Quaternion startRot = BottleHead.rotation;

        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>{GetComponent<ItemPosition>().enabled = false;});
        ///////////////////////////////////////////// Step 1
        sequence.Append(BottleHead.DOMove(TargetBottleHead.position, AnimationDuraction));
        sequence.Join(BottleHead.transform.DORotateQuaternion(TargetBottleHead.rotation, AnimationDuraction));
        ///////////////////////////////////////////// Step 2
        sequence.Append(transform.DOMove(TargetBottle.position, AnimationDuraction));
        sequence.Join(transform.transform.DORotateQuaternion(TargetBottle.rotation, AnimationDuraction)) ;
        ///////////////////////////////////////////// Step 3
        sequence.AppendCallback(() => { Powder.SetActive(true); });
        sequence.Append(transform.DORotate(MaxRotateToScoopPowder, AnimationDuraction));
        //////////////////////////////////////////// Step 4
        sequence.Append(transform.DOMove(BeakerPosition, AnimationDuraction));
        //////////////////////////////////////////// Step 5
        sequence.Append(transform.DORotate(RotateToBeaker, AnimationDuraction));
        /////////////////////////////////////////// Step 6
        sequence.Join(Powder.transform.DOMove(MovePowderPosition, MovePowderPositionDuraction));

        sequence.AppendCallback(() =>
        {
            GetComponent<ItemPosition>().enabled = true;
            GetComponent<ItemPosition>().ReturnToHome();
        });

        /////////////////////////////////////////// Final Step


        //sequence.AppendCallback(() =>
        //{
        //    Powder.SetActive(true);
        //    GetComponent<ItemPosition>().enabled = false;
        //});
        //sequence.Append(transform.DOMove(MoveBeakerPosition, AnimationDuraction));
        //sequence.Append(transform.DORotate(RotateToPlacePowderInBeaker, AnimationDuraction));
        //sequence.JoinCallback(() =>
        //{
        //    Powder.transform.position = new Vector3(Powder.transform.position.x, Powder.transform.position.y, -0.966f);
        //    Powder.transform.DOMove(MovePowderPosition, MovePowderPositionDuraction);
        //}).OnComplete(() =>Powder.SetActive(false));
        //sequence.AppendInterval(.1f);
        //sequence.AppendCallback(() =>
        //{
        //    GetComponent<ItemPosition>().enabled = true;
        //    GetComponent<ItemPosition>().ReturnToHome();
        //});



    }
    public void ChangerLasenersmSpoon()
    {
        StartCoroutine(Enable());
    }
    IEnumerator Enable()
    {
        yield return new WaitForSeconds(1.5f);
        this.gameObject.SetActive(false);
        //CopperLasenersmSpoon.SetActive(true);
    }
    private void Update()
    {

    }
    public void StopAction()
    {
    }
}
