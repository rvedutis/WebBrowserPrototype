namespace PDFConverter
{
    public static class NovaHelpers
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
    }
}
