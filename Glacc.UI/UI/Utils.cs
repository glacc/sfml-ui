// SPDX-License-Identifier: LGPL-3.0-only

using Glacc.UI.Elements;
using SFML.Graphics;

namespace Glacc.UI
{
    public static class Utils
    {
        public static void UpdateTextOrigins(Text text, TextAlign align = TextAlign.TopLeft)
        {
            float origy = text.GetLocalBounds().Top + (text.GetLocalBounds().Height / 2f);
            float origx = 0f;
            switch (align)
            {
                case TextAlign.TopLeft:
                case TextAlign.Left:
                case TextAlign.BottomLeft:
                    origx = text.GetLocalBounds().Left;
                    break;
                case TextAlign.Top:
                case TextAlign.Center:
                case TextAlign.Bottom:
                    origx = text.GetLocalBounds().Left + (text.GetLocalBounds().Width / 2f);
                    break;
                case TextAlign.TopRight:
                case TextAlign.Right:
                case TextAlign.BottomRight:
                    origx = text.GetLocalBounds().Left + text.GetLocalBounds().Width;
                    break;
            }
            switch (align)
            {
                case TextAlign.TopLeft:
                case TextAlign.Top:
                case TextAlign.TopRight:
                    origy = text.GetLocalBounds().Top;
                    break;
                case TextAlign.Left:
                case TextAlign.Center:
                case TextAlign.Right:
                    origy = text.GetLocalBounds().Top + (text.GetLocalBounds().Height / 2f);
                    break;
                case TextAlign.BottomLeft:
                case TextAlign.Bottom:
                case TextAlign.BottomRight:
                    origy = text.GetLocalBounds().Top + text.GetLocalBounds().Height;
                    break;
            }
            text.Origin = new SFML.System.Vector2f((int)origx, (int)origy);
        }

        public static void CheckMouseHover(int px, int py, int width, int height, out bool mouseHover, Element? element = null)
        {
            bool visable = true;
            if (element != null)
                visable = element.visable;

            mouseHover =
            (
                visable &&
                Event.currentViewport == Event.focusingViewport &&
                Event.mouseAvailable &&
                Event.mouseX > px && Event.mouseX < px + width &&
                Event.mouseY > py && Event.mouseY < py + height
            );
        }

        public static void UpdateElements(List<Element?> elements)
        {
           for (int n = elements.Count - 1; n >= 0; n--)
                elements[n]?.Update();
        }

        public static void DrawArray(Drawable?[] drawables, RenderTarget? target)
        {
            if (target == null)
                return;

            foreach (Drawable? drawable in drawables)
            {
                if (drawable != null)
                    target.Draw(drawable, Settings.renderStates);
            }
        }

        public static void DrawElements(List<Element?> elements, RenderTarget? target)
        {
            if (target == null)
                return;

            foreach (Element? elem in elements)
            {
                if (elem == null)
                    continue;

                if (elem.visable)
                    DrawArray(elem.Draw(), target);
            }
        }
    }
}
