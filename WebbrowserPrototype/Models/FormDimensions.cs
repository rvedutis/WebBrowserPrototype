namespace WebbrowserPrototype
{
    public class FormDimensions
    {
        public FormDimensions(int browserWidth, int browserHeight, int pageWidth, int pageHeight)
        {
            BrowserWidth = browserWidth;
            BrowserHeight = browserHeight;
            PageWidth = pageWidth;
            PageHeight = pageHeight;
        }

        public int BrowserWidth { get; set; }
        public int BrowserHeight { get; set; }
        public int PageWidth { get; set; }
        public int PageHeight { get; set; }
        public int RenderedHeight { get; set; }
    }
}