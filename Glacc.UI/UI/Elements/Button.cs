// SPDX-License-Identifier: LGPL-3.0-only

using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacc.UI.Elements
{
    public class Button : Element
    {
        public string text;

        public int px;
        public int py;
        public int width;
        public int height;

        public Font? font;
        public int fontSize = Settings.defaultFontSize;

        public bool enabled = true;

        bool m_isHolding = false;
        public bool isHolding
        {
            get { return m_isHolding; }
        }
        bool m_mouseHover = false;
        public bool mouseHover
        {
            get { return m_mouseHover; }
        }

        RectangleShape bgRect;
        Text txt;

        public TextAlign textAlign = TextAlign.Center;

        Drawable?[] drawables;

        public EventHandler<EventArgs>? onClick = null;

        public override void Update()
        {
            if (enabled)
                Utils.CheckMouseHover(px, py, width, height, out m_mouseHover, this);
            else
            {
                m_isHolding = false;
                m_mouseHover = false;
            }

            if (m_mouseHover && Event.mousePress)
                m_isHolding = true;

            if (m_mouseHover && m_isHolding && Event.mouseRelease && !Event.mouseDragRelease)
                onClick?.Invoke(this, EventArgs.Empty);

            if (!Event.mouseHold)
                m_isHolding = false;
        }

        public override Drawable?[] Draw()
        {
            bgRect.Position = new SFML.System.Vector2f(px, py);
            bgRect.Size = new SFML.System.Vector2f(width, height);
            bgRect.FillColor = (m_mouseHover || m_isHolding) ? Settings.elemBgColorDark : (enabled ? Settings.elemBgColor : Settings.elemBgColorLight);
            drawables[0] = bgRect;

            if (font != null)
            {
                txt.Font = font;
                txt.DisplayedString = text;
                txt.CharacterSize = (uint)fontSize;

                float origy = txt.GetLocalBounds().Top + (txt.GetLocalBounds().Height / 2f);
                float origx = 0f;
                int txtPy = py + (height / 2);
                int txtPx = 0;
                switch (textAlign)
                {
                    case TextAlign.TopLeft:
                    case TextAlign.Left:
                    case TextAlign.BottomLeft:
                        origx = txt.GetLocalBounds().Left;
                        txtPx = px + (fontSize / 2);
                        break;
                    case TextAlign.Top:
                    case TextAlign.Center:
                    case TextAlign.Bottom:
                        origx = txt.GetLocalBounds().Left + (txt.GetLocalBounds().Width / 2f);
                        txtPx = px + (width / 2);
                        break;
                    case TextAlign.TopRight:
                    case TextAlign.Right:
                    case TextAlign.BottomRight:
                        origx = txt.GetLocalBounds().Left + txt.GetLocalBounds().Width;
                        txtPx = px + width - (fontSize / 2);
                        break;
                }

                txt.Origin = new SFML.System.Vector2f((int)origx, (int)origy);
                txt.Position = new SFML.System.Vector2f(txtPx, txtPy);
                txt.FillColor = enabled ? Settings.txtColor : Settings.txtColorLight;
            }

            return drawables;
        }

        public Button(string text, int px, int py, int width, int height)
        {
            this.text = text;
            this.px = px;
            this.py = py;
            this.width = width;
            this.height = height;

            bgRect = new RectangleShape();
            txt = new Text();

            drawables = [bgRect, txt];

            if (Settings.font != null)
                font = Settings.font;
        }

        public Button(string text, int width, int height) : this(text, 0, 0, width, height) { }
    }
}
