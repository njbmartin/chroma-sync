using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultrabox.ChromaSync.Helpers
{
    class Paths
    {

        public static string MainDirectory
        {
            get
            {
                string path = @"%appdata%\ChromaSync";
                path = Environment.ExpandEnvironmentVariables(path);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path;
            }
        }

        public static string Packages
        {
            get
            {
                return Combine("packages");
            }
        }

        public static string Apps
        {
            get
            {
                return Combine("apps");
            }
        }

        public static string Profiles
        {
            get
            {
                return Combine("profiles");
            }
        }


        public static string Scripts
        {
            get
            {
                return Combine("scripts");
            }
        }



        private static string Combine(string p)
        {
            string path = Path.Combine(MainDirectory, p);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }


    }
}
