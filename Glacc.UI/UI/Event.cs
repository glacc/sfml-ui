﻿using Glacc.UI.Elements;
using SFML.Window;

namespace Glacc.UI
{
    public static class Event
    {
        /* Mouse Position */
        public static int mouseX = 0;
        public static int mouseY = 0;
        public static int mouseXold = 0;
        public static int mouseYold = 0;
        public static int mouseXvel = 0;
        public static int mouseYvel = 0;
        public static int mouseXpress = 0;
        public static int mouseYpress = 0;

        public static bool mouseMove = false;
        public static bool mouseAvailable = true;

        public struct MouseState
        {
            public int mouseX;
            public int mouseY;
            public bool mouseAvailable;
            public Viewport? currentViewport;

            public MouseState(int mouseX, int mouseY, bool mouseAvailable, Viewport? currentViewport)
            {
                this.mouseX = mouseX;
                this.mouseY = mouseY;
                this.mouseAvailable = mouseAvailable;
                this.currentViewport = currentViewport;
            }
        }
        public static List<MouseState> mouseStateStack = new List<MouseState>();

        public static Viewport? currentViewport = null;
        public static Viewport? focusingViewport = null;

        /* Mouse Button State */
        public static bool mousePress = false;
        public static bool mouseHold = false;
        public static bool mouseRelease = false;
        public static bool mouseDragRelease = false;

        public static bool mouseDrag = false;
        public static int mouseDragStartThreshold = 25;

        /* Mouse Scroll */
        public static bool mouseScroll = false;
        public static float mouseScrollVelVert = 0f;
        public static float mouseScrollVelHorz = 0f;

        public static bool textEntered = false;
        public static string textUnicode = "";

        public static void PushState(int relX, int relY)
        {
            mouseStateStack.Add(new MouseState(mouseX, mouseY, mouseAvailable, currentViewport));

            mouseX -= relX;
            mouseY -= relY;
        }

        public static void PopState()
        {
            if (mouseStateStack.Count == 0)
                return;

            MouseState lastState = mouseStateStack.Last();
            mouseX = lastState.mouseX;
            mouseY = lastState.mouseY;
            mouseAvailable = lastState.mouseAvailable;
            currentViewport = lastState.currentViewport;

            mouseStateStack.RemoveAt(mouseStateStack.Count - 1);
        }

        static class KeyPressReleaseList
        {
            static public List<Keyboard.Scancode> pressed = new List<Keyboard.Scancode>();
            static public List<Keyboard.Scancode> released = new List<Keyboard.Scancode>();

            static public int scancodeCount = Enum.GetNames(typeof(Keyboard.Scancode)).Length;
            static public bool[] holding = new bool[scancodeCount];

            static public int FindKey(Keyboard.Scancode scancode, ref List<Keyboard.Scancode> keyList)
            {
                for (int n = 0; n < keyList.Count; n++)
                {
                    if (keyList[n] == scancode)
                        return n;
                }

                return -1;
            }

            static public void PressKey(Keyboard.Scancode scancode)
            {
                pressed.Add(scancode);

                if (scancode != Keyboard.Scancode.Unknown)
                    holding[(int)scancode] = true;
            }

            static public void ReleaseKey(Keyboard.Scancode scancode)
            {
                released.Add(scancode);

                if (scancode != Keyboard.Scancode.Unknown)
                    holding[(int)scancode] = false;
            }

            static public void ClearPressReleaseList()
            {
                pressed.Clear();
                released.Clear();
            }
        }

        static public bool IsKeyDown(Keyboard.Scancode scancode)
        {
            if (scancode != Keyboard.Scancode.Unknown && (int)scancode < KeyPressReleaseList.scancodeCount)
                return KeyPressReleaseList.holding[(int)scancode];

            return false;
        }

        static public bool IsKeyPressed(Keyboard.Scancode scancode)
            => (KeyPressReleaseList.FindKey(scancode, ref KeyPressReleaseList.pressed) >= 0);

        static public bool IsKeyReleased(Keyboard.Scancode scancode)
            => (KeyPressReleaseList.FindKey(scancode, ref KeyPressReleaseList.released) >= 0);

        static void OnKeyPress(object? sender, KeyEventArgs e)
            => KeyPressReleaseList.PressKey(e.Scancode);

        static void OnKeyRelease(object? sender, KeyEventArgs e)
            => KeyPressReleaseList.ReleaseKey(e.Scancode);

        static void OnTextEnter(object? sender, TextEventArgs e)
        {
            textEntered = true;
            textUnicode += e.Unicode;
        }

        static void OnMouseMove(object? sender, MouseMoveEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;
        }

        static void OnMousePress(object? sender, MouseButtonEventArgs e)
        {
            mouseXpress = e.X;
            mouseYpress = e.Y;

            mousePress = true;
            mouseHold = true;
        }

        static void OnMouseRelease(object? sender, MouseButtonEventArgs e)
        {
            mouseRelease = true;
            mouseHold = false;
        }

        static void OnMouseScroll(object? sender, MouseWheelScrollEventArgs e)
        {
            mouseScroll = true;

            if (e.Wheel == Mouse.Wheel.VerticalWheel)
                mouseScrollVelVert += e.Delta;

            if (e.Wheel == Mouse.Wheel.HorizontalWheel)
                mouseScrollVelHorz += e.Delta;
        }

        public static void ApplyEventHandlers(Window? window)
        {
            if (window == null)
                return;

            window.KeyPressed += new EventHandler<KeyEventArgs>(OnKeyPress);
            window.KeyReleased += new EventHandler<KeyEventArgs>(OnKeyRelease);
            window.TextEntered += new EventHandler<TextEventArgs>(OnTextEnter);
            window.MouseMoved += new EventHandler<MouseMoveEventArgs>(OnMouseMove);
            window.MouseButtonPressed += new EventHandler<MouseButtonEventArgs>(OnMousePress);
            window.MouseButtonReleased += new EventHandler<MouseButtonEventArgs>(OnMouseRelease);
            window.MouseWheelScrolled += new EventHandler<MouseWheelScrollEventArgs>(OnMouseScroll);
        }

        // Will be combined back later?
        public static void ResetPossiblyRepeatedState()
        {
            KeyPressReleaseList.ClearPressReleaseList();

            mouseStateStack.Clear();

            mouseAvailable = true;

            currentViewport = null;
            focusingViewport = null;

            textEntered = false;
            textUnicode = "";
        }

        public static void ResetState()
        {
            mousePress = false;
            mouseRelease = false;
            mouseDragRelease = false;

            mouseScroll = false;
            mouseScrollVelVert = 0f;
            mouseScrollVelHorz = 0f;

            ResetPossiblyRepeatedState();
        }

        public static void UpdateState()
        {
            mouseXvel = mouseX - mouseXold;
            mouseYvel = mouseY - mouseYold;

            if (mouseHold)
            {
                int distX = mouseX - mouseXpress;
                int distY = mouseY - mouseYpress;
                float dist = MathF.Sqrt((distX * distX) + (distY * distY));

                if (dist > mouseDragStartThreshold)
                    mouseDrag = true;
            }
            else
            {
                if (mouseDrag)
                    mouseDragRelease = true;

                mouseDrag = false;
            }

            mouseXold = mouseX;
            mouseYold = mouseY;
        }

        public static void Update(Window? window, bool resetStates = true, bool separateUpdate = false)
        {
            if (window == null)
                return;

            if (resetStates)
                ResetState();

            window.DispatchEvents();

            if (!separateUpdate)
                UpdateState();
        }
    }
}