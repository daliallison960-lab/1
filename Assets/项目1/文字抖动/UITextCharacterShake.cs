using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unity UI Text（含 Naninovel RevealableUIText）版逐字抖动。
/// 继承 BaseMeshEffect，通过修改顶点流实现每个字符各自偏移。
/// 由 TextShake 命令动态挂载，不要手动挂到 Inspector。
/// </summary>
[RequireComponent(typeof(Text))]
public class UITextCharacterShake : BaseMeshEffect
{
    public float strength = 2f;
    public float speed = 35f;

    // 每字符相位步长：越大，相邻字差异越明显；越小越像整体抖
    private const float CharacterSeedStep = 0.23f;

    void Update()
    {
        // 每帧标记顶点 dirty，触发 ModifyMesh 重新计算
        if (graphic != null)
            graphic.SetVerticesDirty();
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!enabled || !isActiveAndEnabled) return;

        var verts = new List<UIVertex>();
        vh.GetUIVertexStream(verts);

        // UI Text 每个字符对应 6 个顶点（2 个三角形）
        int charCount = verts.Count / 6;

        for (int i = 0; i < charCount; i++)
        {
            float seed = i * CharacterSeedStep;
            float ox = Mathf.Sin(Time.time * speed + seed) * strength;
            float oy = Mathf.Cos(Time.time * speed + seed * 1.7f) * strength;
            var offset = new Vector3(ox, oy, 0f);

            int baseIdx = i * 6;
            for (int v = 0; v < 6; v++)
            {
                UIVertex vert = verts[baseIdx + v];
                vert.position += offset;
                verts[baseIdx + v] = vert;
            }
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);
    }
}
