using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Naninovel;
using Naninovel.Commands;
using Cysharp.Threading.Tasks;

/// <summary>
/// Naninovel 命令：逐字抖动文本组件
///
/// 用法：
///   @textShake on  target:DialogueText strength:2 speed:35
///   @textShake off target:DialogueText
///
/// 自动识别目标是 TextMeshProUGUI 还是 Unity UI Text（包括 RevealableUIText）。
/// </summary>
[CommandAlias("textShake")]
public class TextShake : Command
{
    /// <summary>on / off</summary>
    [ParameterAlias(NamelessParameterAlias)]
    public StringParameter Mode;

    /// <summary>场景中 GameObject 的名称</summary>
    [ParameterAlias("target")]
    public StringParameter Target;

    /// <summary>抖动强度（像素）</summary>
    [ParameterAlias("strength")]
    public DecimalParameter Strength;

    /// <summary>抖动速度</summary>
    [ParameterAlias("speed")]
    public DecimalParameter Speed;

    public override UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        string mode = Assigned(Mode) ? Mode.Value : "on";
        string targetName = Assigned(Target) ? Target.Value : "DialogueText";

        var obj = GameObject.Find(targetName);
        if (obj == null)
        {
            Debug.LogWarning($"[TextShake] 找不到 GameObject: '{targetName}'");
            return UniTask.CompletedTask;
        }

        if (mode == "on")
        {
            EnableShake(obj);
        }
        else
        {
            DisableShake(obj);
        }

        return UniTask.CompletedTask;
    }

    void EnableShake(GameObject obj)
    {
        float str = Assigned(Strength) ? Strength.Value : 2f;
        float spd = Assigned(Speed) ? Speed.Value : 35f;

        // 优先 TMP
        var tmp = obj.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            var shaker = obj.GetComponent<TMPCharacterShake>();
            if (shaker == null)
                shaker = obj.AddComponent<TMPCharacterShake>();
            shaker.strength = str;
            shaker.speed = spd;
            shaker.enabled = true;
            return;
        }

        // 回退到 UI Text / RevealableUIText
        var text = obj.GetComponent<Text>();
        if (text != null)
        {
            var shaker = obj.GetComponent<UITextCharacterShake>();
            if (shaker == null)
                shaker = obj.AddComponent<UITextCharacterShake>();
            shaker.strength = str;
            shaker.speed = spd;
            shaker.enabled = true;
            return;
        }

        Debug.LogWarning($"[TextShake] '{obj.name}' 上没有 TextMeshProUGUI 或 Text 组件");
    }

    void DisableShake(GameObject obj)
    {
        var tmpShaker = obj.GetComponent<TMPCharacterShake>();
        if (tmpShaker != null) tmpShaker.enabled = false;

        var uiShaker = obj.GetComponent<UITextCharacterShake>();
        if (uiShaker != null) uiShaker.enabled = false;
    }
}
