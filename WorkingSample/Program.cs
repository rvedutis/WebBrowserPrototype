using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ConsoleApplication9;
using novapiLib80;
using WebBrowser = System.Windows.Forms.WebBrowser;

namespace CSWebBrowserPrint
{
    public class Program
    {
        private static DirectoryInfo _root;

        private static void WB_OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                var wb = sender as WebBrowser;

                // create the NovaPdfOptions object
                NovaPdfOptions80 pNova = new NovaPdfOptions80();

                // initialize the NovaPdfOptions object
                pNova.InitializeSilent(NovaHelpers.PRINTER_NAME, "");
                // get the active profile ...
                string activeProfile = "";

                pNova.GetActiveProfile(out activeProfile);

                pNova.LoadProfile(activeProfile);

                // and set some	options
                pNova.SetOptionString2(NovaHelpers.NovaOptions.NOVAPDF_DOCINFO_SUBJECT, "ASP.NET Hello document");
                pNova.SetOptionString(NovaHelpers.NovaOptions.NOVAPDF_SAVE_FILE_NAME, $"output");

                pNova.SetOptionLong(NovaHelpers.NovaOptions.NOVAPDF_SAVE_FOLDER_TYPE, (int)NovaHelpers.SaveFolder.SAVEFOLDER_CUSTOM);
                pNova.SetOptionString(NovaHelpers.NovaOptions.NOVAPDF_SAVE_FOLDER, $@"{_root}");

                pNova.SetOptionLong(NovaHelpers.NovaOptions.NOVAPDF_SAVE_FILEEXIST_ACTION, (int)NovaHelpers.SaveFileConflictType.FILE_CONFLICT_STRATEGY_OVERWRITE);
                pNova.SetOptionBool(NovaHelpers.NovaOptions.NOVAPDF_INFO_VIEWER_ENABLE, 0);
                pNova.SetOptionLong(NovaHelpers.NovaOptions.NOVAPDF_SAVE_PROMPT_TYPE, (int)NovaHelpers.SaveDlgType.PROMPT_SAVE_NONE);

                //save the new added profile
                pNova.SaveProfile();

                //set the new profile as the active profile
                pNova.SetActiveProfile(activeProfile);
                pNova.SetDefaultPrinter();

                // Now perform the printing.
                wb.Print();

                //Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Environment.Exit(1);
            }

            /*
            var ie = (InternetExplorer)(wb.ActiveXInstance);

            wb.Dispose();

            ie.ExecWB(OLECMDID.OLECMDID_PRINT, OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER);

            ie.PrintTemplateTeardown += IE_OnPrintTemplateTeardown;
            */
        }

        private static void IE_OnPrintTemplateTeardown(object pdisp)
        {
            Environment.Exit(0);
        }

        private static void Main(string[] args)
        {
            _root = Directory.GetParent(Directory.GetParent(Directory.GetParent(Path.GetDirectoryName(Application.ExecutablePath)).ToString()).ToString());

            // The following code starts a new thread.
            var th = new Thread(() =>
            {
                var wb = new WebBrowser();
                wb.DocumentCompleted += WB_OnDocumentCompleted;

                var fileId = Guid.NewGuid();
                fileId = Guid.Parse("c3e3194b-af15-438a-afee-83ca6e2ce29e");

                //wb.Navigate($@"{_root}\input.html");

                wb.Navigate($@"C:\src\PDFConverter\{fileId}.html");

                Application.Run();
            });

            th.SetApartmentState(ApartmentState.STA);
            th.Name = "WebBrowserPrint";
            th.Start();
        }
    }
}