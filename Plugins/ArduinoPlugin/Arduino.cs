using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace Ultrabox.ChromaSync.Plugin
{
    internal class CSPluginAttribute : Attribute
    {
        private string Name;
        private string Description;
        private int Version;

        public CSPluginAttribute(string name, string desc, int v)
        {
            Name = name;
            Description = desc;
            Version = v;
        }
    }

    // Name, Description, Version
    [CSPlugin("Chroma for Arduino", "This is a description", 20160301)]
    public class Arduino
    {
        public class Serial
        {

            private static SerialPort SPort = new SerialPort();
            private static Timer TimeoutTimer = new Timer();

            private class Defaults
            {
                internal static string Default = "COM1";
                internal static int BaudRate = 115200; // Super fast refresh rate
                internal static int Timeout = 5000;
            }

            // automatically close the connection
            private static void RestartTimer(int timeout = 5000)
            {
                if (TimeoutTimer.Enabled)
                {
                    TimeoutTimer.Stop();
                    // Close the serial 
                    Close();
                }

                TimeoutTimer.Tick += ArduinoTimeout;
                TimeoutTimer.Interval = 5000;

                TimeoutTimer.Start();
            }

            private static void ArduinoTimeout(object sender, EventArgs e)
            {
                Close();
                TimeoutTimer.Stop();
            }

            public static bool Send(string message, int baud = 0, string port = null)
            {
                RestartTimer();

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



            public static bool Close()
            {
                try
                {
                    SPort.Close();
                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                return false;
            }

        }

        public class WIFI
        {
            // Not implemented yet
        }
    }
}
