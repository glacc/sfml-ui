// SPDX-License-Identifier: LGPL-3.0-only

using SFML.Graphics;
using SFML.System;
using System.Numerics;

namespace Glacc.UI.Components
{
    public class Graph<T1, T2> : Element, IDisposable
        where T1 : INumber<T1>
        where T2 : INumber<T2>
    {
        public int px;
        public int py;

        public int width;
        public int height;

        public int safeZonePixels = 16;

        public float left = -10f;
        public float right = 10f;
        public float top = 10f;
        public float bottom = -10f;

        public float sclx = 1f;
        public float scly = 1f;
        public float sclLen = 8f;

        public float stemPtRadius = 2f;

        public bool drawAxis = true;
        public bool isStem = false;

        float yOfXAxis;
        float xOfYAxis;

        public Color bgColor = Settings.bgColor;
        public Color axisColor = new Color(0x666666FF);
        public Color lineColor = Color.Black;

        public T1[] horz = Array.Empty<T1>();
        public T2[] vert = Array.Empty<T2>();

        RenderTexture? renderTexture;
        Sprite? sprite;

        Drawable?[] drawables = Array.Empty<Drawable?>();

        bool CheckDimensions()
            => !(horz.Length == 0 || vert.Length == 0 || horz.Length != vert.Length);

        public void DrawGraphLine()
        {
            if (renderTexture == null)
                return;

            if (!CheckDimensions())
                return;

            VertexArray vertices = new VertexArray(PrimitiveType.LineStrip, (uint)horz.Length);

            int startx = safeZonePixels;
            int endx = width - safeZonePixels;
            int starty = safeZonePixels;
            int endy = height - safeZonePixels;

            float leftToRight = right - left;
            float topToBottom = bottom - top;

            for (int i = 0; i < horz.Length; i++)
            {
                float currx = float.CreateSaturating(horz[i]);
                float curry = float.CreateSaturating(vert[i]);

                float percentx = (currx - left) / leftToRight;
                float percenty = (curry - top) / topToBottom;

                float pointx = startx + ((endx - startx) * percentx);
                float pointy = starty + ((endy - starty) * percenty);

                Vertex vertex = new Vertex(new Vector2f(pointx, pointy), lineColor);

                vertices[(uint)i] = vertex;
            }

            renderTexture.Draw(vertices);
        }

        public void DrawStemLines()
        {
            if (renderTexture == null)
                return;

            if (!CheckDimensions())
                return;

            VertexArray vertices = new VertexArray(PrimitiveType.Lines, (uint)horz.Length * 2);

            int startx = safeZonePixels;
            int endx = width - safeZonePixels;
            int starty = safeZonePixels;
            int endy = height - safeZonePixels;

            float leftToRight = right - left;
            float topToBottom = bottom - top;

            for (int i = 0; i < horz.Length ; i++)
            {
                float currx = float.CreateSaturating(horz[i]);
                float curry = float.CreateSaturating(vert[i]);

                float percentx = (currx - left) / leftToRight;
                float percenty = (curry - top) / topToBottom;

                float pointx = startx + ((endx - startx) * percentx);
                float pointy = starty + ((endy - starty) * percenty);

                vertices[(uint)i * 2]     = new Vertex(new Vector2f(pointx, pointy), lineColor);
                vertices[((uint)i * 2) + 1] = new Vertex(new Vector2f(pointx, yOfXAxis), lineColor);

                CircleShape stemPoint = new CircleShape(stemPtRadius);
                stemPoint.Position = new Vector2f((int)(pointx - stemPtRadius), (int)(pointy - stemPtRadius));
                stemPoint.FillColor = lineColor;
                renderTexture.Draw(stemPoint);
            }

            renderTexture.Draw(vertices);
        }

        void DrawAxis()
        {
            if (renderTexture == null)
                return;

            float axisXPercent = (0 - left) / (right - left);
            float axisYPercent = (0 - top) / (bottom - top);
            if (axisXPercent < 0 || axisXPercent > 1 || axisYPercent < 0 || axisYPercent > 1)
                return;

            List<Vertex> vertexList = new List<Vertex>();

            int startx = safeZonePixels;
            int endx = width - safeZonePixels;
            int starty = safeZonePixels;
            int endy = height - safeZonePixels;

            // Axis
            yOfXAxis = starty + ((endy - starty) * axisYPercent);
            vertexList.Add(new Vertex(new Vector2f(0, yOfXAxis), axisColor));
            vertexList.Add(new Vertex(new Vector2f(width, yOfXAxis), axisColor));

            xOfYAxis = startx + ((endx - startx) * axisXPercent);
            vertexList.Add(new Vertex(new Vector2f(xOfYAxis, 0), axisColor));
            vertexList.Add(new Vertex(new Vector2f(xOfYAxis, height), axisColor));

            // Scale
            if (yOfXAxis >= 0 && yOfXAxis < height)
            {
                float sclStartY = yOfXAxis - (sclLen / 2f);
                float sclEndY = yOfXAxis + (sclLen / 2f);

                float sclGapX = MathF.Abs(sclx / (right - left));
                float sclGapXScrn = sclGapX * (width - (safeZonePixels * 2));

                float sclXScrn = xOfYAxis % width;
                while (sclXScrn > 0)
                    sclXScrn -= sclGapXScrn;
                while (sclXScrn < width)
                {
                    vertexList.Add(new Vertex(new Vector2f(sclXScrn, sclStartY), axisColor));
                    vertexList.Add(new Vertex(new Vector2f(sclXScrn, sclEndY), axisColor));

                    sclXScrn += sclGapXScrn;
                }
            }
            if (xOfYAxis >= 0 && xOfYAxis < width)
            {
                float sclStartX = xOfYAxis - (sclLen / 2f);
                float sclEndX = xOfYAxis + (sclLen / 2f);

                float sclGapY = MathF.Abs(scly / (bottom - top));
                float sclGapYScrn = sclGapY * (height - (safeZonePixels * 2));

                float sclYScrn = yOfXAxis % height;
                while (sclYScrn > 0)
                    sclYScrn -= sclGapYScrn;
                while (sclYScrn < height)
                {
                    vertexList.Add(new Vertex(new Vector2f(sclStartX, sclYScrn), axisColor));
                    vertexList.Add(new Vertex(new Vector2f(sclEndX, sclYScrn), axisColor));

                    sclYScrn += sclGapYScrn;
                }
            }

            VertexArray vertices = new VertexArray(PrimitiveType.Lines, (uint)vertexList.Count);
            for (int i = 0; i < vertexList.Count; i++)
                vertices[(uint)i] = vertexList[i];

            renderTexture.Draw(vertices);
        }

        public void BeginDraw()
        {
            if (renderTexture == null)
                return;

            renderTexture.Clear(bgColor);

            if (drawAxis)
                DrawAxis();
        }

        public void Plot()
        {
            if (!isStem)
                DrawGraphLine();
            else
                DrawStemLines();
        }

        public void EndDraw()
        {
            if (renderTexture == null)
                return;

            renderTexture.Display();

            sprite = new Sprite(renderTexture.Texture);
            sprite.Position = new Vector2f(px, py);

            drawables = [sprite];
        }

        public void UpdateTexture()
        {
            if (renderTexture != null)
                renderTexture.Dispose();

            renderTexture = new RenderTexture((uint)width, (uint)height);
            renderTexture.Smooth = true;
        }

        public override Drawable?[] Draw()
        {
            if (sprite != null)
                sprite.Position = new Vector2f(px, py);

            return drawables;
        }

        public Graph(int px, int py, int width, int height)
        {
            this.px = px;
            this.py = py;
            this.width = width;
            this.height = height;

            UpdateTexture();
        }

        public void Dispose()
        {
            renderTexture?.Dispose();
        }
    }
}
