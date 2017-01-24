using System;
using System.IO;
using System.ServiceProcess;

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

        }

        private void CheckCreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
