using System;
using System.IO;
using System.Threading;
using System.Web.UI;
using PDFConverter.Common;

namespace PDFConverter.Web
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var content = Server.UrlDecode(Request.Form["RenderedContent"]);

            if (content == null)
            {
                RespondWithFailure("Report Data Missing");
            }

            var printRequest = new PrintRequest
            {
                PageWidth = int.Parse(Request.Form["PageWidth"]),
                PageHeight = int.Parse(Request.Form["PageHeight"]),
                MarginLeft = Convert.ToSingle(Request.Form["MarginLeft"]),
                MarginTop = Convert.ToSingle(Request.Form["MarginTop"]),
                MarginRight = Convert.ToSingle(Request.Form["MarginRight"]),
                MarginBottom = Convert.ToSingle(Request.Form["MarginBottom"]),
                FilePath = @"C:\src\PDFConverter\",
                FileName = Guid.NewGuid(),
                Content = content
            };

            try
            {
                PdfConverter.Convert(printRequest);
            }
            catch (Exception ex)
            {
                RespondWithFailure($"Error {ex.InnerException}");
            }

            EnsureFileExists(printRequest);

            SendPdfToClient(printRequest);
        }

        private void RespondWithFailure(string data)
        {
            Response.Write($"Error {data}");
            Response.Flush();
            Response.End();
        }

        private void EnsureFileExists(PrintRequest printRequest)
        {
            var fileInfo = new FileInfo($@"{printRequest.FilePath}{printRequest.FileName}.pdf");

            //TODO: Safeguard this
            while (!IsFileWrittenTo(fileInfo))
            {
                Thread.Sleep(100);
            }
        }

        private void SendPdfToClient(PrintRequest printRequest)
        {
            var stream = new MemoryStream(File.ReadAllBytes($@"{printRequest.FilePath}{printRequest.FileName}.pdf"));

            //TODO: Save to SQL / AWS / Glacier First?
            File.Delete($@"{printRequest.FilePath}{printRequest.FileName}.pdf");

            Response.Buffer = true;
            Response.ClearHeaders();
            Response.ClearContent();
            Response.ContentType = "application/pdf";
            Response.AppendHeader("Content-Disposition", "inline; filename=Your Report.pdf");
            Response.AppendHeader("Content-Transfer-Encoding", "binary");
            Response.AppendHeader("Content-Length", stream.Length.ToString());
            Response.BinaryWrite(stream.ToArray());
            Response.Flush();
            Response.End();
        }

        private static bool IsFileWrittenTo(FileInfo file)
        {
            if (!File.Exists(file.FullName))
            {
                return false;
            }

            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return false;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            //file is not locked
            return true;
        }
    }
}