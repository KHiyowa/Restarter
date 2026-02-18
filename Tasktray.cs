using Restarter.Killer;
using Restarter.Killer.Impl;
using System.ComponentModel;
using System.Diagnostics;

namespace RestartYamabuki
{

    public partial class Tasktray : Component
    {
        private String appName;
        private String appIcon;
        private String exeName;

        readonly Killer killer = new ProcessTreeKiller();
        Process process;

        public Tasktray()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length < 4)
            {
                MessageBox.Show("Usage: restarter.exe App_name ICO_file EXE_file", "Restarter");
                Application.Exit();
                return;
            }

            appName = args[1];
            appIcon = args[2];
            exeName = args[3];

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
