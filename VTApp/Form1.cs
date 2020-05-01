using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            if (System.Windows.Application.Current == null)
            {
                app = new VirusTotalUI.App()
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };
                app.InitializeComponent();
            }
            else
            {
                app = (VirusTotalUI.App)System.Windows.Application.Current;
              //  app.MainWindow = new VirusTotalUI.Views.MainWindow();
                ElementHost.EnableModelessKeyboardInterop(app.MainWindow);
                app.MainWindow.Show();
            }
        }
    }
}
