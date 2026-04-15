using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Utils
{

    public static class DialogueUtils
    {
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

                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                return sprite;
            }
            Debug.LogError("图片不存在: " + fullPath);
            return null;
        }

        public static void SendMessage(string text, Color color, string imagePath = "Image/avatar.png")
        {
            Sprite avatar = LoadSpriteFromImage(imagePath);
            if (avatar == null) return;
            SefiraConversationController.Instance.UpdateConversation(avatar, color, text);
        }

        public static void SendMessage(string text)
        {
            SendMessage(text, new Color(204f / 255f, 0f, 0f, 1f)); // 与源码血蛾相同的红色
        }
    }
}
