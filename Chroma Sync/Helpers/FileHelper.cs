using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultrabox.ChromaSync.Helpers
{
    class FileHelper
    {
        public static bool Exists(string file)
        {
            try
            {
                return File.Exists(file);
            }
            catch (Exception e)
            {
                App.Log.Error(e);
            }

            return false;
        }
    }
}
