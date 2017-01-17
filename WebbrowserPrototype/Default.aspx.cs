using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Web;
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
        private readonly List<Guid> images = new List<Guid>();
        private int _totalPages;

        protected void Page_Load(object sender, EventArgs e)
        {
            var content = Server.UrlDecode(Request.Form["RenderedContent"]);

            if (content == null)
            {
                return;
            }

            var dimensions = new Dimensions
            {
                RenderWidth = int.Parse(Request.Form["RenderWidth"]),
                RenderHeight = int.Parse(Request.Form["RenderHeight"]),
                PageWidth = int.Parse(Request.Form["PageWidth"]),
                PageHeight = int.Parse(Request.Form["PageHeight"]),
                MarginLeft = Convert.ToSingle(Request.Form["MarginLeft"]),
                MarginTop = Convert.ToSingle(Request.Form["MarginTop"]),
                MarginRight = Convert.ToSingle(Request.Form["MarginRight"]),
                MarginBottom = Convert.ToSingle(Request.Form["MarginBottom"]),
                Zoom = int.Parse(Request.Form["Zoom"])
            };

            var splitPages = content.Split(new[] {"#####NEWPAGE#####"}, StringSplitOptions.None);

            var instanceId = ConfigureInstance();

            _totalPages = splitPages.Length;

            Render(splitPages, dimensions, instanceId);
        }

        private Guid ConfigureInstance()
        {
            var instanceId = Guid.NewGuid();

            // instanceId = Guid.Parse("4b4fd2d8-6704-44c4-9532-cc04f1122de3");

            Directory.CreateDirectory(GetInstanceFilePath(instanceId));

            return instanceId;
        }

        private static string GetInstanceFilePath(Guid instanceId)
        {
            return $@"{HttpRuntime.AppDomainAppPath}\artifacts\{instanceId}\";
        }

        private void Render(string[] pages, Dimensions dimensions, Guid instanceId)
        {
            var thread = new Thread(delegate()
            {
                foreach (var page in pages)
                {
                    using (var browser = new WebBrowser())
                    {
                        browser.ScrollBarsEnabled = false;
                        browser.AllowNavigation = false;
                        browser.ScriptErrorsSuppressed = false;

                        browser.DocumentText = page;

                        browser.Width = dimensions.RenderWidth*dimensions.Zoom;
                        browser.Height = dimensions.RenderHeight*dimensions.Zoom;

                        browser.DocumentCompleted += (sender, e) => RenderCompleted(sender, e, dimensions, instanceId);

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

        private void RenderCompleted(object sender, WebBrowserDocumentCompletedEventArgs e, Dimensions dimensions,
            Guid instanceId)
        {
            var browser = sender as WebBrowser;
            if (browser == null)
            {
                return;
            }

            SaveWebPageAsImage(browser, instanceId);

            if (!_totalPages.Equals(images.Count))
            {
                return;
            }

            using (var pdf = new Document())
            {
                pdf.SetMargins(dimensions.MarginLeft, dimensions.MarginTop, dimensions.MarginRight,
                    dimensions.MarginBottom);
                pdf.SetPageSize(new Rectangle(dimensions.PageWidth, dimensions.PageHeight));

                using (var outputStream = new MemoryStream())
                {
                    var writer = PdfWriter.GetInstance(pdf, outputStream);
                    writer.CloseStream = false;

                    pdf.Open();

                    foreach (var image in images)
                    {
                        pdf.NewPage();

                        pdf.Add(CreateImage(image, instanceId, dimensions));
                    }

                    pdf.Close();

                    CleanUpArtifacts(instanceId);

                    SendPdfToClient(outputStream);
                }
            }
        }

        private void CleanUpArtifacts(Guid instanceId)
        {
            Directory.Delete($@"{GetInstanceFilePath(instanceId)}", true);
        }

        private void SaveWebPageAsImage(WebBrowser browser, Guid instanceId)
        {
            var rectangle = new System.Drawing.Rectangle(0, 0, browser.Width, browser.Height);

            using (var bitmap = new Bitmap(browser.Width, browser.Height))
            {
                browser.DrawToBitmap(bitmap, rectangle);

                var imageId = Guid.NewGuid();
                images.Add(imageId);

                bitmap.Save($@"{GetInstanceFilePath(instanceId)}{imageId}.bmp");
            }
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

        private static Image CreateImage(Guid imageId, Guid instanceId, Dimensions dimensions)
        {
            var image = Image.GetInstance($@"{GetInstanceFilePath(instanceId)}{imageId}.bmp");

            image.ScalePercent(100/dimensions.Zoom);

            return image;
        }
    }

    public class Dimensions
    {
        public int RenderWidth { get; set; }
        public int RenderHeight { get; set; }
        public int PageWidth { get; set; }
        public int PageHeight { get; set; }

        public float MarginLeft { get; set; }
        public float MarginTop { get; set; }
        public float MarginRight { get; set; }
        public float MarginBottom { get; set; }

        public int Zoom { get; set; }
    }
}