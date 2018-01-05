using System;

/// <summary>
/// 线性宏
/// items同时执行
/// 执行时间独立计算，不会计入interval
/// 此宏本身不会被执行
/// </summary>
public class BBSeriesMacro : BBMacro
{
    public BBMacro[] items = new BBMacro[0];

    public BBSeriesMacro()
    {

    }

    public BBSeriesMacro(BBMacro[] macros)
    {
        items = new BBMacro[macros.Length];
        Array.Copy(macros, items, items.Length);
    }

    public override BBMacro Clone(bool deepClone)
    {
        BBSeriesMacro macro = base.Clone(deepClone) as BBSeriesMacro;
        if (deepClone)
        {
            macro.items = new BBMacro[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                macro.items[i] = items[i].Clone(deepClone);
            }
        }
        return macro;
    }

    public override BBMacroType macroType
    {
        get
        {
            return BBMacroType.Series;
        }
    }
}