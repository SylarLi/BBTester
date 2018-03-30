using System;
using System.IO;
using System.Text;
using System.Reflection;

/// <summary>
/// 默认宏，按序列执行
/// </summary>
public class BBMacro
{
    /// <summary>
    /// 宏类型
    /// </summary>
    public BBMacroOpCode code = BBMacroOpCode.None;

    /// <summary>
    /// 宏按钮
    /// </summary>
    public BBInputButton button = BBInputButton.None;

    /// <summary>
    /// 宏按键
    /// </summary>
    public BBVirtualKey key = BBVirtualKey.VK_NONE;

    /// <summary>
    /// 宏数据
    /// </summary>
    public float[] data = new float[0];

    /// <summary>
    /// 宏执行次数
    /// </summary>
    public int times = 1;

    /// <summary>
    /// 宏每次执行延迟
    /// </summary>
    public float delay = 0;

    /// <summary>
    /// 宏每次执行花费时间
    /// </summary>
    public float duration = 0;

    /// <summary>
    /// 宏执行前运行的脚本
    /// 注：脚本对宏的修改是永久性的
    /// </summary>
    public string script = "";
    public delegate void ScriptCompileResult(BBMacro macro);
    public ScriptCompileResult scriptCompileResult;

    /// <summary>
    /// 脚本自定义数据
    /// </summary>
    public string scriptData = "";

    /// <summary>
    /// 序列宏衔接的下一个
    /// </summary>
    public BBMacro next;

    public BBMacro()
    {

    }

    public void RunScript()
    {
        if (scriptCompileResult != null)
        {
            scriptCompileResult(this);
        }
    }

    public virtual void Serialize(BinaryWriter writer)
    {
        writer.Write((byte)code);
        writer.Write((byte)button);
        writer.Write((ushort)key);
        writer.Write(data.Length);
        for (int i = 0; i < data.Length; i++)
        {
            writer.Write(data[i]);
        }
        writer.Write(times);
        writer.Write(duration);
        writer.Write(delay);
        writer.Write(script);
        writer.Write(scriptData);
    }

    public virtual void Deserialize(BinaryReader reader)
    {
        code = (BBMacroOpCode)reader.ReadByte();
        button = (BBInputButton)reader.ReadByte();
        key = (BBVirtualKey)reader.ReadUInt16();
        int length = reader.ReadInt32();
        data = new float[length];
        for (int i = 0; i < length; i++)
        {
            data[i] = reader.ReadSingle();
        }
        times = reader.ReadInt32();
        duration = reader.ReadSingle();
        delay = reader.ReadSingle();
        script = reader.ReadString();
        scriptData = reader.ReadString();
    }

    public virtual BBMacro Clone(bool deepClone)
    {
        BBMacro macro = null;
        switch (macroType)
        {
            case BBMacroType.If:
                macro = new BBIfMacro();
                break;
            case BBMacroType.Series:
                macro = new BBSeriesMacro();
                break;
            case BBMacroType.Parallel:
                macro = new BBParallelMacro();
                break;
            case BBMacroType.Normal:
                macro = new BBMacro();
                break;
        }
        macro.code = code;
        macro.button = button;
        macro.key = key;
        macro.data = new float[data.Length];
        Array.Copy(data, macro.data, data.Length);
        macro.times = times;
        macro.duration = duration;
        macro.delay = delay;
        macro.script = script;
        macro.scriptData = scriptData;
        if (deepClone)
        {
            if (next != null)
            {
                macro.next = next.Clone(deepClone);
            }
        }
        return macro;
    }

    public virtual void ToCString(StringBuilder builder, string macroName)
    {
        builder.AppendLine(string.Format("BBMacro {0} = new BBMacro();", macroName));
        string[] fieldNames = new string[] { "code", "button", "key", "data", "times", "duration", "delay", "script", "scriptData" };
        foreach (string fieldName in fieldNames)
        {
            FieldInfo fieldInfo = GetType().GetField(fieldName, BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Public);
            builder.Append(string.Format("{0}.{1} = ", macroName, fieldName));
            BBUtil.ConcatMemberString(builder, fieldInfo.FieldType, fieldInfo.GetValue(this));
            builder.AppendLine(";");
        }
        if (next != null)
        {
            string nextMacroName = macroName + "_n";
            next.ToCString(builder, nextMacroName);
            builder.AppendLine(string.Format("{0}.next = {1};", macroName, nextMacroName));
        }
    }

    public virtual BBMacroType macroType
    {
        get
        {
            return BBMacroType.Normal;
        }
    }

    public static void SerializedMacro(BBMacro macro, BinaryWriter writer)
    {
        writer.Write((byte)macro.macroType);
        macro.Serialize(writer);
    }

    public static byte[] SerializedMacro(BBMacro macro)
    {
        using (var stream = new MemoryStream())
        {
            BinaryWriter writer = new BinaryWriter(stream);
            SerializedMacro(macro, writer);
            return stream.ToArray();
        }
    }

    public static BBMacro DeserializeMacro(BinaryReader reader)
    {
        BBMacro ret = null;
        BBMacroType type = (BBMacroType)reader.ReadByte();
        switch (type)
        {
            case BBMacroType.If:
                ret = new BBIfMacro();
                break;
            case BBMacroType.Series:
                ret = new BBSeriesMacro();
                break;
            case BBMacroType.Parallel:
                ret = new BBParallelMacro();
                break;
            case BBMacroType.Normal:
                ret = new BBMacro();
                break;
        }
        ret.Deserialize(reader);
        return ret;
    }

    public static BBMacro DeserializeMacro(byte[] bytes)
    {
        using (var stream = new MemoryStream(bytes))
        {
            stream.Position = 0;
            BinaryReader reader = new BinaryReader(stream);
            return DeserializeMacro(reader);
        }
    }
}