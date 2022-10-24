using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RestartYamabuki
{

    public partial class Tasktray : Component
    {
        String appName = Environment.GetCommandLineArgs()[1];

        String appIcon = Environment.GetCommandLineArgs()[2];

        String exeName = Environment.GetCommandLineArgs()[3];

        Process process;

        public Tasktray()
        {
            if (Environment.GetCommandLineArgs().Length < 4)
            {
                MessageBox.Show("Usage: restarter.exe App_name ICO_file EXE_file", "Restarter");
                Application.Exit();
            }

            this.setComponents();
            InitializeComponent();
            process = Process.Start(exeName);
        }

        private void setComponents()
        {
            NotifyIcon notifyIcon = new();
            notifyIcon.Icon = new Icon(appIcon);
            notifyIcon.Visible = true;
            notifyIcon.Text = appName + " Restarter";

            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();

            ToolStripMenuItem exit = new ToolStripMenuItem();
            exit.Text = "終了";
            exit.Click += exit_Click;

            ToolStripMenuItem restart = new ToolStripMenuItem();
            restart.Text = appName + " を再起動";
            restart.Click += restart_Click;

            contextMenuStrip.Items.AddRange(new
                System.Windows.Forms.ToolStripItem[] {restart, exit});
            notifyIcon.ContextMenuStrip = contextMenuStrip;
        }

        private void restart_Click(object sender, EventArgs e)
        {
            process.Kill();
            process = Process.Start(exeName);
        }
        private void exit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("終了しますか", appName + " Restarter", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }
            // アプリケーションの終了
            process.Kill();
            Application.Exit();
        }
    }
}
