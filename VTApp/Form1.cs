using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace VTApp
{
    public partial class Form1 : Form
    {
        VirusTotalUI.App app;
        public Form1()
        {
            InitializeComponent();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //ProcessStartInfo processStartInfo=new ProcessStartInfo()
            //{
            //    ErrorDialog = true,
            //    UseShellExecute = true,
            //    Verb ="runas"
            //}
            string filePath = @"C:\Windows\System32\@AppHelpToast.png";

            if (File.Exists(filePath))
            {
                Console.WriteLine("");
            }
            Process p = new Process();
            p.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "VirusTotalUI.exe");
            p.StartInfo.Arguments = $"\"{filePath}\" \"Key.txt\" \".8\" \".25\" \".599\"";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.Verb = "runas";
            p.Start();
            p.WaitForExit();
        }
    }
}
