using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkingSample
{
    public static class ArgumentsHelper
    {
        public static Options ParseArguments(string[] args)
        {
            var options = new Options();

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--fileName":
                        options.FileName = Guid.Parse(args[i + 1]);
                        break;
                    case "--filePath":
                        options.FilePath = args[i + 1];
                        break;
                }
            }
            return options;
        }
    }
}
