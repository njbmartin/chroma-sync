using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using System.Drawing;
using Newtonsoft.Json;
using Ultrabox.ChromaSync.Properties;
using System.Configuration;
using System.Windows.Forms;
using System.Diagnostics;
using Ultrabox.ChromaSync.Helpers;

namespace Ultrabox.ChromaSync
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
            public string Cmd { get; set; }
            public string File { get; set; }
            public string Description { get; set; }
            public Destination Destination { get; set; }

        }

        public class Destination
        {
            public string Type { get; set; }
            public string Folder { get; set; }
        }

        public static List<Package> packages;
        private static FileSystemWatcher watcher;


        public static void Start()
        {
            Watch();
        }

        public static void Watch()
        {
            watcher = new FileSystemWatcher();
            string path = @"%appdata%\ChromaSync";
            path = Environment.ExpandEnvironmentVariables(path);

            path = Path.Combine(path, "packages");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            //watcher.Created += new FileSystemEventHandler(OnChanged);
            //watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Debug.WriteLine("Changed");
            // TODO: ShowPackages(); -- Needs to use background worker
            GetPackages();
            // https://msdn.microsoft.com/en-us/library/waw3xexc(v=vs.110).aspx
        }

        public static List<Package> GetPackages()
        {
            packages = new List<Package>();
            App.NewPackagesContext();


            foreach (string st in Directory.GetFiles(Paths.Packages, "*.csp", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(st).StartsWith("."))
                    continue;
                Console.WriteLine("Found package:" + st);
                // Open the package for reading
                using (ZipArchive archive = ZipFile.OpenRead(st))
                {
                    var p = PackageDetails(archive);
                    p.Container = st;

                    var v = RegistryKeeper.GetValue(p.Container);
                    if (v == p.Product.Version)
                        p.Enabled = true;

                    if (p.Product.Name != null)
                    {
                        packages.Add(p);
                        

                    }

                }
            }

            return packages;
        }

        public static Package GetPackage(string container)
        {
            var p = packages.Find(x => x.Container == container);
            return p;
        }

        private static void MenuItem_Click(object sender, EventArgs e)
        {
            MenuItem s = (MenuItem)sender;
            InstallPackage((Package)s.Tag);
        }

        public static bool InstallPackage(Package p)
        {
            var message = MessageBox.Show("Would you like to install the package " + p.Product.Name + "?", "Chroma Sync: " + p.Product.Name,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (message == DialogResult.No)
                return false;
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
                                if (entry.FullName.StartsWith(step.Folder, StringComparison.OrdinalIgnoreCase) && entry.Name.Length > 0)
                                {
                                    switch (step.Action)
                                    {
                                        case "extract":
                                            var path = step.Destination.Folder;
                                            path = Environment.ExpandEnvironmentVariables(path);
                                            Console.WriteLine(entry.FullName);
                                            if (step.Destination.Type == "steamapp")
                                            {
                                                var steamFolder = GameLocator.InstallFolder(step.Destination.Folder);
                                                if (steamFolder == null)
                                                {
                                                    Console.WriteLine("Could not find steam folder: " + steamFolder);
                                                    return false;
                                                }
                                                path = steamFolder;
                                            }
                                            var sp = entry.FullName.Remove(0, step.Folder.Length + 1);
                                            var pa = Path.Combine(path, sp);
                                            if (!Directory.Exists(path))
                                                Directory.CreateDirectory(path);

                                            try
                                            {
                                                entry.ExtractToFile(pa, true);
                                            }
                                            catch (Exception e)
                                            {
                                                App.Log.Error(e);
                                            }
                                            break;

                                        case "execute":
                                            if (!entry.Name.Equals(step.File))
                                                continue;

                                            if (step.Description != null)
                                            {
                                                message = MessageBox.Show(step.Description, "Chroma Sync: " + p.Product.Name,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                            }
                                            else {
                                                message = MessageBox.Show("The following file needs to be installed: " + step.File + ". Would you like to continue?", "Chroma Sync: " + p.Product.Name,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                            }

                                            if (message == DialogResult.No)
                                                return false;
                                            Directory.CreateDirectory("tmp");
                                            var tmp = Path.Combine("tmp", entry.Name);
                                            try
                                            {
                                                entry.ExtractToFile(tmp);
                                            }
                                            catch (Exception e)
                                            {
                                                App.Log.Error(e);
                                            }

                                            System.Diagnostics.Process process = new System.Diagnostics.Process();
                                            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                                            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                            startInfo.FileName = entry.Name;
                                            // temp folder in the Chroma Sync directory
                                            startInfo.WorkingDirectory = "tmp";
                                            startInfo.Arguments = step.Cmd;
                                            process.StartInfo = startInfo;
                                            process.Start();
                                            process.WaitForExit();
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            RegistryKeeper.UpdateReg(p.Container, p.Product.Version);
            GetPackages();
            App.RestartServices();
            return true;
        }

        public static bool RemovePackage(Package p)
        {
            var message = MessageBox.Show("Are you sure you would like to remove the package " + p.Product.Name + "?", "Chroma Sync: " + p.Product.Name,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (message == DialogResult.No)
                return false;

            App.RestartServices();
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
                                if (entry.FullName.StartsWith(step.Folder, StringComparison.OrdinalIgnoreCase) && entry.Name.Length > 0)
                                {
                                    switch (step.Action)
                                    {
                                        case "extract":
                                            var path = step.Destination.Folder;
                                            path = Environment.ExpandEnvironmentVariables(path);
                                            Console.WriteLine(entry.FullName);
                                            if (step.Destination.Type == "steamapp")
                                            {

                                                var steamFolder = GameLocator.InstallFolder(step.Destination.Folder);
                                                if (steamFolder == null)
                                                {
                                                    Console.WriteLine("Could not find steam folder: " + steamFolder);
                                                    return false;
                                                }
                                                path = steamFolder;
                                            }
                                            var sp = entry.FullName.Remove(0, step.Folder.Length + 1);
                                            var pa = Path.Combine(path, sp);

                                            try
                                            {
                                                File.Delete(pa);
                                            }
                                            catch (Exception e)
                                            {
                                                App.Log.Error(e);
                                            }
                                            break;

                                        case "execute":
                                            if (!entry.Name.Equals(step.File))
                                                continue;


                                            var tmp = Path.Combine("tmp", entry.Name);
                                            try
                                            {
                                                File.Delete(tmp);
                                            }
                                            catch (Exception e)
                                            {
                                                App.Log.Error(e);
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            try
            {
                File.Delete(p.Container);
                RegistryKeeper.UpdateReg(p.Container, "");
                GetPackages();
                App.RestartServices();
                return true;
            }
            catch (Exception e)
            {
                App.Log.Error(e);
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