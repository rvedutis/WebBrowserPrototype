﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace PDFConverter.Web
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

            var pages = content.Split(new[] {"#####NEWPAGE#####"}, StringSplitOptions.None);

            // START OF NEW CODE //
            var pdfs = new List<byte[]>();
            foreach (var page in pages.Take(1))
            {
                var pdf = PDFConverter.Convert(page, dimensions);
                pdfs.Add(pdf);
            }

            var testpdf = pdfs.First();

            SendPdfToClient(testpdf);

            /*
            using (var outputStream = new MemoryStream())
            {
                var writer = PdfWriter.GetInstance(testpdf, outputStream);
                writer.CloseStream = false;
                SendPdfToClient(outputStream);
            }*/
        }

        private void SendPdfToClient(byte[] pdf)
        {
            Response.Buffer = true;
            Response.ClearHeaders();
            Response.ClearContent();
            Response.ContentType = "application/pdf";
            Response.AppendHeader("Content-Disposition", "inline; filename=Report.pdf");
            Response.AppendHeader("Content-Transfer-Encoding", "binary");
            Response.AppendHeader("Content-Length", pdf.Length.ToString());
            Response.BinaryWrite(pdf);
            Response.Flush();
            Response.End();
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