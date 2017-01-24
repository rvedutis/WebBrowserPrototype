using System;
using System.IO;
using System.ServiceProcess;
using PDFConverter.Handler;
using System.Xml;
using PDFConverter.Common;

namespace PDFConverter.Processor
{
    public partial class HtmlFileWatcher : ServiceBase
    {
        private string _inPath;
        private string _outPath;
        private FileSystemWatcher _fileWatcher;

        public HtmlFileWatcher()
        {
            _inPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"in\");
            _outPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"out\");

            CheckCreateFolder(_inPath);
            CheckCreateFolder(_outPath);

            _fileWatcher = new FileSystemWatcher();
            _fileWatcher.Path = _inPath;
            _fileWatcher.Created += new FileSystemEventHandler(OnFileAdded);

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _fileWatcher.EnableRaisingEvents = true;
        }

        protected override void OnStop()
        {
            _fileWatcher.EnableRaisingEvents = false;
        }

        private static void OnFileAdded(object source, FileSystemEventArgs e)
        {

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(e.FullPath);

            string markup = xmlDoc.DocumentElement.SelectSingleNode("markup").InnerText;

            Dimensions dimensions = new Dimensions
            {
                RenderWidth = int.Parse(xmlDoc.DocumentElement.SelectSingleNode("dimensions/renderWidth").InnerText),
                RenderHeight = int.Parse(xmlDoc.DocumentElement.SelectSingleNode("dimensions/renderHeight").InnerText),
                PageWidth = int.Parse(xmlDoc.DocumentElement.SelectSingleNode("dimensions/pageWidth").InnerText),
                PageHeight = int.Parse(xmlDoc.DocumentElement.SelectSingleNode("dimensions/pageHeight").InnerText),
                MarginLeft = Convert.ToSingle(xmlDoc.DocumentElement.SelectSingleNode("dimensions/marginLeft").InnerText),
                MarginTop = Convert.ToSingle(xmlDoc.DocumentElement.SelectSingleNode("dimensions/marginTop").InnerText),
                MarginRight = Convert.ToSingle(xmlDoc.DocumentElement.SelectSingleNode("dimensions/marginRight").InnerText),
                MarginBottom = Convert.ToSingle(xmlDoc.DocumentElement.SelectSingleNode("dimensions/marginBottom").InnerText),
                Zoom = int.Parse(xmlDoc.DocumentElement.SelectSingleNode("dimensions/zoom").InnerText)
            };
            
            HtmlToPdf.Convert(markup, Path.GetFileNameWithoutExtension(e.FullPath), dimensions);

            File.Delete(e.FullPath);
        }

        private void CheckCreateFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
