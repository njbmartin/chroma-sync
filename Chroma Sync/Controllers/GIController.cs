using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Ultrabox.ChromaSync.Helpers;
using Ultrabox.ChromaSync.Models;
using Ultrabox.ChromaSync.Pages;

namespace Ultrabox.ChromaSync.Controllers
{
    class GIController
    {

        private static UniformGrid _listView;
        private static DockPanel _detailsView;
        private static DetailsControl _details;

        private static int currentSelection = -1;
        internal static List<GIPackage> integrationsList;

        private static ListItemControl _CurrentControl;
        internal static readonly string Description = @"Sync your games to Chroma lighting effects with the following game mods and integrations.";

        internal static void Prepare(MainBrowser mainBrowser)
        {
            _listView = mainBrowser.ListView;
            _detailsView = mainBrowser.DetailsView;
        }

        public static void GenerateList()
        {
            _listView.Children.Clear();
            foreach (var integration in integrationsList)
            {
                ListItemControl item = new ListItemControl();
                item.Title.Content = integration.Name;
                item.Type.Content = integration.Type;
                TextHelper.SetText(item.Summary, integration.Summary);
                item.image.Source = ImageHelper.GetImage(integration.ImageURL);
                item.Tag = integrationsList.IndexOf(integration);
                item.MouseLeftButtonUp += Item_MouseLeftButtonUp;
                item.MouseEnter += Item_MouseEnter;
                item.MouseLeave += Item_MouseLeave;
                _listView.Children.Add(item);
            }

            _detailsView.Children.Clear();
            if (currentSelection >= 0)
                ShowDetails(currentSelection);

        }


        public static void ShowDetails(int i)
        {
            currentSelection = i;
            var p = integrationsList[i];
            _details = new DetailsControl();
            _details.Title.Text = p.Name;
            _details.Author.Text = p.Author;
            _details.Downloads.Text = p.Downloads.ToString();

            _details.Image.Source = ImageHelper.GetImage(p.ImageURL);
            TextHelper.SetText(_details.Description, p.Description);
            if (p.PackageURL != null)
            {
                try
                {
                    Uri uri = new Uri(p.PackageURL);
                    string filename = Path.GetFileName(uri.LocalPath);

                    if (FileHelper.Exists(Path.Combine(Paths.Packages, filename)))
                    {
                        _details.ActionButton.Content = "Disable Integration";
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
            _detailsView.Children.Clear();
            _detailsView.Children.Add(_details);
        }

        private static void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var s = (Button)sender;
            int i = (int)s.Tag;
            var path = Paths.Packages;

            Uri uri = new Uri(integrationsList[i].PackageURL);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);
            string file = System.IO.Path.Combine(path, filename);
            var p = PackageManager.GetPackage(file);
            if (p != null)
            {
                if (PackageManager.RemovePackage(p))
                {
                    MainBrowser._messageBox.Text = integrationsList[i].Name + " has been uninstalled successfully";
                    MainBrowser.removeMessage();
                }
                GenerateList();
                return;
            }

            s.Content = "Downloading...";
            s.IsEnabled = false;
            DownloadPackage(integrationsList[i]);
        }

        private static void Item_MouseLeave(object sender, MouseEventArgs e)
        {
            var s = (ListItemControl)sender;
            if (!s.Equals(_CurrentControl))
                s.Deselect();
        }



        private static void Item_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var s = (ListItemControl)sender;
            if (_CurrentControl != null)
                _CurrentControl.Deselect();
            _CurrentControl = s;


            int i = (int)s.Tag;
            ShowDetails(i);
        }

        private static void Item_MouseEnter(object sender, MouseEventArgs e)
        {
            var s = (ListItemControl)sender;
            s.Background.Opacity = 1;
        }



        private static void DownloadPackage(GIPackage p)
        {

            Uri uri = new Uri(p.PackageURL);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);
            string file = System.IO.Path.Combine(Paths.Packages, filename);
            if (!Directory.Exists(Paths.Packages))
                Directory.CreateDirectory(Paths.Packages);
            if (File.Exists(file))
                File.Delete(file);
            string tmp = System.IO.Path.Combine(Paths.Packages, "." + filename);

            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
                client.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => Completed(sender, e, file, tmp));
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler((sender, e) => ProgressChanged(sender, e, p.Name));
                client.DownloadFileAsync(uri, tmp);
            }
        }


        private static void ProgressChanged(object sender, DownloadProgressChangedEventArgs e, string file)
        {

            MainBrowser._messageBox.Text = "Downloading " + file + ": " + e.ProgressPercentage + "% complete";
        }

        private static void Completed(object sender, AsyncCompletedEventArgs e, string path, string tmp)
        {

            MainBrowser.removeMessage();

            if (e.Error != null)
            {
                App.Log.Error(e.Error);
                //MessageBox.Show(e.Error.Message);
                File.Delete(tmp);
                MainBrowser._messageBox.Text = e.Error.Message;
                GenerateList();
                return;
            }

            if (File.Exists(path))
                File.Delete(path);

            File.Move(tmp, path);


            MainBrowser._messageBox.Text = "Downloaded successfully";
            PackageManager.GetPackages();
            var p = PackageManager.GetPackage(path);
            if (p != null)
            {
                if (PackageManager.InstallPackage(p))
                {
                    MainBrowser._messageBox.Text = p.Product.Name + " successfully installed";
                    MainBrowser.removeMessage();
                }
            }
            GenerateList();
        }

    }

}

