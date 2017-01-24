using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.UI;
using PDFConverter.Common;

namespace PDFConverter.Web
{
    public partial class _Default : Page
    {
        private Guid _fileId = Guid.NewGuid();

        protected void Page_Load(object sender, EventArgs e)
        {
            var content = Server.UrlDecode(Request.Form["RenderedContent"]);

            if (content == null)
            {
                return;
            }

            var dimensions = new Dimensions
            {
                PageWidth = int.Parse(Request.Form["PageWidth"]),
                PageHeight = int.Parse(Request.Form["PageHeight"]),
                MarginLeft = Convert.ToSingle(Request.Form["MarginLeft"]),
                MarginTop = Convert.ToSingle(Request.Form["MarginTop"]),
                MarginRight = Convert.ToSingle(Request.Form["MarginRight"]),
                MarginBottom = Convert.ToSingle(Request.Form["MarginBottom"])
            };


            File.WriteAllText($@"C:\src\PDFConverter\{_fileId}.html", content);

            var process = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = @"C:\src\PDFConverter\WorkingSample\bin\Debug\",
                    FileName = @"C:\src\PDFConverter\WorkingSample\bin\Debug\WorkingSample.exe",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = false,
                    Arguments = $"--fileName {_fileId}"
                }
            };

            process.Start();

            process.WaitForExit();

            var exitCode = process.ExitCode;

            if (exitCode == 0)
            {
                var stream = new MemoryStream(File.ReadAllBytes($@"C:\src\PDFConverter\{_fileId}.pdf"));
                SendPdfToClient(stream);

            }
            else
            {
                Response.Write("Error");
                Response.Flush();
                Response.End();
            }
        }

        private void SendPdfToClient(MemoryStream outputStream)
        {
            Response.Buffer = true;
            Response.ClearHeaders();
            Response.ClearContent();
            Response.ContentType = "application/pdf";
            Response.AppendHeader("Content-Disposition", "inline; filename=Your Report.pdf");
            Response.AppendHeader("Content-Transfer-Encoding", "binary");
            Response.AppendHeader("Content-Length", outputStream.Length.ToString());
            Response.BinaryWrite(outputStream.ToArray());
            Response.Flush();
            Response.End();
        }

    }
}