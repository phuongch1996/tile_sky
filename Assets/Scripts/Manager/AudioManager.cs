using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BGMData
{
    public AudioManager.BGM id;
    public AudioClip audioClip;
    public float volume;
}
[Serializable]
public class SFXData
{
    public AudioManager.SFX id;
    public AudioClip audioClip;
    public float volume;
}
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private void Awake()
    {
        if (instance != null && instance.gameObject != gameObject)
            Destroy(gameObject);
        else
            instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public bool isInit;
    private void Start()
    {
        GameConfig.OnSoundChange += OnSoundChange;
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;

        sfxSources = new();
        foreach (SFXData data in sfxList)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = data.audioClip;
            //source.volume = data.volume;
            source.loop = false;
            sfxSources.Add(source);
        }
        GameConfig.GetSoundSettings();
        OnSoundChange();
        isInit = true;
    }
    void OnSoundChange()
    {
        float bgmvolume = 1;
        if (bgmSource.clip != null)
            bgmvolume = bgmList.Find(x => x.audioClip == bgmSource.clip).volume;
        //bgmSource.volume = (float)Config.bgm * bgmvolume;
        bgmSource.volume = (GameConfig.bgmToggle ? 0.8f : 0) * bgmvolume;
        foreach (var sfx in sfxSources)
            //sfx.volume = (float)Config.sfx * sfxList.Find(x => x.audioClip == sfx.clip).volume;
            sfx.volume = (GameConfig.sfxToggle ? 0.8f : 0) * sfxList.Find(x => x.audioClip == sfx.clip).volume;
        //bgmSource.volume = (float)Config.bgm;
    }
    public enum BGM
    {
        NULL,
        HOME_01 = 1,
        GAME_01 = 11,
        GAME_02
    }
    public enum SFX
    {
        NULL, TAP, WIN, LOSE, REWARD, BREAK, BUTTON, V_GREAT, V_GOOD, V_EXCELLENT, V_AWESOME, V_AMAZING, V_PERFECT,
    }
    public BGM currentBGM;
    public List<BGMData> bgmList;
    public List<SFXData> sfxList;
    private AudioSource bgmSource;
    private List<AudioSource> sfxSources;
    public bool? isHomeBGM;
    public void PlayBGMHome()
    {
        if (isHomeBGM == true)
            return;
        BGM id = BGM.HOME_01;
        int index = bgmList.FindIndex(x => x.id == id);
        Debug.Log("bgmindex " + index);
        if (index >= 0)
        {
            currentBGM = bgmList[index].id;
            bgmSource.clip = bgmList[index].audioClip;
            bgmSource.volume = bgmList[index].volume * (GameConfig.bgmToggle ? 0.8f : 0);
            isHomeBGM = true;
            bgmSource.Play();
        }
    }
    public void PlayBGMGame()
    {
        if (isHomeBGM == false)
            return;
        BGM id = GameConfig.GetRandom(BGM.GAME_01, BGM.GAME_02);
        int index = bgmList.FindIndex(x => x.id == id);
        Debug.Log("bgmindex " + index);
        if (index >= 0)
        {
            currentBGM = bgmList[index].id;
            bgmSource.clip = bgmList[index].audioClip;
            bgmSource.volume = bgmList[index].volume * (GameConfig.bgmToggle ? 0.8f : 0);
            isHomeBGM = false;
            bgmSource.Play();
        }
    }
    public void PlaySFXOneShot(SFX id)
    {
        if (id == SFX.NULL)
            return;
        int index = sfxList.FindIndex(x => x.id == id);
        if (index >= 0)
        {
            if (id == SFX.WIN || id == SFX.LOSE || id == SFX.REWARD)
                StartCoroutine(BGMFade(sfxSources[index]));
            else
                sfxSources[index].PlayOneShot(sfxSources[index].clip);
        }
    }
    IEnumerator BGMFade(AudioSource source)
    {
        bgmSource.DOFade((GameConfig.bgmToggle ? 0.8f : 0) * bgmList.Find(x => x.audioClip == bgmSource.clip).volume / 2, 0.1f);
        source.PlayOneShot(source.clip);
        yield return new WaitForSeconds(source.clip.length - 0.5f);
        bgmSource.DOFade((GameConfig.bgmToggle ? 0.8f : 0) * bgmList.Find(x => x.audioClip == bgmSource.clip).volume, 0.1f);
    }
    public void PlaySFXLoop(SFX id)
    {
        int index = sfxList.FindIndex(x => x.id == id);
        if (index >= 0)
        {
            sfxSources[index].loop = true;
            sfxSources[index].Play();
        }
    }
    public void StopSFXLoop(SFX id)
    {
        int index = sfxList.FindIndex(x => x.id == id);
        if (index >= 0)
            sfxSources[index].Stop();
    }
}
