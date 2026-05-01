using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class CoverOption
{
    public Sprite coverSprite;
    public AudioClip bgmClip;
}

public class TitleCoverSwitcher : MonoBehaviour
{
    [Header("封面配置")]
    public CoverOption[] options;

    [Header("UI 引用")]
    public Image backgroundImage;

    [Header("溶解时长（秒）")]
    public float fadeDuration = 0.5f;

    // 叠在 backgroundImage 正上方，显示即将切入的封面
    private Image fadeImage;
    private bool isSwitching;

    // 静态保存：返回主菜单时不重置
    private static int savedIndex = 0;
    private int currentIndex;

    void Start()
    {
        currentIndex = savedIndex;
        CreateFadeImageOnce();

        if (options != null && options.Length > 0)
            ApplyCoverInstant(options[currentIndex]);
    }

    void CreateFadeImageOnce()
    {
        var go = new GameObject("_FadeImage", typeof(Image));
        go.transform.SetParent(backgroundImage.transform.parent, false);

        // 完全对齐 backgroundImage
        var rt   = go.GetComponent<RectTransform>();
        var bgRt = backgroundImage.GetComponent<RectTransform>();
        rt.anchorMin = bgRt.anchorMin;
        rt.anchorMax = bgRt.anchorMax;
        rt.offsetMin = bgRt.offsetMin;
        rt.offsetMax = bgRt.offsetMax;

        // 保证 fadeImage 在 backgroundImage 上层
        go.transform.SetSiblingIndex(backgroundImage.transform.GetSiblingIndex() + 1);

        fadeImage               = go.GetComponent<Image>();
        fadeImage.raycastTarget = false;
        SetAlpha(fadeImage, 0f);
    }

    // 绑定 Change 按钮 OnClick
    public void SwitchNext()
    {
        if (isSwitching || options == null || options.Length == 0) return;
        SwitchTo((currentIndex + 1) % options.Length);
    }

    void SwitchTo(int index)
    {
        if (isSwitching) return;

        var target = options[index];
        isSwitching = true;

        // 新封面放在上层，初始透明
        fadeImage.sprite = target.coverSprite;
        SetAlpha(fadeImage, 0f);
        SetAlpha(backgroundImage, 1f);

        // 旧图淡出 + 新图淡入，同时进行 —— 就是 Ren'Py dissolve
        var seq = DOTween.Sequence();
        seq.Join(backgroundImage.DOFade(0f, fadeDuration));
        seq.Join(fadeImage.DOFade(1f, fadeDuration));
        seq.SetEase(Ease.InOutSine);
        seq.OnComplete(() =>
        {
            // 动画结束：把新封面交还给底层，重置上层
            backgroundImage.sprite = target.coverSprite;
            SetAlpha(backgroundImage, 1f);
            SetAlpha(fadeImage, 0f);

            currentIndex = index;
            savedIndex   = index;
            isSwitching  = false;

            if (target.bgmClip != null)
                GlobalBGMPlayer.Instance?.PlayMenuBgm(target.bgmClip, 0.3f);
        });
    }

    void ApplyCoverInstant(CoverOption option)
    {
        backgroundImage.sprite = option.coverSprite;
        SetAlpha(backgroundImage, 1f);
        if (option.bgmClip != null)
            GlobalBGMPlayer.Instance?.PlayMenuBgm(option.bgmClip, 0f);
    }

    static void SetAlpha(Image img, float a)
    {
        var c = img.color;
        c.a = a;
        img.color = c;
    }

    void OnDestroy()
    {
        DOTween.Kill(backgroundImage);
        if (fadeImage != null) DOTween.Kill(fadeImage);
    }
}
