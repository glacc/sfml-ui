// SPDX-License-Identifier: LGPL-3.0-only
// Copyright (C) 2025 Glacc

using SFML.Graphics;

namespace Glacc.UI.Elements
{
    public class Label : Element
    {
        public string text;

        public int px;
        public int py;

        public Font? font;
        public Color color = Settings.txtColorInversed;
        public int fontSize = Settings.defaultFontSize;

        public TextAlign textAlign = TextAlign.TopLeft;

        Drawable?[] drawables;

        Text txt;

        public override Drawable?[] Draw()
        {
            if (font != null)
            {
                txt.Font = font;
                txt.DisplayedString = text;
                txt.CharacterSize = (uint)fontSize;
                Utils.UpdateTextOrigins(txt, textAlign);
                txt.Position = new SFML.System.Vector2f(px, py);
                txt.FillColor = color;
            }

            return drawables;
        }

        public Label(string text, int px, int py, int fontSize, Font? font = null)
        {
            this.text = text;
            this.px = px;
            this.py = py;
            this.fontSize = fontSize;

            txt = new Text();

            drawables = new Drawable[1] { txt };

            if (font == null)
            {
                if (Settings.font != null)
                    this.font = Settings.font;
            }
            else
                this.font = font;
        }

        public Label(string text, int fontSize, Font? font = null) : this(text, 0, 0, fontSize, font) { }
    }
}
