using System.Collections.Generic;

public static class LogUtils
{
    // 定义颜色枚举
    public enum ColorType
    {
        Default,
        Notice,
        R,
        W,
        B,
        P  
    }

    // 枚举 → Hex 映射字典
    private static readonly Dictionary<ColorType, string> ColorHexMap = new Dictionary<ColorType, string>
    {
        { ColorType.Default,"000000" },
        { ColorType.Notice, "4B8A18" },
        { ColorType.R,      "D92B3B" },
        { ColorType.W,      "F2F0D0" },
        { ColorType.B,      "A057A0" },
        { ColorType.P,      "4ECDC4" }
    };

    public static string GetColorHex(ColorType color)
    {
        if (ColorHexMap.TryGetValue(color, out string hex))
            return hex;
        return ColorHexMap[ColorType.Default];
    }

    public static string Colorize(ColorType color, string content)
    {
        return string.Format("<color=#{0}>{1}</color>", GetColorHex(color), content);
    }

    public static void SendLog(string content)
    {
        Notice.instance.Send("AddSystemLog", new object[] { content });
    }
}