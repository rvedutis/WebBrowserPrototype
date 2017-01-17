using Persits.PDF;

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

            var doc = manager.CreateDocument();
            var page = doc.Pages.Add(dimensions.PageWidth*pixelToPdf, dimensions.PageHeight*pixelToPdf);

            var imageHeight = BuildParam(
                manager,
                new[] {"PageHeight"},
                new[] {(dimensions.RenderHeight*dimensions.Zoom).ToString()}
                );

            var image = doc.OpenUrl(markup, imageHeight);

            var scale = page.Width/image.Width;

            var imageScale = BuildParam(
                manager,
                new[]
                {
                    "x",
                    "y",
                    "ScaleX",
                    "ScaleY"
                },
                new[]
                {
                    (dimensions.MarginLeft*pixelToPdf).ToString(),
                    (page.Height - dimensions.MarginTop*pixelToPdf - image.Height*scale).ToString(),
                    scale.ToString(),
                    scale.ToString()
                }
                );

            page.Canvas.DrawImage(image, imageScale);

            return doc.SaveToMemory();
        }

        private static PdfParam BuildParam(PdfManager manager, string[] keys, string[] values)
        {
            var paramString = string.Empty;

            for (var index = 0; index < keys.Length; index++)
            {
                paramString = paramString + keys[index] + "=" + values[index] +
                              (index + 1 != keys.Length ? "; " : string.Empty);
            }

            return manager.CreateParam(paramString);
        }
    }
}