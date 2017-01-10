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
using Rectangle = System.Drawing.Rectangle;

namespace WebbrowserPrototype
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            GrabMarkup();
        }

        private void GrabMarkup()
        {
            var thread = new Thread(delegate ()
            {
                using (var browser = new WebBrowser())
                {
                    browser.ScrollBarsEnabled = false;
                    browser.AllowNavigation = false;
                    browser.Navigate("http://127.0.0.1/test.asp");
                    browser.Width = 768;
                    browser.Height = 1056;
                    browser.DocumentCompleted += DocCompleted;

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

        private void DocCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var browser = sender as WebBrowser;
            if (browser == null)
            {
                return;
            }

            var scrollHeight = browser.Document.Body.ScrollRectangle.Height;
            var pageHeight = 984;

            browser.Height = scrollHeight;

            using (var bitmap = new Bitmap(browser.Width, scrollHeight))
            {
                browser.DrawToBitmap(bitmap, new Rectangle(0, 0, browser.Width, scrollHeight));

                using (var doc = new Document())
                {
                    using (var outputStream = new MemoryStream())
                    {
                        var writer = PdfWriter.GetInstance(doc, outputStream);
                        writer.CloseStream = false;

                        doc.Open();

                        for (var page = 0; page < scrollHeight / pageHeight + 1; page++)
                        {
                            using (var image = new ImageFactory())
                            {
                                using (var imageStream = new MemoryStream())
                                {
                                    image.Quality(100);
                                    image.Load(bitmap);
                                    image.Crop(new Rectangle(0, page * pageHeight, browser.Width, pageHeight));

                                    image.Save(imageStream);

                                    var img = Image.GetInstance(System.Drawing.Image.FromStream(imageStream), ImageFormat.Bmp);

                                    doc.SetPageSize(new iTextSharp.text.Rectangle(816, 1056));

                                    doc.NewPage();

                                    doc.Add(img);
                                }
                            }
                        }

                        doc.Close();

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
        }
    }
}