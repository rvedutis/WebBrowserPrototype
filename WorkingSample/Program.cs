﻿using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using novapiLib80;
using SHDocVw;
using WebBrowser = System.Windows.Forms.WebBrowser;

namespace WorkingSample
{
    public class Program
    {
        private static DirectoryInfo _root;
        private static Options _options = new Options();

        private static void Main(string[] args)
        {
            _options = ArgumentsHelper.ParseArguments(args);

            _root =
                Directory.GetParent(
                    Directory.GetParent(
                        Directory.GetParent(Path.GetDirectoryName(Application.ExecutablePath)).ToString()).ToString());

            // The following code starts a new thread.
            var th = new Thread(() =>
            {
                var wb = new WebBrowser();
                wb.DocumentCompleted += WB_OnDocumentCompleted;

                var fileId = _options.FileName;

                //wb.Navigate($@"{_root}\input.html");

                wb.Navigate($@"{_options.FilePath}{fileId}.html");

                Application.Run();
            });

            th.SetApartmentState(ApartmentState.STA);
            th.Name = "WebBrowserPrint";
            th.Start();
        }

        private static void WB_OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                File.Delete($@"{_options.FilePath}{_options.FileName}.html");

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
                pNova.SetOptionString(NovaHelpers.NovaOptions.NOVAPDF_SAVE_FILE_NAME, _options.FileName.ToString());

                pNova.SetOptionLong(NovaHelpers.NovaOptions.NOVAPDF_SAVE_FOLDER_TYPE,
                    (int)NovaHelpers.SaveFolder.SAVEFOLDER_CUSTOM);
                pNova.SetOptionString(NovaHelpers.NovaOptions.NOVAPDF_SAVE_FOLDER, $@"{_root}");

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
                var ie = (InternetExplorer)wb.ActiveXInstance;

                ie.ExecWB(OLECMDID.OLECMDID_PRINT, OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER);

                ie.PrintTemplateTeardown += pdisp => IE_OnPrintTemplateTeardown(pdisp, wb);
            }
            catch (Exception ex)
            {
                Environment.Exit(1);
            }
        }

        private static bool IsFileWrittenTo(FileInfo file)
        {
            if (!File.Exists(file.FullName))
            {
                return false;
            }

            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return false;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            //file is not locked
            return true;
        }

        private static void IE_OnPrintTemplateTeardown(object pdisp, WebBrowser wb)
        {
            var fileInfo = new FileInfo($@"{_options.FilePath}{_options.FileName}.pdf");

            while (!IsFileWrittenTo(fileInfo))
            {
                Thread.Sleep(100);
            }

            wb.Dispose();

            Environment.Exit(0);
        }
    }
}