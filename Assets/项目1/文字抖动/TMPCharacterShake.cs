using UnityEngine;
using TMPro;

/// <summary>
/// TextMeshProUGUI 版逐字抖动。
/// 由 TextShake 命令动态挂载，不要手动挂到 Inspector。
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPCharacterShake : MonoBehaviour
{
    public float strength = 2f;
    public float speed = 35f;

    // 每个字符用不同相位，产生各自独立的抖动感
    private const float CharacterSeedStep = 0.23f;

    private TextMeshProUGUI tmp;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (tmp == null) return;

        tmp.ForceMeshUpdate();
        var textInfo = tmp.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIdx = charInfo.materialReferenceIndex;
            int vertIdx = charInfo.vertexIndex;
            var verts = textInfo.meshInfo[matIdx].vertices;

            float seed = i * CharacterSeedStep;
            float ox = Mathf.Sin(Time.time * speed + seed) * strength;
            float oy = Mathf.Cos(Time.time * speed + seed * 1.7f) * strength;
            var offset = new Vector3(ox, oy, 0f);

            // 每个字符 4 个顶点
            for (int v = 0; v < 4; v++)
                verts[vertIdx + v] += offset;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var mesh = textInfo.meshInfo[i].mesh;
            mesh.vertices = textInfo.meshInfo[i].vertices;
            tmp.UpdateGeometry(mesh, i);
        }
    }
}
