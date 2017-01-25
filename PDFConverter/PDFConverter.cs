using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using novapiLib80;
using PDFConverter.Common;
using WebBrowser = System.Windows.Forms.WebBrowser;

namespace PDFConverter
{
    public static class PdfConverter
    {
        public static void Convert(PrintRequest printRequest)
        {
            var th = new Thread(() =>
            {
                var wb = new WebBrowser();
                wb.DocumentCompleted += (sender, e) => WB_OnDocumentCompleted(sender, e, printRequest);
                wb.DocumentText = printRequest.Content;

                Application.Run();
            });

            th.SetApartmentState(ApartmentState.STA);
            th.Name = "WebBrowserPrint";
            th.Start();
        }

        private static void WB_OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e, PrintRequest printRequest)
        {
            var wb = sender as WebBrowser;

            // create the NovaPdfOptions object
            var pNova = new NovaPdfOptions80();

            // initialize the NovaPdfOptions object
            pNova.InitializeSilent(NovaHelpers.PRINTER_NAME, "");
            // get the active profile ...
            var activeProfile = "";

            pNova.GetActiveProfile(out activeProfile);

            pNova.LoadProfile(activeProfile);

            // and set some	options
            pNova.SetOptionString2(NovaHelpers.NovaOptions.NOVAPDF_DOCINFO_SUBJECT, "ASP.NET Hello document");
            pNova.SetOptionString(NovaHelpers.NovaOptions.NOVAPDF_SAVE_FILE_NAME, printRequest.FileName.ToString());

            pNova.SetOptionLong(NovaHelpers.NovaOptions.NOVAPDF_SAVE_FOLDER_TYPE,
                (int)NovaHelpers.SaveFolder.SAVEFOLDER_CUSTOM);
            pNova.SetOptionString(NovaHelpers.NovaOptions.NOVAPDF_SAVE_FOLDER, $@"{printRequest.FilePath}");

            pNova.SetOptionLong(NovaHelpers.NovaOptions.NOVAPDF_SAVE_FILEEXIST_ACTION,
                (int)NovaHelpers.SaveFileConflictType.FILE_CONFLICT_STRATEGY_OVERWRITE);
            pNova.SetOptionBool(NovaHelpers.NovaOptions.NOVAPDF_INFO_VIEWER_ENABLE, 0);
            pNova.SetOptionLong(NovaHelpers.NovaOptions.NOVAPDF_SAVE_PROMPT_TYPE,
                (int)NovaHelpers.SaveDlgType.PROMPT_SAVE_NONE);

            //save the new added profile
            pNova.SaveProfile();

            //set the new profile as the active profile
            pNova.SetActiveProfile(activeProfile);
            pNova.SetDefaultPrinter();

            // Now perform the printing.
            wb.Print();
        }
    }
}