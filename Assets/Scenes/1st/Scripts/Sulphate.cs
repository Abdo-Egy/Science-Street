using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class Sulphate : MonoBehaviour, ItemAction
{
    [SerializeField] float AnimationDuration;
    [SerializeField] float MaxRotation;
    [SerializeField] int Loops;
    [SerializeField] ParticleSystem ParticleEffect;
    [SerializeField] Transform Powder;
    [SerializeField] float OffsetBeforeAnimation = 0.025f;  
    [SerializeField] float MaxPosX = 0.045f;
    Vector3 PowderPos;
    private void Start()
    {
        PowderPos = Powder.transform.localPosition;
    }

    public void StartAction()
    {
        Powder.transform.localPosition = new Vector3(PowderPos.x - OffsetBeforeAnimation, PowderPos.y, PowderPos.z);
        Powder.DOLocalMoveX(MaxPosX, AnimationDuration).SetLoops(Loops, LoopType.Yoyo);

        int step = 0;
        Tween tween = null;
        bool isPlayingEffect = false;
        tween = transform.DORotate(new Vector3(transform.rotation.x, transform.rotation.y, MaxRotation),
            AnimationDuration)
            .SetLoops(Loops, LoopType.Yoyo).OnUpdate(() =>
            {
                float progress = tween.ElapsedDirectionalPercentage();
                if (!isPlayingEffect && progress >= 0.5f && step % 2 != 0)
                {
                    ParticleEffect.Play();
                    isPlayingEffect = true;
                }
            }).OnStepComplete(() =>
            {
                step++;
                isPlayingEffect = false;
            })
            .OnComplete(() =>
            {
                ParticleEffect.Stop();
                Powder.transform.localPosition = PowderPos;
                GetComponent<ItemPosition>().ReturnToHome();
            });
    }
    public void StopAction()
    {
        DOTween.KillAll();
    }
}
