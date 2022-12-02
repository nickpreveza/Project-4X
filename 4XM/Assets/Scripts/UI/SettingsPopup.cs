using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : UIPopup
{
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider audioSlider;
    [SerializeField] Slider musicSlider;

    public override void Setup()
    {
        if (SI_AudioManager.Instance != null)
        {
            float masterVolume;
            SI_AudioManager.Instance.masterMixer.GetFloat("masterVolume", out masterVolume);
            masterSlider.value = masterVolume;

            float audioVolume;
            SI_AudioManager.Instance.masterMixer.GetFloat("audioVolume", out audioVolume);
            audioSlider.value = audioVolume;

            float musicVolume;
            SI_AudioManager.Instance.masterMixer.GetFloat("musicVolume", out musicVolume);
            musicSlider.value = musicVolume;
        }

    }

    public void SetMasterVolume(float volume)
    {
        SI_AudioManager.Instance.masterMixer.SetFloat("masterVolume", volume);
    }

    public void SetAudiovolume(float volume)
    {
        SI_AudioManager.Instance.masterMixer.SetFloat("audioVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        SI_AudioManager.Instance.masterMixer.SetFloat("musicVolume", volume);
    }

}
