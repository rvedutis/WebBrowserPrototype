using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ASPPDFLib;

namespace PDFConverter
{
    public static class PDFConverter
    {
        private const float PixelToPdf = .75F;

        public static void Convert(ref Pdf pdf, Dimensions dimensions)
        {
            var pdfs = new[] { pdf };
            ConvertMarkup(ref pdfs, dimensions);
        }

        /*
        public static void Convert(Pdf[] pdfs, Dimensions dimensions)
        {
            Combine(ConvertMarkup(pdfs, dimensions));
        }*/

        public static byte[] Combine(List<Pdf> pdfs)
        {
            var manager = GetPdfManager();
            var combinedDoc = manager.CreateDocument();

            foreach (var pdf in pdfs)
            {
                try
                {
                    var doc = manager.OpenDocumentBinary(pdf.Bytes);

                    for (var index = 0; index < doc.Pages.Count; index++)
                    {
                        var graphics = combinedDoc.CreateGraphicsFromPage(doc, index + 1);
                        var newPage = combinedDoc.Pages.Add(doc.Pages[index + 1].Width, doc.Pages[index + 1].Height);

                        var imageScale = BuildParam(
                            manager,
                            new Dictionary<string, object>
                            {
                                {"x", 0},
                                {"y", 0},
                                {"ScaleX", 1},
                                {"ScaleY", 1}
                            }
                            );

                        newPage.Canvas.DrawGraphics(graphics, imageScale);
                        ReleaseComObjects(new object[] {imageScale, newPage, graphics});
                    }

                    ReleaseComObjects(new object[] {doc});
                }
                catch (Exception e)
                {

                }

            }

            var docBytes = combinedDoc.SaveToMemory();

            ReleaseComObjects(new object[] { combinedDoc, manager });

            return docBytes;
        }

        private static void ConvertMarkup(ref Pdf[] pdfs, Dimensions dimensions)
        {
            foreach (var pdf in pdfs)
            {
                MarkupToPdf(pdf, dimensions);
            }
        }

        private static void MarkupToPdf(Pdf pdf, Dimensions dimensions)
        {
            var manager = GetPdfManager();
            var doc = manager.CreateDocument();
            var page = doc.Pages.Add(dimensions.PageWidth * PixelToPdf, dimensions.PageHeight * PixelToPdf);

            var browserOptions = BuildParam(
                manager,
                new Dictionary<string, object>
                {
                    {"WindowHeight", dimensions.RenderHeight*dimensions.Zoom},
                    {"WindowWidth", dimensions.RenderWidth*dimensions.Zoom},
                    {"Scripts", false}
                }
                );

            try
            {
                var image = doc.OpenUrl(pdf.Markup, browserOptions);

                var imageScale = BuildParam(
                    manager,
                    new Dictionary<string, object>
                    {
                    {"x", dimensions.MarginLeft*PixelToPdf},
                    {"y", page.Height - dimensions.MarginTop*PixelToPdf - image.Height*(page.Width/image.Width)},
                    {"ScaleX", page.Width/image.Width},
                    {"ScaleY", page.Width/image.Width}
                    }
                    );

                page.Canvas.DrawImage(image, imageScale);

                var docBytes = doc.SaveToMemory();

                ReleaseComObjects(new object[] { imageScale, image, browserOptions, page, doc, manager });

                pdf.Bytes = docBytes;
            }
            catch (Exception e)
            {
            }
        }

        private static PdfManager GetPdfManager()
        {
            return new PdfManager
            {
                RegKey = "IWezgv+yqt4GIAc213KacCXF1YqRnVGi/TkEGJnd8oBlQMSy/9zzPdk7EzuLOVwZ5X1Vzc5OANZ3"
            };
        }

        private static IPdfParam BuildParam(PdfManager manager, Dictionary<string, object> options)
        {
            var paramString = string.Empty;
            var counter = 1;

            foreach (var key in options.Keys)
            {
                paramString = paramString + key + "=" + options[key] +
                              (counter < options.Keys.Count ? "; " : string.Empty);
                counter++;
            }

            return manager.CreateParam(paramString);
        }

        private static void ReleaseComObjects(object[] comObjects)
        {
            foreach (var comObject in comObjects)
            {
                Marshal.ReleaseComObject(comObject);
            }
        }
    }
}