using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgmManager : MonoBehaviour
{
    public bool isAwake = true;

    public float duration = 4f;//间隔时间

    public int times = 1;

    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (isAwake)
        {
            StartCoroutine(PlayMusicLoop());
        }
    }

    IEnumerator PlayMusicLoop()
    {
        AudioClip musicClip = audioSource.clip;
        

        while (true)
        {
            yield return new WaitForSeconds(duration); // 每次播放前等待4秒


            for (int i = 0; i < times; i++)
            {
                audioSource.Play();

                yield return StartCoroutine(FadeIn(2f));

                yield return new WaitForSeconds(musicClip.length - 4f);

                // 淡出效果
                yield return StartCoroutine(FadeOut(2f));

                audioSource.Stop();
            }
        }
    }

    IEnumerator FadeOut(float duration)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }
    }
    IEnumerator FadeIn(float duration)
    {
        float targetVolume = 1f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, timer / duration);
            yield return null;
        }
    }
}
