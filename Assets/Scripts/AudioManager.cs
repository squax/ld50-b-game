using Squax.Patterns;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AudioBank
{
    [SerializeField]
    private string id;

    [SerializeField]
    private AudioClip audioClip;

    public string ID { get { return id;  }  }

    public AudioClip AudioClip { get { return audioClip; } }
}

public class AudioManager : UnitySingleton<AudioManager>
{
    [SerializeField]
    private List<AudioBank> audioLibrary = new List<AudioBank>();

    private Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    private AudioSource audioSource;

    public float SFXVolume { get; set; }

    void Start()
    {
        foreach( var clip in audioLibrary)
        {
            cache.Add(clip.ID, clip.AudioClip);
        }

        audioSource = GetComponent<AudioSource>();

        SFXVolume = 1.0f;
    }

    public void PlayOneShot(string id, AudioSource source, float volume)
    {
        if(cache.ContainsKey(id) == false)
        {
            return;
        }

        var clip = cache[id];

        source.PlayOneShot(clip, volume * SFXVolume);
    }

    public void PlayOneShot(string id, float volume)
    {
        if (cache.ContainsKey(id) == false)
        {
            return;
        }

        var clip = cache[id];

        audioSource.PlayOneShot(clip, volume * SFXVolume);
    }
}
