using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ultrabox.ChromaSync.Helpers;
using Ultrabox.ChromaSync.Models;
using Ultrabox.ChromaSync.Pages;

namespace Ultrabox.ChromaSync.Controllers
{
    class GIController
    {

        private static StackPanel _listView;
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
            _detailsView.Children.Clear();
            _detailsView.Children.Add(_details);
        }

        private static void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var s = (Button)sender;
            int i = (int)s.Tag;
            var path = PackageManager.AppPath;

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
            //DownloadPackage(gameIntegrationList[i]);
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

    }

}

