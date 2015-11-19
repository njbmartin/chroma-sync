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
            //ShowPackages();
            // https://msdn.microsoft.com/en-us/library/waw3xexc(v=vs.110).aspx
        }

        private void ShowPackages()
        {
            packageList.Items.Clear();
            PackageManager.GetPackages();
            var packages = PackageManager.packages;
            foreach (var package in packages)
            {
                Debug.WriteLine(package.Product.Name);
                var item = new ListViewItem();
                item.ToolTipText = package.Product.Description;
                item.ForeColor = Color.DeepPink;
                item.Font = new Font(item.Font,
                    item.Font.Style | FontStyle.Bold);

                item.SubItems.Add(package.Product.Author);
                item.SubItems.Add(package.Product.Type);
                item.SubItems.Add(package.Product.Version);
                item.Text = package.Product.Name;

                if (package.Image != null)
                {
                    packageList.LargeImageList.Images.Add(package.Product.Name, package.Image);
                    item.ImageKey = package.Product.Name;
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

        private void listView1_SelectedIndexChanged(object sender, MouseEventArgs e)
        {
            Console.WriteLine("changed");
            var senderList = (ListView)sender;
            var clickedItem = senderList.HitTest(e.Location).Item;
            if (clickedItem != null)
            {
                //do something
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ShowPackages();
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

        private void packageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection selected = packageList.SelectedItems;
            
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            LuaScripting.ReloadScripts();
        }
    }
}
