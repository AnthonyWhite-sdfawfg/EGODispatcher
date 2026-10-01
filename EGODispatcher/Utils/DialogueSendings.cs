using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Utils
{

    /// <summary>
    /// 对话发送工具：把文本与头像送进游戏的 Sefira 对话控件，并负责从模组目录加载头像图片。
    /// <para>图片路径相对于<b>程序集所在目录</b>解析，因此默认头像需与 DLL 一同部署。</para>
    /// </summary>
    public static class DialogueSendings
    {
        /// <summary>
        /// 模组根目录（程序集所在目录）。
        /// <para>通过反射取得自身程序集路径再取目录，因此与部署位置无关。</para>
        /// </summary>
        private static string ModRootPath
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                Uri uri = new Uri(codeBase);
                string path = uri.LocalPath;
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// 从模组目录加载图片并转换为 <see cref="Sprite"/>。
        /// </summary>
        /// <param name="relativePath">相对于模组根目录的图片路径</param>
        /// <returns>加载成功返回 Sprite；文件不存在或解码失败时返回 null</returns>
        public static Sprite LoadSpriteFromImage(string relativePath)
        {
            string fullPath = Path.Combine(ModRootPath, relativePath);
            if (File.Exists(fullPath))
            {
                byte[] bytes = File.ReadAllBytes(fullPath);
                Texture2D tex = new Texture2D(2, 2);
                if (!ImageConversion.LoadImage(tex, bytes))
                {
                    return null;
                }

                // 以图片中心为轴心、每单位 100 像素创建 Sprite
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                return sprite;
            }
            Debug.LogError("图片不存在: " + fullPath);
            return null;
        }

        /// <summary>
        /// 发送一条带指定文本色与头像的对话。
        /// <para>头像加载失败时直接放弃发送，不产生空头像的对话。</para>
        /// </summary>
        /// <param name="text">对话正文</param>
        /// <param name="color">文本颜色</param>
        /// <param name="imagePath">头像图片路径，相对于模组根目录</param>
        public static void SendMessage(string text, Color color, string imagePath = "Image/avatar.png")
        {
            Sprite avatar = LoadSpriteFromImage(imagePath);
            if (avatar == null) return;
            SefiraConversationController.Instance.UpdateConversation(avatar, color, text);
        }

        /// <summary>
        /// 以默认的深红色与默认头像发送对话。
        /// </summary>
        /// <param name="text">对话正文</param>
        public static void SendMessage(string text)
        {
            // 默认色为 #CC0000（EGODispatcher 的代表色）
            SendMessage(text, new Color(204f / 255f, 0f, 0f, 1f));
        }
    }
}
