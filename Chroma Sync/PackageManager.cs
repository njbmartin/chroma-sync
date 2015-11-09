using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using System.Drawing;
using Newtonsoft.Json;
using ChromaSync.Properties;
using System.Configuration;

namespace ChromaSync
{



    public class PackageManager
    {
        public class Package
        {
            public Product Product { get; set; }
            public Image Image { get; set; }
            public string Container { get; set; }
            public List<InstallationStep> Installation { get; set; }
            public bool Installed { get; set; }
            public bool Enabled { get; set; }
        }

        public class Product
        {
            public string Name { get; set; }
            public string Author { get; set; }
            public string Description { get; set; }
            public string Version { get; set; }
            public string Type { get; set; }
        }

        public class InstallationStep
        {
            public string Action { get; set; }
            public string Folder { get; set; }
            public Destination Destination { get; set; }

        }

        public class Destination
        {
            public string Type { get; set; }
            public string Folder { get; set; }
        }

        public static List<Package> packages;

        public static void GetPackages()
        {
            packages = new List<Package>();
            if (!Directory.Exists("packages\\"))
                Directory.CreateDirectory("packages");

            foreach (string st in Directory.GetFiles("packages\\", "*.zip", SearchOption.AllDirectories))
            {
                Console.WriteLine("Found package:" + st);
                // Open the package for reading
                using (ZipArchive archive = ZipFile.OpenRead(st))
                {
                    var p = PackageDetails(archive);
                    p.Container = st;


                    p.Enabled = RegistryKeeper.CheckActive(p.Container);
                    RegistryKeeper.UpdateReg(p.Container, !p.Enabled);

                    if (p.Product.Name != null)
                        packages.Add(p);
                    InstallPackage(p);
                }
            }
        }

        public static bool InstallPackage(Package p)
        {
            foreach (var step in p.Installation)
            {

                using (ZipArchive archive = ZipFile.OpenRead(p.Container))
                {
                    if (p.Product.Name != null)
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (step.Folder != null)
                            {
                                if (entry.FullName.StartsWith(step.Folder, StringComparison.OrdinalIgnoreCase))
                                {
                                    switch (step.Action)
                                    {
                                        case "extract":
                                            var path = step.Destination.Folder;
                                            Console.WriteLine(entry.FullName);
                                            if (step.Destination.Type == "steamapp")
                                            {
                                                var steamFolder = GameLocator.InstallFolder("Counter-Strike Global Offensive");
                                                if (steamFolder == null)
                                                {
                                                    Console.WriteLine("Could not find folder: " + steamFolder);
                                                    return false;
                                                }
                                                path = steamFolder;
                                            }

                                            var pa = Path.Combine(path, entry.FullName);
                                            try {
                                                entry.ExtractToFile(pa);
                                            }
                                            catch(Exception e)
                                            {
                                                Console.WriteLine(e.Message);
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // No product thingy
                    }
                }
            }
            return false;
        }

        private static Package PackageDetails(ZipArchive archive)
        {
            var p = new Package();
            var details = archive.GetEntry("details.json");
            if (details != null)
            {
                using (StreamReader sr = new StreamReader(details.Open()))
                {

                    JObject j = JObject.Parse(sr.ReadToEnd());
                    p = j.ToObject<Package>();

                    var image = archive.GetEntry("capsule.jpg");
                    if (image != null)
                        p.Image = Image.FromStream(image.Open());
                }
            }

            return p;
        }

    }
}