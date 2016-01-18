using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

            DetailsControl details = new DetailsControl();
            details.Title.Text = packages[0].Name;
            details.Author.Text = packages[0].Author;
            details.Description.Text = packages[0].Description;
            details.Version.Text = packages[0].Version;
            details.Image.Source = GetImage(packages[0].ImageURL);
            DetailsView.Children.Add(details);
        }



        private void Clicked(object sender, EventArgs e)
        {
            var s = (ListItemControl)sender;
            int i = (int)s.Tag;
            DetailsControl details = new DetailsControl();
            details.Title.Text = packages[i].Name;
            details.Author.Text = packages[i].Author;
            details.Image.Source = GetImage(packages[i].ImageURL);
            details.Description.Text = packages[i].Description;
            details.Version.Text = packages[i].Version;
            DetailsView.Children.Clear();
            DetailsView.Children.Add(details);
        }

        private ImageSource GetImage(string url)
        {

            ImageSource imgsr = new BitmapImage(new Uri(url));
            return imgsr;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            GetPackages();
        }
    }
}
