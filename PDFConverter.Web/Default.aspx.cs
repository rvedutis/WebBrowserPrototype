using System;
using System.Web.UI;
using System.Xml;
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

            foreach (var page in pages)
            {
                var fileName = Guid.NewGuid();
                var xmlDoc = new XmlDocument();
                var root = xmlDoc.CreateElement("page");
                var pageDimensions = xmlDoc.CreateElement("dimensions");
                var pageMarkup = xmlDoc.CreateElement("markup");

                pageMarkup.InnerText = page;

                var renderWidth = xmlDoc.CreateElement("renderWidth");
                renderWidth.InnerText = dimensions.RenderWidth.ToString();
                pageDimensions.AppendChild(renderWidth);

                var renderHeight = xmlDoc.CreateElement("renderHeight");
                renderHeight.InnerText = dimensions.RenderHeight.ToString();
                pageDimensions.AppendChild(renderHeight);

                var pageWidth = xmlDoc.CreateElement("pageWidth");
                pageWidth.InnerText = dimensions.PageWidth.ToString();
                pageDimensions.AppendChild(pageWidth);

                var pageHeight = xmlDoc.CreateElement("pageHeight");
                pageHeight.InnerText = dimensions.PageHeight.ToString();
                pageDimensions.AppendChild(pageHeight);

                var marginLeft = xmlDoc.CreateElement("marginLeft");
                marginLeft.InnerText = dimensions.MarginLeft.ToString();
                pageDimensions.AppendChild(marginLeft);

                var marginTop = xmlDoc.CreateElement("marginTop");
                marginTop.InnerText = dimensions.MarginTop.ToString();
                pageDimensions.AppendChild(marginTop);

                var marginRight = xmlDoc.CreateElement("marginRight");
                marginRight.InnerText = dimensions.MarginRight.ToString();
                pageDimensions.AppendChild(marginRight);

                var marginBottom = xmlDoc.CreateElement("marginBottom");
                marginBottom.InnerText = dimensions.MarginBottom.ToString();
                pageDimensions.AppendChild(marginBottom);

                var zoom = xmlDoc.CreateElement("zoom");
                zoom.InnerText = dimensions.Zoom.ToString();
                pageDimensions.AppendChild(zoom);

                root.AppendChild(pageMarkup);
                root.AppendChild(pageDimensions);
                xmlDoc.AppendChild(root);

                // TO DO: config switch
                xmlDoc.Save(@"C:\WebBrowserPrototype\PDFConverter.Processor\bin\Debug\in\" + fileName.ToString() + ".xml");
            }
        }
    }
}