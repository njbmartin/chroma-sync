using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ultrabox.ChromaSync.Pages;

namespace Ultrabox.ChromaSync
{
    /// <summary>
    /// Interaction logic for MainBrowser.xaml
    /// </summary>
    public partial class MainBrowser : Window
    {
        public List<Package> packages;
        private int currentSelection;
        public static DetailsControl _details;
        public class Package
        {
            public string Name { get; set; }
            public string Author { get; set; }
            public string Description { get; set; }
            public string Summary { get; set; }
            public string Version { get; set; }
            public string Type { get; set; }
            public List<string> Tags { get; set; }
            public List<string> Devices { get; set; }
            public int Downloads { get; set; }
            public string ImageURL { get; set; }
            public string PackageURL { get; set; }
            public string ProjectURL { get; set; }
        }


        public MainBrowser()
        {
            InitializeComponent();
            GetPackages();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);


        }

        private void GetPackages()
        {

            packages = new List<Package>();
            var webRequest = WebRequest.Create(@"https://ultrabox.s3.amazonaws.com/ChromaSync/packages.json");

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                ListView.Children.Clear();
                DetailsView.Children.Clear();
                string packageString = reader.ReadToEnd();
                JObject o = JObject.Parse(packageString);
                packages = o.GetValue("packages").ToObject<List<Package>>();
            }

            foreach (var p in packages)
            {
                ListItemControl item = new ListItemControl();
                item.Title.Content = p.Name;
                item.Summary.Text = p.Summary;
                item.image.Source = GetImage(p.ImageURL);
                item.Tag = packages.IndexOf(p);
                item.MouseLeftButtonDown += new MouseButtonEventHandler(Clicked);
                ListView.Children.Add(item);
            }

            ShowDetails(currentSelection);
        }



        private void Clicked(object sender, EventArgs e)
        {
            var s = (ListItemControl)sender;
            int i = (int)s.Tag;
            ShowDetails(i);
        }

        public void ShowDetails(int i)
        {
            currentSelection = i;
            var p = packages[i];
            _details = new DetailsControl();
            _details.Title.Text = p.Name;
            _details.Author.Text = p.Author;
            _details.Image.Source = GetImage(p.ImageURL);
            _details.Description.Text = p.Description;
            try
            {
                Uri uri = new Uri(p.PackageURL);
                string filename = System.IO.Path.GetFileName(uri.LocalPath);

                if (PackageManager.FileExists(filename))
                {
                    _details.ActionButton.Content = "Uninstall Package";
                }


                _details.ActionButton.Tag = i;
                _details.ActionButton.Click += ActionButton_Click;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex);
                _details.ActionButton.Visibility = Visibility.Hidden;
            }
            _details.Version.Text = p.Version;
            DetailsView.Children.Clear();
            DetailsView.Children.Add(_details);
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var s = (Button)sender;
            int i = (int)s.Tag;
            s.Content = "Downloading...";
            s.IsEnabled = false;
            DownloadPackage(packages[i]);
        }

        private ImageSource GetImage(string url)
        {

            ImageSource imgsr = new BitmapImage(new Uri(url));
            return imgsr;
        }


        private void DownloadPackage(Package p)
        {
            var path = PackageManager.AppPath;

            Uri uri = new Uri(p.PackageURL);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
                client.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => Completed(sender, e, path));
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler((sender, e) => ProgressChanged(sender, e));
                client.DownloadFileAsync(uri, System.IO.Path.Combine(path, filename));
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            
            StatusText.Text= "Downloading: " + e.ProgressPercentage + "% complete";
        }

        private void Completed(object sender, AsyncCompletedEventArgs e, object path)
        {
            StatusText.Text = "";
            _details.ActionButton.IsEnabled = true;
            _details.ActionButton.Content = "Uninstall Package";
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            GetPackages();
        }
    }
}
