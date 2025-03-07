// SPDX-License-Identifier: LGPL-3.0-only

using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Diagnostics;

namespace Glacc.UI
{
    public class AppWindow
    {
        string m_title = string.Empty;
        public string title
        {
            get => m_title;

            set
            {
                window?.SetTitle(value);
                m_title = value;
            }
        }

        int m_width;
        public int width
        {
            get => m_width;

            set
            {
                m_width = value;
            }
        }
        int m_height;
        public int height
        {
            get => m_height;

            set
            {
                m_height = value;
            }
        }
        bool resizable = false;

        public bool allowTransparentWindow = false;

        float m_updateTickrate = 60f;
        float timeEachUpdate;
        public float updateTickrate
        {
            get => m_updateTickrate;
            set
            {
                m_updateTickrate = value;
                timeEachUpdate = 1000f / value;
            }
        }
        public int maxUpdateEachDraw = 10;

        public RenderWindow? window = null;
        public RenderTexture? texture = null;
        Sprite? spriteOfRenderTexture = null;

        public EventHandler<EventArgs>? userInit = null;
        public EventHandler<EventArgs>? userUpdate = null;
        public EventHandler<EventArgs>? userDraw = null;
        public EventHandler<EventArgs>? afterClosing = null;

        public static RenderTarget? GetRenderTarget(AppWindow? appWindow)
        {
            if (appWindow == null)
                return null;
            if (appWindow.window == null)
                return null;

            RenderTarget? renderTarget = appWindow.window;

            return renderTarget;
        }

        public void SetSize(int width, int height, bool checkWhetherTheSizeIsTheSame = false)
        {
            if ((width == m_width && height == m_height) && checkWhetherTheSizeIsTheSame)
                return;

            m_width = width;
            m_height = height;

            if (window == null)
                return;

            window.Size = new Vector2u((uint)width, (uint)height);

            SizeEvent sizeEvent = new SizeEvent();
            sizeEvent.Width = (uint)width;
            sizeEvent.Height = (uint)height;
            OnResize(
                window,
                new SizeEventArgs(sizeEvent)
            );
        }

        void OnClose(object? sender, EventArgs e)
            => window?.Close();

        void OnResize(object? sender, SizeEventArgs e)
        {
            if (window == null)
                return;

            uint newWidth = e.Width;
            uint newHeight = e.Height;

            m_width = (int)newWidth;
            m_height = (int)newHeight;

            window.SetView
            (
                new View
                (
                    new FloatRect
                    (
                        0f,
                        0f,
                        m_width,
                        m_height
                    )
                )
            );

            if (texture == null)
                return;
            texture.Dispose();
            texture = new RenderTexture((uint)m_width, (uint)m_height);

            if (spriteOfRenderTexture == null)
                return;
            spriteOfRenderTexture.Dispose();
            spriteOfRenderTexture = new Sprite(texture.Texture);
        }

        public void Run()
        {
            if (window != null)
                return;

            Settings.InitSettings();

            Styles style = Styles.Titlebar | Styles.Close;
            if (resizable)
                style |= Styles.Resize;
            window = new RenderWindow
            (
                new VideoMode
                (
                    (uint)m_width,
                    (uint)m_height
                ),
                m_title,
                style
            );
            // renderWindow.SetFramerateLimit(60);
            window.SetVerticalSyncEnabled(true);

            texture = new RenderTexture((uint)m_width, (uint)m_height);
            spriteOfRenderTexture = new Sprite(texture.Texture);

            window.Closed += OnClose;
            window.Resized += OnResize;

            Event.ApplyEventHandlers(window);

            userInit?.Invoke(this, EventArgs.Empty);

            bool drawAndResetState = true;

            timeEachUpdate = 1000f / m_updateTickrate;
            float timeAfterLastUpdate = timeEachUpdate;
            Stopwatch stopwatch = new Stopwatch();

            while (window.IsOpen)
            {
                Event.Update(window, drawAndResetState, true);
                drawAndResetState = false;

                stopwatch.Stop();
                double elaspedMs = stopwatch.Elapsed.TotalMilliseconds;
                stopwatch.Restart();
                timeAfterLastUpdate += (float)elaspedMs;

                if (allowTransparentWindow)
                    window.Clear(Color.Transparent);

                int updateCount = 0;
                while (timeAfterLastUpdate >= timeEachUpdate)
                {
                    bool firstUpdate = (updateCount == 0);

                    Event.UpdateState();

                    userUpdate?.Invoke(this, EventArgs.Empty);

                    drawAndResetState = true;

                    timeAfterLastUpdate -= timeEachUpdate;

                    if (firstUpdate)
                    {
                        texture.Clear(Settings.bgColor);

                        userDraw?.Invoke(this, EventArgs.Empty);

                        texture.Display();
                    }

                    Event.ResetState();

                    updateCount++;
                    if (updateCount >= maxUpdateEachDraw)
                    {
                        timeAfterLastUpdate %= timeEachUpdate;
                        break;
                    }
                }

                window.Draw(spriteOfRenderTexture);

                window.Display();
            }

            afterClosing?.Invoke(this, EventArgs.Empty);
        }

        public AppWindow(string title, int width, int height, bool resizable = false)
        {
            m_title = title;
            m_width = width;
            m_height = height;
            this.resizable = resizable;
        }
    }
}
