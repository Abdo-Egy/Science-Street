using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class SoundEffect : MonoBehaviour
{
    [SerializeField] float PlayAfter;
    [SerializeField] float FadeDuraction;
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void StopSound()
    {
        audioSource.DOFade(0, FadeDuraction).OnComplete(() => audioSource.Stop());
    }
    public void PlaySound()
    {
        StartCoroutine(Play());
    }
    IEnumerator Play()
    {
        audioSource.volume = 1;
        yield return new WaitForSeconds(PlayAfter);
        audioSource.Play();
    }
}
