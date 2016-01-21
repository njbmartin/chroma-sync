using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ultrabox.ChromaSync
{
    /// <summary>
    /// Interaction logic for AutoUpdate.xaml
    /// </summary>
    public partial class AutoUpdate : Window
    {
        private static int buildNumber;
        private static int newBuildNumber;
        private static string version;

        private class VersionInfo
        {
            public string Version { get; set; }
            public int Build { get; set; }
            public string Checksum { get; set; }
            public string SetupURL { get; set; }
            public string ReleaseNotesURL { get; set; }
        }

        private static VersionInfo LatestVersion;


        public AutoUpdate()
        {
            InitializeComponent();
        }

        public void CheckUpdate()
        {

            try
            {
                buildNumber = App.GetCSVersion();
                App.Log.Info("Current Build: " + buildNumber);
                newBuildNumber = buildNumber;
                var webRequest = WebRequest.Create(@"http://cdn.chromasync.io/version.json");

                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new StreamReader(content))
                {
                    string newVersionA = reader.ReadToEnd();
                    JObject o = JObject.Parse(newVersionA);
                    LatestVersion = o.ToObject<VersionInfo>();
                    newBuildNumber = LatestVersion.Build;
                    version = LatestVersion.Version;
                }

                if (newBuildNumber > buildNumber)
                {
                    App.Log.Info("New version available: " + version);
                    updateText.Text = "A new version is available: " + version;
                    buttons.Visibility = Visibility.Visible;
                    return;
                }
                Hide();
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.Message);
                Hide();
            }

        }

        void ExecuteUpdate()
        {
            string path = @"%appdata%\ChromaSync";
            path = Environment.ExpandEnvironmentVariables(path);

            path = System.IO.Path.Combine(path, "updater");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
                client.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => Completed(sender, e, path));
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler((sender, e) => ProgressChanged(sender, e));
                client.DownloadFileAsync(new Uri(LatestVersion.SetupURL), path + @"\update.exe");
            }
        }

        private void Completed(object sender, AsyncCompletedEventArgs e, string updatedFile)
        {
            FileInfo destFile = new FileInfo(System.IO.Path.Combine(updatedFile, "update.exe"));
            updateText.Text = "Installing update...";

            if (e.Error != null)
            {
                App.Log.Error(e);
                Hide();
                System.Windows.MessageBox.Show(e.Error.Message, e.Error.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            updateText.Text = "Closing Updater";
            if (destFile.Exists)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = destFile.FullName;
                //startInfo.Arguments = @" /S /v/qn";
                Process.Start(startInfo);
            }
            App.Quit();
            Hide();
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            updateText.Text = "Downloading update: " + e.ProgressPercentage + "% complete";
        }


        private void button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button b = (System.Windows.Controls.Button)sender;
            if (b.Name == "updateButton")
            {
                buttons.Visibility = Visibility.Collapsed;
                ExecuteUpdate();
            }
            else
            {
                Hide();
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            CheckUpdate();

        }

        private void release_notes(object sender, RoutedEventArgs e)
        {
            Process.Start(LatestVersion.ReleaseNotesURL);
        }
    }
}
