using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace ChromaSync
{
    public partial class Form1 : Form
    {
        private string AppName = "Chroma Sync";

        public Form1()
        {
            InitializeComponent();
            GetStartup();
            Watch();
        }

        public void Watch()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();

            watcher.Path = Directory.GetCurrentDirectory() + "\\packages";
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
            ShowPackages();
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Debug.WriteLine("Changed");
            // TODO: ShowPackages(); -- Needs to use background worker
            ShowPackages();
            // https://msdn.microsoft.com/en-us/library/waw3xexc(v=vs.110).aspx
        }

        private void ShowPackages()
        {
            packageList.Items.Clear();
            PackageManager.GetPackages();
            var packages = PackageManager.packages;
            foreach (var package in packages)
            {
                Debug.WriteLine(package.Name);
                var item = new ListViewItem();
                item.ToolTipText = package.Description;
                item.ForeColor = Color.DeepPink;
                item.Font = new Font(item.Font,
                    item.Font.Style | FontStyle.Bold);

                item.SubItems.Add(package.Author);
                item.SubItems.Add(package.Type);
                item.SubItems.Add(package.Version);
                item.Text = package.Name;

                if (package.Image != null)
                {
                    packageList.LargeImageList.Images.Add(package.Name, package.Image);
                    item.ImageKey = package.Name;
                }

                packageList.Items.Add(item);
            }
        }

        private void SetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (checkBox1.Checked)
                rk.SetValue(AppName, Application.ExecutablePath.ToString());
            else
                rk.DeleteValue(AppName, false);
        }

        private void GetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rk.GetValue(AppName) != null)
                checkBox1.Checked = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }



        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var f = Environment.CurrentDirectory + "\\scripts";
            Process.Start("explorer.exe", f);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SetStartup();
        }
    }
}
