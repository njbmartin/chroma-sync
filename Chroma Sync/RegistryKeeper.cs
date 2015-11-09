using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChromaSync
{
    class RegistryKeeper
    {
        public static void CreateSettings()
        {
            RegistryKey ProgSettings = Registry.CurrentUser.OpenSubKey("Software", true);
            ProgSettings.CreateSubKey("ChromaSync");
            ProgSettings.Close();
        }

        public static bool CheckActive(string s)
        {
            CreateSettings();
            RegistryKey ProgSettings = Registry.CurrentUser.OpenSubKey("Software\\ChromaSync", true);
            var settings = ProgSettings.GetValue(s, false).ToString(); // retrieve settings    
            ProgSettings.Close();
            return Boolean.Parse(settings);
        }


        public static void UpdateReg(string s, bool t)
        {
            CreateSettings();
            RegistryKey ProgSettings = Registry.CurrentUser.OpenSubKey("Software\\ChromaSync", true);
            ProgSettings.SetValue(s, t); // retrieve settings    
            ProgSettings.Close();
        }

    }
}
