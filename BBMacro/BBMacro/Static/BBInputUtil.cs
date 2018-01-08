public sealed class BBInputUtil
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

    public static bool IsMouseInputType(BBInputType type)
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

    public static bool IsMouseOpCode(BBMacroOpCode code)
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
}