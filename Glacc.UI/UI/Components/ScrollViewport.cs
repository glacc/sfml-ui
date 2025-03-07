// SPDX-License-Identifier: LGPL-3.0-only

using Glacc.UI.Components;
using SFML.Graphics;

namespace Glacc.UI.Elements
{
    public class ScrollViewport : Viewport
    {
        int visableWidth;
        int visableHeight;

        // idk whether this is necessary but i want to keep it before i figured out better solution
        int refSizeViewport = 256;
        int refSizeContent = 512;
        //float refSpeed = 0.01f;
        float refScrollBarSizeFac = 0.3f;

        public int scrollSpeedPixels = 6;
        public float mouseWheelMultiplier = 20f;

        int m_scrollBarWidth = 16;
        public int scrollBarWidth
        {
            get => m_scrollBarWidth;
            set
            {
                if (m_scrollBarWidth != value)
                {
                    m_scrollBarWidth = value;
                }
            }
        }

        public float scrollX
        {
            get => scrollHorz.scrollPercent;
            set => scrollHorz.scrollPercent = value;
        }
        public float scrollY
        {
            get => scrollVert.scrollPercent;
            set => scrollVert.scrollPercent = value;
        }
        public bool scrollable = true;

        Viewport? m_viewport;
        public Viewport? viewport
        {
            get => m_viewport;
            set
            {
                viewportVisableArea.elements.Remove(m_viewport);

                m_viewport = value;

                UpdateVisableArea();

                viewportVisableArea.elements.Add(m_viewport);
            }
        }
        Viewport viewportVisableArea;

        ScrollBar scrollHorz;
        ScrollBar scrollVert;

        void CalcScrollBarPara(ref ScrollBar scrollBar, int lenViewport, int lenContent)
        {
            float refFac = ((float)lenContent / lenViewport) / ((float)refSizeContent / refSizeViewport);

            // float speed = refSpeed / refFac;
            float scrollerSizeFac = MathF.Max(0.1f, MathF.Min(refScrollBarSizeFac / refFac, 0.6f));

            // scrollBar.scrollSpeedPercent = 0.01f * ((float)lenViewport / lenContent);
            scrollBar.scrollerSizeScale = scrollerSizeFac;

            // scrollBar.scrollSpeedPercent = scrollSpeedPixels / (float)(lenContent - lenViewport);
            scrollBar.scrollSpeedPercent = scrollSpeedPixels / (float)(lenContent - lenViewport);
        }

        public void UpdateVisableArea()
        {
            if (m_viewport == null)
            {
                visableWidth = width;
                visableHeight = height;

                return;
            }

            if (m_viewport.width > width)
            {
                visableHeight = height - scrollBarWidth;
                scrollHorz.visable = true;
            }
            else
            {
                visableHeight = height;
                scrollHorz.visable = false;
            }

            if (m_viewport.height > height)
            {
                visableWidth = width - scrollBarWidth;
                scrollVert.visable = true;
            }
            else
            {
                visableWidth = width;
                scrollVert.visable = false;
            }

            scrollHorz.length = visableWidth;
            scrollVert.length = visableHeight;

            viewportVisableArea.width = visableWidth;
            viewportVisableArea.height = visableHeight;

            CalcScrollBarPara(ref scrollHorz, width, m_viewport.width);
            CalcScrollBarPara(ref scrollVert, height, m_viewport.height);

            viewportVisableArea.UpdateSize();
        }

        protected override void UpdateSizeCustom(bool diff)
        {
            if (diff)
            {
                scrollHorz.py = height - scrollBarWidth;
                scrollVert.px = width - scrollBarWidth;

                UpdateVisableArea();
            }
        }

        protected override void UpdateCustom()
        {
            if (m_viewport != null)
            {
                if (mouseHover)
                {
                    if (scrollable)
                    {
                        if (Event.mouseScroll)
                        {
                            if (scrollVert.visable)
                                scrollVert.ScrollPercent(-scrollVert.scrollSpeedPercent * Event.mouseScrollVelVert * mouseWheelMultiplier);

                            if (scrollHorz.visable)
                                scrollHorz.ScrollPercent(-scrollHorz.scrollSpeedPercent * Event.mouseScrollVelHorz * mouseWheelMultiplier);
                        }
                    }
                }

                int visableAreaOffsetX = (int)((width - m_viewport.width) * scrollHorz.scrollPercent);
                int visableAreaOffsetY = (int)((height - m_viewport.height) * scrollVert.scrollPercent);

                m_viewport.px = visableAreaOffsetX;
                m_viewport.py = visableAreaOffsetY;
            }
        }

        public ScrollViewport(Viewport? viewport, int px, int py, int width, int height) : base(px, py, width, height)
        {
            visableWidth = width - scrollBarWidth;
            visableHeight = height - scrollBarWidth;

            viewportVisableArea = new Viewport(0, 0, visableWidth, visableHeight);
            viewportVisableArea.bgColor = Color.Transparent;

            scrollHorz = new ScrollBar(0, height - scrollBarWidth, scrollBarWidth, width, ScrollBarDirection.Horizontal);
            scrollVert = new ScrollBar(width - scrollBarWidth, 0, scrollBarWidth, height, ScrollBarDirection.Vertical);

            this.viewport = viewport;

            UpdateVisableArea();

            elements.Add(viewportVisableArea);
            elements.Add(scrollHorz);
            elements.Add(scrollVert);
        }

        public ScrollViewport(Viewport? viewport, int width, int height) : this(viewport, 0, 0, width, height) { }
    }
}
