using System;
using System.Text;
using System.Text.RegularExpressions;

public sealed class BBUtil
{
    public static bool IsExtendedKey(BBVirtualKey key)
    {
        return key == BBVirtualKey.VK_MENU ||
            key == BBVirtualKey.VK_LMENU ||
            key == BBVirtualKey.VK_RMENU ||
            key == BBVirtualKey.VK_CONTROL ||
            key == BBVirtualKey.VK_RCONTROL ||
            key == BBVirtualKey.VK_INSERT ||
            key == BBVirtualKey.VK_DELETE ||
            key == BBVirtualKey.VK_HOME ||
            key == BBVirtualKey.VK_END ||
            key == BBVirtualKey.VK_PRIOR ||
            key == BBVirtualKey.VK_NEXT ||
            key == BBVirtualKey.VK_RIGHT ||
            key == BBVirtualKey.VK_UP ||
            key == BBVirtualKey.VK_LEFT ||
            key == BBVirtualKey.VK_DOWN ||
            key == BBVirtualKey.VK_NUMLOCK ||
            key == BBVirtualKey.VK_CANCEL ||
            key == BBVirtualKey.VK_SNAPSHOT ||
            key == BBVirtualKey.VK_DIVIDE;
    }

    public static bool IsStrokeInputType(BBInputType type)
    {
        return type == BBInputType.Press ||
            type == BBInputType.Move ||
            type == BBInputType.Release ||
            type == BBInputType.Wheel;
    }

    public static bool IsKeyboardInputType(BBInputType type)
    {
        return type == BBInputType.KeyDown ||
            type == BBInputType.KeyUp;
    }

    public static BBMacroOpCode InputType2OpCode(BBInputType type)
    {
        BBMacroOpCode code = BBMacroOpCode.None;
        switch (type)
        {
            case BBInputType.Press:
                code = BBMacroOpCode.Press;
                break;
            case BBInputType.Move:
                code = BBMacroOpCode.Move;
                break;
            case BBInputType.Release:
                code = BBMacroOpCode.Release;
                break;
            case BBInputType.Wheel:
                code = BBMacroOpCode.Wheel;
                break;
            case BBInputType.KeyDown:
                code = BBMacroOpCode.KeyDown;
                break;
            case BBInputType.KeyUp:
                code = BBMacroOpCode.KeyUp;
                break;
        }
        return code;
    }

    public static bool IsStrokeOpCode(BBMacroOpCode code)
    {
        return code == BBMacroOpCode.Press ||
            code == BBMacroOpCode.Release ||
            code == BBMacroOpCode.Move ||
            code == BBMacroOpCode.Click ||
            code == BBMacroOpCode.Drag ||
            code == BBMacroOpCode.Wheel;
    }

    public static bool IsKeyboardOpCode(BBMacroOpCode code)
    {
        return code == BBMacroOpCode.KeyDown ||
            code == BBMacroOpCode.KeyUp ||
            code == BBMacroOpCode.KeyPress;
    }

    public static void ConcatMemberString(StringBuilder builder, Type memberType, object memberValue)
    {
        if (memberType.Equals(typeof(bool)))
        {
            builder.Append((bool)memberValue ? "true" : "false");
        }
        else if (memberType.IsPrimitive)
        {
            builder.Append(memberValue.ToString());
            if (memberType.Equals(typeof(float)))
            {
                builder.Append("f");
            }
            else if (memberType.Equals(typeof(double)))
            {
                builder.Append("d");
            }
        }
        else if (memberType.Equals(typeof(string)))
        {
            builder.Append("\"");
            string value = memberValue.ToString();
            value = value.Replace("\"", "\\\"");
            value = Regex.Replace(value, @"\r*\n", "\\n");
            builder.Append(value);
            builder.Append("\"");
        }
        else if (memberType.IsArray)
        {
            Type elType = memberType.GetElementType();
            builder.Append("new ");
            builder.Append(elType.FullName.Replace("+", "."));
            builder.Append("[] { ");
            Array elArray = memberValue as Array;
            for (int i = 0; i < elArray.Length; i++)
            {
                ConcatMemberString(builder, elType, elArray.GetValue(i));
                builder.Append(", ");
            }
            builder.Append("}");
        }
        else if (memberType.IsEnum)
        {
            string enumName = Enum.GetName(memberType, memberValue);
            if (string.IsNullOrEmpty(enumName))
            {
                int enumValue = (int)memberValue;
                var values = Enum.GetValues(memberType);
                var index = 0;
                foreach (var value in values)
                {
                    if (((int)value & enumValue) > 0)
                    {
                        index += 1;
                    }
                }
                foreach (var value in values)
                {
                    if (((int)value & enumValue) > 0)
                    {
                        index -= 1;
                        builder.Append(memberType.FullName.Replace("+", "."));
                        builder.Append(".");
                        builder.Append(Enum.GetName(memberType, value));
                        if (index > 0)
                        {
                            builder.Append(" | ");
                        }
                    }
                }
            }
            else
            {
                builder.Append(memberType.FullName.Replace("+", "."));
                builder.Append(".");
                builder.Append(enumName);
            }
        }
        else
        {
            throw new NotSupportedException(memberType.Name);
        }
    }
}