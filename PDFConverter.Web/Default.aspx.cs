using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            var fileId = Guid.NewGuid();
            fileId = Guid.Parse("c3e3194b-af15-438a-afee-83ca6e2ce29e");

            File.WriteAllText($@"C:\src\PDFConverter\{fileId}.html", content);

            var process = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = @"C:\src\PDFConverter\WorkingSample\bin\Debug\",
                    FileName = @"C:\src\PDFConverter\WorkingSample\bin\Debug\WorkingSample.exe",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = false
                }
            };

            process.Start();

            process.WaitForExit();

            var exitCode = process.ExitCode;

            Debugger.Break();

        }
    }
}