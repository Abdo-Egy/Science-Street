using DG.Tweening;
using LiquidVolumeFX;
using System.Collections;
using UnityEngine;

public class Water : MonoBehaviour, ItemAction
{
    [SerializeField] float AnimationDuration;
    [SerializeField] ParticleSystem WaterEffect;
    [SerializeField] LiquidVolume LiquidVolume;
    [SerializeField] GameObject AnotherWater;
    [Header("Effect")]
    [SerializeField] Vector3 maxRotate;
    [SerializeField] float ToLiquidVolume;
    [SerializeField] float timeToLiquidVolume;

    public void StartAction()
    {
        transform.DORotate(maxRotate, AnimationDuration);
        LiquidVolume ThisLiquidVolume = GetComponentInChildren<LiquidVolume>();
        DOTween.To(() => ThisLiquidVolume.level, x => ThisLiquidVolume.level = x, ToLiquidVolume, timeToLiquidVolume);
        GetComponent<SoundEffect>().PlaySound();
        WaterEffect.Play();
        LiquidVolume.alpha = 1;
        DOTween.To(() => LiquidVolume.level, x => LiquidVolume.level = x, .3f, AnimationDuration).OnComplete(() =>
        {
            GetComponent<ItemPosition>().ReturnToHome();
            WaterEffect.Stop();
            GetComponent<SoundEffect>().StopSound();

            LiquidVolume.turbulence2 = 0;
        });
    }

    public void ChangerWater()
    {
        if(AnotherWater!=null)
            StartCoroutine(Enable());
    }
    IEnumerator Enable()
    {
        yield return new WaitForSeconds(1.5f);
        AnotherWater.SetActive(true);
        this.gameObject.SetActive(false);
    }
    public void StopAction()
    {
        WaterEffect.Stop();
        DOTween.KillAll();
    }
}
