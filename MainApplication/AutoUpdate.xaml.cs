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
        public AutoUpdate()
        {
            InitializeComponent();
            //CheckUpdate();
        }

        public void CheckUpdate()
        {

            try
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                int version = fvi.ProductPrivatePart;
                Debug.WriteLine(version);

                int newVersion = version;
                var webRequest = WebRequest.Create(@"https://ultrabox.s3.amazonaws.com/ChromaSync/version.json");

                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new StreamReader(content))
                {
                    string newVersionA = reader.ReadToEnd();
                    JObject o = JObject.Parse(newVersionA);
                    newVersion = o.GetValue("version").ToObject<int>();
                }

                Debug.WriteLine(newVersion);

                int cV = version;
                int nV = newVersion;

                if (nV > cV)
                {
                    updateText.Text = "Downloading new version: " + nV;
                    ExecuteUpdate();
                    // start download

                    //string updatedFile = System.IO.Path.GetDirectoryName(pluginData.pluginFile.FullName);

                    //ProcessStartInfo startInfo = new ProcessStartInfo();
                    //startInfo.FileName = updatedFile + @"\lib\updater.exe";
                    //startInfo.Arguments = "\"" + updatedFile + "\"";
                    //Process.Start(startInfo);
                    return;
                }
                Hide();
            }
            catch (Exception ex)
            {
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
                client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler((sender, e) => Completed(sender, e, path));
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler((sender, e) => ProgressChanged(sender, e));
                client.DownloadFileAsync(new Uri("https://ultrabox.s3.amazonaws.com/ChromaSync/setup.exe"), path + @"\update.exe");
            }
        }

        private void Completed(object sender, AsyncCompletedEventArgs e, string updatedFile)
        {
            FileInfo destFile = new FileInfo(System.IO.Path.Combine(updatedFile, "update.exe"));
            updateText.Text = "Installing update...";



            //File.Delete(updatedFile + @"\Chromatics.zip");
            updateText.Text = "Closing Updater";
            if (destFile.Exists)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = destFile.FullName;
                //startInfo.Arguments = @" /S /v/qn";
                Process.Start(startInfo);
            }
            App.shouldQuit = true;
            Hide();
            //System.Windows.Application.Current.Shutdown();
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            updateText.Text = "Downloading update: " + e.ProgressPercentage + "% complete";
        }


        private void button_Click(object sender, RoutedEventArgs e)
        {

        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            CheckUpdate();

        }
    }
}
