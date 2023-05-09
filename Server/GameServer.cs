using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using static Server.Encryption;

namespace Server {
    public partial class GameServer : Form
    {
        /* Used to enable Console
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
*/

        #region Black Title Bar
        [DllImport("DwmApi")]                                                                       // Black Title Bar 
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);
        protected override void OnHandleCreated(EventArgs e)
        {
            if (DwmSetWindowAttribute(Handle, 19, new[] { 1 }, 4) != 0)
                DwmSetWindowAttribute(Handle, 20, new[] { 1 }, 4);
        }
        #endregion
        #region Reduce Control Flicker
        protected override CreateParams CreateParams                                                // Reduce Control Flicker 
        {
            get {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }
        #endregion

        #region General Declarations
        private int DebugLVL = 1;                                                                   // <0|1|2> = Sets Debug console output level used in Stacktrace

        #region Network Settings
        private Task send = null;
        private static readonly string AeS = "bbroygbvgw202333bbce2ea2315a1916";                    // AES Key
        #endregion
        #endregion

        public GameServer(/*string PlayerName*/)                                                    // Main 
        {
            InitializeComponent();
            //AllocConsole();
            txtName.Text = Environment.UserName;

            /* // Opens multiple forms
            txtName.Text = PlayerName;
            if (PlayerName == "Host") {
                GameServer Frm1 = new("Player 1");
                Frm1.StartPosition = FormStartPosition.Manual;
                Frm1.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Frm1.Width, 0);
                Frm1.Show();

                GameServer Frm2 = new("Player 2");
                Frm2.StartPosition = FormStartPosition.Manual;
                Frm2.Location = new Point(0, Screen.PrimaryScreen.WorkingArea.Height - Frm2.Height);
                Frm2.Show();

                GameServer Frm3 = new("Player 3");
                Frm3.StartPosition = FormStartPosition.Manual;
                Frm3.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Frm3.Width, Screen.PrimaryScreen.WorkingArea.Height - Frm3.Height);
                Frm3.Show();
            }
            */

            #region Prep Drawing Objects
            BM = new(picDrawing.Width, picDrawing.Height);
            picDrawing.Image = BM;
            G = Graphics.FromImage(BM);
            G.Clear(Color.Transparent);
            #endregion
        }


        #region Form
        private void GameServer_Load(object sender, EventArgs e)                                    // On Open 
        {
            LoadSettings();

            LoadThemes();

            Darkmode(DarkMode);

            CreateTCM();
        }

        private void GameServer_FormClosing(object sender, FormClosingEventArgs e)                  // On Exit - Clean up - Save Settings 
        {
            if (connected) {
                Disconnect();
                listening = false;
            }
            if (connected) {
                clientObject.client.Close();
                connected = false;
            }

            picDrawing.Dispose();
            BM.Dispose();
            G.Dispose();

            DeleteTemps();
            SaveSettings();
        }
        #endregion


        #region Network
        // Ping
        private void Ping_Tick(object sender, EventArgs e)                                          // Timer for Pings 
        {
            if (listening) {
                for (int i = 1; i <= players.Count; i++) {
                    var pingCell = (DataGridViewTextBoxCell)clientsDataGridView.Rows[i].Cells[3];
                    pingCell.Value = Ping(GetPlayerAddress(i));
                }
            } else if (connected) {
                var pingCell = (DataGridViewTextBoxCell)clientsDataGridView.Rows[0].Cells[3];
                pingCell.Value = Ping(txtAddress.Text);
            }
        }
        static double Ping(string address)                                                          // Perform Ping Return average of 4 as Long 
        {
            long totalTime = 0;
            Ping ping = new();
            for (int i = 0; i < 4; i++) {
                PingReply reply = ping.Send(address, 999);
                if (reply.Status == IPStatus.Success) {
                    totalTime += reply.RoundtripTime;
                }
            }
            return totalTime / 4;
        }
        private string GetPlayerAddress(int id)                                                     // Get IP Address for Ping 
        {
            if (id != 0) {
                try {
                    players.TryGetValue(id, out MyPlayers obj);
                    if (obj == null) { return "127.0.0.1"; }
                    string ipAddress = ((IPEndPoint)obj.client.Client.RemoteEndPoint).Address.ToString();
                    return ipAddress;
                } catch (Exception ex) {
                    Console(ErrorMsg("GPA: " + ex.Message));
                    return "127.0.0.1";
                }
            } else return "127.0.0.1";

        }

        // Handshake
        private void ReadAuth(IAsyncResult result)                                                  // Handshake Readers 
        {
            if (listening) {                                                                        // Host read
                HostAuthReader(result);
            } else {                                                                                // Client Read
                ClientAuthReader(result);
            }
        }

        // Host and Client Read
        private void Read(IAsyncResult result)                                                      // Read Routine 
        {
            if (listening) HostStreamReader(result);                                                // Host stream reader
            else { ClientStreamReader(result); }                                                    // Client stream Reader
        }
        #endregion


        #region Routines 
        private void LoadSettings()                                                                 // Load settings from File 
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);// Retrieve the application data folder path
            string appFolder = Path.Combine(appDataFolder, "DoolittleInc\\GSDEV");                  // Create a directory for your application if it doesn't exist yet
            string settingsFilePath = Path.Combine(appFolder, $"config.txt");                       // File in the application folder

            if (File.Exists(settingsFilePath)) {                                                    // Check if the loadSettings file exists
                CustomizationSettings loadSettings = new();                                            // Create a new loadSettings object
                using StreamReader reader = new(settingsFilePath);                                // Read the loadSettings from the loadSettings file
                string line;
                while ((line = reader.ReadLine()) != null) {
                    string[] parts = line.Split('=');
                    string key = parts[0];
                    string value = parts[1];

                    switch (key) {
                        case "UserName":
                            loadSettings.UserName = value;
                            txtName.Text = loadSettings.UserName;
                            break;
                        case "LastLogin":
                            loadSettings.LastLogin = DateTime.Parse(value);
                            break;
                        case "IsDarkModeEnabled":
                            loadSettings.IsDarkModeEnabled = bool.Parse(value);
                            DarkMode = loadSettings.IsDarkModeEnabled;
                            break;
                        case "ThemeName":
                            loadSettings.ThemeName = value;
                            Theme = loadSettings.ThemeName;
                            break;
                        case "UserColor":
                            loadSettings.UserColor = value;
                            cmdColor.SelectedColor = ArgbColor(loadSettings.UserColor);
                            break;
                        case "RoomKey":
                            loadSettings.RoomKey = value;
                            txtRoomKey.Text = loadSettings.RoomKey;
                            break;
                        case "Address":
                            loadSettings.Address = value;
                            txtAddress.Text = loadSettings.Address;
                            break;
                        case "Port":
                            loadSettings.Port = value;
                            txtPort.Text = loadSettings.Port;
                            break;
                    }
                }
            } else {
                Console("Settings file not found - loading defaults.");                             // Output load fail
                Theme = "Default";
            }
        }
        private void SaveSettings()                                                                 // Save settings to File 
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);// Retrieve the application data folder path            
            string appFolder = Path.Combine(appDataFolder, "DoolittleInc\\GSDEV");                  // Create a directory for your application if it doesn't exist yet
            if (!Directory.Exists(appFolder)) {
                Directory.CreateDirectory(appFolder);
            }
            string settingsFilePath = Path.Combine(appFolder, $"config.txt");                       // File in the application folder

            Color pCol = cmdColor.SelectedColor;
            CustomizationSettings saveSettings = new() {
                UserName = txtName.Text.Trim(),
                UserColor = ColorToString(ref pCol),
                RoomKey = txtRoomKey.Text,
                Address = txtAddress.Text,
                Port = txtPort.Text,
                IsDarkModeEnabled = DarkMode,
                ThemeName = Theme,
                LastLogin = DateTime.Now
            };                                                // Settings to Save

            using StreamWriter writer = new(settingsFilePath);                                    // Save settings
            foreach (var prop in typeof(CustomizationSettings).GetProperties()) {
                string key = prop.Name;
                string value = prop.GetValue(saveSettings).ToString();
                writer.WriteLine(key + "=" + value);
            }
        }




        private string ColorToString(ref Color color)                                               // Converts Color to "A,R,G,B" 
        {
            int cA = color.A; int cB = color.B; int cR = color.R; int cG = color.G;
            string colorString = string.Format("{0},{1},{2},{3}", cA, cR, cG, cB);
            return colorString;
        }
        private Color ArgbColor(string argb)                                                        // Converts "A,R,G,B" to Color 
        {
            string[] ColorParts = argb.Split(',');                                                  // Seperate colour parts
            int ColA = Convert.ToInt32(ColorParts[0]);
            int ColR = Convert.ToInt32(ColorParts[1]);
            int ColG = Convert.ToInt32(ColorParts[2]);
            int ColB = Convert.ToInt32(ColorParts[3]);
            Color argbColor = Color.FromArgb(ColA, ColR, ColG, ColB);                               // Colour from ARGB
            return argbColor;
        }
        #endregion


        #region Shared Controls
        private void CbMask_CheckedChanged(object sender, EventArgs e)                              // Handles RoomKey Mask 
        {
            if (txtRoomKey.PasswordChar == '*') {
                txtRoomKey.PasswordChar = '\0';
            } else {
                txtRoomKey.PasswordChar = '*';
            }
        }


        private void ToggleNetworkControls()                                                        // Toggle controls for Host / Client 
        {
            txtAddress.Enabled = !txtAddress.Enabled;
            txtPort.Enabled = !txtPort.Enabled;
            txtName.Enabled = !txtName.Enabled;
            txtRoomKey.Enabled = !txtRoomKey.Enabled;

            cmdHost.Enabled = !cmdHost.Enabled;
            cmdJoin.Enabled = !cmdJoin.Enabled;

            tPing.Enabled = !tPing.Enabled;
        }
        #endregion


        #region Items Not Completed
        private void TabSections_DrawItem(object sender, DrawItemEventArgs e)                       // Trying to Draw custom Tabs 
        {
            /*            TabPage page = tabSections.TabPages[e.Index];
                        e.Graphics.FillRectangle(new SolidBrush(page.BackColor), e.Bounds);

                        Rectangle paddedBounds = e.Bounds;
                        int yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
                        paddedBounds.Offset(1, yOffset);
                        TextRenderer.DrawText(e.Graphics, page.Text, e.Font, paddedBounds, page.ForeColor);*/



            Font f;
            Brush backBrush;
            Brush foreBrush;
            f = e.Font;
            backBrush = new SolidBrush(Color.Red/*tabSections.BorderColor*/);
            foreBrush = new SolidBrush(e.ForeColor);

            string tabName = this.tabSections.TabPages[e.Index].Text;
            StringFormat sf = new() {
                Alignment = StringAlignment.Center
            };
            e.Graphics.FillRectangle(backBrush, tabSections.Bounds);
            e.Graphics.FillRectangle(backBrush, e.Bounds);
            Rectangle r = e.Bounds;
            r = new Rectangle(r.X, r.Y + 6, r.Width, r.Height - 3);
            e.Graphics.DrawString(tabName, f, foreBrush, r, sf);

            sf.Dispose();
            if (e.Index == this.tabSections.SelectedIndex) {
                //f.Dispose();
                backBrush.Dispose();
            } else {
                backBrush.Dispose();
                foreBrush.Dispose();
            }

        }

        #endregion
    }
}
/*
 * Snips
 
if (listening) { ;}
else if (connected) { ;}





*/