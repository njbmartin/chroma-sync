using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultrabox.ChromaSync.Models
{
    public class Script
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
}
