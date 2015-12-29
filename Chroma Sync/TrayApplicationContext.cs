using System;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;

using Newtonsoft.Json.Linq;
using Corale.Colore.Core;
using Corale.Colore.Razer;
using System.Threading;
using System.Diagnostics;
using ChromaSync.Properties;
using System.IO;
using Neo.IronLua;
using System.Collections.Generic;
using System.Reflection;

namespace ChromaSyncOld
{


    public class TrayApplicationContext : ApplicationContext
    {
        private static NotifyIcon _icon;

        private Form1 _mainWindow;
        private Thread _serverThread;
        private Thread _clientThread;
        private Thread _luaThread;
        private UserControl1 _main;

        //Program configWindow = new Program();
        public TrayApplicationContext()
        {
            System.Windows.Window window = new System.Windows.Window
            {
                Title = "My User Control Dialog",
                Content = new UserControl1(),
                AllowsTransparency = true,
                WindowStyle = System.Windows.WindowStyle.None
            };

            window.Show();

            // Onboarding
            if (!Settings.Default.FirstRun)
            {
                //Settings.Default.FirstRun = true;
                //_mainWindow.Show();
                //Settings.Default.Save();
            }


            Application.ApplicationExit += Application_ApplicationExit;
            MenuItem about = new MenuItem("Visit website", showAbout);
            MenuItem updates = new MenuItem("Check for updates...", ShowConfig);
            MenuItem openScripts = new MenuItem("Scripts Folder", BrowseScripts);
            MenuItem packages = new MenuItem("Packages Folder", BrowsePackages);
            MenuItem reload = new MenuItem("Reload Scripts", ReloadScripts);
            MenuItem exitMenuItem = new MenuItem("Exit", Exit);
            var cm = new ContextMenu();
            cm.MenuItems.Add(about);
            cm.MenuItems.Add(reload);
            cm.MenuItems.Add(packages);
            cm.MenuItems.Add(openScripts);
            cm.MenuItems.Add(exitMenuItem);

            _icon = new NotifyIcon
            {
                Icon = Resources.favicon,
                ContextMenu = cm,
                Visible = true,
                Text = Resources.ExitMenuText,
            };

            PluginManager.EnablePlugins();

            _serverThread = new Thread(RunServer);
            _serverThread.Start();

            _luaThread = new Thread(LuaScripting.LuaThread);
            _luaThread.Start();

            _mainWindow = new Form1();

            


        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            LuaScripting.CloseScripts();
            Exit();
        }

        void showAbout(object sender, EventArgs e)
        {
            Process.Start("http://chromasync.io/");
        }

        /*
        public static class GTA
        {
            private static double _currentAmmo, _wantedLevel;

            public static void Task(object folder)
            {
                string f = (string)folder;
                Debug.WriteLine(f);
                FileSystemWatcher watcher = new FileSystemWatcher();

                watcher.Path = f;
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Only watch text files.
                watcher.Filter = "chroma_sync_*";

                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnChanged);
                watcher.EnableRaisingEvents = true;
            }




            private static void OnChanged(object source, FileSystemEventArgs e)
            {
                // Specify what is done when a file is changed, created, or deleted.
                string readText = "";
                try
                {
                    readText = File.ReadAllText(e.FullPath);
                }
                catch (Exception error)
                {
                    return;
                }

                if (e.Name.Contains("ammo"))
                {
                    int ammo = int.Parse(readText);

                    UpdateAmmo(ammo);
                }

                if (e.Name.Contains("police"))
                {
                    _wantedLevel = double.Parse(readText);
                    Debug.WriteLine(_wantedLevel);
                }
            }

            public static void UpdateAmmo(int ammo)
            {
                var keys = Math.Round((double)ammo / 100 * 12);
                if (keys != _currentAmmo)
                {
                    for (uint i = 1; i <= 12; i++)
                    {
                        try
                        {
                            Keyboard.Instance.SetPosition(0, 2 + i, keys >= i ? Color.Red : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Escape, Color.White);
                        }
                        catch (Exception e) { }
                    }
                }
                _currentAmmo = keys;
            }


            public static void PoliceAnimation()
            {
                int currentLevel = 0;
                while (true)
                {
                    if (currentLevel != _wantedLevel)
                    {
                        double wLevel = _wantedLevel;
                        try
                        {
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro1, wLevel > 0 ? Color.Red : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro2, wLevel > 1 ? Color.Red : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro3, wLevel > 2 ? Color.Red : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro4, wLevel > 3 ? Color.Red : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro5, wLevel > 4 ? Color.Red : Color.Black);
                            Thread.Sleep(200);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro1, wLevel > 0 ? Color.Blue : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro2, wLevel > 1 ? Color.Blue : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro3, wLevel > 2 ? Color.Blue : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro4, wLevel > 3 ? Color.Blue : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro5, wLevel > 4 ? Color.Blue : Color.Black);
                        }
                        catch (Exception e) { }
                    }
                    Thread.Sleep(200);
                }
            }
        }
        */

        private void CopyResource(Stream stream, string file)
        {
            using (stream)
            {
                if (stream == null)
                {
                    throw new ArgumentException("No such resource", "resourceName");
                }
                using (Stream output = File.OpenWrite(file))
                {
                    stream.CopyTo(output);
                }
            }
        }

        public static void BalloonTip(string title, string text, int timeout = 200)
        {
            _icon.BalloonTipTitle = title;
            _icon.BalloonTipText = text;
            _icon.ShowBalloonTip(timeout);
        }

        Color RGBAFix(Color color)
        {
            var a = color.A;

            return new Color(
                (1 - a) * 0 - a * color.R,
                (1 - a) * 0 - a * color.G,
                (1 - a) * 0 - a * color.B
            );
        }

        void ShowConfig(object sender, EventArgs e)
        {
            if (_mainWindow.IsDisposed)
            {
                _mainWindow = new Form1();
            }
            // If we are already showing the window, merely focus it.
            if (_mainWindow.Visible)
            {
                _mainWindow.Activate();
            }
            else
            {
                _mainWindow.ShowDialog();
            }

        }


        void ReloadScripts(object sender, EventArgs e)
        {
            LuaScripting.ReloadScripts();
        }

        void BrowseScripts(object sender, EventArgs e)
        {
            string path = @"%appdata%\ChromaSync";
            path = Environment.ExpandEnvironmentVariables(path);

            path = Path.Combine(path, "scripts");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            Process.Start("explorer.exe", path);
        }

        void BrowsePackages(object sender, EventArgs e)
        {
            string path = @"%appdata%\ChromaSync";
            path = Environment.ExpandEnvironmentVariables(path);

            path = Path.Combine(path, "packages");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            Process.Start("explorer.exe", path);
        }

        void Exit(object sender, EventArgs e)
        {
            Exit();
        }

        private void Exit()
        {
            // We must manually tidy up and remove the icon before we exit.
            // Otherwise it will be left behind until the user mouses over.
            _icon.Visible = false;

            // Abort all threads
            if (_serverThread != null && _serverThread.IsAlive)
                _serverThread.Abort();
            if (_luaThread != null && _luaThread.IsAlive)
                _luaThread.Abort();
            //_gtaThread.Abort();
            if (_clientThread != null && _clientThread.IsAlive)
                _clientThread.Abort();

            LuaScripting.CloseScripts();
            Application.Exit();
        }

        private void RunServer()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                const int port = 13000;

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(IPAddress.Any, port);
                //BalloonTip("Server running", IPAddress.Any.ToString());
                // Start listening for client requests.
                server.Start();

                while (true)
                {
                    if (server.Pending())
                    {
                        _clientThread = new Thread(() =>
                        {
                            using (TcpClient client = server.AcceptTcpClient())
                            {
                                ParseData(client);
                            }
                        });
                        _clientThread.Start();
                    }
                    Thread.Sleep(10);
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void ParseData(TcpClient listener)
        {

            // Buffer for reading data
            Byte[] bytes = new Byte[256];

            try
            {
                // Enter the listening loop. 
                Debug.Write("Got a connection... ");

                // Perform a blocking call to accept requests. 
                // You could also user server.AcceptSocket() here.
                Debug.WriteLine("Connected!");


                // Get a stream object for reading and writing
                NetworkStream stream = listener.GetStream();


                byte[] myReadBuffer = new byte[1024];
                StringBuilder myCompleteMessage = new StringBuilder();
                int numberOfBytesRead = 0;

                // Incoming message may be larger than the buffer size. 
                do
                {
                    numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);

                    myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

                }
                while (stream.DataAvailable);
                if (myCompleteMessage.Length != 0)
                {

                    // Print out the received message to the console.
                    try
                    {
                        var split = myCompleteMessage.ToString().Split(new char[] { '{' }, 2);

                        if (split.Length > 1)
                        {

                            String ns = "{" + myCompleteMessage.ToString().Split(new char[] { '{' }, 2)[1];
                            myCompleteMessage = null;
                            //Debug.Write(myCompleteMessage);
                            string header = string.Format("HTTP/1.1 {0}\r\n"
                                              + "Server: {1}\r\n"
                                              + "Content-Type: {2}\r\n"
                                              + "Keep-Alive: Close\r\n"
                                              + "\r\n",
                                              "HTTP 200 OK", "Chroma Sync", "application/json");
                            // Send header & data
                            var headerBytes = Encoding.ASCII.GetBytes(header);
                            stream.Write(headerBytes, 0, headerBytes.Length);

                            try
                            {
                                JObject o = JObject.Parse(ns);
                                LuaScripting.PassThrough(o);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                            }
                            return;
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }
            }
            catch { }
            finally
            {
                try
                {
                    listener.Close();
                }
                catch (Exception) { }
            }
        }



    }
}
