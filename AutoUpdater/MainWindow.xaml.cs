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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
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
                    // start download
                    //ExecuteUpdate();


                    return;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                this.Close();
            }
            finally
            {

            }
        }

        void ExecuteUpdate()
        {
            string[] args = Environment.GetCommandLineArgs();
            string updatedFile = args[1];

            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
                client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler((sender, e) => Completed(sender, e, updatedFile));
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler((sender, e) => ProgressChanged(sender, e));
                client.DownloadFileAsync(new Uri("http://thejourneynetwork.net/chromatics/update/Chromatics.zip"), @"\Update.exe");
            }

        }

        private void Completed(object sender, AsyncCompletedEventArgs e, string updatedFile)
        {
            FileInfo destFile = new FileInfo(System.IO.Path.Combine(updatedFile, @"\Chromatics.zip"));
            /*
            if (destFile.Extension.ToLower() == ".zip")
            {
                updateText.Text = "Extracting Update..";

                ZipArchive zipArchive = ZipFile.OpenRead(updatedFile + @"\Chromatics.zip");

                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    string fullPath = Path.Combine(updatedFile + @"\", entry.FullName);
                    if (String.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    else
                    {
                        if (!entry.Name.Equals("updater.exe"))
                        {
                            entry.ExtractToFile(fullPath, true);
                        }
                    }
                }

                zipArchive.Dispose();
                //File.Delete(updatedFile + @"\Chromatics.zip");
                updateText.Text = "Closing Updater";
                if (File.Exists(@"C:\Program Files (x86)\Advanced Combat Tracker\Advanced Combat Tracker.exe"))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = @"C:\Program Files (x86)\Advanced Combat Tracker\Advanced Combat Tracker.exe";
                    Process.Start(startInfo);
                }
                
                //System.Windows.Forms.Application.Exit();
            }
            else
            {
                updateText.Text = "The downloaded file did not contain a valid Chromatics file (Unknown file type)";
            }
            */

        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            updateText.Text = "Downloading Update: " + e.ProgressPercentage + "% Complete";
        }


        private void button_Click(object sender, RoutedEventArgs e)
        {

        }

        protected override void OnActivated(EventArgs e)
        {
            CheckUpdate();
            base.OnActivated(e);
        }
    }


}

