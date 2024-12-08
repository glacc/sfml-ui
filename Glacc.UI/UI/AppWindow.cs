using SFML.Graphics;
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
        {
            if (sender == null)
                return;

            renderWindow?.Close();
        }

        public void Run()
        {
            if (renderWindow != null)
                return;

            Settings.InitSettings();

            renderWindow = new RenderWindow
            (
                new SFML.Window.VideoMode
                (
                    (uint)m_width,
                    (uint)m_height
                ),
                m_title,
                SFML.Window.Styles.Titlebar | SFML.Window.Styles.Close
            );
            // renderWindow.SetFramerateLimit(60);
            renderWindow.SetVerticalSyncEnabled(true);

            renderTexture = new RenderTexture((uint)m_width, (uint)m_height);
            spriteOfRenderTexture = new Sprite(renderTexture.Texture);

            renderWindow.Closed += OnClose;

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

                    if (firstUpdate)
                        renderTexture.Clear(Settings.bgColor);

                    userUpdate?.Invoke(this, EventArgs.Empty);

                    drawAndResetState = true;

                    timeAfterLastUpdate -= timeEachUpdate;

                    if (firstUpdate)
                    {
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

        public AppWindow(string title, int width, int height)
        {
            m_title = title;
            m_width = width;
            m_height = height;
        }
    }
}
