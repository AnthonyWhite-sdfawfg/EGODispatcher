using System.Collections.Generic;

namespace Utils {

    /// <summary>
    /// 系统日志发送工具：把文本按指定颜色写入游戏内的 systemLog。
    /// <para>颜色以 Unity 富文本标签实现，因此可安全用于日志正文。</para>
    /// </summary>
    public static class LogSendings
    {
        /// <summary>
        /// 日志配色方案。
        /// <para><see cref="R"/>、<see cref="W"/>、<see cref="B"/>、<see cref="P"/> 对应游戏的四种伤害类型，
        /// 与 <c>CreatureTools.WorkType</c> 中的工作配色保持一致。</para>
        /// </summary>
        public enum ColorType
        {
            /// <summary>默认色（黑）</summary>
            Default,
            /// <summary>提示色（绿），用于设备状态一类的通知</summary>
            Notice,
            /// <summary>红伤害色</summary>
            R,
            /// <summary>白伤害色</summary>
            W,
            /// <summary>紫伤害色</summary>
            B,
            /// <summary>青伤害色</summary>
            P
        }

        // 枚举 → Hex 映射字典
        /// <summary>配色枚举到十六进制色号的映射表</summary>
        private static readonly Dictionary<ColorType, string> ColorHexMap = new Dictionary<ColorType, string>
    {
        { ColorType.Default,"000000" },
        { ColorType.Notice, "4B8A18" },
        { ColorType.R,      "D92B3B" },
        { ColorType.W,      "F2F0D0" },
        { ColorType.B,      "A057A0" },
        { ColorType.P,      "4ECDC4" }
    };

        /// <summary>
        /// 取配色方案对应的十六进制色号。
        /// </summary>
        /// <param name="color">配色方案</param>
        /// <returns>六位十六进制色号；未登记的颜色回退为默认色</returns>
        public static string GetColorHex(ColorType color)
        {
            if (ColorHexMap.TryGetValue(color, out string hex))
                return hex;
            return ColorHexMap[ColorType.Default];
        }

        /// <summary>
        /// 为文本套上指定颜色的富文本标签。
        /// </summary>
        /// <param name="color">配色方案</param>
        /// <param name="content">待着色的文本</param>
        /// <returns>带 &lt;color&gt; 标签的文本</returns>
        public static string Colorize(ColorType color, string content)
        {
            return string.Format("<color=#{0}>{1}</color>", GetColorHex(color), content);
        }

        /// <summary>
        /// 向游戏的系统日志发送一条文本（不额外着色）。
        /// </summary>
        /// <param name="content">日志正文</param>
        public static void SendLog(string content)
        {
            Notice.instance.Send("AddSystemLog", new object[] { content });
        }
    }

}

