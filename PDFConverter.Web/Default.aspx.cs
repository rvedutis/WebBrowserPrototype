﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Threading.Tasks;

namespace PDFConverter.Web
{
    public partial class _Default : Page
    {
        private readonly List<Guid> images = new List<Guid>();

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

            var pages = content.Split(new[] { "#####NEWPAGE#####" }, StringSplitOptions.None);

            var pdfs = pages.Select((t, i) => new Pdf
            {
                Index = i,
                Markup = t,
                Bytes = null
            }).ToList();

            Parallel.ForEach(pdfs, (pdf) =>
            {
                PDFConverter.Convert(ref pdf, dimensions);
            });

            SendPdfToClient(PDFConverter.Combine(pdfs.OrderBy(p => p.Index).ToList()));
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
    }
}