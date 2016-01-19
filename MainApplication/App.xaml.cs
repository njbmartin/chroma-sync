using System;
using System.Windows;
using System.Windows.Forms;

using System.Threading;
using System.Diagnostics;
using Ultrabox.ChromaSync.Properties;
using System.IO;
using System.Drawing;
using Corale.Colore.Core;
using log4net;
using log4net.Core;

namespace Ultrabox.ChromaSync
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {

        internal static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        internal static NotifyIcon _icon;
        internal static Thread _serverThread;
        internal static Thread _luaThread;
        internal static Thread _packagesThread;
        internal static ContextMenu _iconMenu;
        internal static MenuItem scriptsMenu;
        internal static MenuItem packagesMenu;
        internal static bool shouldQuit;
        internal MainBrowser mb;
        public static IChroma c = Chroma.Instance;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            LogManager.GetRepository().Threshold = Level.Info;
            Log.Info("Hello World");
            base.OnStartup(e);
        }

        public static void NewScriptsContext()
        {
            scriptsMenu.MenuItems.Clear();
            MenuItem openScripts = new MenuItem("Scripts Folder", BrowseScripts);
            MenuItem reload = new MenuItem("Reload Scripts", ReloadScripts);
            scriptsMenu.MenuItems.Add(openScripts);
            scriptsMenu.MenuItems.Add(reload);
            scriptsMenu.MenuItems.Add("-");
        }

        internal static void StartServices()
        {
            PluginManager.EnablePlugins();

            _packagesThread = new Thread(PackageManager.Start);
            _packagesThread.Start();

            _serverThread = new Thread(Server.RunServer);
            _serverThread.Start();

            _luaThread = new Thread(LuaScripting.LuaThread);
            _luaThread.Start();
        }

        public static int GetCSVersion()
        {

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            int version = fvi.ProductPrivatePart;
            return version;
        }
        public static void NewPackagesContext()
        {

            packagesMenu.MenuItems.Clear();
            MenuItem packages = new MenuItem("Packages Folder", BrowsePackages);

            packagesMenu.MenuItems.Add(packages);
            packagesMenu.MenuItems.Add("-");
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {

            string errorMessage = string.Format("An unhandled exception occurred: {0}", e.Exception.Message);
            Log.Error(errorMessage);
            //System.Windows.MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        public void App_Startup(object sender, StartupEventArgs e)
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            // Application is running
            //Check version
            AutoUpdate updater = new AutoUpdate();
            mb = new MainBrowser();
            var t = updater.ShowDialog();
            if (shouldQuit)
            {
                Quit();
                return;
            }

            FirstRun();

            _iconMenu = new ContextMenu();


            _icon = new NotifyIcon
            {
                Icon = new Icon("chromasync.ico"),
                ContextMenu = _iconMenu,
                Visible = true,

            };

            _icon.MouseClick += new MouseEventHandler(ShowBrowser);
            MenuItem about = new MenuItem("Visit website", showAbout);
            scriptsMenu = new MenuItem("Scripts");
            packagesMenu = new MenuItem("Packages");
            _iconMenu.MenuItems.Add(about);

            NewScriptsContext();
            NewPackagesContext();

            MenuItem exitMenuItem = new MenuItem("Exit", Quit);
            _iconMenu.MenuItems.Add(packagesMenu);
            _iconMenu.MenuItems.Add(scriptsMenu);
            _iconMenu.MenuItems.Add(exitMenuItem);

            // Start services
            StartServices();

            // TODO: Browser

            mb.Show();
        }

        private void ShowBrowser(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!mb.IsLoaded)
                {
                    mb = new MainBrowser();

                }

                mb.Show();
                if (mb.WindowState == WindowState.Minimized)
                    mb.WindowState = WindowState.Normal;
            }
        }

        private void Uninit(object sender, EventArgs e)
        {
            LuaScripting.CloseScripts();
            Debug.WriteLine(Chroma.Instance.Initialized);
        }

        private void FirstRun()
        {
            // Check if first run
            int v = 0;
            try
            {
                v = int.Parse(RegistryKeeper.GetValue("lastversion"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            if (GetCSVersion() > v)
            {
                Intro iWindow = new Intro();
                iWindow.ShowDialog();
                RegistryKeeper.UpdateReg("lastversion", GetCSVersion().ToString());
            }
        }

        static void ReloadScripts(object sender, EventArgs e)
        {
            LuaScripting.ReloadScripts();
        }

        void showAbout(object sender, EventArgs e)
        {
            Process.Start("http://chromasync.io/");
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
