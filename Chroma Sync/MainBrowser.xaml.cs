using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Ultrabox.ChromaSync.Controllers;
using Ultrabox.ChromaSync.Helpers;
using Ultrabox.ChromaSync.Models;
using Ultrabox.ChromaSync.Pages;

namespace Ultrabox.ChromaSync
{
    /// <summary>
    /// Interaction logic for MainBrowser.xaml
    /// </summary>
    public partial class MainBrowser : Window
    {
        public static List<GIPackage> gameIntegrationList;
        public static List<ChromaApp> appsList;
        public static List<Script> scriptsList;
        internal static int currentSelection = -1;
        public static DetailsControl _details;


        public static TextBlock _messageBox;

        public enum Tabs
        {
            GameIntegrations,
            Scripts,
            Plugins,
            Workshop,
            Settings,
            Secret
        }

        internal static Tabs currentTab = Tabs.GameIntegrations;
        private static Label currentTabLabel;

        public MainBrowser()
        {
            InitializeComponent();
            ScriptViewController.Prepare(this);
            GIController.Prepare(this);
            AppsController.Prepare(this);
            ProfilesController.Prepare(this);
            currentTabLabel = GamesTab;
            _messageBox = StatusText;
            GetPackages();
        }

        public string LocalJson
        {
            get
            {
                var path = System.IO.Path.Combine(Paths.MainDirectory, "packages.json");
                return path;
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }

        private void GetPackages()
        {

            var uri = new Uri(@"http://cdn.chromasync.io/packages.json");
            var tmp = System.IO.Path.Combine(Paths.MainDirectory, ".packages.json");
            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
                client.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => Completed(sender, e, LocalJson, tmp));
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler((sender, e) => ProgressChanged(sender, e, "Package List"));
                client.DownloadFileAsync(uri, tmp);
            }
        }


        public void HideRows(GIPackage p)
        {
            foreach (var a in p.GetType().GetProperties())
            {
                if (p.GetType().GetProperty(a.Name).GetValue(p, null) == null)
                {
                    _details.Details.RowDefinitions.Where(x => x.Name == "Row" + a.Name).First().Height = new GridLength(0, GridUnitType.Pixel);
                }

            }

        }



        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e, string file)
        {

            StatusText.Text = "Downloading " + file + ": " + e.ProgressPercentage + "% complete";
        }

        private void Completed(object sender, AsyncCompletedEventArgs e, string path, string tmp)
        {

            removeMessage();

            if (e.Error != null)
            {

                App.Log.Error(e.Error);
                //MessageBox.Show(e.Error.Message);
                File.Delete(tmp);
                
                StatusText.Text = "It appears as though you do not have an internet connection";

                PopulateList();
                return;
            }

            if (File.Exists(path))
                File.Delete(path);

            File.Move(tmp, path);

            if (path.Equals(LocalJson))
            {
                StatusText.Text = "Retrieved list";
                PopulateList();
                return;
            }
            StatusText.Text = "Downloaded successfully";
            PackageManager.GetPackages();
            var p = PackageManager.GetPackage(path);
            if (p != null)
            {
                if (PackageManager.InstallPackage(p))
                {
                    StatusText.Text = p.Product.Name + " successfully installed";
                    removeMessage();
                }
            }
            PopulateList();
        }

        internal static void removeMessage()
        {
            Task.Delay(3000).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.BeginInvoke(
                  DispatcherPriority.Background,
                  new Action(() => _messageBox.Text = ""));
            });
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            ResizeMode = ResizeMode == ResizeMode.CanResizeWithGrip ? ResizeMode.NoResize : ResizeMode.CanResizeWithGrip;
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void PopulateList()
        {

            try
            {
                JObject j = JObject.Parse(File.ReadAllText(LocalJson));
                GIController.integrationsList = j.GetValue("packages").ToObject<List<GIPackage>>();
                ScriptViewController.scriptsList = j.GetValue("scripts").ToObject<List<Script>>();
            }
            catch (Exception e)
            {
                App.Log.Error(e);
                return;
            }


            switch (currentTab)
            {
                case Tabs.GameIntegrations:
                    ListView.Columns = 1;
                    TabDescription.Text = GIController.Description;
                    GIController.GenerateList();
                    break;
                case Tabs.Scripts:
                    ListView.Columns = 1;
                    TabDescription.Text = ScriptViewController.Description;
                    ScriptViewController.GenerateList();
                    break;
                case Tabs.Workshop:
                    ListView.Columns = 1;
                    TabDescription.Text = AppsController.Description;
                    AppsController.GetApps();
                    break;
                case Tabs.Settings:
                    TabDescription.Text = ProfilesController.Description;
                    ListView.Columns = 3;
                    ProfilesController.GetApps();
                    break;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetPackages();
        }

        private void Tab_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Label l = (Label)sender;
            
            currentTabLabel.Foreground = new SolidColorBrush((Color)Application.Current.Resources["ChromaSyncMedGrey"]);
            l.Foreground= new SolidColorBrush((Color)Application.Current.Resources["ChromaSyncPink"]);
            currentTabLabel = l;
            ChangeTab(int.Parse((string)l.Tag));
        }

        private void ChangeTab(int tag)
        {
            currentTab = (Tabs)tag;
            ListView.Children.Clear();
            DetailsView.Children.Clear();
            PopulateList();
        }

        private void secret_click(object sender, MouseButtonEventArgs e)
        {
            SecretTab.Visibility = Visibility.Visible;
        }
    }
}
