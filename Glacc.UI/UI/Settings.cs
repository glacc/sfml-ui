// SPDX-License-Identifier: LGPL-3.0-only

using SFML.Graphics;
using System.Reflection;

namespace Glacc.UI
{
    public static class Settings
    {
        public static Color bgColor = new Color(0xDF, 0xDF, 0xDF);

        public static Font? font = null;
        public static string defaultFontFileName = "HarmonyOS_Sans_SC_Regular.ttf";
        public static int defaultFontSize = 16;
        public static Color txtColor = Color.White;
        public static Color txtColorLight = new Color(0xFF, 0xFF, 0xFF, 127);
        public static Color txtColorInversed = Color.Black;

        public static Color elemBgColor = new Color(0x00, 0x00, 0x00, 63);
        public static Color elemBgColorLight = new Color(0x00, 0x00, 0x00, 31);
        public static Color elemBgColorDark = new Color(0x00, 0x00, 0x00, 127);

        public static char inputBoxCursor = '<';
        public static int inputBoxBlinkTime = 15;

        public static RenderStates renderStates = RenderStates.Default;

        static bool inited = false;

        public static void LoadFont(string? fontFileName = null)
        {
            if (fontFileName == null)
                fontFileName = defaultFontFileName;

            string assemblyPath = string.Join('\\', Assembly.GetExecutingAssembly().Location.Split("\\").SkipLast(1));
            string fontFilePath = Path.Combine(assemblyPath, fontFileName);

            font?.Dispose();
            font = new Font(fontFilePath);
        }

        public static void InitSettings()
        {
            if (inited)
                return;

            if (font == null)
                LoadFont();

            inited = true;
        }
    }
}
