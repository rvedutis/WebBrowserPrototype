using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using novapiLib80;
using SHDocVw;
using WebBrowser = System.Windows.Forms.WebBrowser;

namespace ConsoleConverter
{
    public class Program
    {
        public static string PRINTER_NAME = "novaPDF SDK 8";
        public static string NOVAPDF_INFO_SUBJECT = "Document Subject";
        public static string PROFILE_NAME = "PestPacForms";
        public static int PROFILE_IS_PUBLIC = 1;

        public enum SaveFolder
        {
            SAVEFOLDER_APPLICATION = 1,
            SAVEFOLDER_LAST = 2,
            SAVEFOLDER_CUSTOM = 3,
            SAVEFOLDER_MYDOCUMENTS = 4
        }

        public enum SaveDlgType
        {
            PROMPT_SAVE_STNANDARD = 0,
            PROMPT_SAVE_NONE = 1,
            PROMPT_SAVE_SIMPLE = 2
        }

        public enum SaveFileConflictType
        {
            FILE_CONFLICT_STRATEGY_PROMPT = 0,
            FILE_CONFLICT_STRATEGY_AUTONUMBER_NEW = 1,
            FILE_CONFLICT_STRATEGY_APPEND_DATE = 2,
            FILE_CONFLICT_STRATEGY_OVERWRITE = 3,
            FILE_CONFLICT_STRATEGY_AUTONUMBER_EXIST = 4,
            FILE_CONFLICT_STRATEGY_APPEND = 5,
            FILE_CONFLICT_STRATEGY_INSERT_BEFORE = 6,
            FILE_CONFLICT_STRATEGY_DONTSAVE = 7
        }

        public class NovaOptions
        {
            public static int NOVAPDF_DOCINFO_SUBJECT = 68;
            public static int NOVAPDF_SAVE_FILE_NAME = 104;
            public static int NOVAPDF_SAVE_FOLDER = 103;
            public static int NOVAPDF_SAVE_PROMPT_TYPE = 101;
            public static int NOVAPDF_SAVE_FILEEXIST_ACTION = 108;
            public static int NOVAPDF_SAVE_FOLDER_TYPE = 260;
            public static int NOVAPDF_INFO_VIEWER_ENABLE = 8;
        }

        public class NovaErrors
        {
            public static long NV_NO_ACTIVE_PROFILE = 0xD5DA0028;
        }

        public static void Main(string[] args)
        {
            var thread = new Thread(delegate ()
            {
                using (var browser = new WebBrowser())
                {
                    browser.ScrollBarsEnabled = false;
                    browser.AllowNavigation = false;
                    browser.ScriptErrorsSuppressed = false;

                    browser.DocumentText = "<html><head></head><body>Hi There</body></html>";

                    browser.Width = 816;
                    browser.Height = 1056;

                    browser.DocumentCompleted += (sender, e) => RenderCompleted(sender, e);

                    while (browser.ReadyState != WebBrowserReadyState.Complete)
                    {
                        Application.DoEvents();
                    }

                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "WebBrowserPrint";
            thread.Start();
        }

        private static void RenderCompleted(object sender, WebBrowserDocumentCompletedEventArgs webBrowserDocumentCompletedEventArgs)
        {
            var browser = sender as WebBrowser;
            if (browser == null)
            {
                return;
            }

            var ie = (InternetExplorer) browser.ActiveXInstance;

            var documentId = Guid.NewGuid();

            // create the NovaPdfOptions object
            NovaPdfOptions80 pNova = new NovaPdfOptions80();

            // initialize the NovaPdfOptions object
            pNova.InitializeSilent(PRINTER_NAME, "");
            // get the active profile ...
            string activeProfile = "";

            pNova.GetActiveProfile(out activeProfile);

            pNova.LoadProfile(activeProfile);

            // and set some	options
            pNova.SetOptionString2(NovaOptions.NOVAPDF_DOCINFO_SUBJECT, "ASP.NET Hello document");
            pNova.SetOptionString(NovaOptions.NOVAPDF_SAVE_FILE_NAME, $"{documentId}");

            pNova.SetOptionLong(NovaOptions.NOVAPDF_SAVE_FOLDER_TYPE, (int)SaveFolder.SAVEFOLDER_CUSTOM);
            pNova.SetOptionString(NovaOptions.NOVAPDF_SAVE_FOLDER, @"c:\users\bob\desktop");

            pNova.SetOptionLong(NovaOptions.NOVAPDF_SAVE_FILEEXIST_ACTION, (int)SaveFileConflictType.FILE_CONFLICT_STRATEGY_OVERWRITE);
            pNova.SetOptionBool(NovaOptions.NOVAPDF_INFO_VIEWER_ENABLE, 0);
            pNova.SetOptionLong(NovaOptions.NOVAPDF_SAVE_PROMPT_TYPE, (int)SaveDlgType.PROMPT_SAVE_NONE);

            //save the new added profile
            pNova.SaveProfile();

            //set the new profile as the active profile
            pNova.SetActiveProfile(activeProfile);
            pNova.SetDefaultPrinter();

            ie.ExecWB(OLECMDID.OLECMDID_PRINT, OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER, 0);
            //browser.Print();
            
            File.Create($@"c:\Users\bob\Desktop\Done.txt");
        }
    }
}
