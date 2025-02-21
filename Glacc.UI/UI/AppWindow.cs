using SFML.Graphics;
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
                renderWindow?.SetTitle(value);
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

        public RenderWindow? renderWindow = null;
        public RenderTexture? renderTexture = null;
        Sprite? spriteOfRenderTexture = null;

        public EventHandler<EventArgs>? userInit = null;
        public EventHandler<EventArgs>? userUpdate = null;
        public EventHandler<EventArgs>? userDraw = null;
        public EventHandler<EventArgs>? afterClosing = null;

        public static RenderTarget? GetRenderTarget(AppWindow? appWindow)
        {
            if (appWindow == null)
                return null;
            if (appWindow.renderWindow == null)
                return null;

            RenderTarget? renderTarget = appWindow.renderWindow;

            return renderTarget;
        }

        void OnClose(object? sender, EventArgs e)
            => renderWindow?.Close();

        void OnResize(object? sender, SizeEventArgs e)
        {
            if (renderWindow == null)
                return;

            uint newWidth = e.Width;
            uint newHeight = e.Height;

            m_width = (int)newWidth;
            m_height = (int)newHeight;

            renderWindow.SetView
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

            if (renderTexture == null)
                return;
            renderTexture.Dispose();
            renderTexture = new RenderTexture((uint)m_width, (uint)m_height);

            if (spriteOfRenderTexture == null)
                return;
            spriteOfRenderTexture.Dispose();
            spriteOfRenderTexture = new Sprite(renderTexture.Texture);
        }

        public void Run()
        {
            if (renderWindow != null)
                return;

            Settings.InitSettings();

            SFML.Window.Styles style = SFML.Window.Styles.Titlebar | SFML.Window.Styles.Close;
            if (resizable)
                style |= SFML.Window.Styles.Resize;
            renderWindow = new RenderWindow
            (
                new SFML.Window.VideoMode
                (
                    (uint)m_width,
                    (uint)m_height
                ),
                m_title,
                style
            );
            // renderWindow.SetFramerateLimit(60);
            renderWindow.SetVerticalSyncEnabled(true);

            renderTexture = new RenderTexture((uint)m_width, (uint)m_height);
            spriteOfRenderTexture = new Sprite(renderTexture.Texture);

            renderWindow.Closed += OnClose;
            renderWindow.Resized += OnResize;

            Event.ApplyEventHandlers(renderWindow);

            userInit?.Invoke(this, EventArgs.Empty);

            bool drawAndResetState = true;

            timeEachUpdate = 1000f / m_updateTickrate;
            float timeAfterLastUpdate = timeEachUpdate;
            Stopwatch stopwatch = new Stopwatch();

            while (renderWindow.IsOpen)
            {
                Event.Update(renderWindow, drawAndResetState, true);
                drawAndResetState = false;

                stopwatch.Stop();
                double elaspedMs = stopwatch.Elapsed.TotalMilliseconds;
                stopwatch.Restart();
                timeAfterLastUpdate += (float)elaspedMs;

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
                        renderTexture.Clear(Settings.bgColor);

                        userDraw?.Invoke(this, EventArgs.Empty);

                        renderTexture.Display();
                    }

                    Event.ResetState();

                    updateCount++;
                    if (updateCount >= maxUpdateEachDraw)
                    {
                        timeAfterLastUpdate %= timeEachUpdate;
                        break;
                    }
                }

                renderWindow.Draw(spriteOfRenderTexture);

                renderWindow.Display();
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
