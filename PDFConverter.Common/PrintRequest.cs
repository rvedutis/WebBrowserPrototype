using System;

namespace PDFConverter.Common
{
    public class PrintRequest
    {
        public int PageWidth { get; set; }
        public int PageHeight { get; set; }
        public float MarginLeft { get; set; }
        public float MarginTop { get; set; }
        public float MarginRight { get; set; }
        public float MarginBottom { get; set; }
        public string FilePath { get; set; }
        public Guid FileName { get; set; }
        public string Content { get; set; }
    }
}
