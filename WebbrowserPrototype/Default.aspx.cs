using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Web.UI;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;

namespace WebbrowserPrototype
{
    public partial class _Default : Page
    {
        private List<Bitmap> _bitmaps = new List<Bitmap>();
        private int _totalPages;
        private Dimensions _dimensions = new Dimensions();
        private const string _separator = "#####NEWPAGE#####";

        protected void Page_Load(object sender, EventArgs e)
        {
            var content = Server.UrlDecode(Request.Form["RenderedContent"]);

            if (content == null)
            {
                return;
            }

            _dimensions.BrowserWidth = int.Parse(Request.Form["BrowserWidth"]);
            _dimensions.BrowserHeight = int.Parse(Request.Form["BrowserHeight"]);
            _dimensions.PageWidth = int.Parse(Request.Form["PageWidth"]);
            _dimensions.PageHeight = int.Parse(Request.Form["PageHeight"]);
            _dimensions.MarginLeft = Convert.ToSingle(Request.Form["MarginLeft"]);
            _dimensions.MarginTop = Convert.ToSingle(Request.Form["MarginTop"]);
            _dimensions.MarginRight = Convert.ToSingle(Request.Form["MarginRight"]);
            _dimensions.MarginBottom = Convert.ToSingle(Request.Form["MarginBottom"]);
            _dimensions.Zoom = int.Parse(Request.Form["Zoom"]);

            var pages = content.Split(new[] { _separator }, StringSplitOptions.None);

            _totalPages = pages.Length;

            GeneratePdf(pages);
        }

        private void GeneratePdf(string[] pages)
        {
            var thread = new Thread(delegate ()
            {
                foreach (var page in pages)
                {
                    using (var browser = new WebBrowser())
                    {
                        browser.ScrollBarsEnabled = false;
                        browser.AllowNavigation = false;
                        browser.ScriptErrorsSuppressed = false;

                        browser.DocumentText = page;

                        browser.Width = _dimensions.BrowserWidth * _dimensions.Zoom;
                        browser.Height = _dimensions.BrowserHeight * _dimensions.Zoom;

                        browser.DocumentCompleted += (sender, e) => DocCompleted(sender, e);

                        while (browser.ReadyState != WebBrowserReadyState.Complete)
                        {
                            System.Windows.Forms.Application.DoEvents();
                        }
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private void DocCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var browser = sender as WebBrowser;
            if (browser == null)
            {
                return;
            }

            var webPageAsBitmap = GetWebPageAsBitmap(browser);

            _bitmaps.Add(webPageAsBitmap);

            if (!_totalPages.Equals(_bitmaps.Count))
            {
                return;
            }

            using (var pdf = new Document())
            {
                pdf.SetMargins(_dimensions.MarginLeft, _dimensions.MarginTop, _dimensions.MarginRight, _dimensions.MarginBottom);
                pdf.SetPageSize(new Rectangle(_dimensions.PageWidth, _dimensions.PageHeight));

                using (var outputStream = new MemoryStream())
                {
                    var writer = PdfWriter.GetInstance(pdf, outputStream);
                    writer.CloseStream = false;

                    pdf.Open();

                    foreach (var bitmap in _bitmaps)
                    {
                        pdf.NewPage();

                        pdf.Add(CreateImage(bitmap));
                    }

                    pdf.Close();

                    SendPdfToClient(outputStream);
                }
            }
        }

        private Bitmap GetWebPageAsBitmap(WebBrowser browser)
        {
            var imageHeight = _dimensions.BrowserHeight * _dimensions.Zoom;
            var imageWidth = _dimensions.BrowserWidth * _dimensions.Zoom;

            var bitmap = new Bitmap(imageWidth, imageHeight);

            browser.DrawToBitmap(bitmap, new System.Drawing.Rectangle(0, 0, imageWidth, imageHeight));

            bitmap.Save($@"c:\users\bob\desktop\{Guid.NewGuid()}.bmp");

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

        private Image CreateImage(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Bmp);
                var image = Image.GetInstance(ms.ToArray());

                image.ScalePercent(100 / _dimensions.Zoom);

                return image;
            }
        }
    }
}