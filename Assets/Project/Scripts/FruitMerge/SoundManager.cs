using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFXType { Merge, Drop, Bomb }

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip mergeClip;
    [SerializeField] private AudioClip dropClip;
    [SerializeField] private AudioClip bombClip;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PlaySFX(SFXType type)
    {
        switch (type)
        {
            case SFXType.Merge: sfxSource.PlayOneShot(mergeClip); break;
            case SFXType.Drop: sfxSource.PlayOneShot(dropClip); break;
            case SFXType.Bomb: sfxSource.PlayOneShot(bombClip); break;
        }
    }

    public void SetMusicVolume(float value) => musicSource.volume = value;
    public void SetSFXVolume(float value) => sfxSource.volume = value;
}

