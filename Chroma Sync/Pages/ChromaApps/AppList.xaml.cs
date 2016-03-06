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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ultrabox.ChromaSync.Pages
{
    /// <summary>
    /// Interaction logic for ListItemControl.xaml
    /// </summary>
    public partial class AppList : UserControl
    {

        public bool isSelected;

        public AppList()
        {
            InitializeComponent();
            Background.Opacity = 0;
        }

        public void Deselect()
        {
            Background.Opacity = 0;
        }
    }
}
