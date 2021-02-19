﻿using System;
using System.Collections.Generic;
using System.Globalization;

namespace PlaywrightSharp
{
    public class Polling
    {
        public static Polling WithRequestAnimationFrame = new(true);

        public float? Interval { get; }
        public bool RequestAnimationFrame { get; }

        private Polling(bool setRaf)
        {
            this.RequestAnimationFrame = setRaf;
        }

        private Polling(float interval)
        {
            this.Interval = interval;
        }

        public static implicit operator Polling(float pollingInterval)
        {
            return new Polling(pollingInterval);
        }
    }
}
