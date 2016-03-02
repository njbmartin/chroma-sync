using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ultrabox.ChromaSync.Plugin
{
    public class Arduino
    {
        private static SerialPort SPort = new SerialPort();
        public enum Type {
            COM = 0,
            WIFI
        }

        private class Defaults {
            internal static string Default = "COM5";
            internal static int BaudRate = 115200;
        }

        public static Boolean Send(Type type, string message, int baud = 0, string port = null)
        {
            if (!SPort.IsOpen)
            {
                SPort.BaudRate = (baud > 0 ? baud : Defaults.BaudRate);
                SPort.PortName = (port != null ? port : Defaults.Default);

                try
                {
                    SPort.Open();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return false;
                }
            }

            SPort.WriteLine(message);             
            return false;
        }

        public static Boolean Close()
        {
            try
            {
                SPort.Close();
            }catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return false;
        }
    }
}
