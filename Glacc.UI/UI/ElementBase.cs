﻿// SPDX-License-Identifier: LGPL-3.0-only
// Copyright (C) 2025 Glacc

using SFML.Graphics;

namespace Glacc.UI
{
    public class Element
    {
        public bool visable = true;

        public string customData = "";

        public virtual void Update() { }

        public virtual Drawable?[] Draw() { return Array.Empty<Drawable>(); }
    }
}
