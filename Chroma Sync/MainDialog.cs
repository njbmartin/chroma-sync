using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ChromaSync
{
    public partial class Form1 : Form
    {
        private string AppName = "Chroma Sync";

        public Form1()
        {
            InitializeComponent();
            GetStartup();
            ShowPackages();
        }

        private void ShowPackages()
        {
            packageList.Items.Clear();
            var packages = PackageManager.packages;
            foreach (var package in packages)
            {
                Console.WriteLine(package.Name);
                var item = new ListViewItem();
                item.ToolTipText = package.Description;
                item.ForeColor = Color.DeepPink;
                item.Font= new Font(item.Font,
       item.Font.Style | FontStyle.Bold);
                item.SubItems.Add(package.Author);
                
                item.Text = package.Name;

                if (package.Image != null)
                {
                    packageList.LargeImageList.Images.Add(package.Name, package.Image);
                    item.ImageKey =package.Name;
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

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SetStartup();
        }
    }
}
