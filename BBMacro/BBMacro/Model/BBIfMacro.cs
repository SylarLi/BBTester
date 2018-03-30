using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

/// <summary>
/// 判断成功之后再执行对应序号的item
/// 如果times > 1，再次执行前亦会进行判断
/// 执行item的时间独立计算，不会计入interval
/// 此宏本身不会被执行
/// </summary>
public class BBIfMacro: BBMacro
{
    public delegate bool TestCompileResult();

    /// <summary>
    /// 条件判断语句
    /// 例如:
    /// var a = GameObject.Find("a");
    /// return a != null;
    /// </summary>
    public string[] statements = new string[0];

    public TestCompileResult[] testCompileResults = new TestCompileResult[0];

    public BBMacro[] items = new BBMacro[0];

    /// <summary>
    /// else: 所有条件判断均失败之后的处理
    /// - Continue 继续执行下一次
    /// - Block 阻塞执行直到某个条件满足为止
    /// - Break 跳出循环
    /// </summary>
    public BBLoopAction action = BBLoopAction.Block;

    public BBIfMacro()
    {

    }

    public BBIfMacro(string[] statements, BBMacro[] items)
    {
        this.statements = statements;
        this.items = items;
        if (items.Any(item => item == null))
        {
            throw new InvalidDataException();
        }
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write((byte)action);
        writer.Write(statements.Length);
        for (int i = 0; i < statements.Length; i++)
        {
            writer.Write(statements[i]);
        }
        base.Serialize(writer);
    }

    public override void Deserialize(BinaryReader reader)
    {
        action = (BBLoopAction)reader.ReadByte();
        int length = reader.ReadInt32();
        statements = new string[length];
        for (int i = 0; i < length; i++)
        {
            statements[i] = reader.ReadString();
        }
        base.Deserialize(reader);
    }

    public bool Test(out BBMacro item)
    {
        for (int i = 0; i < testCompileResults.Length; i++)
        {
            if (testCompileResults[i] != null && 
                testCompileResults[i]())
            {
                item = items[i];
                return true;
            }
        }
        item = null;
        return false;
    }

    public override BBMacro Clone(bool deepClone)
    {
        BBIfMacro macro = base.Clone(deepClone) as BBIfMacro;
        macro.action = action;
        macro.statements = new string[statements.Length];
        Array.Copy(statements, macro.statements, statements.Length);
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
        builder.AppendLine(string.Format("BBIfMacro {0} = new BBIfMacro();", macroName));
        string[] fieldNames = new string[] { "statements", "times", "duration", "delay" };
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
            return BBMacroType.If;
        }
    }
}