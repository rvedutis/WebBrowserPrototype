using System.Collections.Generic;
using ASPPDFLib;

namespace PDFConverter
{
    public static class PDFConverter
    {
        private const float pixelToPdf = .75F;

        private static PdfManager manager = new PdfManager
        {
            RegKey = "IWezgv+yqt4GIAc213KacCXF1YqRnVGi/TkEGJnd8oBlQMSy/9zzPdk7EzuLOVwZ5X1Vzc5OANZ3"
        };

        public static byte[] Convert(string markup, Dimensions dimensions)
        {
            return ConvertMarkup(new string[] { markup }, dimensions)[0];
        }

        public static byte[] Convert(string[] markupList, Dimensions dimensions)
        {
            return Combine(ConvertMarkup(markupList, dimensions));
        }

        private static List<byte[]> ConvertMarkup(string[] markupList, Dimensions dimensions)
        {
            var pdfs = new List<byte[]>();

            foreach (var markup in markupList)
            {
                pdfs.Add(MarkupToPDF(markup, dimensions));
            }

            return pdfs;
        }

        private static byte[] MarkupToPDF(string markup, Dimensions dimensions)
        {
            var doc = manager.CreateDocument();

            var page = doc.Pages.Add(dimensions.PageWidth * pixelToPdf, dimensions.PageHeight * pixelToPdf);

            var browserOptions = new Dictionary<string, object>
                {
                    { "PageHeight", dimensions.RenderHeight * dimensions.Zoom },
                    { "Scripts", false }
                };

            var image = doc.OpenUrl(markup, BuildParam(manager, browserOptions));

            var imageScale = new Dictionary<string, object>
                {
                    { "x", dimensions.MarginLeft * pixelToPdf },
                    { "y", page.Height - (dimensions.MarginTop * pixelToPdf) - (image.Height * (page.Width / image.Width)) },
                    { "ScaleX", page.Width / image.Width },
                    { "ScaleY", page.Width / image.Width }
                };

            page.Canvas.DrawImage(image, BuildParam(manager, imageScale));

            return doc.SaveToMemory();
        }

        public static byte[] Combine(List<byte[]> pdfs)
        {
            var combinedDoc = manager.CreateDocument();

            foreach (var pdf in pdfs)
            {
                var doc = manager.OpenDocumentBinary(pdf);

                for (int index = 0; index < doc.Pages.Count; index++)
                {
                    var graphics = combinedDoc.CreateGraphicsFromPage(doc, index + 1);
                    var newPage = combinedDoc.Pages.Add(doc.Pages[index + 1].Width, doc.Pages[index + 1].Height);

                    var imageScale = new Dictionary<string, object>
                    {
                        { "x", 0 },
                        { "y", 0 },
                        { "ScaleX", 1 },
                        { "ScaleY", 1 }
                    };

                    newPage.Canvas.DrawGraphics(graphics, BuildParam(manager, imageScale));
                }
            }

            return combinedDoc.SaveToMemory();
        }

        private static IPdfParam BuildParam(PdfManager manager, Dictionary<string, object> options)
        {
            var paramString = string.Empty;
            int counter = 1;

            foreach (var key in options.Keys)
            {
                paramString = paramString + key + "=" + options[key].ToString() + (counter < options.Keys.Count ? "; " : string.Empty);
                counter++;
            }

            return manager.CreateParam(paramString);
        }
    }
}