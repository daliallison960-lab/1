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

    [Header("溶解效果")]
    [Tooltip("将 Assets/shader/DissolveUI.shader 对应的 Material 拖入，或留空自动创建")]
    public Material dissolveMaterial;

    [Tooltip("溶解边缘发光颜色（Alpha 控制强度，0 = 无边缘）")]
    public Color edgeColor = new Color(1f, 1f, 1f, 0.6f);

    [Tooltip("溶解边缘宽度")]
    [Range(0f, 0.15f)]
    public float edgeWidth = 0.04f;

    [Tooltip("噪声频率，越高颗粒越细")]
    [Range(1f, 16f)]
    public float noiseScale = 5f;

    [Header("时长")]
    public float fadeDuration = 0.5f;

    // 启动时创建一次，之后复用
    private Image fadeImage;
    private bool isSwitching;

    // 静态保存索引：返回主菜单时不重置
    private static int savedIndex = 0;
    private int currentIndex;

    void Start()
    {
        currentIndex = savedIndex;

        EnsureDissolveMaterial();
        CreateFadeImageOnce();

        if (options != null && options.Length > 0)
            ApplyCoverInstant(options[currentIndex]);
    }

    // ── 材质 ──────────────────────────────────────────────────────────────

    void EnsureDissolveMaterial()
    {
        if (dissolveMaterial == null)
        {
            var shader = Shader.Find("UI/DissolveUI");
            if (shader == null)
            {
                Debug.LogError("[TitleCoverSwitcher] 找不到 UI/DissolveUI shader，请确认 Assets/shader/DissolveUI.shader 已导入。");
                return;
            }
            dissolveMaterial = new Material(shader);
        }

        dissolveMaterial.SetTexture("_NoiseTex", BuildNoiseTexture(256, noiseScale));
        dissolveMaterial.SetFloat("_EdgeWidth", edgeWidth);
        dissolveMaterial.SetColor("_EdgeColor", edgeColor);
        dissolveMaterial.SetFloat("_Threshold", 0f);
    }

    // 程序化生成 Perlin 噪声贴图，无需外部资源
    static Texture2D BuildNoiseTexture(int size, float scale)
    {
        var tex = new Texture2D(size, size, TextureFormat.R8, false)
        {
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear
        };

        // 随机偏移让每次游戏的溶解图案不同
        float ox = Random.Range(0f, 99f);
        float oy = Random.Range(0f, 99f);
        var pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float v = Mathf.PerlinNoise(x / (float)size * scale + ox,
                                        y / (float)size * scale + oy);
            pixels[y * size + x] = new Color(v, v, v, 1f);
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    // ── fadeImage ─────────────────────────────────────────────────────────

    void CreateFadeImageOnce()
    {
        var go = new GameObject("_FadeImage", typeof(Image));
        go.transform.SetParent(backgroundImage.transform.parent, false);

        var rt   = go.GetComponent<RectTransform>();
        var bgRt = backgroundImage.GetComponent<RectTransform>();
        rt.anchorMin      = bgRt.anchorMin;
        rt.anchorMax      = bgRt.anchorMax;
        rt.offsetMin      = bgRt.offsetMin;
        rt.offsetMax      = bgRt.offsetMax;

        go.transform.SetSiblingIndex(backgroundImage.transform.GetSiblingIndex() + 1);

        fadeImage                = go.GetComponent<Image>();
        fadeImage.raycastTarget  = false;
        fadeImage.material       = dissolveMaterial;

        // 初始完全隐藏（Threshold = 0 时 shader 丢弃全部像素）
        SetThreshold(0f);
    }

    // ── 切换逻辑 ──────────────────────────────────────────────────────────

    // 绑定到 Change 按钮 OnClick
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

        // 把新封面贴到 fadeImage，重置 Threshold 为 0（全隐）
        fadeImage.sprite = target.coverSprite;
        SetThreshold(0f);

        // 重新生成噪声，每次溶解图案不一样
        if (dissolveMaterial != null)
            dissolveMaterial.SetTexture("_NoiseTex", BuildNoiseTexture(256, noiseScale));

        // 用 DOTween 把 Threshold 从 0 → 1，触发溶解动画
        float threshold = 0f;
        DOTween.To(
            () => threshold,
            v  => { threshold = v; SetThreshold(v); },
            1f,
            fadeDuration
        )
        .SetEase(Ease.InOutCubic)
        .OnComplete(() =>
        {
            // 溶解完成：新封面移入底层，fadeImage 重置
            backgroundImage.sprite = target.coverSprite;
            SetThreshold(0f);

            currentIndex = index;
            savedIndex   = index;
            isSwitching  = false;

            if (target.bgmClip != null)
                GlobalBGMPlayer.Instance?.PlayMenuBgm(target.bgmClip, 0.3f);
        });
    }

    void SetThreshold(float t)
    {
        if (dissolveMaterial != null)
            dissolveMaterial.SetFloat("_Threshold", t);
    }

    void ApplyCoverInstant(CoverOption option)
    {
        backgroundImage.sprite = option.coverSprite;
        if (option.bgmClip != null)
            GlobalBGMPlayer.Instance?.PlayMenuBgm(option.bgmClip, 0f);
    }

    void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
