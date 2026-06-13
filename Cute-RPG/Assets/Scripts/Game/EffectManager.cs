using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public AudioSource audioSource;

    [Space]
    public AudioClip UI;
    public AudioClip jump;
    public AudioClip Victory;
    public AudioClip eat;

    private void Awake()
    {
        GlobalData.effectManager = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayEffect(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
