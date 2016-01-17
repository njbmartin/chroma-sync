using System;
using System.Collections.Generic;
using System.Linq;
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
        public MainBrowser()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            for (var i = 0; i < 10; i++)
            {
                ListItemControl item = new ListItemControl();
                item.Title.Content = "CS:GO Chromatic";
                item.Description.Text = "Provides awesome effects for CSGO";
                ListView.Children.Add(item);
            }

            DetailsControl details = new DetailsControl();
            details.Title.Text = "CS:GO Chromatic";
            details.Description.Text = "Provides awesome effects for CSGO";
            DetailsView.Children.Add(details);
        }


    }
}
