public enum BBMacroOpCode
{
    None = 0,

    /// <summary>
    /// 鼠标消息
    /// </summary>
    Press = 1,
    Release = 2,
    Move = 3,
    Wheel = 4,
    Click = 5,
    Drag = 6,
   
    /// <summary>
    /// 键盘消息
    /// </summary>
    KeyDown = 101,
    KeyUp = 102,
    KeyPress = 103,
}