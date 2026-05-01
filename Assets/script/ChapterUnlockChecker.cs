using System.Collections;
using UnityEngine;
using Naninovel;

[System.Serializable]
public class ChapterEntry
{
    [Tooltip("Naninovel 全局变量名，例如 G_ch2_unlocked，值为字符串 \"true\" 时视为已解锁")]
    public string variableName;
    public GameObject lockedNode;
    public GameObject unlockedNode;
}

public class ChapterUnlockChecker : MonoBehaviour
{
    public ChapterEntry[] chapters;

    IEnumerator Start()
    {
        // 必须等 Naninovel 引擎初始化完成才能读变量
        while (!Engine.Initialized)
            yield return null;

        var varManager = Engine.GetService<ICustomVariableManager>();

        foreach (var entry in chapters)
        {
            if (string.IsNullOrEmpty(entry.variableName)) continue;

            string val = varManager.GetVariableValue(entry.variableName);
            bool unlocked = val == "true";

            if (entry.lockedNode != null)
                entry.lockedNode.SetActive(!unlocked);
            if (entry.unlockedNode != null)
                entry.unlockedNode.SetActive(unlocked);
        }
    }
}
