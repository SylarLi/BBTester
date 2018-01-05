using System;
using System.IO;
using System.Linq;

/// <summary>
/// 判断成功之后再执行future
/// 如果times>1，再次执行前亦会进行判断
/// 执行future的时间独立计算，不会计入interval
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

    /// <summary>
    /// else: 是否阻塞
    /// </summary>
    public bool blockWhenTestFailed = true;

    public TestCompileResult[] testCompileResults = new TestCompileResult[0];

    public BBMacro[] futures = new BBMacro[0];

    public BBIfMacro()
    {

    }

    public BBIfMacro(string[] statements, BBMacro[] futures)
    {
        this.statements = statements;
        this.futures = futures;
        if (futures.Any(future => future == null))
        {
            throw new InvalidDataException();
        }
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(statements.Length);
        for (int i = 0; i < statements.Length; i++)
        {
            writer.Write(statements[i]);
        }
        base.Serialize(writer);
    }

    public override void Deserialize(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        statements = new string[length];
        for (int i = 0; i < length; i++)
        {
            statements[i] = reader.ReadString();
        }
        base.Deserialize(reader);
    }

    public bool Test(out BBMacro future)
    {
        for (int i = 0; i < testCompileResults.Length; i++)
        {
            if (testCompileResults[i] != null && 
                testCompileResults[i]())
            {
                future = futures[i];
                return true;
            }
        }
        future = null;
        return false;
    }

    public override BBMacro Clone(bool deepClone)
    {
        BBIfMacro macro = base.Clone(deepClone) as BBIfMacro;
        macro.statements = new string[statements.Length];
        Array.Copy(statements, macro.statements, statements.Length);
        if (deepClone)
        {
            macro.futures = new BBMacro[futures.Length];
            for (int i = 0; i < futures.Length; i++)
            {
                macro.futures[i] = futures[i].Clone(deepClone);
            }
        }
        return macro;
    }

    public override BBMacroType macroType
    {
        get
        {
            return BBMacroType.If;
        }
    }
}