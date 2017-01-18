using Persits.PDF;
using System.Collections.Generic;

namespace PDFConverter
{
    public static class PDFConverter
    {
        private const float pixelToPdf = .75F;

        public static byte[] Convert(string page, Dimensions dimensions)
        {
            if (page == null)
            {
                return null;
            }

            return Generate(page, dimensions);
        }

        public static byte[] Generate(string markup, Dimensions dimensions)
        {
            var manager = new PdfManager
            {
                RegKey = "GYlb/v2DbZp5SEU9AfMJwoqj07r/YHR+I9XLiQ3ykzkHH8ftMVaYBR/+twbs3fcRhit+VMCVPj5W"
            };

            using (var doc = manager.CreateDocument())
            {
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
        }

        private static PdfParam BuildParam(PdfManager manager, Dictionary<string, object> options)
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