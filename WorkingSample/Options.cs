﻿using System;

namespace CSWebBrowserPrint
{
    public class Options
    {
        public Options()
        {
            FilePath = string.Empty;
            FileName = Guid.Empty;
        }

        public string FilePath { get; set; }
        public Guid FileName { get; set; }
    }
}