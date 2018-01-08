using System;
using System.IO;

/// <summary>
/// 全局设置
/// 注1：屏幕坐标原点在左下角，GameView窗口坐标在左上角
/// </summary>
[Serializable]
public class BBConfig
{
    /// <summary>
    /// 宏坐标类型
    /// </summary>
    public enum AxisType
    {
        /// <summary>
        /// 绝对坐标
        /// </summary>
        Absolute = 0,

        /// <summary>
        /// 相对坐标(归一化)
        /// </summary>
        Relative = 1,

        /// <summary>
        /// 裁剪到固定比例再取归一化坐标
        /// </summary>
        ClipRelative = 2,
    }

    public AxisType axisType = AxisType.ClipRelative;

    /// <summary>
    /// 裁剪宽高比，axisType == AxisType.Clip时有效
    /// </summary>
    public float clipRatio = 16 / 9f;

    /// <summary>
    /// 屏幕宽高
    /// </summary>
    public int screenWidth = 1;

    public int screenHeight = 1;

    /// <summary>
    /// 监听的鼠标按钮事件
    /// </summary>
    public BBInputButton monitoringButtons = BBInputButton.MouseLeft | BBInputButton.MouseMiddle | BBInputButton.MouseRight;

    /// <summary>
    /// 是否监听键盘事件
    /// </summary>
    public bool monitoringKeyboard = true;

    /// <summary>
    /// 消息坐标到标准化坐标
    /// </summary>
    /// <param name="local"></param>
    /// <returns></returns>
    public float[] Screen2Axis(float[] local)
    {
        var ret = new float[local.Length];
        Array.Copy(local, ret, local.Length);
        switch (axisType)
        {
            case AxisType.Relative:
                {
                    ret[0] = ret[0] / screenWidth;
                    ret[1] = ret[1] / screenHeight;
                    break;
                }
            case AxisType.ClipRelative:
                {
                    var ratio = (float)screenWidth / screenHeight;
                    float offsetX = 0, offsetY = 0;
                    if (ratio > clipRatio)
                    {
                        offsetX = (ratio - clipRatio) * 0.5f * screenHeight;
                    }
                    else if (ratio < clipRatio)
                    {
                        offsetY = (1 / ratio - 1 / clipRatio) * 0.5f * screenWidth;
                    }
                    ret[0] = (ret[0] - offsetX) / (screenWidth - 2 * offsetX);
                    ret[1] = (ret[1] - offsetY) / (screenHeight - 2 * offsetY);
                    break;
                }
        }
        return ret;
    }

    /// <summary>
    /// 标准化坐标到消息坐标
    /// </summary>
    /// <param name="axis"></param>
    /// <returns></returns>
    public float[] Axis2Screen(float[] axis)
    {
        var ret = new float[axis.Length];
        Array.Copy(axis, ret, axis.Length);
        switch (axisType)
        {
            case AxisType.Relative:
                {
                    ret[0] = ret[0] * screenWidth;
                    ret[1] = ret[1] * screenHeight;
                    break;
                }
            case AxisType.ClipRelative:
                {
                    var ratio = (float)screenWidth / screenHeight;
                    float offsetX = 0, offsetY = 0;
                    if (ratio > clipRatio)
                    {
                        offsetX = (ratio - clipRatio) * 0.5f * screenHeight;
                    }
                    else if (ratio < clipRatio)
                    {
                        offsetY = (1 / ratio - 1 / clipRatio) * 0.5f * screenWidth;
                    }
                    ret[0] = ret[0] * (screenWidth - 2 * offsetX) + offsetX;
                    ret[1] = ret[1] * (screenHeight - 2 * offsetY) + offsetY;
                    break;
                }
        }
        return ret;
    }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write((byte)axisType);
        writer.Write(clipRatio);
        writer.Write(screenWidth);
        writer.Write(screenHeight);
        writer.Write((int)monitoringButtons);
        writer.Write(monitoringKeyboard);
    }

    public void Deserialize(BinaryReader reader)
    {
        axisType = (AxisType)reader.ReadByte();
        clipRatio = reader.ReadSingle();
        screenWidth = reader.ReadInt32();
        screenHeight = reader.ReadInt32();
        monitoringButtons = (BBInputButton)reader.ReadInt32();
        monitoringKeyboard = reader.ReadBoolean();
    }
}
