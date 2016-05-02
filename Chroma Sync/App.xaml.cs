using System;
using System.Windows;
using System.Windows.Forms;

using System.Threading;
using System.Diagnostics;
using Ultrabox.ChromaSync.Engines;
using System.IO;
using System.Drawing;
using Corale.Colore.Core;
using log4net;
using log4net.Core;
using Ultrabox.ChromaSync.Services;
using SKYPE4COMLib;
using Microsoft.Office;

namespace Ultrabox.ChromaSync
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {

        internal static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static NotifyIcon _icon;
        internal static ApiServer _apiServer;

        internal static Thread _apiServerThread;

        internal static Thread _codemastersAPIThread;

        internal static LuaEngine _luaEngine;
        internal static Thread _luaEngineThread;

        internal static JavascriptEngine _jsEngine;
        internal static Thread _jsEngineThread;

        internal static Thread _packagesThread;

        internal static USBDetection _usbDetection;
        internal static Thread _usbDetectionThread;

        internal static ContextMenu _iconMenu;
        internal static MenuItem scriptsMenu;
        internal static MenuItem packagesMenu;

        private static Skype _skype = new Skype();




        internal MainBrowser mb;
        public static IChroma c = Chroma.Instance;

        protected override void OnStartup(StartupEventArgs e)
        {
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
            c = Chroma.Instance;
            if (!c.Initialized)
                c.Initialize();

            PackageManager.GetPackages();
            NewScriptsContext();
            //_packagesThread = new Thread(PackageManager.Start);
            //_packagesThread.Start();

            _apiServer = new ApiServer();

            _apiServerThread = new Thread(_apiServer.Start);
            _apiServerThread.Start();

            //_codemastersAPIThread = new Thread(CodemastersAPI.RunServer);
            //_codemastersAPIThread.Start();


            _usbDetection = new USBDetection();
            _usbDetectionThread = new Thread(_usbDetection.Start);
            _usbDetectionThread.Start();


            _luaEngine = new LuaEngine();
            _luaEngineThread = new Thread(_luaEngine.Start);
            _luaEngineThread.Start();

            //_jsEngine = new JavascriptEngine();
            //_jsEngineThread = new Thread(_jsEngine.Start);
            //_jsEngineThread.Start();

            var temp = Temperature.Temperatures;
            EventHook.SetHook();

            if (_skype.Client.IsRunning)
            {
                _skype.MessageStatus += _skype_MessageStatus;
                _skype.CallStatus += _skype_CallStatus;
                //_skype._ISkypeEvents_Event_AttachmentStatus += OnAttach;
                _skype.Attach(7, false);
            }






        }

        private static void Application_NewMail()
        {
            throw new NotImplementedException();
        }

        private static void Outlook_NewMail()
        {
            throw new NotImplementedException();
        }

        private static void _skype_CallStatus(Call pCall, TCallStatus Status)
        {
            //return;

            if (Status != TCallStatus.clsRinging)
                return;


            
            
            try
            {
                _skype.get_Call(pCall.Id).Finish();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void _skype_MessageStatus(ChatMessage pMessage, TChatMessageStatus Status)
        {
            if (Status != TChatMessageStatus.cmsReceived)
                return;

            // simple echo service.
            //_skype.get_Chat(pMessage.ChatName).SendMessage("Hello from Chroma Sync!");
        }

        internal static void StopServices()
        {
            // Request Plugins to Stop

            //PluginManager.StopPlugins();

            // Abort all threads
            if (_luaEngine != null && _luaEngineThread != null)
            {
                _luaEngine.RequestStop();
                _luaEngineThread.Join();
            }


            if (_jsEngine != null && _jsEngineThread != null)
            {
                _jsEngine.RequestStop();
                _jsEngineThread.Join();
            }

            if (_apiServer != null && _apiServerThread != null)
            {
                _apiServer.RequestStop();
                _apiServerThread.Join();
            }

            //c.Uninitialize();

        }

        public static string CurrentVersion
        {
            get
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                return assembly.GetName().Version.ToString();
            }
        }

        public static int CurrentBuild
        {
            get
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                return assembly.GetName().Version.Build;
            }
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
            Updater updater = new Updater();
            mb = new MainBrowser();
            updater.Show();

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
            var version = new MenuItem("version: " + CurrentVersion);
            version.Enabled = false;
            _iconMenu.MenuItems.Add(version);

            _iconMenu.MenuItems.Add("-");
            _iconMenu.MenuItems.Add(about);

            NewPackagesContext();



            MenuItem exitMenuItem = new MenuItem("Exit", Quit);
            _iconMenu.MenuItems.Add(scriptsMenu);
            _iconMenu.MenuItems.Add(exitMenuItem);


            PluginManager.LoadPlugins();
            // Start services
            StartServices();

            // TODO: Browser

            mb.Show();
            FirstRun();
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
            StopServices();

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
            if (CurrentBuild > v)
            {
                Intro iWindow = new Intro();
                iWindow.ShowDialog();
                RegistryKeeper.UpdateReg("lastversion", CurrentBuild.ToString());
                BalloonTip("Chroma Sync", "Chroma Sync continues to run in the background");
            }
        }

        static void ReloadScripts(object sender, EventArgs e)
        {
            RestartServices();
        }

        public static void RestartServices()
        {

            StopServices();
            StartServices();
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

        public static void Quit()
        {
            // We must manually tidy up and remove the icon before we exit.
            // Otherwise it will be left behind until the user mouses over.



            //_icon.Visible = false;
            Current.Shutdown();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Debug.WriteLine("Exiting");
            Quit();
        }


        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            StopServices();
        }

        public static void BalloonTip(string title, string description)
        {
            _icon.ShowBalloonTip(2000, title, description, ToolTipIcon.None);
        }


    }
}
