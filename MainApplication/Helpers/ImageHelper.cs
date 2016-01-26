using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ultrabox.ChromaSync.Helpers
{
    class ImageHelper
    {

        internal static ImageSource GetImage(string url)
        {
            ImageSource imgsr = new BitmapImage(new Uri(url));
            return imgsr;
        }

    }
}
