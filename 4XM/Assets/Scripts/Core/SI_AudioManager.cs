using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;
/// <summary>
/// Four built-in functions. Play, Stop, Pause and Unpause. Requires the string of the target sound clip for all.
/// </summary>
public class SI_AudioManager : MonoBehaviour
{
    public static SI_AudioManager Instance;

    public AudioMixer masterMixer;


    public bool musicOn;
    public bool audioOn;
    string themeName;

  

    public string uiClick;
    public string uiConfirm;
    public string menuTheme;

    public string greekTheme;
    public string romeTheme;
    public string egyptTheme;
    public string celtsTheme;


    //write new things above that pls <3 
    public HB_Sound[] sounds;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        GameObject audioSources = new GameObject { name = "Audio Sources" };
        audioSources.transform.SetParent(transform);
        foreach (var s in sounds)
        {
            s.source = audioSources.AddComponent<AudioSource>();
            s.source.outputAudioMixerGroup = s.mixerGroup;
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.mute = s.mute;
            s.source.loop = s.loop;
            s.source.playOnAwake = s.playOnAwake;

        }

        audioOn = true;
        musicOn = true;
        masterMixer.SetFloat("MusicVolume", 0f);
        masterMixer.SetFloat("SfxVolume", 0f);
    }

    private void Start()
    {
        PlayTheme(Civilizations.None);
    }

    public void PlayClick()
    {
        Play(uiClick);
    }

    public void PlayTheme(Civilizations civType)
    {
        string themeToPlay = menuTheme;
        switch (civType)
        {
            case Civilizations.None:
                themeToPlay = menuTheme;
                break;
            case Civilizations.Greeks:
                themeToPlay = greekTheme;
                break;
            case Civilizations.Egyptians:
                themeToPlay = egyptTheme;
                break;
            case Civilizations.Celts:
                themeToPlay = celtsTheme;
                break;
            case Civilizations.Romans:
                themeToPlay = romeTheme;
                break;
        }

        PlayTheme(themeToPlay);
    }


    public void ToggleMusic()
    {
        musicOn = !musicOn;

        if (musicOn)
        {
            masterMixer.SetFloat("MusicVolume", 0f);
        }
        else
        {
            masterMixer.SetFloat("MusicVolume", -80f);
        }
      
    }

    public void ToggleAudio()
    {
        audioOn = !audioOn;

        if (audioOn)
        {
            masterMixer.SetFloat("SfxVolume", 0f);
        }
        else
        {
            masterMixer.SetFloat("SfxVolume", -80f);
        }
    }
    /// <summary>
    /// <paramref name="soundName"/> is case sensitive!
    /// </summary>
    /// <param name="soundName"></param>
    public void Play(string soundName)
    {
        HB_Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.Log($"The sound called ''{soundName}'' is not found and cannot be played."); return;
        }

        if (s.clipVariants.Length > 1)
        {
            s.clip = s.clipVariants[UnityEngine.Random.Range(0, s.clipVariants.Length)];
            s.source.clip = s.clip;
        }

       
        s.source.Play();
    }

    /// <summary>
    /// <paramref name="soundName"/> is case sensitive!
    /// </summary>
    /// <param name="soundName"></param>
    public void Stop(string soundName)
    {
        HB_Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.Log($"The sound called ''{soundName}'' is not found and cannot be stopped."); return;
        }
        s.source.Stop();
    }

    public void PlayTheme(string newTheme)
    {
        StopTheme();
        themeName = newTheme;
        Play(newTheme);
    }

    public void StopTheme()
    {
        if (themeName != null || themeName != "")
        {
            Stop(themeName);
        }
    }

    public void PauseTheme()
    {
        if (themeName != null || themeName != "")
        {
            Pause(themeName);
        }
    }

    /// <summary>
    /// <paramref name="soundName"/> is case sensitive!
    /// </summary>
    /// <param name="soundName"></param>
    public void Pause(string soundName)
    {
        HB_Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.Log($"The sound called ''{soundName}'' is not found and cannot be paused."); return;
        }
        s.source.Pause();
    }

    /// <summary>
    /// <paramref name="soundName"/> is case sensitive!
    /// </summary>
    /// <param name="soundName"></param>
    public void Unpause(string soundName)
    {
        HB_Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.Log($"The sound called ''{soundName}'' is not found and cannot be unpaused."); return;
        }
        s.source.UnPause();
    }
}

[System.Serializable]
public class HB_Sound
{
    public string name;
    public AudioClip clip;
    public AudioClip[] clipVariants;
    public AudioMixerGroup mixerGroup;
    [Range(0, 1)] public float volume = 0.5f;
    [Range(0, 3)] public float pitch = 1f;
    public bool loop = false;
    public bool mute = false;
    public bool playOnAwake = false;
    [HideInInspector] public AudioSource source;
}