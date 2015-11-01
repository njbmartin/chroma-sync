using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Drawing;

namespace ChromaSync
{



    public class PackageManager
    {
        public class Package
        {
            public string Name { get; set; }
            public string Author { get; set; }
            public string Description { get; set; }
            public string Version { get; set; }
            public Image Image { get; set; }
        }

        public static List<Package> packages;

        public static void GetPackages()
        {
            packages = new List<Package>();
            if (!Directory.Exists("packages\\"))
                Directory.CreateDirectory("packages");

            foreach (string st in Directory.GetFiles("packages\\", "*.zip", SearchOption.AllDirectories))
            {
                // Open the package for reading
                using (ZipArchive archive = ZipFile.OpenRead(st))
                {
                    var p = new Package();
                    var details = archive.GetEntry("details.json");
                    if (details != null)
                    {
                        using (StreamReader sr = new StreamReader(details.Open()))
                        {
                            var j = JObject.Parse(sr.ReadToEnd());
                            p.Name = j["name"].ToString();
                            p.Author = j["author"].ToString();
                            p.Description = j["description"].ToString();
                        }
                    }

                    var image = archive.GetEntry("image.jpg");
                    if (image != null)
                        p.Image = Image.FromStream(image.Open());

                    if (p.Name != null)
                        packages.Add(p);
                }
            }
        }
    }
}