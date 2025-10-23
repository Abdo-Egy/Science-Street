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
    [SerializeField] float maxRotate;
    [SerializeField] float ToLiquidVolume;
    [SerializeField] float timeToLiquidVolume;

    public void StartAction()
    {
        transform.DORotate(new Vector3(2.035555e-13f, 180f, maxRotate), AnimationDuration);
        LiquidVolume ThisLiquidVolume = GetComponentInChildren<LiquidVolume>();
        DOTween.To(() => ThisLiquidVolume.level, x => ThisLiquidVolume.level = x, ToLiquidVolume, timeToLiquidVolume);
        GetComponent<SoundEffect>().PlaySound();
        WaterEffect.Play();
        LiquidVolume.alpha = 1;
        DOTween.To(() => LiquidVolume.level, x => LiquidVolume.level = x, .6f, AnimationDuration).OnComplete(() =>
        {
            GetComponent<ItemPosition>().ReturnToHome();
            WaterEffect.Stop();
            LiquidVolume.turbulence2 = 0;
        });
    }

    public void ChangerWater()
    {
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
