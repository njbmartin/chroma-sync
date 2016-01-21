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

        public List<Package> GetLocal()
        {
            packages = new List<Package>();
            return packages;
        }


        public string LocalJson
        {
            get
            {
                var path = System.IO.Path.Combine(PackageManager.AppPath, "packages.json");
                return path;
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);


        }

        private void GetPackages()
        {

            var uri = new Uri(@"https://ultrabox.s3.amazonaws.com/ChromaSync/packages.json");
            var path = System.IO.Path.Combine(PackageManager.AppPath, "packages.json");
            var tmp = System.IO.Path.Combine(PackageManager.AppPath, ".packages.json");
            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
                client.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => Completed(sender, e, LocalJson, tmp));
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler((sender, e) => ProgressChanged(sender, e, "Package List"));
                client.DownloadFileAsync(uri, tmp);
            }
        }

        public void ShowDetails(int i)
        {
            currentSelection = i;
            var p = packages[i];
            _details = new DetailsControl();
            _details.Title.Text = p.Name;
            _details.Author.Text = p.Author;
            _details.Downloads.Text = p.Downloads.ToString();

            _details.Image.Source = GetImage(p.ImageURL);
            SetText(_details.Description, p.Description);
            if (p.PackageURL != null)
            {
                try
                {
                    Uri uri = new Uri(p.PackageURL);
                    string filename = System.IO.Path.GetFileName(uri.LocalPath);

                    if (PackageManager.FileExists(filename))
                    {
                        _details.ActionButton.Content = "Remove";
                    }

                    _details.ActionButton.Tag = i;
                    _details.ActionButton.Click += ActionButton_Click;
                }
                catch (Exception ex)
                {
                    App.Log.Error(ex);
                    _details.ActionButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                _details.ActionButton.Visibility = Visibility.Collapsed;
            }
            _details.Version.Text = p.Version;
            //HideRows(p);
            DetailsView.Children.Clear();
            DetailsView.Children.Add(_details);
        }

        public void HideRows(Package p)
        {
            foreach(var a in p.GetType().GetProperties())
            {
                if(p.GetType().GetProperty(a.Name).GetValue(p,null) == null)
                {
                    _details.Details.RowDefinitions.Where(x => x.Name == "Row" + a.Name).First().Height = new GridLength(0, GridUnitType.Pixel);
                }

            }
            
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var s = (Button)sender;
            int i = (int)s.Tag;
            var path = PackageManager.AppPath;

            Uri uri = new Uri(packages[i].PackageURL);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);
            string file = System.IO.Path.Combine(path, filename);
            var p = PackageManager.GetPackage(file);
            if (p != null)
            {
                if (PackageManager.RemovePackage(p))
                {
                    StatusText.Text = packages[i].Name + " has been uninstalled successfully";
                    removeMessage();
                }
                DisplayPackages();
                return;
            }
            
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
            string file = System.IO.Path.Combine(path, filename);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (File.Exists(file))
                File.Delete(file);
            string tmp = System.IO.Path.Combine(path, "." + filename);

            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
                client.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => Completed(sender, e, file, tmp));
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler((sender, e) => ProgressChanged(sender, e, p.Name));
                client.DownloadFileAsync(uri, tmp);
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
                MessageBox.Show(e.Error.Message);
                File.Delete(tmp);
                StatusText.Text = e.Error.Message;
                DisplayPackages();
                return;
            }

            if (File.Exists(path))
                File.Delete(path);

            File.Move(tmp, path);

            if (path.Equals(LocalJson))
            {
                StatusText.Text = "Retrieved list";
                DisplayPackages();
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
            DisplayPackages();
        }

        private void removeMessage()
        {
            Task.Delay(3000).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.BeginInvoke(
                  DispatcherPriority.Background,
                  new Action(() => StatusText.Text = ""));
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

        private void DisplayPackages()
        {
            try
            {
                JObject j = JObject.Parse(File.ReadAllText(LocalJson));

                packages = j.GetValue("packages").ToObject<List<Package>>();
            }
            catch (Exception e)
            {
                App.Log.Error(e);
                return;
            }
            ListView.Children.Clear();
            foreach (var package in packages)
            {
                ListItemControl item = new ListItemControl();
                item.Title.Content = package.Name;
                item.Type.Content = package.Type;
                SetText(item.Summary, package.Summary);
                item.image.Source = GetImage(package.ImageURL);
                item.Tag = packages.IndexOf(package);
                item.MouseLeftButtonUp += Item_MouseLeftButtonUp;
                item.MouseEnter += Item_MouseEnter;
                item.MouseLeave += Item_MouseLeave;
                ListView.Children.Add(item);
            }
            DetailsView.Children.Clear();
            ShowDetails(currentSelection);
        }

        private void Item_MouseLeave(object sender, MouseEventArgs e)
        {
            var s = (ListItemControl)sender;
            if (!s.Equals(_CurrentControl))
                s.Deselect();
        }

        private ListItemControl _CurrentControl;

        private void Item_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var s = (ListItemControl)sender;
            if (_CurrentControl != null)
                _CurrentControl.Deselect();
            _CurrentControl = s;


            int i = (int)s.Tag;
            ShowDetails(i);
        }

        private void Item_MouseEnter(object sender, MouseEventArgs e)
        {
            var s = (ListItemControl)sender;
            s.Background.Opacity = 1;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private static readonly Regex RE_URL = new Regex(@"(?#Protocol)(?:(?:ht|f)tp(?:s?)\:\/\/|~/|/)?(?#Username:Password)(?:\w+:\w+@)?(?#Subdomains)(?:(?:[-\w]+\.)+(?#TopLevel Domains)(?:com|org|net|gov|mil|biz|info|mobi|name|aero|jobs|museum|travel|[a-z]{2}))(?#Port)(?::[\d]{1,5})?(?#Directories)(?:(?:(?:/(?:[-\w~!$+|.,=]|%[a-f\d]{2})+)+|/)+|\?|#)?(?#Query)(?:(?:\?(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)(?:&(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)*)*(?#Anchor)(?:#(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)?");
        private void SetText(TextBlock text_block, string new_text)
        {
            if (text_block == null)
                return;

            text_block.Inlines.Clear();
            if (string.IsNullOrEmpty(new_text))
                return;

            // Find all URLs using a regular expression
            int last_pos = 0;
            foreach (Match match in RE_URL.Matches(new_text))
            {
                // Copy raw string from the last position up to the match
                if (match.Index != last_pos)
                {
                    var raw_text = new_text.Substring(last_pos, match.Index - last_pos);
                    text_block.Inlines.Add(new Run(raw_text));
                }

                // Create a hyperlink for the match
                var link = new Hyperlink(new Run(match.Value))
                {
                    NavigateUri = new Uri(match.Value)
                };
                link.Click += OnUrlClick;

                text_block.Inlines.Add(link);

                // Update the last matched position
                last_pos = match.Index + match.Length;
            }

            // Finally, copy the remainder of the string
            if (last_pos < new_text.Length)
                text_block.Inlines.Add(new Run(new_text.Substring(last_pos)));
        }

        private static void OnUrlClick(object sender, RoutedEventArgs e)
        {
            var link = (Hyperlink)sender;
            // Do something with link.NavigateUri like:
            Process.Start(link.NavigateUri.ToString());
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetPackages();
        }
    }
}
