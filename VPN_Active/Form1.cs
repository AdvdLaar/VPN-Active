using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Win32;

namespace VPN_Active
{
    public partial class Main : Form
    {
        public static Main It;   // Singleton.
        public String ExternalHostName = "";
        string RegistryApplicationKey = "";
        string IPNumber = "";
        const double TIMEOUT = 500; // milliseconds

        public Main()
        {
            InitializeComponent();
            It = this;
            RegistryApplicationKey = "SOFTWARE\\" + Application.ProductName;
            NetworkChange.NetworkAddressChanged += new
            NetworkAddressChangedEventHandler(AddressChangedCallback);
            notifyIcon1.Icon = Properties.Resources.VPN_blue;
            toolStripStatusLabel1.ForeColor = Color.Black;
            toolStripStatusLabel1.Text = "";
            autoStartAtWindowsStartupToolStripMenuItem.Checked = CheckStartup();
            autoStartAtWindowsStartupToolStripMenuItem.Enabled = true;
            enableSoundsWhenConnectionChangesToolStripMenuItem.Checked = GetBoolValue("PlaySound", true);
            enableSoundsWhenConnectionChangesToolStripMenuItem.Enabled = true;
            string[] args = Environment.GetCommandLineArgs();
            if ((args.Length==2) && (args[1]=="/minimize"))
            {
                timer2.Enabled = true;
            }
            RestartTimer();
            textBox1.Text = GetStringValue("NoVPN", "operator");
            textBox1.TextChanged += textBox1_TextChanged;
        }

        private void RestartTimer()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(delegate
                {
                    RestartTimer();
                }));
                return;
            }
            timer1.Stop();
            timer1.Start();
        }

        private bool StoreBoolValue(string ValueStr, Boolean Bool)
        {
            bool ret = false;
            string DefaultStr = "False";
            Boolean StoredValue;
            RegistryKey rkey = Registry.CurrentUser.CreateSubKey(RegistryApplicationKey);
            if (rkey != null)
            {
                if (Bool)
                {
                    DefaultStr = "False";
                }
                else
                {
                    DefaultStr = "True";
                }
                StoredValue = Convert.ToBoolean(rkey.GetValue(ValueStr, DefaultStr));
                if (Bool != StoredValue) rkey.SetValue(ValueStr, Bool);
                return (true);
            }
            return ret;
        }

        private Boolean GetBoolValue(string ValueStr, Boolean Default)
        {
            string DefaultStr = "False";
            bool Result = Default;
            RegistryKey rkey = Registry.CurrentUser.OpenSubKey(RegistryApplicationKey);
            if (rkey != null)
            {
                if (Default) DefaultStr = "True";
                Result = Convert.ToBoolean(rkey.GetValue(ValueStr, DefaultStr));
            }
            return (Result);
        }

        private void SetStartup(bool Enable)
        {
            string AppName = Application.ProductName;
            string Args = "/minimize";
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (Enable)
            {
                if (Args != "")
                {
                    rk.SetValue(AppName, "\"" + Application.ExecutablePath + "\" /minimize");
                }
                else
                {
                    rk.SetValue(AppName, Application.ExecutablePath);
                }
            }
            else
                rk.DeleteValue(AppName, false);
            rk.Close();
        }

        private bool CheckStartup()
        {
            object CheckRegistry;
            string AppName = Application.ProductName;
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
            CheckRegistry = rk.GetValue(AppName);
            rk.Close();
            return(CheckRegistry != null);
        }

        private bool StoreStringValue(string ValueStr, string Str)
        {
            string StoredValue;
            RegistryKey rkey = Registry.CurrentUser.CreateSubKey(RegistryApplicationKey);
            if (rkey != null)
            {
                StoredValue = (string)rkey.GetValue(ValueStr, Str + "!");
                if (Str != StoredValue) rkey.SetValue(ValueStr, Str);
                return (true);
            }
            return false;
        }

        private string GetStringValue(string ValueStr, string Default)
        {
            string Result = Default;
            RegistryKey rkey = Registry.CurrentUser.OpenSubKey(RegistryApplicationKey);
            if (rkey != null)
            {
                Result = (string)rkey.GetValue(ValueStr, Default);
            }
            return (Result);
        }

        public static string GetExternalIPAndHostName()
        {
            try
            {
                It.IPNumber = "";
                string url = "http://checkip.dyndns.org";
                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                string response = sr.ReadToEnd().Trim();
                string[] a = response.Split(':');
                string a2 = a[1].Substring(1);
                string[] a3 = a2.Split('<');
                string a4 = a3[0];
                It.IPNumber = a4;
                // return a4;
                var timeout = TimeSpan.FromSeconds(0.2);
                var task = Dns.GetHostEntryAsync(a4);
                if (!task.Wait(timeout))
                {
                    return ("");
                }
                return (task.Result.HostName);
            }
            catch
            {
                It.timer3.Start();
                return ("");
            }
        }

        public static void UpdateIcons(Icon NewIcon)
        {
            if (It.InvokeRequired)
            {
                It.BeginInvoke(new Action(delegate
                {
                    UpdateIcons(NewIcon);
                }));
                return;
            }
            else
            {
                It.Icon = NewIcon;
                It.notifyIcon1.Icon = NewIcon;
            }
        }

        public void AddressChanged()
        {
            ExternalHostName = GetExternalIPAndHostName();
            if (It.IPNumber != "")
            {
                if (It.CheckActiveVPN(It.ExternalHostName))
                {
                    It.toolStripStatusLabel1.ForeColor = Color.DarkGreen;                    
                    if (It.ExternalHostName == "")
                    {
                        It.toolStripStatusLabel1.Text = It.IPNumber;
                        It.notifyIcon1.Text = It.IPNumber;
                        It.notifyIcon1.BalloonTipText = It.IPNumber;                        
                    }
                    else
                    {
                        It.toolStripStatusLabel1.Text = It.ExternalHostName;
                        It.notifyIcon1.Text = It.ExternalHostName;
                        It.notifyIcon1.BalloonTipText = It.ExternalHostName;
                    }
                    UpdateIcons(Properties.Resources.VPN_green);
                    if (enableSoundsWhenConnectionChangesToolStripMenuItem.Checked)
                    {
                        System.Media.SystemSounds.Hand.Play();
                    }
                }
                else
                {
                    It.toolStripStatusLabel1.ForeColor = Color.DarkRed;
                    It.toolStripStatusLabel1.Text = It.ExternalHostName;
                    It.notifyIcon1.Text = It.ExternalHostName;
                    It.notifyIcon1.BalloonTipText = It.ExternalHostName;
                    It.notifyIcon1.ShowBalloonTip(5000);
                    UpdateIcons(Properties.Resources.VPN_red);
                    if (enableSoundsWhenConnectionChangesToolStripMenuItem.Checked)
                    {
                        System.Media.SystemSounds.Exclamation.Play();
                    }
                }
            }
            else
            {
                UpdateIcons(Properties.Resources.VPN_blue);
                It.notifyIcon1.BalloonTipText = "";
                It.toolStripStatusLabel1.ForeColor = Color.Black;
                It.toolStripStatusLabel1.Text = "";
                It.notifyIcon1.Text = "";
            }
        }

        static void AddressChangedCallback(object sender, EventArgs e)
        {
            It.RestartTimer();
        }

        public bool CheckActiveVPN(String CurrentExternalHostName)
        {
            string[] words;
            int Count;
            bool VPN = true;
            String Hosts = It.textBox1.Text;
            words = Hosts.Split(',');
            Count = 0;
            if (words.Length > 0)
            {
                while ((VPN) && (Count<words.Length))
                {
                    VPN = !(CurrentExternalHostName.Contains(words[Count++]));
                }
            }
            return (VPN);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RestartTimer();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(delegate
                {
                    timer1_Tick(sender,e);
                }));
                return;
            }
            timer1.Stop();
            AddressChanged();
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                this.Hide();
            }
            if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }                
            RestartTimer();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
            }
            RestartTimer();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (System.Windows.Forms.Application.MessageLoop)
            {
                // WinForms app
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                // Console app
                System.Environment.Exit(1);
            }
        }

        private void autoStartAtWindowsStartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetStartup(autoStartAtWindowsStartupToolStripMenuItem.Checked);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Stop();
            this.WindowState = FormWindowState.Minimized;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            StoreStringValue("NoVPN", textBox1.Text);
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            timer3.Stop();
            RestartTimer();
        }

        private void enableSoundsWhenConnectionChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StoreBoolValue("PlaySound", enableSoundsWhenConnectionChangesToolStripMenuItem.Checked);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutWindow = new AboutBox1();
            aboutWindow.Show();
        }
    }
}
