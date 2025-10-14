using DG.Tweening;
using LiquidVolumeFX;
using System.Collections;
using UnityEngine;
using static UnityEditor.Progress;

public class Water : MonoBehaviour, ItemAction
{
    [SerializeField] float AnimationDuration;
    [SerializeField] ParticleSystem WaterEffect;
    [SerializeField] LiquidVolume LiquidVolume;
    [SerializeField] GameObject AnotherWater;

    bool doneWithFrist;
    public void StartAction()
    {
        WaterEffect.Play();
        DOTween.To(() => LiquidVolume.level, x => LiquidVolume.level = x, .6f, AnimationDuration).OnComplete(() =>
        {
            GetComponent<ItemPosition>().ReturnToHome();
            WaterEffect.Stop();
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
    void ItemAction.StopAction()
    {
        WaterEffect.Stop();
        DOTween.KillAll();
    }
}
