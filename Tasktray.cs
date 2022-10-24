using Microsoft.VisualBasic;
using Restarter.Killer;
using Restarter.Killer.Impl;
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
        readonly String appName = Environment.GetCommandLineArgs()[1];
        readonly String appIcon = Environment.GetCommandLineArgs()[2];
        readonly String exeName = Environment.GetCommandLineArgs()[3];

        readonly Killer killer = new ProcessTreeKiller();
        Process process;

        public Tasktray()
        {
            if (Environment.GetCommandLineArgs().Length < 4)
            {
                MessageBox.Show("Usage: restarter.exe App_name ICO_file EXE_file", "Restarter");
                Application.Exit();
            }

            this.SetComponents();
            InitializeComponent();
            process = Process.Start(exeName);
        }

        private void SetComponents()
        {
            NotifyIcon notifyIcon = new();
            notifyIcon.Icon = new Icon(appIcon);
            notifyIcon.Visible = true;
            notifyIcon.Text = appName + " Restarter";

            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();

            ToolStripMenuItem exit = new ToolStripMenuItem();
            exit.Text = "終了";
            exit.Click += Exit_Click;

            ToolStripMenuItem restart = new ToolStripMenuItem();
            restart.Text = appName + " を再起動";
            restart.Click += Restart_Click;

            contextMenuStrip.Items.AddRange(new
                System.Windows.Forms.ToolStripItem[] {restart, exit});
            notifyIcon.ContextMenuStrip = contextMenuStrip;
        }

        private void Restart_Click(object sender, EventArgs e)
        {
            killer.Kill(process);
            process = Process.Start(exeName);
        }
        private void Exit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("終了しますか", appName + " Restarter", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }
            // アプリケーションの終了
            killer.Kill(process);
            Application.Exit();
        }
    }
}
