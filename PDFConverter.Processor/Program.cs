using System.ServiceProcess;

namespace PDFConverter.Processor
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;

            ServicesToRun = new ServiceBase[]
            {
                new HtmlFileWatcher()
            };

            ServiceBase.Run(ServicesToRun);
        }
    }
}
