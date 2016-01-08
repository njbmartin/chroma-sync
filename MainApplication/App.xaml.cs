using System;
using System.Windows;
using System.Windows.Forms;

using System.Threading;
using System.Diagnostics;
using Ultrabox.ChromaSync.Properties;
using System.IO;
using System.Drawing;
using Corale.Colore.Core;

namespace Ultrabox.ChromaSync
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        internal static NotifyIcon _icon;
        internal Thread _serverThread;
        internal Thread _luaThread;
        internal Thread _packagesThread;
        internal static ContextMenu _iconMenu;
        internal static MenuItem scriptsMenu;
        internal static MenuItem packagesMenu;

        public static void NewScriptsContext()
        {
            scriptsMenu.MenuItems.Clear();
            MenuItem openScripts = new MenuItem("Scripts Folder", BrowseScripts);
            MenuItem reload = new MenuItem("Reload Scripts", ReloadScripts);
            scriptsMenu.MenuItems.Add(openScripts);
            scriptsMenu.MenuItems.Add(reload);
            scriptsMenu.MenuItems.Add("-");
        }

        public static void NewPackagesContext()
        {
            
            packagesMenu.MenuItems.Clear();
            MenuItem packages = new MenuItem("Packages Folder", BrowsePackages);
            
            packagesMenu.MenuItems.Add(packages);
            packagesMenu.MenuItems.Add("-");
        }

        public void App_Startup(object sender, StartupEventArgs e)
        {
            // Application is running
            //Check version
            AutoUpdate updater = new AutoUpdate();
            updater.ShowDialog();


            _iconMenu = new ContextMenu();
            
            
            
            _icon = new NotifyIcon
            {
                Icon = new Icon("chromasync.ico"),
                ContextMenu = _iconMenu,
                Visible = true,
            };


            MenuItem about = new MenuItem("Visit website", showAbout);
            scriptsMenu = new MenuItem("Scripts");
            packagesMenu = new MenuItem("Packages");

            _iconMenu.MenuItems.Add(about);
            //_iconMenu.MenuItems.Add(updates);

            NewScriptsContext();
            NewPackagesContext();



            //MenuItem uninit = new MenuItem("Uninit", Uninit);

            MenuItem exitMenuItem = new MenuItem("Exit", Quit);



            _iconMenu.MenuItems.Add(packagesMenu);
            _iconMenu.MenuItems.Add(scriptsMenu);
            _iconMenu.MenuItems.Add(exitMenuItem);
            PluginManager.EnablePlugins();

            _packagesThread = new Thread(PackageManager.Start);
            _packagesThread.Start();

            _serverThread = new Thread(Server.RunServer);
            _serverThread.Start();

            _luaThread = new Thread(LuaScripting.LuaThread);
            _luaThread.Start();

        }


        static void ReloadScripts(object sender, EventArgs e)
        {
            LuaScripting.ReloadScripts();
        }

        void showAbout(object sender, EventArgs e)
        {
            Process.Start("http://chromasync.io/");
        }

        void ShowConfig(object sender, EventArgs e)
        {
            /*
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
            */
        }

        static void BrowseScripts(object sender, EventArgs e)
        {
            string path = @"%appdata%\ChromaSync";
            path = Environment.ExpandEnvironmentVariables(path);

            path = Path.Combine(path, "scripts");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            Process.Start("explorer.exe", path);
        }

        static void BrowsePackages(object sender, EventArgs e)
        {
            string path = @"%appdata%\ChromaSync";
            path = Environment.ExpandEnvironmentVariables(path);

            path = Path.Combine(path, "packages");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            Process.Start("explorer.exe", path);
        }



        void Quit(object sender, EventArgs e)
        {
            Debug.WriteLine("Exiting");
            Quit();
        }

        private void Quit()
        {
            // We must manually tidy up and remove the icon before we exit.
            // Otherwise it will be left behind until the user mouses over.
            
            LuaScripting.CloseScripts();
            // Abort all threads
            
            if (_luaThread != null && _luaThread.IsAlive)
                _luaThread.Abort();
            //_gtaThread.Abort();
            if (Server._clientThread != null && Server._clientThread.IsAlive)
                Server._clientThread.Abort();

            if (_serverThread != null && _serverThread.IsAlive)
                _serverThread.Abort();

            //_icon.Visible = false;
            System.Windows.Application.Current.Shutdown();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Debug.WriteLine("Exiting");
            Quit();
        }
    }
}
