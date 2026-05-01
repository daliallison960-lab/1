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

    [Header("淡变时长")]
    public float fadeDuration = 0.3f;

    // 启动时创建一次，之后一直复用，避免每次切换都 new/Destroy
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

        // 与 backgroundImage 完全重叠
        var rt = go.GetComponent<RectTransform>();
        var bgRt = backgroundImage.GetComponent<RectTransform>();
        rt.anchorMin = bgRt.anchorMin;
        rt.anchorMax = bgRt.anchorMax;
        rt.offsetMin = bgRt.offsetMin;
        rt.offsetMax = bgRt.offsetMax;

        // 叠在 backgroundImage 正上方
        go.transform.SetSiblingIndex(backgroundImage.transform.GetSiblingIndex() + 1);

        fadeImage = go.GetComponent<Image>();
        fadeImage.raycastTarget = false;

        var c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
    }

    // 绑定到 Change 按钮的 OnClick
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

        // 把新封面放到 fadeImage，从全透明淡入
        fadeImage.sprite = target.coverSprite;
        var col = fadeImage.color;
        col.a = 0f;
        fadeImage.color = col;

        fadeImage.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            // 淡入完成：将新封面赋给底层 backgroundImage，fadeImage 重置透明
            backgroundImage.sprite = target.coverSprite;
            col = fadeImage.color;
            col.a = 0f;
            fadeImage.color = col;

            currentIndex = index;
            savedIndex = index;
            isSwitching = false;

            // 切换对应 BGM
            if (target.bgmClip != null)
                GlobalBGMPlayer.Instance?.PlayMenuBgm(target.bgmClip, fadeDuration);
        });
    }

    void ApplyCoverInstant(CoverOption option)
    {
        backgroundImage.sprite = option.coverSprite;
        if (option.bgmClip != null)
            GlobalBGMPlayer.Instance?.PlayMenuBgm(option.bgmClip, 0f);
    }

    void OnDestroy()
    {
        if (fadeImage != null)
            DOTween.Kill(fadeImage);
    }
}
