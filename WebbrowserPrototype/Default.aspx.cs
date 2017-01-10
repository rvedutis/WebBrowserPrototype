using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Web.UI;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using ImageProcessor;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;

namespace WebbrowserPrototype
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var content = Server.UrlDecode(Request.Form["RenderedContent"]);

            var dimensions = new FormDimensions(768, 1056, 768, 984);

            GeneratePdf(content, dimensions);
        }

        private void GeneratePdf(string content, FormDimensions dimensions)
        {
            var thread = new Thread(delegate ()
            {
                using (var browser = new WebBrowser())
                {
                    browser.ScrollBarsEnabled = false;
                    browser.AllowNavigation = false;
                    browser.ScriptErrorsSuppressed = true;

                    browser.Navigate("about:blank");

                    browser.DocumentText = content;

                    browser.Width = dimensions.BrowserWidth;
                    browser.Height = dimensions.BrowserHeight;
                    browser.DocumentCompleted += (sender, e) => DocCompleted(sender, e, dimensions);

                    while (browser.ReadyState != WebBrowserReadyState.Complete)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private void DocCompleted(object sender, WebBrowserDocumentCompletedEventArgs e, FormDimensions dimensions)
        {
            var browser = sender as WebBrowser;
            if (browser == null)
            {
                return;
            }

            dimensions.RenderedHeight = GetRenderedHeight(browser);

            var webPageAsBitmap = GetWebPageAsBitmap(browser, dimensions);

            var pageCount = PrintablePageCount(dimensions);

            using (var doc = new Document())
            {
                using (var outputStream = new MemoryStream())
                {
                    var writer = PdfWriter.GetInstance(doc, outputStream);
                    writer.CloseStream = false;

                    doc.Open();

                    for (var page = 0; page < pageCount; page++)
                    {
                        var img = SliceImage(webPageAsBitmap, dimensions, page);

                        doc.SetPageSize(new Rectangle(dimensions.BrowserWidth, dimensions.BrowserHeight));
                        doc.NewPage();
                        doc.Add(img);
                    }

                    doc.Close();

                    SendPdfToClient(outputStream);
                }
            }
        }

        private static int PrintablePageCount(FormDimensions dimensions)
        {
            return (int)Math.Floor((decimal)dimensions.RenderedHeight / (decimal)dimensions.PageHeight);
        }

        private Image SliceImage(Bitmap webPageAsBitmap, FormDimensions dimensions, int page)
        {
            using (var image = new ImageFactory())
            {
                using (var imageStream = new MemoryStream())
                {
                    image.Quality(100);
                    image.Load(webPageAsBitmap);
                    image.Crop(new System.Drawing.Rectangle(0, page * dimensions.PageHeight, dimensions.BrowserWidth,
                        dimensions.PageHeight));

                    image.Save(imageStream);

                    return Image.GetInstance(System.Drawing.Image.FromStream(imageStream),
                        ImageFormat.Bmp);
                }
            }
        }

        private int GetRenderedHeight(WebBrowser browser)
        {
            return browser.Document.Body.ScrollRectangle.Height;
        }

        private Bitmap GetWebPageAsBitmap(WebBrowser browser, FormDimensions dimensions)
        {
            browser.Height = dimensions.RenderedHeight;

            var bitmap = new Bitmap(dimensions.BrowserWidth, dimensions.RenderedHeight);

            browser.DrawToBitmap(bitmap,
                new System.Drawing.Rectangle(0, 0, dimensions.BrowserWidth, dimensions.RenderedHeight));

            return bitmap;
        }

        private void SendPdfToClient(MemoryStream outputStream)
        {
            Response.Buffer = true;
            Response.ClearHeaders();
            Response.ClearContent();
            Response.ContentType = "application/pdf";
            Response.AppendHeader("Content-Disposition", "inline; filename=Report.pdf");
            Response.AppendHeader("Content-Transfer-Encoding", "binary");
            Response.AppendHeader("Content-Length", outputStream.Length.ToString());
            Response.BinaryWrite(outputStream.ToArray());
            Response.Flush();
            Response.End();
        }
    }
}