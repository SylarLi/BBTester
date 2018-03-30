using System;
using System.Text;
using System.Reflection;

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

    public override void ToCString(StringBuilder builder, string macroName)
    {
        builder.AppendLine(string.Format("BBSeriesMacro {0} = new BBSeriesMacro();", macroName));
        string[] fieldNames = new string[] { "times", "duration", "delay" };
        foreach (string fieldName in fieldNames)
        {
            FieldInfo fieldInfo = GetType().GetField(fieldName, BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Public);
            builder.Append(string.Format("{0}.{1} = ", macroName, fieldName));
            BBUtil.ConcatMemberString(builder, fieldInfo.FieldType, fieldInfo.GetValue(this));
            builder.AppendLine(";");
        }
        builder.AppendLine(string.Format("{0}.items = new BBMacro[{1}];", macroName, items.Length));
        for (int i = 0; i < items.Length; i++)
        {
            string itemMacroName = macroName + "_" + i;
            items[i].ToCString(builder, itemMacroName);
            builder.AppendLine(string.Format("{0}.items[{1}] = {2};", macroName, i, itemMacroName));
        }
        if (next != null)
        {
            string nextMacroName = macroName + "_n";
            next.ToCString(builder, nextMacroName);
            builder.AppendLine(string.Format("{0}.next = {1};", macroName, nextMacroName));
        }
    }

    public override BBMacroType macroType
    {
        get
        {
            return BBMacroType.Series;
        }
    }
}