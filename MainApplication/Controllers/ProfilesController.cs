using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Ultrabox.ChromaSync.Helpers;
using Ultrabox.ChromaSync.Models;
using Ultrabox.ChromaSync.Pages;

namespace Ultrabox.ChromaSync.Controllers
{
    class ProfilesController
    {
        private static UniformGrid _listView;
        private static DockPanel _detailsView;
        private static ProfileDetails _details;

        private static int currentSelection = -1;

        internal static readonly string Description = @"Download new profiles for your Chroma devices created by Razer and the community. Powered by the Razer Chroma Workshop.";
        internal static List<ChromaProfile> profileList;

        internal static void Prepare(MainBrowser mainBrowser)
        {
            _listView = mainBrowser.ListView;
            _detailsView = mainBrowser.DetailsView;
        }

        public static string LocalJson
        {
            get
            {
                var path = Path.Combine(PackageManager.AppPath, "profiles.json");
                return path;
            }
        }


        internal static void GetApps()
        {

            var uri = new Uri(@"http://www.razerzone.com/?ACT=34&action=process_chroma_profile_list");
            var path = System.IO.Path.Combine(PackageManager.AppPath, "profiles.json");
            var tmp = System.IO.Path.Combine(PackageManager.AppPath, ".profiles.json");
            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
                client.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => Completed(sender, e, LocalJson, tmp));
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler((sender, e) => ProgressChanged(sender, e, "Profiles"));
                client.DownloadFileAsync(uri, tmp);
            }
        }


        public static void GenerateList()
        {
            _listView.Children.Clear();
            foreach (var profile in profileList)
            {
                ProfileList item = new ProfileList();
                item.Title.Content = profile.Name;
                //item.Type.Content = app.Type;
                //TextHelper.SetText(item.Summary, app.Summary);
                item.image.Source = GetImage(profile.Video);
                item.Type.Content = profile.Supported_Devices;
                item.Author.Content = profile.Comms_Id;
                item.Tag = profileList.IndexOf(profile);
                item.MouseLeftButtonUp += Item_MouseLeftButtonUp;
                item.MouseEnter += Item_MouseEnter;
                item.MouseLeave += Item_MouseLeave;
                _listView.Children.Add(item);
            }

            _detailsView.Children.Clear();
            if (currentSelection >= 0)
                ShowDetails(currentSelection);

        }

        public static ImageSource GetImage(string video)
        {
            var youtube = video.Split('=')[1];
            return ImageHelper.GetImage(@"http://img.youtube.com/vi/" + youtube + @"/0.jpg");
        }

        public static void ShowDetails(int i)
        {
            currentSelection = i;
            var profile = profileList[i];
            _details = new ProfileDetails();
            _details.Title.Text = profile.Name;
            _details.Author.Text = profile.Comms_Id;
            _details.Description.Text = profile.Supported_Devices;
            _details.Image.Source = GetImage(profile.Video);
            if (profile.Download != null)
            {
                try
                {
                    Uri uri = new Uri(profile.Download);
                    string filename = Path.GetFileName(uri.LocalPath);

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
            //_details.Version.Text = app.Version;
            //HideRows(p);
            _detailsView.Children.Clear();
            _detailsView.Children.Add(_details);
        }

        private static void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var s = (Button)sender;
            int i = (int)s.Tag;
            var path = PackageManager.AppPath;

            Uri uri = new Uri(profileList[i].Download);
            string filename = Path.GetFileName(uri.LocalPath);
            string file = Path.Combine(path, filename);
            var p = PackageManager.GetPackage(file);
            if (p != null)
            {
                if (PackageManager.RemovePackage(p))
                {
                    MainBrowser._messageBox.Text = profileList[i].Name + " has been uninstalled successfully";
                    MainBrowser.removeMessage();
                }
                GenerateList();
                return;
            }

            s.Content = "Downloading...";
            s.IsEnabled = false;
            DownloadPackage(profileList[i]);
        }

        


        private static ProfileList _CurrentControl;

        private static void Item_MouseLeave(object sender, MouseEventArgs e)
        {
            var s = (ProfileList)sender;
            if (!s.Equals(_CurrentControl))
                s.Deselect();
        }
        

        private static void Item_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var s = (ProfileList)sender;
            if (_CurrentControl != null)
                _CurrentControl.Deselect();
            _CurrentControl = s;


            int i = (int)s.Tag;
            ShowDetails(i);
        }

        private static void Item_MouseEnter(object sender, MouseEventArgs e)
        {
            var s = (ProfileList)sender;
            s.Background.Opacity = 1;
        }

        private static void DownloadPackage(ChromaProfile app)
        {
            var path = PackageManager.AppPath;

            Uri uri = new Uri(app.Download);
            string filename = Path.GetFileName(uri.LocalPath);
            string file = Path.Combine(path, filename);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (File.Exists(file))
                File.Delete(file);
            string tmp = Path.Combine(path, "." + filename);

            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
                client.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => Completed(sender, e, file, tmp));
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler((sender, e) => ProgressChanged(sender, e, app.Name));
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
                MessageBox.Show(e.Error.Message);
                File.Delete(tmp);
                MainBrowser._messageBox.Text = e.Error.Message;
                GenerateList();
                return;
            }

            if (File.Exists(path))
                File.Delete(path);

            File.Move(tmp, path);


            if (path.Equals(LocalJson))
            {
                try
                {
                    JArray j = JArray.Parse(File.ReadAllText(LocalJson));
                    profileList = j.ToObject<List<ChromaProfile>>();
                }
                catch (Exception ex)
                {
                    App.Log.Error(ex);
                    return;
                }
                GenerateList();
                return;
            }

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

