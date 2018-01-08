using System.IO;

public sealed class BBPersistent
{
    public static void Save(BBConfig config, BBMacro macro, string path)
    {
        if (macro == null)
        {
            throw new InvalidDataException();
        }
        using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
        {
            stream.SetLength(0);
            BinaryWriter writer = new BinaryWriter(stream);
            config.Serialize(writer);
            Save(macro, writer);
            stream.Flush();
        }
    }

    private static void Save(BBMacro macro, BinaryWriter writer)
    {
        BBMacro.SerializedMacro(macro, writer);
        switch (macro.macroType)
        {
            case BBMacroType.If:
                {
                    BBIfMacro ifmacro = macro as BBIfMacro;
                    writer.Write(ifmacro.items.Length);
                    for (int i = 0; i < ifmacro.items.Length; i++)
                    {
                        Save(ifmacro.items[i], writer);
                    }
                    break;
                }
            case BBMacroType.Series:
                {
                    BBSeriesMacro srmacro = macro as BBSeriesMacro;
                    writer.Write(srmacro.items.Length);
                    for (int i = 0; i < srmacro.items.Length; i++)
                    {
                        Save(srmacro.items[i], writer);
                    }
                    break;
                }
            case BBMacroType.Parallel:
                {
                    BBParallelMacro prmacro = macro as BBParallelMacro;
                    writer.Write(prmacro.items.Length);
                    for (int i = 0; i < prmacro.items.Length; i++)
                    {
                        Save(prmacro.items[i], writer);
                    }
                    break;
                }
        }
        if (macro.next != null)
        {
            writer.Write(true);
            Save(macro.next, writer);
        }
        else
        {
            writer.Write(false);
        }
    }

    public static void Load(string path, out BBConfig config, out BBMacro macro)
    {
        if (!File.Exists(path))
        {
            throw new InvalidDataException();
        }
        using (FileStream stream = new FileStream(path, FileMode.Open))
        {
            stream.Position = 0;
            BinaryReader reader = new BinaryReader(stream);
            config = new BBConfig();
            config.Deserialize(reader);
            macro = Load(reader);
        }
    }

    private static BBMacro Load(BinaryReader reader)
    {
        BBMacro macro = BBMacro.DeserializeMacro(reader);
        switch (macro.macroType)
        {
            case BBMacroType.If:
                {
                    BBIfMacro ifmacro = macro as BBIfMacro;
                    int itemLen = reader.ReadInt32();
                    ifmacro.items = new BBMacro[itemLen];
                    for (int i = 0; i < itemLen; i++)
                    {
                        ifmacro.items[i] = Load(reader);
                    }
                    break;
                }
            case BBMacroType.Series:
                {
                    BBSeriesMacro srmacro = macro as BBSeriesMacro;
                    int itemLen = reader.ReadInt32();
                    srmacro.items = new BBMacro[itemLen];
                    for (int i = 0; i < itemLen; i++)
                    {
                        srmacro.items[i] = Load(reader);
                    }
                    break;
                }
            case BBMacroType.Parallel:
                {
                    BBParallelMacro prmacro = macro as BBParallelMacro;
                    int itemLen = reader.ReadInt32();
                    prmacro.items = new BBMacro[itemLen];
                    for (int i = 0; i < itemLen; i++)
                    {
                        prmacro.items[i] = Load(reader);
                    }
                    break;
                }
        }
        if (reader.ReadBoolean())
        {
            macro.next = Load(reader);
        }
        return macro;
    }
}