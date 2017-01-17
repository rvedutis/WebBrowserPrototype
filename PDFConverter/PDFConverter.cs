using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Image = iTextSharp.text.Image;
using Rectangle = System.Drawing.Rectangle;

namespace PDFConverter
{
    public static class PDFConverter
    {
        public static Document Convert(string page, Dimensions dimensions)
        {
            if (page == null)
            {
                return null;
            }

            var instanceId = ConfigureInstance();
            
            return Render(page, dimensions, instanceId);
        }


        private static Guid ConfigureInstance()
        {
            var instanceId = Guid.NewGuid();

            Directory.CreateDirectory(GetInstanceFilePath(instanceId));

            return instanceId;
        }

        private static string GetInstanceFilePath(Guid instanceId)
        {
            return $@"{HttpRuntime.AppDomainAppPath}\artifacts\{instanceId}\";
        }

        private static Document Render(string page, Dimensions dimensions, Guid instanceId)
        {
            var thread = new Thread(delegate()
            {
                using (var browser = new WebBrowser())
                {
                    browser.ScrollBarsEnabled = false;
                    browser.AllowNavigation = false;
                    browser.ScriptErrorsSuppressed = false;

                    browser.DocumentText = page;

                    browser.Width = dimensions.RenderWidth*dimensions.Zoom;
                    browser.Height = dimensions.RenderHeight*dimensions.Zoom;

                    var rectangle = new Rectangle(0, 0, browser.Width, browser.Height);

                    browser.DocumentCompleted += (sender, e) => RenderCompleted(sender, e, dimensions, instanceId, rectangle);

                    while (browser.ReadyState != WebBrowserReadyState.Complete)
                    {
                        Application.DoEvents();
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            return new Document();
        }

        private static void RenderCompleted(object sender, WebBrowserDocumentCompletedEventArgs e, Dimensions dimensions,
            Guid instanceId, Rectangle rectangle)
        {
            var browser = sender as WebBrowser;
            if (browser == null)
            {
                return;
            }

            var imageId = SaveWebPageAsImage(browser, instanceId, rectangle);

            using (var pdf = new Document())
            {
                pdf.SetMargins(dimensions.MarginLeft, dimensions.MarginTop, dimensions.MarginRight,
                    dimensions.MarginBottom);
                pdf.SetPageSize(new iTextSharp.text.Rectangle(dimensions.PageWidth, dimensions.PageHeight));

                using (var outputStream = new MemoryStream())
                {
                    var writer = PdfWriter.GetInstance(pdf, outputStream);
                    writer.CloseStream = false;

                    pdf.Open();

                    pdf.NewPage();

                    pdf.Add(CreateImage(imageId, instanceId, dimensions));

                    pdf.Close();

                    CleanUpArtifacts(instanceId);
                }
            }
        }

        private static void CleanUpArtifacts(Guid instanceId)
        {
            Directory.Delete($@"{GetInstanceFilePath(instanceId)}", true);
        }

        private static Guid SaveWebPageAsImage(WebBrowser browser, Guid instanceId, Rectangle rectangle)
        {
            using (var bitmap = new Bitmap(browser.Width, browser.Height))
            {
                bitmap.SetResolution(1000.0f, 1000.0f);

                browser.DrawToBitmap(bitmap, rectangle);

                var imageId = Guid.NewGuid();

                bitmap.Save($@"{GetInstanceFilePath(instanceId)}{imageId}.bmp");

                return imageId;
            }
        }

        private static Image CreateImage(Guid imageId, Guid instanceId, Dimensions dimensions)
        {
            var image = Image.GetInstance($@"{GetInstanceFilePath(instanceId)}{imageId}.bmp");

            image.ScalePercent(100/dimensions.Zoom);

            return image;
        }
    }
}