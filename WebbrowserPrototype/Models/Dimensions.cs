using System;

namespace WebbrowserPrototype
{
    public class Dimensions
    {
        public int RenderWidth { get; set; }
        public int RenderHeight { get; set; }
        public int PageWidth { get; set; }
        public int PageHeight { get; set; }

        public float MarginLeft { get; set; }
        public float MarginTop { get; set; }
        public float MarginRight { get; set; }
        public float MarginBottom { get; set; }

        public int Zoom { get; set; }
    }
}