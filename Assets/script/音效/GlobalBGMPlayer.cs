using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class GlobalBGMPlayer : MonoBehaviour
{
    public static GlobalBGMPlayer Instance { get; private set; }

    private AudioSource audioSource;

    // 记住最后一首菜单 BGM，返回主菜单时调用 ResumeMenuBgm() 恢复
    private AudioClip lastMenuBgm;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
    }

    // 主菜单切封面时调用
    public void PlayMenuBgm(AudioClip clip, float fadeTime = 0.3f)
    {
        if (clip == null) return;
        lastMenuBgm = clip;
        PlayClip(clip, fadeTime);
    }

    // 从其它界面返回主菜单时调用，继续上次的菜单音乐
    public void ResumeMenuBgm(float fadeTime = 0.3f)
    {
        if (lastMenuBgm != null)
            PlayClip(lastMenuBgm, fadeTime);
    }

    // 通用播放，带淡入淡出
    public void PlayClip(AudioClip clip, float fadeTime = 0.3f)
    {
        if (clip == null) return;
        if (audioSource.clip == clip && audioSource.isPlaying) return;

        DOTween.Kill(audioSource);

        if (fadeTime <= 0f)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.volume = 1f;
            audioSource.Play();
        }
        else
        {
            float half = fadeTime * 0.5f;
            audioSource.DOFade(0f, half).OnComplete(() =>
            {
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.loop = true;
                audioSource.volume = 0f;
                audioSource.Play();
                audioSource.DOFade(1f, half);
            });
        }
    }

    public void StopBgm(float fadeTime = 0.3f)
    {
        DOTween.Kill(audioSource);
        if (fadeTime <= 0f)
        {
            audioSource.Stop();
        }
        else
        {
            audioSource.DOFade(0f, fadeTime).OnComplete(() => audioSource.Stop());
        }
    }
}
