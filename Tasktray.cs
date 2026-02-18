using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Restarter
{
    public partial class Tasktray : Component
    {
        private String appName;
        private String appIcon;
        private String exeName;
        private String exeArgs = "";

        private Process process;
        private Job job;
        private IconWatchdog watchdog;
        private NotifyIcon notifyIcon;

        private bool isRestarting = false;

        public Tasktray()
        {
            string[] args = Environment.GetCommandLineArgs();

            appName = args[1];
            appIcon = args[2];
            exeName = args[3];

            if (args.Length > 4)
            {
                exeArgs = args[4];
            }

            this.SetComponents();
            InitializeComponent();

            job = new Job();
            StartApp();

            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            Application.ApplicationExit += (s, e) =>
            {
                SystemEvents.PowerModeChanged -= OnPowerModeChanged;
                if (watchdog != null) watchdog.ReleaseHandle();
                if (job != null) job.Dispose();
            };

            _ = EnsureIconVisibleAsync();
        }

        private void SetComponents()
        {
            notifyIcon = new NotifyIcon();
            try { notifyIcon.Icon = new Icon(appIcon); } catch { }
            notifyIcon.Visible = true;
            notifyIcon.Text = appName + " Restarter";

            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();

            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString() ?? "N/A";

            ToolStripMenuItem versionItem = new ToolStripMenuItem();
            versionItem.Text = $"Restarter v{version}";
            versionItem.Enabled = false;

            ToolStripMenuItem restart = new ToolStripMenuItem();
            restart.Text = appName + " を再起動";
            restart.Click += (s, e) => { _ = RestartAppAsync(0); };

            ToolStripMenuItem exit = new ToolStripMenuItem();
            exit.Text = "終了";
            exit.Click += Exit_Click;

            contextMenuStrip.Items.AddRange(new ToolStripItem[] {
                versionItem,
                new ToolStripSeparator(),
                restart,
                exit 
            });
            notifyIcon.ContextMenuStrip = contextMenuStrip;

            watchdog = new IconWatchdog(() =>
            {
                if (notifyIcon != null)
                {
                    notifyIcon.Visible = false;
                    notifyIcon.Visible = true;
                }
            });
        }

        private void StartApp()
        {
            try
            {
                process = Process.Start(exeName, exeArgs);
                if (process != null)
                {
                    job.AddProcess(process.Handle);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"アプリの起動に失敗しました。\n{ex.Message}", "エラー");
            }
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                _ = RestartAppAsync(5000);
            }
        }

        private async Task RestartAppAsync(int delayMs)
        {
            if (isRestarting) return;
            isRestarting = true;

            try
            {
                if (delayMs > 0)
                {
                    await Task.Delay(delayMs);
                }

                if (process != null && !process.HasExited)
                {
                    try { process.Kill(); } catch { }
                    await Task.Delay(500);
                }

                StartApp();
            }
            finally
            {
                isRestarting = false;
            }
        }

        private async Task EnsureIconVisibleAsync()
        {
            await Task.Delay(3000);
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Visible = true;
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("終了しますか", appName + " Restarter", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }
            job.Dispose();
            Application.Exit();
        }

        private class IconWatchdog : NativeWindow
        {
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            static extern uint RegisterWindowMessage(string lpString);
            private uint wmTaskbarCreated;
            private Action onTaskbarCreated;

            public IconWatchdog(Action callback)
            {
                onTaskbarCreated = callback;
                wmTaskbarCreated = RegisterWindowMessage("TaskbarCreated");
                this.CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == wmTaskbarCreated)
                {
                    onTaskbarCreated?.Invoke();
                }
                base.WndProc(ref m);
            }
        }
    }
}