// SPDX-License-Identifier: LGPL-3.0-only
// Copyright (C) 2025 Glacc

using SFML.Window;

namespace Glacc.UI.KeyPress
{
    public class NeutralKeys
    {
        Keyboard.Scancode key1;
        Keyboard.Scancode key2;

        public delegate void KeyDownEvent(bool key2Pressed);
        public delegate void KeyUpEvent();

        public KeyDownEvent? keyDownEvent;
        public KeyUpEvent? keyUpEvent;

        public void Update()
        {
            bool key1Down = Event.IsKeyDown(key1);
            bool key2Down = Event.IsKeyDown(key2);
            bool bothKeysDown = key1Down && key2Down;
            key1Down = !bothKeysDown && key1Down;
            key2Down = !bothKeysDown && key2Down;
            if (key1Down)
            {
                if (keyDownEvent != null)
                    keyDownEvent(false);
            }
            else if (key2Down)
            {
                if (keyDownEvent != null)
                    keyDownEvent(true);
            }
            else
            {
                if (keyUpEvent != null)
                    keyUpEvent();
            }
        }

        public NeutralKeys(Keyboard.Scancode key1, Keyboard.Scancode key2, KeyDownEvent? keyDownEvent = null, KeyUpEvent? keyUpEvent = null)
        {
            this.key1 = key1;
            this.key2 = key2;

            this.keyDownEvent = keyDownEvent;
            this.keyUpEvent = keyUpEvent;
        }
    }

    public class SingleTriggerKey
    {
        Keyboard.Scancode key;

        bool hold = false;
        public bool triggered = false;

        public delegate void KeyTriggerEvent();

        public KeyTriggerEvent? onTrigger;

        public bool Update()
        {
            triggered = false;

            if (Event.IsKeyDown(key))
            {
                if (!hold)
                {
                    triggered = true;

                    if (onTrigger != null)
                        onTrigger();
                }
                hold = true;
            }
            else
                hold = false;

            return triggered;
        }

        public SingleTriggerKey(Keyboard.Scancode key, KeyTriggerEvent? onTrigger = null)
        {
            this.key = key;
            this.onTrigger = onTrigger;
        }
    }
}
