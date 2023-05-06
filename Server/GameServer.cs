using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Unclassified.UI;
using static Server.Encryption;

namespace Server
{
    public partial class GameServer : Form
    {
        /* Used to enable Console
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
*/


        [DllImport("DwmApi")]                                                                       // Black Title Bar 
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);
        protected override void OnHandleCreated(EventArgs e)
        {
            if (DwmSetWindowAttribute(Handle, 19, new[] { 1 }, 4) != 0)
                DwmSetWindowAttribute(Handle, 20, new[] { 1 }, 4);
        }

        #region Declarations
        #region Host Specific Declarations
        private bool listening = false;                                                             // Host / Client Mode Marker
        private Thread listener = null;                                                             // Host TCP Listener
        private Thread disconnect = null;                                                           // Manage Disconnects
        public class PlayerPackage                                                                  // To Broadcast to Clients 
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int[] Color { get; set; }
        }
        public class MyPlayers                                                                      // Handles Connections 
        {
#pragma warning disable IDE1006                             // Naming Styles
            public long id { get; set; }
            public Color color { get; set; }
            public StringBuilder username { get; set; }
            public TcpClient client { get; set; }
            public NetworkStream stream { get; set; }
            public byte[] buffer { get; set; }
            public StringBuilder data { get; set; }
            public EventWaitHandle handle { get; set; }
#pragma warning restore IDE1006                             // Naming Styles
        };
        private static readonly ConcurrentDictionary<long, MyPlayers> players = new();
        #endregion

        #region Client Specific Declarations
        private bool connected = false;                                                             // Client / Host Mode Marker
        private Thread client = null;                                                               // The Client TCP
        private class Client                                                                        // Client information to send to Host 
        {
            public string username;
            public string key;
            public Color color;
            public TcpClient client;
            public NetworkStream stream;
            public byte[] buffer;
            public StringBuilder data;
            public EventWaitHandle handle;
        };
        private Client clientObject;                                                                // The Client Object
        #endregion Declarations

        #region Drawing Delclarations
        public class DrawPackage                                                                    // Details needed to Draw and Broadcast 
        {
            public string PenColor { get; set; }
            public string BrushColor { get; set; }
            public string DrawType { get; set; }
            public Point[] PPath { get; set; }
            public bool Fill { get; set; }
            public int PenSize { get; set; }
            public Point PT1 { get; set; }
            public Point PT2 { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int PT1X { get; set; }
            public int PT1Y { get; set; }
            public int PT2X { get; set; }
            public int PT2Y { get; set; }
        }
        public class FillPackage                                                                    // Details needed to Fill and Broadcast 
        {
            public string FillColor { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }

        readonly Bitmap BM;                                                                         // The Drawing
        Graphics G;                                                                                 // The Canvas
        GraphicsPath mPath;                                                                         // Pen Path
        Point PT2, PT1;                                                                             // Points for Drawing
        int x, y, PT1X, PT1Y, PT2X, PT2Y;                                                           // Math Points
        private bool Drawing = false;                                                               // Local Drawing Toggle
        private bool remoteDraw = false;                                                            // Remote Drawing Toggle
        private Color _preBC;                                                                       // BG replace
        #endregion

        #region General Declarations
        private int DebugLVL = 1;                                                                   // <0|1|2> = Sets Debug console output level used in Stacktrace
        private class CustomizationSettings                                                         // Details save to and loaded from File 
        {
            public string UserName { get; set; }
            public string UserColor { get; set; }
            public string RoomKey { get; set; }
            public string Address { get; set; }
            public string Port { get; set; }
            public string ThemeName { get; set; }
            public bool IsDarkModeEnabled { get; set; }
            public DateTime LastLogin { get; set; }
        }

        #region Theme Settings
        private bool DarkMode { get; set; }
        private static string Theme { get; set; }
        enum ThemeColor
        {
            Primary,
            Secondary,
            Tertiary,
            Quaternary
        }
        private static readonly Dictionary<string, Dictionary<ThemeColor, Color>> Themes = new(StringComparer.OrdinalIgnoreCase);
        // Dictionary<string, Dictionary<ThemeColor, Color>> Themes = new();
        #endregion

        #region Message Settings
        private class MessagePackage                                                                // Details needed to Draw and Broadcast 
        {
            public string Msg { get; set; }
            public string From { get; set; }
            public string To { get; set; }
            public string MsgType { get; set; }
        }
        private bool remoteMsg = false;
        private TabPage pmTab;
        private readonly List<string> MSGHistory = new();                                           // CLI History
        private int HistoryIndex = -1;
        #endregion

        #region Network Settings
        private Task send = null;
        private static readonly string AeS = "bbroygbvgw202333bbce2ea2315a1916";                    // AES Key
        #endregion


        #endregion
        #endregion


        #region Form
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

            #region Drawing 
            BM = new(picDrawing.Width, picDrawing.Height);
            picDrawing.Image = BM;
            G = Graphics.FromImage(BM);
            G.Clear(Color.Transparent);
            #endregion
        }
        protected override CreateParams CreateParams                                                // Reduce Control Flicker 
        {
            get {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        private void GameServer_Load(object sender, EventArgs e)                                    // On Open 
        {
            LoadSettings();

            #region Themes
            Themes.Add("Nature", new Dictionary<ThemeColor, Color>
            {
                { ThemeColor.Primary, Color.Green },
                { ThemeColor.Secondary, Color.Brown },
                { ThemeColor.Tertiary, Color.YellowGreen },
                { ThemeColor.Quaternary, Color.DarkSeaGreen }
            });
            Themes.Add("Aqua", new Dictionary<ThemeColor, Color>
            {
                { ThemeColor.Primary, Color.Blue },
                { ThemeColor.Secondary, Color.Aquamarine },
                { ThemeColor.Tertiary, Color.DarkSeaGreen },
                { ThemeColor.Quaternary, Color.Turquoise }
            });
            Themes.Add("Greens", new Dictionary<ThemeColor, Color>
            {
                { ThemeColor.Primary, Color.ForestGreen },
                { ThemeColor.Secondary, Color.DarkGreen },
                { ThemeColor.Tertiary, Color.YellowGreen },
                { ThemeColor.Quaternary, Color.LimeGreen }
            });
            Themes.Add("Rose", new Dictionary<ThemeColor, Color>
            {
                { ThemeColor.Primary, Color.DarkRed },
                { ThemeColor.Secondary, Color.MistyRose },
                { ThemeColor.Tertiary, Color.Red },
                { ThemeColor.Quaternary, Color.RosyBrown }
            });
            Themes.Add("Yellows", new Dictionary<ThemeColor, Color>
            {
                { ThemeColor.Primary, Color.DarkKhaki },
                { ThemeColor.Secondary, Color.Gold },
                { ThemeColor.Tertiary, Color.Yellow },
                { ThemeColor.Quaternary, Color.LightYellow }
            });
            Themes.Add("Blues", new Dictionary<ThemeColor, Color>
            {
                { ThemeColor.Primary, Color.Blue },
                { ThemeColor.Secondary, Color.LightSkyBlue },
                { ThemeColor.Tertiary, Color.LightSteelBlue },
                { ThemeColor.Quaternary, Color.DarkBlue }
            });
            Themes.Add("Browns", new Dictionary<ThemeColor, Color>
            {
                { ThemeColor.Primary, Color.SaddleBrown },
                { ThemeColor.Secondary, Color.Chocolate },
                { ThemeColor.Tertiary, Color.Brown },
                { ThemeColor.Quaternary, Color.Tan }
            });
            Themes.Add("Oranges", new Dictionary<ThemeColor, Color>
            {
                { ThemeColor.Primary, Color.DarkOrange },
                { ThemeColor.Secondary, Color.OrangeRed },
                { ThemeColor.Tertiary, Color.Orange },
                { ThemeColor.Quaternary, Color.Goldenrod }
            });
            Themes.Add("Default", new Dictionary<ThemeColor, Color>
            {
                { ThemeColor.Primary, SystemColors.WindowText },
                { ThemeColor.Secondary, SystemColors.Window },
                { ThemeColor.Tertiary, SystemColors.ControlText },
                { ThemeColor.Quaternary, SystemColors.Control }
            });
            Themes.Add("BlackRed", new Dictionary<ThemeColor, Color>
{
                { ThemeColor.Primary, Color.Black },
                { ThemeColor.Secondary, Color.Red },
                { ThemeColor.Tertiary, Color.Black },
                { ThemeColor.Quaternary, Color.LightSalmon }
            });
            Themes.Add("BlackGreen", new Dictionary<ThemeColor, Color>
{
                { ThemeColor.Primary, Color.Black },
                { ThemeColor.Secondary, Color.Green },
                { ThemeColor.Tertiary, Color.Black },
                { ThemeColor.Quaternary, Color.LimeGreen }
            });
            Themes.Add("BlackBlue", new Dictionary<ThemeColor, Color>
{
                { ThemeColor.Primary, Color.Black },
                { ThemeColor.Secondary, Color.Blue },
                { ThemeColor.Tertiary, Color.Black },
                { ThemeColor.Quaternary, Color.LightSteelBlue }
            });
            #endregion

            Darkmode(DarkMode);

            ContextMenu tabCM = new();
            tabCM.MenuItems.Add("BG Color");
            tabCM.MenuItems.Add("Clear");
            tabCM.MenuItems.Add("Export");
            tabCM.MenuItems[0].Click += new EventHandler(ChangeTXTBackColor);
            tabCM.MenuItems[1].Click += new EventHandler(ClearTXT);
            tabCM.MenuItems[2].Click += new EventHandler(ExportText);
            tabSections.ContextMenu = tabCM;
            pmTab = tabSections.TabPages[2];                                                        // Save the PM Tab
            tabSections.TabPages.Remove(pmTab);                                                     // "hIdE tHe tAB"

            /* // Fills the Grid            
            for (int i = 0; i <= 3;i++){
                string[] row = new string[] { i.ToString(), "", "", "0", "" };
                clientsDataGridView.Rows.Add(row);
            }*/
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


        #region Routines
        private void CommandHandler(object sender, KeyEventArgs e, string msg)                      // Handles "/" CLI commands 
        {
            if (msg.StartsWith("/darkmode")) {                                                      // UI - Toggles Theme/DarMode
                if (msg.IndexOf("on", StringComparison.OrdinalIgnoreCase) >= 0 || msg.Contains("1")) {
                    DarkMode = true;
                } else if (msg.IndexOf("off", StringComparison.OrdinalIgnoreCase) >= 0 || msg.Contains("0")) {
                    DarkMode = false;
                } else { DarkMode = !DarkMode; }

                Darkmode(DarkMode);
            }

            if (msg.StartsWith("/debug")) {                                                        // Sets Debug message level <0=Off|1=Normal|2=More>
                if (Convert.ToInt16(msg.Substring(6).Trim()) >= 0 && Convert.ToInt16(msg.Substring(6).Trim()) <= 2) { DebugLVL = Convert.ToInt16(msg.Substring(6).Trim()); }
            }

            if (msg.StartsWith("/export")) {                                                        // Export Tab related Text
                ExportText(sender, e);
            }

            if (msg.StartsWith("/help")) {                                                          // Console out Help
                Console("https://raw.githubusercontent.com/DSBash/GameServer/master/README.md");
            }

            if (msg.StartsWith("/msg")) {                                                           // Client Send PM
                foreach (DataGridViewRow row in clientsDataGridView.Rows) {
                    if (msg.Contains(row.Cells[1].Value.ToString())) {                              // if name in grid
                        int index = msg.IndexOf(row.Cells[1].Value.ToString());                     // get start of name
                        if (index != -1) { index += row.Cells[1].Value.ToString().Length; }         // get end of name
                        string hostPM = msg.Substring(index);                                       // take text after length of name

                        PostChat(MessagePack(hostPM, txtName.Text.Trim(), row.Cells[1].Value.ToString(), "Private"));   // Post Private Message
                    }
                }
            }

            if (msg.StartsWith("/picme")) {                                                         // Drawing - Get Drawing from Server
                if (connected) { FileReceive(); }
            }

            if (msg.StartsWith("/save")) {                                                          // Drawing - Saves the Image to File
                SaveDrawing();
            }

            if (msg.StartsWith("/send")) {                                                          // Drawing - Send the Image to Clients
                if (listening) { SendDrawing(); }
            }

            if (msg.StartsWith("/theme")) {                                                         // UI - Theme                
                if (msg.Substring(6).Trim() == "") {                                                // If Blank List Themes
                    foreach (var theme in Themes) {
                        Console(theme.Key);
                    }
                } else {
                    Theme = msg.Substring(6).Trim();                                                // Sets the Theme
                }

                Darkmode(false);
            }
        }
        private void HostDataHandler(MyPlayers obj, string clientData)                              // Handles incoming Data from Client  
        {

            if (clientData.StartsWith("{\"Msg\"")) {                                                // Host Receive - Message Package           
                remoteMsg = true;
                MessagePackage remoteMP = JsonConvert.DeserializeObject<MessagePackage>(clientData);
                switch (remoteMP.MsgType) {
                    case "Private":                                                                     // Private Message
                        for (int p = 1; p <= players.Count; p++) {                                      // Determine the PM Dest
                            if (remoteMP.To == players[p].username.ToString()) {                        // Dest is a player   
                                HostSendPrivate(clientData, players[p]);                                // Host - Send PM
                                Console(SystemMsg("PM Relayed."));
                                obj.data.Clear();
                                obj.handle.Set();
                                //return;
                                break;
                            } else if (remoteMP.To == txtName.Text.Trim()) {                            // Dest is host
                                PostChat(remoteMP);                                                     // Post Host PM
                                obj.data.Clear();
                                obj.handle.Set();
                                //return;
                                break;
                            }
                        }
                        break;

                    case "Public":                                                                      // Public Message
                        PostChat(remoteMP);
                        HostSendPublic(clientData, obj.id);                                             // Host relay public message to other clients
                        obj.data.Clear();
                        obj.handle.Set();
                        break;
                    default:
                        break;
                }
            }

            /*                            if (clientData.Contains("/msg")) {
                                            string[] pMSG = clientData.Split(':');
                                            for (int p = 1; p <= players.Count; p++) {                          // Determine the PM Dest
                                                if (pMSG[1] == players[p].username.ToString()) {                // Dest is a player        
                                                    HostSendPrivate(string.Format("{0} to you: {1}", obj.username, pMSG[2]), players[p]);   // Host - Send PM
                                                    Console(SystemMsg("PM Sent."));
                                                    obj.data.Clear();
                                                    obj.handle.Set();
                                                    return;
                                                } else if (pMSG[1] == txtName.Text.Trim()) {                    // Dest is host
                                                    PostChat(obj.username.ToString() + "to you", pMSG[2]);      // Post Host PM
                                                    obj.data.Clear();
                                                    obj.handle.Set();
                                                    return;
                                                }
                                            }
                                            Console(SystemMsg(string.Format("PM Not Sent. From: {0}  To: {1} -- {2} --", obj.username, pMSG[1], pMSG[2])));
                                            obj.data.Clear();
                                            obj.handle.Set();
                                            return;
                                        }*/

            // Host receive - Drawing
            if (clientData.StartsWith("{\"PenColor\"")) {
                remoteDraw = true;
                DrawPackage remoteDP = JsonConvert.DeserializeObject<DrawPackage>(clientData);
                DrawShape(remoteDP);
                HostSendPublic(clientData, obj.id);                                  // Host relay client drawing to other clients
                obj.data.Clear();
                obj.handle.Set();
                return;
            }

            // Host receive - FillTool
            if (clientData.StartsWith("{\"FillColor\"")) {
                FillPackage remoteFP = JsonConvert.DeserializeObject<FillPackage>(clientData);
                FillTool(BM, remoteFP.X, remoteFP.Y, ArgbColor(remoteFP.FillColor));
                HostSendPublic(clientData, obj.id);                                  // Host relay client drawing to other clients
                obj.data.Clear();
                obj.handle.Set();
                return;
            }

            /*                            // Host Receive - Public Message
                                        PostChat(obj.username.ToString(), clientData);                          // Host post
                                        HostSendPublic(string.Format("{0}:{1}", obj.username, clientData), obj.id);    // Host relay public message to other clients
                                        obj.data.Clear();
                                        obj.handle.Set();*/
        }
        private void ClientDataHandler(string hostData)                                             // Handles incoming Data from Host  
        {
            if (hostData.StartsWith("{\"Id\"")) {                                                   // Grid update
                ClearDataGrid();                                                                    // Clear DataGrid 
                string[] messages = hostData.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string message in messages) {
                    PlayerPackage player = JsonConvert.DeserializeObject<PlayerPackage>(message);
                    Color argbColor = Color.FromArgb(player.Color[0], player.Color[1], player.Color[2], player.Color[3]);
                    AddToGrid(player.Id, player.Name, argbColor);
                }
                clientObject.data.Clear();
                clientObject.handle.Set();
                //return;
            }
            if (hostData.StartsWith("{\"Msg\"")) {                                                  // Client Receive - Message
                remoteMsg = true;
                MessagePackage remoteMP = JsonConvert.DeserializeObject<MessagePackage>(hostData);
                PostChat(remoteMP);
                clientObject.data.Clear();
                clientObject.handle.Set();
                //return;
            }
            if (hostData.StartsWith("SYSTEM:")) {                                                   // Client Receive - System Message
                string[] sysParts = hostData.Split(':');
                Console(sysParts[2]);
                clientObject.data.Clear();
                clientObject.handle.Set();
                //return;
            }

            if (hostData.StartsWith("{\"PenColor\"")) {                                             // Client Receive - Drawing
                string[] messages = hostData.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string message in messages) {
                    DrawPackage remoteDP = JsonConvert.DeserializeObject<DrawPackage>(message);
                    remoteDraw = true;
                    DrawShape(remoteDP);
                }
                clientObject.data.Clear();
                clientObject.handle.Set();
                //return;
            }

            if (hostData.StartsWith("{\"FillColor\"")) {                                            // Client Receive - FillTool
                FillPackage remoteFP = JsonConvert.DeserializeObject<FillPackage>(hostData);
                FillTool(BM, remoteFP.X, remoteFP.Y, ArgbColor(remoteFP.FillColor));
                clientObject.data.Clear();
                clientObject.handle.Set();
                //return;
            }

            if (hostData.StartsWith("CMD:")) {                                                      // Client Receive - Commands from Host
                string[] cmdParts = hostData.Split(':');
                switch (cmdParts[1]) {
                    case "ClearDrawing":
                        Button sender = new();
                        EventArgs e = new();
                        CmdClear_Click(sender, e);
                        break;
                    case "ReceiveDrawing":
                        ListenForFile();                                                                // Client open server to receive
                        break;
                    default:
                        break;
                }
                clientObject.data.Clear();
                clientObject.handle.Set();
                //return;
            }

            Console("Unrecognized: " + hostData);                                                   // Client Receive - Unformated Text
            clientObject.data.Clear();
            clientObject.handle.Set();
        }

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

        private void Darkmode(bool status)                                                          // Toggles Darkmode vs Theme 
        {
            DarkMode = status;
            if (DarkMode) {
                ChangeControlColors(this, Color.Black, SystemColors.ControlDark, SystemColors.ControlText, SystemColors.ControlDarkDark);
            } else {
                if (Themes.TryGetValue(Theme, out Dictionary<ThemeColor, Color> matchingDictionary)) {  // Find the dictionary
                    Color primaryColor = matchingDictionary[ThemeColor.Primary];
                    Color secondaryColor = matchingDictionary[ThemeColor.Secondary];
                    Color tertiaryColor = matchingDictionary[ThemeColor.Tertiary];
                    Color quaternaryColor = matchingDictionary[ThemeColor.Quaternary];

                    ChangeControlColors(this, primaryColor, secondaryColor, tertiaryColor, quaternaryColor); // Set the Theme
                }

                /*
                switch (Theme) {
                    case "Nature":
                        ChangeControlColors(this, Nature[ThemeColor.Primary], Nature[ThemeColor.Secondary], Nature[ThemeColor.Tertiary], Nature[ThemeColor.Quaternary]);
                        break;
                    default:
                        ChangeControlColors(this, SystemColors.WindowText, SystemColors.Window, SystemColors.WindowText, SystemColors.ScrollBar);
                        break;
                */
            }
        }
        private void ChangeControlColors(Control control, Color primaryCol, Color secondaryCol, Color tertiaryCol, Color quaternaryCol)
        { /// Changes the Fore, Back, BackGround
            foreach (Control childControl in control.Controls) {
                if (DebugLVL == 2) { Console(childControl.GetType().Name + " " + childControl.Name); }
                switch (childControl.GetType().Name) {
                    case "BorderedButton":
                        BorderedButton cBB = childControl as BorderedButton;
                        cBB.ForeColor = primaryCol;
                        cBB.BackColor = secondaryCol;
                        cBB.BorderColor = primaryCol;
                        break;
                    case "BorderedTabControl":
                        BorderedTabControl cBTC = childControl as BorderedTabControl;
                        cBTC.BorderColor = primaryCol;
                        foreach (TabPage page in childControl.Controls) {
                            page.ForeColor = tertiaryCol;
                            page.BackColor = quaternaryCol;
                        }
                        break;
                    case "BorderedTextBox":
                        BorderedTextBox cBTB = childControl as BorderedTextBox;
                        cBTB.ForeColor = primaryCol;
                        cBTB.BackColor = secondaryCol;
                        cBTB.BorderColor = primaryCol;
                        break;
/*                    case "BorderedRichTextBox":
                        BorderedRichTextBox cBRTB = childControl as BorderedRichTextBox;
                        cBRTB.ForeColor = primaryCol;
                        cBRTB.BackColor = secondaryCol;
                        cBRTB.BorderColor = primaryCol;
                        break;*/
                    case "Button":
                    case "TextBox":
                    case "RichTextBox":
                        childControl.ForeColor = primaryCol;
                        childControl.BackColor = secondaryCol;
                        break;
                    case "SplitContainer":
                        childControl.ForeColor = tertiaryCol;
                        childControl.BackColor = quaternaryCol;
                        break;
                    case "SplitterPanel":
                    case "FlowLayoutPanel":
                        foreach (Panel panel in control.Controls) {
                            panel.ForeColor = tertiaryCol;
                            panel.BackColor = quaternaryCol;
                        }
                        break;
                    case "TabControl":
                        foreach (TabPage page in childControl.Controls) {
                            page.ForeColor = tertiaryCol;
                            page.BackColor = quaternaryCol;
                        }
                        break;
                    case "DataGridView":
                        ((System.Windows.Forms.DataGridView)childControl).BackgroundColor = quaternaryCol;
                        foreach (DataGridViewColumn col in ((System.Windows.Forms.DataGridView)childControl).Columns) {
                            col.HeaderCell.Style.BackColor = secondaryCol;                          // Change the color to your desired color
                            col.HeaderCell.Style.ForeColor = primaryCol;
                            col.HeaderCell.Style.SelectionForeColor = Color.Red;
                            col.HeaderCell.Style.SelectionBackColor = Color.Red;

                            ((System.Windows.Forms.DataGridView)childControl).GridColor = primaryCol;
                        }

                        break;
                }

                if (childControl.HasChildren) {
                    if (DebugLVL == 2) { Console("Spawn Found"); }
                    ChangeControlColors(childControl, primaryCol, secondaryCol, tertiaryCol, quaternaryCol);   // if childControl has child controls, recurse
                }
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


        #region Controls

        private class BorderedGroupBox                                                                /* Group Box Color Borders */ : System.Windows.Forms.GroupBox
        {
            public static Color BorderColor { get; internal set; }
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                if (Theme != null) {
                    if (Themes.TryGetValue(Theme, out Dictionary<ThemeColor, Color> matchingDictionary)) {  // If a matching dictionary was found
                        Color SecondaryColor = matchingDictionary[ThemeColor.Secondary];
                        BorderColor = SecondaryColor;
                    }
                } else { BorderColor = Color.Transparent; }

                using Pen pen = new(BorderColor, 2);                                                // Create a new Pen with the desired border color
                Rectangle rect = new(1, 7, Width - 2, Height - 8);                                  // Draw a rectangle around the group box
                e.Graphics.DrawRectangle(pen, rect);
            }
        }
        private class BorderedButton : Button
        {
            public Color BorderColor { get; internal set; }
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == 0xf) {
                    using var g = Graphics.FromHwnd(Handle);
                    using var p = new Pen(BorderColor, 4);
                    g.DrawRectangle(p, new Rectangle(2, 2, Width - 4, Height - 4));
                }
            }
        }
        private class BorderedTextBox : TextBox
        {
            public Color BorderColor { get; internal set; }
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == 0xf) {
                    using var g = Graphics.FromHwnd(Handle);
                    using var p = new Pen(BorderColor, 4);
                    g.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));
                }
            }
        }
        private class BorderedTabControl : TabControl
        {
            public Color BorderColor { get; internal set; }
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == 0xf) {
                    using var g = Graphics.FromHwnd(Handle);
                    using var p = new Pen(BorderColor, 4);
                    g.DrawRectangle(p, new Rectangle(2, 23, Width - 4, Height - 25));               // - 26 Will remove the White line below the Tab Control
                }
            }
        }
        private class BorderedRichTextBox : RichTextBox
        {
            public Color BorderColor { get; internal set; }
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == 0xf) {
                    using var g = Graphics.FromHwnd(Handle);
                    using var p = new Pen(BorderColor, 5);
                    g.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));

                }
            }
        }

        /*        public class BorderedTextBox : UserControl {
                    TextBox textBox;

                    public BorderedTextBox() {
                        textBox = new TextBox() {
                            BorderStyle = BorderStyle.FixedSingle,
                            Location = new Point(-1, -1),
                            Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                                     AnchorStyles.Left | AnchorStyles.Right
                        };
                        Control container = new ContainerControl() {
                            Dock = DockStyle.Fill,
                            Padding = new Padding(-1)
                        };
                        container.Controls.Add(textBox);
                        this.Controls.Add(container);

                        if (Theme != null) {
                            if (Themes.TryGetValue(Theme, out Dictionary<ThemeColor, Color> matchingDictionary)) {  // If a matching dictionary was found 
                                DefaultBorderColor = matchingDictionary[ThemeColor.Secondary];
                                FocusedBorderColor = matchingDictionary[ThemeColor.Primary];
                                BackColor = matchingDictionary[ThemeColor.Secondary];
                            }
                        } else {
                            DefaultBorderColor = Color.Transparent; 
                            FocusedBorderColor = Color.Transparent;
                            BackColor = DefaultBorderColor;
                        }
                        Padding = new Padding(1);
                        Size = textBox.Size;
                    }

                    public Color DefaultBorderColor { get; set; }
                    public Color FocusedBorderColor { get; set; }

                    public override string Text {
                        get { return textBox.Text; }
                        set { textBox.Text = value; }
                    }

                    protected override void OnEnter(EventArgs e) {
                        BackColor = FocusedBorderColor;
                        base.OnEnter(e);
                    }

                    protected override void OnLeave(EventArgs e) {
                        BackColor = DefaultBorderColor;
                        base.OnLeave(e);
                    }

                    protected override void SetBoundsCore(int x, int y,
                        int width, int height, BoundsSpecified specified) {
                        base.SetBoundsCore(x, y, width, textBox.PreferredHeight, specified);
                    }
                }*/

        private void ChangeTXTBackColor(object sender, EventArgs e)                                 // Set the background color of respective TextBox 
        {
            if (sender is System.Windows.Forms.MenuItem) {
                RichTextBox textBox = ReturnFirstBoxFrom(sender);

                ColorDialog MyDialog = new() {
                    AllowFullOpen = true,
                    ShowHelp = true,
                    CustomColors = new int[] { textBox.BackColor.ToArgb(), 0xFFFF00, 0x00FFFF },
                };

                if (MyDialog.ShowDialog() == DialogResult.OK) {
                    textBox.BackColor = MyDialog.Color;                                         // Update the text box color if the user clicks OK
                }
            }
        }
        private void ClearTXT(object sender, EventArgs e)                                           // Clear the respective Textbox 
        {
            if (tabSections.SelectedTab.Name == "tConsole") { Console(); }
            if (tabSections.SelectedTab.Name == "tLobby") { PostChat(MessagePack("", "", "", "")); }
            if (tabSections.SelectedTab.Tag.ToString() == "PM") { ReturnFirstBoxFrom(sender).Text = ""; }
        }
        private void TabSections_SelectedIndexChanged(object sender, EventArgs e)                   // Remove the *unread8 indicators 
        {
            TabControl tabControl = (TabControl)sender;
            TabPage selectedTabPage = tabControl.SelectedTab;

            if (selectedTabPage.Text.Contains("*")) {
                selectedTabPage.Text = selectedTabPage.Text.Replace("*", "");
            }
        }

        private RichTextBox ReturnFirstBoxFrom(object sender)                                       // Get RTB from Sender(Menu) 
        {
            MenuItem menuItem = sender as MenuItem;                                                 // Get the MenuItem that triggered the event
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;                               // Get the immediate parent of the MenuItem, which should be the ContextMenu
            TabControl tabControl = contextMenu.SourceControl as TabControl;                        // Get the control on which the ContextMenu was displayed, which should be the TabControl
            TabPage selectedTab = tabControl.SelectedTab;                                           // Get the selected TabPage
            RichTextBox textBox = selectedTab.Controls.OfType<RichTextBox>().FirstOrDefault();      // Get the textbox on the selected TabPage
            return textBox;
        }
        private void ToggleNetworkControls()                                                        // Toggle controls for networking 
        {
            txtAddress.Enabled = !txtAddress.Enabled;
            txtPort.Enabled = !txtPort.Enabled;
            txtName.Enabled = !txtName.Enabled;
            txtRoomKey.Enabled = !txtRoomKey.Enabled;

            cmdHost.Enabled = !cmdHost.Enabled;
            cmdJoin.Enabled = !cmdJoin.Enabled;

            tPing.Enabled = !tPing.Enabled;
        }

        private void FillToggle_CheckedChanged(object sender, EventArgs e)                          // Change to /w Close if on Pen or start 
        {
            if (!cbFillDraw.Checked || cbBType.Text == "Pen" || cbBType.Text == "Shape / Style") {
                cbBType.SelectedItem = "Pen w/ Close";
            }
        }
        private void Trans_CheckedChanged(object sender, EventArgs e)                               // Transparent Canvas 
        {
            if (cbTrans.Checked) {
                this.TransparencyKey = Color.LightGoldenrodYellow;                                  // 250,250,210
                picDrawing.BackColor = Color.LightGoldenrodYellow;
                btnBGColor.SelectedColor = Color.LightGoldenrodYellow;
            }
        }
        private void CbMask_CheckedChanged(object sender, EventArgs e)                              // Handles RoomKey Mask 
        {
            if (txtRoomKey.PasswordChar == '*') {
                txtRoomKey.PasswordChar = '\0';
            } else {
                txtRoomKey.PasswordChar = '*';
            }
        }
        private void CmdHost_Click(object sender, EventArgs e)                                      // Host start 
        {
            var source = new CancellationTokenSource();
            Task.Factory.StartNew(() => {
                if (listening) {
                    listening = false;
                    RemoveFromGrid(0);
                } else if (listener == null || !listener.IsAlive) {
                    string address = txtAddress.Text.Trim();
                    string number = txtPort.Text.Trim();
                    if (txtName.Text.Trim() == "Player 1") { txtName.Text = "Host"; cmdColor.SelectedColor = Color.Yellow; }
                    string username = txtName.Text.Trim();
                    bool error = false;
                    IPAddress ip = null;
                    if (address.Length < 1) {
                        error = true;
                        Console(SystemMsg("Address is required"));
                    } else {
                        try {
                            ip = Dns.GetHostEntry(address)
                                .AddressList
                                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                        } catch {
                            error = true;
                            Console(SystemMsg("Address is not valid"));
                        }
                    }
                    int port = -1;
                    if (number.Length < 1) {
                        error = true;
                        Console(SystemMsg("Port number is required"));
                    } else if (!int.TryParse(number, out port)) {
                        error = true;
                        Console(SystemMsg("Port number is not valid"));
                    } else if (port < 0 || port > 65535) {
                        error = true;
                        Console(SystemMsg("Port number is out of range"));
                    }
                    if (username.Length < 1) {
                        error = true;
                        Console(SystemMsg("Username is required"));
                    }
                    if (!error) {
                        listener = new Thread(() => Listener(ip, port)) {
                            IsBackground = true
                        };
                        listener.Start();

                        ClearDataGrid();
                        AddToGrid(0, username, cmdColor.SelectedColor);
                    }
                }
            }, source.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            txtMessage.Focus();
            txtMessage.SelectionStart = txtMessage.Text.Length;
        }
        private void CmdJoin_Click(object sender, EventArgs e)                                      // Client start 
        {
            var source = new CancellationTokenSource();
            Task.Factory.StartNew(() => {
                if (connected) {
                    clientObject.client.Close();
                } else if (client == null || !client.IsAlive) {
                    string address = txtAddress.Text.Trim();
                    string number = txtPort.Text.Trim();
                    if (txtName.Text.Trim() == "Player 1") { cmdColor.SelectedColor = Color.Red; }
                    if (txtName.Text.Trim() == "Player 2") { cmdColor.SelectedColor = Color.Green; }
                    if (txtName.Text.Trim() == "Player 3") { cmdColor.SelectedColor = Color.Blue; }
                    string username = txtName.Text.Trim();
                    bool error = false;
                    IPAddress ip = null;
                    if (address.Length < 1) {
                        error = true;
                        Console(SystemMsg("Address is required"));
                    } else {
                        try {
                            ip = Dns.GetHostEntry(address)
                                .AddressList
                                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                        } catch {
                            error = true;
                            Console(SystemMsg("Address is not valid"));
                        }
                    }
                    int port = -1;
                    if (number.Length < 1) {
                        error = true;
                        Console(SystemMsg("Port number is required"));
                    } else if (!int.TryParse(number, out port)) {
                        error = true;
                        Console(SystemMsg("Port number is not valid"));
                    } else if (port < 0 || port > 65535) {
                        error = true;
                        Console(SystemMsg("Port number is out of range"));
                    }
                    if (username.Length < 1) {
                        error = true;
                        Console(SystemMsg("Username is required"));
                    }
                    if (!error) {
                        client = new Thread(() => Connection(ip, port, username, txtRoomKey.Text)) {
                            IsBackground = true
                        };
                        client.Start();
                    }
                }
            }, source.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        private void ClientsDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)      // Grid Button Clicks / Private Message / Ping / DC 
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == clientsDataGridView.Columns["dc"].Index) {      // DC button
                long.TryParse(clientsDataGridView.Rows[e.RowIndex].Cells["identifier"].Value.ToString(), out long id);
                if (id == 0) { id = -1; }
                Disconnect(id);
            }
            if (e.RowIndex >= 0 && e.ColumnIndex == clientsDataGridView.Columns["latency"].Index) { // Ping selected client
                var BtnCell = (DataGridViewTextBoxCell)clientsDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                BtnCell.Value = Ping(GetPlayerAddress(e.RowIndex));
            }
            if (e.RowIndex >= 0 && e.ColumnIndex == clientsDataGridView.Columns["name"].Index) {    // Private msg
                string name = clientsDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                string prevMSG = txtMessage.Text;
                if (prevMSG.EndsWith("ype and press enter to send.")) { txtMessage.Text = prevMSG = ""; }
                if (prevMSG.StartsWith("/msg")) {
                    string[] msgParts = prevMSG.Split(' ');
                    txtMessage.Text = string.Format("/msg {0} {1}", name, msgParts[2].Trim());
                } else { txtMessage.Text = string.Format("/msg {0} {1}", name, prevMSG); }
                txtMessage.Focus();
                txtMessage.SelectionStart = txtMessage.Text.Length;
            }
        }
        #endregion


        #region Console & Chats & Message formatters
        private string StackTrace()                                                                 // Better Debug Info 
        {
            StackTrace stackTrace = new();
            string stackList = null;
            for (int i = 2; i < stackTrace.FrameCount; i++) {                                       // start at 2 for ErrorMSG --> StackTrace(Self)
                StackFrame callingFrame = stackTrace.GetFrame(i);                                   // get the stack frame for the calling method
                MethodBase callingMethod = callingFrame.GetMethod();                                // get information about the calling method
                string callingMethodName = callingMethod.Name + "-->";                              // get the name of the calling method
                stackList = string.Concat(callingMethodName, stackList);
                if (i == DebugLVL + 2) { break; }                                                   // Sets the Thread Depth
            }
            return stackList;
        }
        private string ErrorMsg(string msg)                                                         // Format Errors 
        {
            return string.Format("ERROR: {0} : {1}", StackTrace(), msg);
        }
        private string SystemMsg(string msg)                                                        // Format System 
        {
            return string.Format("SYSTEM: {0}", msg);
        }

        private void Console(string msg = "")                                                       // Console message / Clear if empty 
        {
            txtConsole.Invoke((MethodInvoker)delegate {
                if (msg.Length > 0) {
                    txtConsole.AppendText(string.Format("{2}[ {0} ] {1}", DateTime.Now.ToString("HH:mm:ss"), msg, Environment.NewLine), txtAddress.ForeColor);
                } else {
                    txtConsole.Clear();
                }
                if (tabSections.SelectedIndex != 0) {
                    tabSections.Invoke((MethodInvoker)delegate {
                        tabSections.TabPages[0].Text = "*Console*";
                    });
                }
            });
        }
        private MessagePackage MessagePack(string msg, string from, string to, string type)         // Create new Message Packages 
        {
            MessagePackage msgPack = new() {
                Msg = msg,                                                                          // Add details
                From = from,
                To = to,
                MsgType = type
            };

            return msgPack;
        }
        private void PostChat(MessagePackage msgPack)                                               // Post the message / Clear if empty 
        {
            if (msgPack.Msg == null || msgPack.Msg.Length < 1) {
                txtLobby.Invoke((MethodInvoker)delegate {
                    txtLobby.Clear();
                });
            } else {
                #region Message Color
                string playerColor = "255,0,0,0,0";
                foreach (DataGridViewRow row in clientsDataGridView.Rows) {                         // Determine color for post using Grid
                    if (row.Cells[1].Value.ToString() == msgPack.From) {
                        playerColor = row.Cells[2].Value?.ToString();
                        break;
                    }
                }
                string[] colParts = playerColor.Split(',');                                         // Format string to Color
                Color msgColor = Color.FromArgb(Convert.ToInt32(colParts[0]), Convert.ToInt32(colParts[1]), Convert.ToInt32(colParts[2]), Convert.ToInt32(colParts[3]));
                #endregion

                switch (msgPack.MsgType) {
                    #region Private Messages
                    case "Private":
                        string formattedMSG = "";                                                       // To store formated Message
                        if (msgPack.To == txtName.Text || msgPack.From == txtName.Text) {               // Mine in any way
                            TabPage tabPage = null;
                            string tabName = "Lobby";

                            if (msgPack.To == txtName.Text) {
                                tabName = msgPack.From;
                            } else {
                                tabName = msgPack.From;
                                if (msgPack.From == txtName.Text) { tabName = msgPack.To; }
                            }

                            tabSections.Invoke((MethodInvoker)delegate {

                                foreach (TabPage tab in tabSections.TabPages) {
                                    if (tab.Text == tabName || tab.Text == "*" + tabName + "*") {       // Search for a tab with the specified username
                                        tabPage = tab;
                                        break;
                                    }
                                }

                                if (tabPage == null) {                                                  // If the tab is not found
                                    tabPage = new() {
                                        Name = "t" + tabName,
                                        Tag = "PM",
                                        Text = tabName
                                    };

                                    foreach (Control control in pmTab.Controls) {                       // create copy the controls from pmTab
                                        Control newControl = (Control)Activator.CreateInstance(control.GetType());
                                        newControl.Location = control.Location;
                                        newControl.Size = control.Size;
                                        newControl.Name = control.Name;
                                        newControl.Tag = control.Tag;
                                        newControl.BackColor = control.BackColor;
                                        tabPage.Controls.Add(newControl);
                                    }
                                    tabSections.TabPages.Add(tabPage);                                  // Add the new PM tab
                                }

                                if (tabPage.Controls.Find("txtPM", true).FirstOrDefault() is RichTextBox textBox) { // Find the TextBox control
                                    formattedMSG = string.Format("{0}[{1}] {2} whispers: {3}", Environment.NewLine, DateTime.Now.ToString("HH:mm:ss"), msgPack.From, msgPack.Msg);

                                    if (tabSections.SelectedTab != tabPage) {                           // If not in focus change the Tab text
                                        tabPage.Text = string.Format("*" + tabName + "*");
                                    }
                                    textBox.AppendText(formattedMSG, msgColor);                         // Post Private msg in Tab
                                }

                            });
                        }
                        break;
                    #endregion

                    case "Public":
                        formattedMSG = string.Format("{0}[{1}] From {2}: {3}", Environment.NewLine, DateTime.Now.ToString("HH:mm:ss"), msgPack.From, msgPack.Msg);
                        tabSections.Invoke((MethodInvoker)delegate {
                            if (tabSections.SelectedIndex != 1) {                                       // If not in focus change the Tab text
                                tabSections.TabPages[1].Text = string.Format("*Lobby ({0})*", clientsDataGridView.Rows.Count);
                            }
                        });
                        txtLobby.AppendText(formattedMSG, msgColor);                                    // Post Public msg in Lobby
                        break;

                    case "Console":
                        Console(msgPack.Msg);
                        break;

                    case "System":
                        SystemMsg(msgPack.Msg);
                        break;

                    default:
                        break;
                }

                string json = JsonConvert.SerializeObject(msgPack) + "\n";                          // Format the Package 
                if (listening && !remoteMsg) {
                    HostSendPublic(json);                                                           // Host Send Draw Package
                } else if (connected && !remoteMsg) {
                    Send(json);                                                                     // Client Send Draw Package
                }
                remoteMsg = false;

                /*                if (username.Contains("to you")) {                                                  // Private Messages
                                    username = username.Replace("to you", "").Trim();
                                    TabPage tabPage = null;
                                    tabSections.Invoke((MethodInvoker)delegate {
                                        foreach (TabPage tab in tabSections.TabPages) {
                                            if (tab.Text == username
                                            || tab.Text == "*" + username + "*" 
                                            || tab.Text == txtName.Text.Trim()) {                                   // Search for a tab with the specified username
                                                tabPage = tab;
                                                break;
                                            }
                                        }

                                        if (tabPage == null) {                                                      // If the tab is not found
                                            tabPage = new();
                                            tabPage.Name = "t" + username;
                                            tabPage.Tag = "PM";
                                            tabPage.Text = username;

                                            foreach (Control control in pmTab.Controls) {                           // create copy the controls from pmTab
                                                Control newControl = (Control)Activator.CreateInstance(control.GetType());
                                                newControl.Location = control.Location;
                                                newControl.Size = control.Size;
                                                newControl.Name = control.Name;
                                                newControl.Tag = control.Tag;
                                                newControl.BackColor = control.BackColor;
                                                tabPage.Controls.Add(newControl);
                                            }                            
                                            tabSections.TabPages.Add(tabPage);                                      // Add the new PM tab
                                        }

                                        if (tabPage.Controls.Find("txtPM", true).FirstOrDefault() is RichTextBox textBox) { // Find the TextBox control
                                            formattedMSG = string.Format("{0}[{1}] {2}: {3}", Environment.NewLine, DateTime.Now.ToString("HH:mm:ss"), username, msg);

                                            if (tabSections.SelectedTab != tabPage) {                               // If not in focus change the Tab text
                                                    tabPage.Text = string.Format("*" + username + "*");
                                            }
                                            textBox.AppendText(formattedMSG, msgColor);                             // Post actual msg
                                            txtMessage.Focus();                                                     // Leave focus for next message
                                        }
                                    });
                                #endregion 
                                } else {                                                                            // Public Messages
                                    formattedMSG = string.Format("{0}[{1}] {2}: {3}", Environment.NewLine, DateTime.Now.ToString("HH:mm:ss"), username, msg);
                                    tabSections.Invoke((MethodInvoker)delegate {
                                        if (tabSections.SelectedIndex != 1) {                                       // If not in focus change the Tab text
                                                tabSections.TabPages[1].Text = string.Format("*Lobby ({0})*", clientsDataGridView.Rows.Count);
                                        }
                                    });
                                        txtLobby.AppendText(formattedMSG, msgColor);                                // Post actual msg
                                        txtMessage.Focus();                                                         // Leave focus for next message
                                }*/
            }
        }
        private void ExportText(object sender, EventArgs e)                                         // Export to \exports\<tabname>.txts 
        {
            RichTextBox textBox = ReturnFirstBoxFrom(sender);                                       // 

            string fileName = textBox.Parent.Tag.ToString();
            if (fileName == "PM") { fileName = fileName + "_" + textBox.Parent.Text.ToString(); }
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\exports\\" + fileName + ".txt";// Get the file path

            if (textBox != null) {
                string contents = Environment.NewLine + DateTime.Now.ToString("F") + Environment.NewLine + textBox.Text + Environment.NewLine;
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "exports");
                Directory.CreateDirectory(folderPath);                                              // create folder
                File.AppendAllText(path, contents);                                                 // Save the file
            }
        }
        private void TxtMessage_Enter(object sender, EventArgs e)                                   // Messagebox 
        {
            if (txtMessage.Text == "Type and press enter to send.") {
                txtMessage.Text = "";
                if (tabSections.SelectedTab.Tag.ToString() == "PM") {                               // If a PM tab is selected
                    txtMessage.Text = "/msg " + tabSections.SelectedTab.Text + " ";                 // make PM easier
                }
            }
        }
        private void TxtMessage_Leave(object sender, EventArgs e)                                   // Messagebox Note 
        {
            if (txtMessage.Text == "") {
                txtMessage.Text = "Type and press enter to send.";
            }
        }
        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)                              // Commands / History / Send on <Enter> 
        {
            #region Command Line History
            if (e.KeyCode == Keys.Up) {                                                             // User pressed up arrow key
                e.SuppressKeyPress = true;
                if (HistoryIndex > 0) {
                    HistoryIndex--;                                                                 // Decrement the command index to retrieve the previous command
                    txtMessage.Text = MSGHistory[HistoryIndex];
                    SetMessageCursor();
                }
            } else if (e.KeyCode == Keys.Down) {                                                    // User pressed down arrow key
                if (HistoryIndex < MSGHistory.Count - 1) {
                    HistoryIndex++;                                                                 // Increment the command index to retrieve the next command
                    txtMessage.Text = MSGHistory[HistoryIndex];
                    SetMessageCursor();
                } else {
                    HistoryIndex = MSGHistory.Count;                                                // User has reached the end of the command history
                    txtMessage.Clear();                                                             // Clear the textbox
                }
                #endregion
            } else if (e.KeyCode == Keys.Enter) {
                e.Handled = true;
                e.SuppressKeyPress = true;

                MSGHistory.Add(txtMessage.Text);                                                    // CLI History
                HistoryIndex = MSGHistory.Count;

                string msg = txtMessage.Text;
                if (txtMessage.Text.Length > 0 && !txtMessage.Text.StartsWith("/")) {
                    PostChat(MessagePack(txtMessage.Text.Trim(), txtName.Text.Trim(), "Lobby", "Public"));   // Client - Post Public Message
                } else {
                    CommandHandler(sender, e, msg);
                }
                txtMessage.Clear();
            }
        }
        private void SetMessageCursor()                                                             // Puts the cursor at the end of the CLI History 
        {
            txtMessage.SelectionStart = txtMessage.Text.Length;
            txtMessage.SelectionLength = 0;
            txtMessage.Focus();
        }
        #endregion


        #region ClientsDataGrid Management
        private void AddToGrid(long id, string name, Color color)                                   // Add Client details to the Grid 
        {
            clientsDataGridView.Invoke((MethodInvoker)delegate {
                if (clientsDataGridView.RowCount > (int)id) {                                       // Update Row
                    DataGridViewCell cellID = clientsDataGridView.Rows[(int)id].Cells[0];
                    cellID.Value = id.ToString();
                    DataGridViewCell cellName = clientsDataGridView.Rows[(int)id].Cells[1];
                    cellName.Value = name.ToString();
                    DataGridViewCell cellCol = clientsDataGridView.Rows[(int)id].Cells[2];
                    cellCol.Value = ColorToString(ref color);
                    DataGridViewCell cellLat = clientsDataGridView.Rows[(int)id].Cells[3];
                    cellLat.Value = "ms";
                } else {
                    string dcText = "DC";
                    if (id == 0) { dcText = "All"; }                                                // Host button
                    string[] row = new string[] { id.ToString(), name, ColorToString(ref color), "ms", dcText };
                    clientsDataGridView.Rows.Add(row);                                              // Add row
                }
            });
            DataGridViewRow nrow = clientsDataGridView.Rows[(int)id];
            nrow.DefaultCellStyle.BackColor = color;                                                // Set Row Color
            tabSections.Invoke((MethodInvoker)delegate {
                tabSections.TabPages[1].Text = string.Format("Lobby ({0})", clientsDataGridView.Rows.Count);    // Update the Connection count on Lobby tab
            });
            if (listening) { UpdateDataContents(); }                                                // If hosting broadcast to Clients
        }
        private void RemoveFromGrid(long id)                                                        // Remove Client from Grid 
        {
            clientsDataGridView.Invoke((MethodInvoker)delegate {
                foreach (DataGridViewRow row in clientsDataGridView.Rows) {
                    if (row.Cells["identifier"].Value.ToString() == id.ToString()) {                // If the row matches
                        clientsDataGridView.Rows.RemoveAt(row.Index);                               // Remove the row
                        break;
                    }
                }
            });
            tabSections.Invoke((MethodInvoker)delegate {
                tabSections.TabPages[1].Text = string.Format("Lobby ({0})", clientsDataGridView.Rows.Count);
            });
            if (listening) { UpdateDataContents(); }
        }
        private void ClearDataGrid()                                                                // Clear DataGrid 
        {
            clientsDataGridView.Invoke((MethodInvoker)delegate {
                clientsDataGridView.Rows.Clear();
            });
        }
        private void UpdateDataContents()                                                           // Host - Collect, format and send DataGrid contents 
        {
            if (clientsDataGridView.RowCount > 1) {
                string GridContents = null;

                for (int row = 0; row < clientsDataGridView.Rows.Count; row++) {
                    string[] argbColor = clientsDataGridView.Rows[row].Cells[2].Value.ToString().Split(',');    // Seperate colour parts to INT components
                    int colA = Convert.ToInt32(argbColor[0]);
                    int colR = Convert.ToInt32(argbColor[1]);
                    int colG = Convert.ToInt32(argbColor[2]);
                    int colB = Convert.ToInt32(argbColor[3]);

                    PlayerPackage player = new() {                                                  // Create a player object to send
                        Id = row,
                        Name = clientsDataGridView.Rows[row].Cells[1].Value.ToString(),
                        Color = new int[] { colA, colR, colG, colB }
                    };
                    string json = JsonConvert.SerializeObject(player);                              // Format the object
                    GridContents += json + "\n";
                }
                HostSendPublic(GridContents);                                                       // Host Send Grid row 
            }
        }
        #endregion


        #region Drawing
        private DrawPackage PrepareDrawPackage()                                                    // Draw Pakage Constructor 
        {
            Color color = btnColor.SelectedColor;
            Color fillcolor = btnFillColor.SelectedColor;

            DrawPackage drawPack = new() {                                                          // Create the Draw Package
                PenColor = ColorToString(ref color),
                PenSize = (int)nudSize.Value,
                BrushColor = ColorToString(ref fillcolor),
                DrawType = cbBType.Text,
                Fill = cbFillDraw.Checked,

                PT1 = PT1,
                PT2 = PT2,
                X = x,
                Y = y,
                PT1X = PT1X,
                PT1Y = PT1Y,
                PT2X = PT2X,
                PT2Y = PT2Y
            };
            if (mPath != null) {
                drawPack.PPath ??= new Point[mPath.PointCount];                                     // Create a path for the package
                for (int i = 0; i < mPath.PointCount; i++) {
                    drawPack.PPath[i] = new((int)mPath.PathPoints[i].X, (int)mPath.PathPoints[i].Y);    // Add path points to package
                }
            }

            mPath.Reset();

            return drawPack;                                                                        // Return Package to caller
        }

        private void SetDraw_Click(object sender, EventArgs e)                                      // Toggles Settings and Drawings Group Boxes 
        {
            if (!gbSettings.Visible) {
                Drawing = gbDrawings.Visible = false;
                gbSettings.Visible = true;
                picDrawing.Visible = false;
                btnSetDraw.Text = "Drawings";
            } else {
                Drawing = gbDrawings.Visible = true;
                gbSettings.Visible = false;
                picDrawing.Visible = true;
                btnSetDraw.Text = "Settings";
            }
        }
        private void CmdClear_Click(object sender, EventArgs e)                                     // Clears the gfx 
        {
            G.Clear(btnBGColor.SelectedColor);
            picDrawing.Invoke((MethodInvoker)delegate {
                picDrawing.Refresh();
            });
        }
        private void CmdClearAll_Click(object sender, EventArgs e)                                  // Clears gfx and Sends Clear to Clients 
        {
            CmdClear_Click(sender, e);
            HostSendPublic("CMD:ClearDrawing");
        }

        private void Drawing_MouseDown(object sender, MouseEventArgs e)                             // Start Drawing points 
        {
            if (Drawing && e.Button == MouseButtons.Left) {
                PT1 = e.Location;
                PT1X = e.X;
                PT1Y = e.Y;
            }
        }
        private void Drawing_MouseMove(object sender, MouseEventArgs e)                             // Continue Drawing 
        {
            if (Drawing && e.Button == MouseButtons.Left) {
                PT2 = e.Location;                                                                   // Update PT2 for Mouse up event                 
                switch (cbBType.Text) {
                    case "Circle":
                    case "Fill Tool":
                    case "Line":
                    case "Rectangle":
                    case "Triangle":
                        break;
                    default:
                        mPath ??= new();                                                            // New Path if needed
                        Point mP = new(e.Location.X, e.Location.Y);
                        mPath.AddLines(new Point[] {                                               // Add points to Path
                            mP
                        });
                        PT1 = PT2;
                        break;
                }
                picDrawing.Refresh();
                x = e.X;
                y = e.Y;
                PT2X = e.X - PT1X;
                PT2Y = e.Y - PT1Y;
            }
        }
        private void Drawing_MouseUp(object sender, MouseEventArgs e)                               // Draw shape on Mouse Up 
        {
            if (Drawing && e.Button == MouseButtons.Left) {
                PT2X = x - PT1X;                                                                    // Eclipse Width
                PT2Y = y - PT1Y;                                                                    // Eclipse Height
                DrawPackage localDP = PrepareDrawPackage();                                         // Create the Package 
                DrawShape(localDP);                                                                 // Draw it
                x = e.Location.X; y = e.Location.Y;
            }
        }
        private void Drawing_Paint(object sender, PaintEventArgs e)                                 // Shape Preview 
        {
            if (Drawing) {
                Graphics G = e.Graphics;
                Pen pen = new(btnColor.SelectedColor, (int)nudSize.Value);
                SolidBrush brush = new(btnFillColor.SelectedColor);
                switch (cbBType.Text) {
                    case "Circle":
                        G.DrawEllipse(pen, PT1X, PT1Y, PT2X, PT2Y);                                 // Draw
                        if (cbFillDraw.Checked) {
                            G.FillEllipse(brush, PT1X, PT1Y, PT2X, PT2Y);                           // Fill
                        }
                        break;
                    case "Fill Tool":
                        break;
                    case "Line":
                        G.DrawLine(pen, PT1X, PT1Y, x, y);
                        break;
                    case "Rectangle":
                        var rc = new Rectangle(
                            Math.Min(PT1.X, PT2.X),
                            Math.Min(PT1.Y, PT2.Y),
                            Math.Abs(PT2.X - PT1.X),
                            Math.Abs(PT2.Y - PT1.Y));
                        G.DrawRectangle(pen, rc);                                                   // Draw
                        if (cbFillDraw.Checked) {
                            e.Graphics.FillRectangle(brush, rc);                                    // Fill
                        }
                        break;
                    case "Triangle":
                        double midX = (PT1.X + PT2.X) / 2;
                        Point first = new(PT1.X, PT2.Y);
                        Point mid = new((int)midX, PT1.Y);
                        var tPath = new GraphicsPath();
                        tPath.AddLines(new PointF[] {
                            first, mid, PT2,
                        });
                        tPath.CloseFigure();
                        G.DrawPath(pen, tPath);                                                     // Draw
                        if (cbFillDraw.Checked) {
                            G.FillPath(brush, tPath);                                               // Fill
                        }
                        break;
                    case "Pen w/ Close":
                        G.DrawLine(pen, PT1X, PT1Y, x, y);
                        goto default;
                    default:
                        mPath ??= new();
                        G.DrawPath(pen, mPath);
                        break;
                }
            }
        }
        private void Drawing_Resize(object sender, EventArgs e)                                     // Canvas resize 
        {
            UpdateCanvas();
        }
        private void UpdateCanvas()                                                                 // Update the PictureBox and Graphics 
        {
            Bitmap savedImage = (Bitmap)picDrawing.Image;                                           // Save the current image to a Bitmap object
            Bitmap BM = new(picDrawing.Width, picDrawing.Height);                                   // Create a new Bitmap object with the resized dimensions
            using (Graphics G = Graphics.FromImage(BM)) {                                           // Draw the saved image onto the resized Bitmap
                G.DrawImage(savedImage, new Rectangle(0, 0, picDrawing.Width, picDrawing.Height));
            }
            picDrawing.Image = BM;                                                                  // Set the resized Bitmap as the new image for the PictureBox
            G = Graphics.FromImage(BM);                                                             // Set the canvas
        }

        private void Drawing_MouseClick(object sender, MouseEventArgs e)                            // Fill Tool 
        {
            if (Drawing && cbBType.Text == "Fill Tool" && e.Button == MouseButtons.Left) {
                Color fillcolor = btnFillColor.SelectedColor;
                Point canvas = Set_Point(picDrawing, e.Location);                                   // Get location of canvas
                FillTool(BM, canvas.X, canvas.Y, fillcolor);                                        // Do the Fill

                FillPackage fillPack = new() {                                                      // Details for Broadcast
                    FillColor = ColorToString(ref fillcolor),                                      // Convert colours to strings of their ARGB
                    X = canvas.X,
                    Y = canvas.Y
                };

                if (listening) {
                    string json = JsonConvert.SerializeObject(fillPack);                            // Format the Package 
                    HostSendPublic(json);                                                           // Host Send Fill Package
                } else if (connected) {
                    string json = JsonConvert.SerializeObject(fillPack);                            // Format the Package 
                    Send(json);                                                                     // Client Send Fill Package
                }
            }
        }
        static Point Set_Point(PictureBox pb, Point pt)                                             // Calculare Fill Point on Canvas 
        {
            float pX = 1f * pb.Image.Width / pb.Width;
            float pY = 1f * pb.Image.Height / pb.Height;
            return new Point((int)(pt.X * pX), (int)(pt.Y * pY));
        }
        private void FillTool(Bitmap bm, int x, int y, Color c2)                                    // Fill Process 
        {
            Color c1 = bm.GetPixel(x, y);                                                           // Get color of clicked pixel
            Stack<Point> pixel = new();
            pixel.Push(new Point(x, y));
            bm.SetPixel(x, y, c2);                                                                  // Replaced clicked pixel color
            if (c1 == c2) return;
            while (pixel.Count > 0) {
                Point pt = (Point)pixel.Pop();
                if (pt.X > 0 && pt.Y > 0) {                                                         // check 4 pixels around XY
                    Validate(bm, pixel, pt.X - 1, pt.Y, c1, c2);
                    Validate(bm, pixel, pt.X, pt.Y - 1, c1, c2);
                    Validate(bm, pixel, pt.X + 1, pt.Y, c1, c2);
                    Validate(bm, pixel, pt.X, pt.Y + 1, c1, c2);
                }
            }
        }
        private void Validate(Bitmap bm, Stack<Point> sp, int x, int y, Color c1, Color c2)         // Fill 
        {
            if (x < bm.Width && y < bm.Height) {
                Color cx = bm.GetPixel(x, y);                                                       // Get pixel color
                if (cx == c1) {
                    sp.Push(new Point(x, y));
                    bm.SetPixel(x, y, c2);                                                          // Change pixel color
                }
            }
        }

        private void ReplaceTargetColor(Bitmap BM, Color tCol, Color rCol)                          // Replace target pixel colour 
        {
            if (tCol == Color.Empty) { tCol = Color.LightGoldenrodYellow; }                         // If Empty, replaces TrueTransP Key Color 
            if (rCol == Color.Empty) { rCol = Color.Transparent; }                                  // with Color.Transparancy

            for (int x = 0; x < BM.Width; x++) {
                for (int y = 0; y < BM.Height; y++) {                                               // Loop over image 
                    if (BM.GetPixel(x, y) == tCol) {                                                // When mathching target caolour
                        BM.SetPixel(x, y, rCol);                                                    // Change pixel to replacement colour
                    }
                }
            }
        }
        private void BGColorChange(object sender, EventArgs e)                                      // Background - Uses Replace - This happens 3 Times and I'm not sure why 
        {
            //ReplaceTargetColor(BM, _preBC, ((ColorButton)sender).SelectedColor);                  // BUG - faster but only works the second time

            for (int x = 0; x < BM.Width; x++) {                                                    // BUG Slow - Iterate over the pixels in the bitmap
                for (int y = 0; y < BM.Height; y++) {
                    if (BM.GetPixel(x, y).A == _preBC.A
                        && BM.GetPixel(x, y).R == _preBC.R
                        && BM.GetPixel(x, y).G == _preBC.G
                        && BM.GetPixel(x, y).B == _preBC.B) {                                       // Check if the current pixel has the original color                        
                        BM.SetPixel(x, y, ((ColorButton)sender).SelectedColor);                     // Replace the color of the current pixel with the replacement color
                    }
                }
            }
            _preBC = ((ColorButton)sender).SelectedColor;
            picDrawing.Image = BM;
            G = Graphics.FromImage(BM);
        }

        private void DeleteTemps(string filePath = "")                                              // Remove tmp image file 
        {
            if (filePath.Length > 0) {
                try {
                    File.Delete(filePath);                                                          // Delete specified temp file
                    Console("Temp file successfully deleted.");
                } catch (Exception ex) {
                    Console(ErrorMsg("DF1: " + ex.Message));
                }
            } else {
                string runPath = Application.StartupPath;                                           // Directory to delete from
                string[] tempFiles = Directory.GetFiles(runPath, "*.tmp");                          // Filetype to delete
                foreach (string tempFile in tempFiles) {
                    try {
                        File.Delete(tempFile);                                                      // Delete those found
                        Console("Temp file successfully deleted.");
                    } catch (Exception ex) {
                        Console(ErrorMsg("DF2: " + ex.Message));
                    }
                }
            }
        }
        private void SaveDrawing()                                                                  // Drawing Save Routine 
        {
            SaveFileDialog saveFileDialog = new() {                                                 // Prompt user for Save File
                Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp|GIF Image|*.gif"
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                string fileName = saveFileDialog.FileName;                                          // What and Where
                string extension = Path.GetExtension(fileName);
                ImageFormat imageFormat;
                switch (extension.ToLower()) {                                                      // ImageFormat based on the file extension
                    case ".jpg":
                    case ".jpeg":
                        imageFormat = ImageFormat.Jpeg;
                        break;
                    case ".png":
                        imageFormat = ImageFormat.Png;
                        break;
                    case ".bmp":
                        imageFormat = ImageFormat.Bmp;
                        break;
                    case ".gif":
                        imageFormat = ImageFormat.Gif;
                        break;
                    default:
                        Console(ErrorMsg("Invalid file format."));
                        return;
                }
                Bitmap btm = BM.Clone(new Rectangle(0, 0, picDrawing.Width, picDrawing.Height), BM.PixelFormat);
                if (imageFormat == ImageFormat.Png) { ReplaceTargetColor(btm, Color.Empty, Color.Empty); }    // TrueTransP Key colour conversion
                btm.Save(saveFileDialog.FileName, imageFormat);                                     // Save the image in the selected format                
                Console("Image Saved: " + saveFileDialog.FileName);
            }
        }
        private void DrawShape(DrawPackage drawPackage)                                             // Draw the shape from package 
        {
            picDrawing.Invoke((MethodInvoker)delegate {
                using var pen = new Pen(ArgbColor(drawPackage.PenColor), drawPackage.PenSize);      // Set Pen
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                SolidBrush brush = new(ArgbColor(drawPackage.BrushColor));                          // Set Brush

                GraphicsPath pPath = new();                                                         // Set Path
                if (drawPackage.PPath != null && drawPackage.PPath.Length > 0) {
                    for (int i = 0; i < drawPackage.PPath.Length; i++) {                            // Add each Point
                        pPath.AddLines(new PointF[] {                                               // to Draw Package
                                drawPackage.PPath[i]
                        });
                    }
                }

                switch (drawPackage.DrawType) {
                    case "Fill Tool":
                        break;
                    case "Line":
                        G.DrawLine(pen, drawPackage.PT1.X, drawPackage.PT1.Y, drawPackage.X, drawPackage.Y);
                        break;
                    case "Circle":
                        if (drawPackage.Fill) {
                            G.FillEllipse(brush, drawPackage.PT1.X, drawPackage.PT1.Y, drawPackage.PT2X, drawPackage.PT2Y);
                        }
                        G.DrawEllipse(pen, drawPackage.PT1.X, drawPackage.PT1.Y, drawPackage.PT2X, drawPackage.PT2Y);
                        break;
                    case "Rectangle":
                        var rc = new Rectangle(
                            Math.Min(drawPackage.PT1.X, drawPackage.PT2.X),
                            Math.Min(drawPackage.PT1.Y, drawPackage.PT2.Y),
                            Math.Abs(drawPackage.PT2.X - drawPackage.PT1.X),
                            Math.Abs(drawPackage.PT2.Y - drawPackage.PT1.Y));
                        if (drawPackage.Fill) {
                            G.FillRectangle(brush, rc);                                             // Fill
                        }
                        G.DrawRectangle(pen, rc);                                                   // Draw
                        break;
                    case "Triangle":
                        double midX = (drawPackage.PT1.X + drawPackage.PT2.X) / 2;
                        double midY = (drawPackage.PT1.Y + drawPackage.PT2.Y) / 2;
                        Point first = new(drawPackage.PT1.X, drawPackage.PT2.Y);
                        Point mid = new((int)midX, drawPackage.PT1.Y);
                        var tPath = new GraphicsPath();
                        tPath.AddLines(new PointF[] {
                            first, mid, drawPackage.PT2,
                        });
                        tPath.CloseFigure();
                        if (drawPackage.Fill) {
                            G.FillPath(brush, tPath);                                               // Fill
                        }
                        G.DrawPath(pen, tPath);                                                     // Draw
                        break;
                    case "Pen w/ Close":
                        if (drawPackage.Fill) {
                            G.FillPath(brush, pPath);
                        }
                        G.DrawLine(pen, drawPackage.PT1X, drawPackage.PT1Y, drawPackage.X, drawPackage.Y);
                        goto default;
                    default:
                        G.DrawPath(pen, pPath);
                        break;
                }

                if (listening && !remoteDraw) {
                    string json = JsonConvert.SerializeObject(drawPackage) + "\n";                  // Format the Package 
                    HostSendPublic(json);                                                           // Host Send Draw Package
                } else if (connected && !remoteDraw) {
                    string json = JsonConvert.SerializeObject(drawPackage) + "\n";                  // Format the Package 
                    Send(json);                                                                     // Client Send Draw Package
                }
                picDrawing.Refresh();                                                               // Update the Canvas
                pPath.Reset();                                                                      // Clean up
                pen.Dispose();
                brush.Dispose();
                remoteDraw = false;
            });
        }
        #endregion


        #region Network
        // Host, Join, and DC
        private void Connected(bool status)                                                         // Client - Mode Tasks 
        {
            cmdJoin.Invoke((MethodInvoker)delegate {
                connected = status;
                if (status) {
                    ToggleNetworkControls();
                    clientsDataGridView.Columns["dc"].Visible = false;
                    clientsDataGridView.Columns["latency"].Visible = true;

                    btnClearAll.Visible = false;
                    btnColor.SelectedColor = cmdColor.SelectedColor;

                    cmdJoin.Enabled = !cmdJoin.Enabled;
                    cmdJoin.Text = "Disconnect";

                    Program.MainForm.Text = "Joined as " + txtName.Text.Trim();
                    Console(SystemMsg("You are now connected"));
                    FileReceive();                                                                  // Get Drawing on connect
                } else {
                    ToggleNetworkControls();
                    cmdJoin.Enabled = !cmdJoin.Enabled;
                    cmdJoin.Text = "Connect";

                    Program.MainForm.Text = "Join or Host";
                    ClearDataGrid();
                    Console(SystemMsg("You are now disconnected"));
                }
            });
        }
        private void Listening(bool status)                                                         // Host - Mode Tasks 
        {
            cmdHost.Invoke((MethodInvoker)delegate {
                listening = status;
                if (status) {
                    ToggleNetworkControls();
                    clientsDataGridView.Columns["dc"].Visible = true;
                    clientsDataGridView.Columns["latency"].Visible = true;

                    btnClearAll.Visible = true;
                    btnColor.SelectedColor = cmdColor.SelectedColor;

                    cmdHost.Enabled = !cmdHost.Enabled;
                    cmdHost.Text = "Stop";

                    Program.MainForm.Text = txtName.Text.Trim() + " is Hosting";
                    Console(SystemMsg("Server has started"));
                    FileServe();                                                                    // Start File Server
                } else {
                    ToggleNetworkControls();
                    cmdHost.Text = "Host";
                    Program.MainForm.Text = "Join or Host";

                    ClearDataGrid();
                    Console(SystemMsg("Server has stopped"));
                }
            });
        }
        private void Disconnect(long id = -1)                                                       // Host - Disconnect ID / All if empty 
        {
            if (disconnect == null || !disconnect.IsAlive) {
                disconnect = new Thread(() => {                                                     // New Thread
                    if (id > 0) {
                        players.TryGetValue(id, out MyPlayers obj);                                 // Check for the Specified Connection
                        RemoveFromGrid(obj.id);                                                     // Remove from Grid
                        obj.client.Close();                                                         // Close the Connection
                    } else {
                        foreach (KeyValuePair<long, MyPlayers> obj in players) {                    // Each Client
                            RemoveFromGrid(obj.Value.id);                                           // Remove and
                            obj.Value.client.Close();                                               // Close
                        }
                    }
                }) { IsBackground = true };
                disconnect.Start();
            }
        }

        private void Connection(IPAddress ip, int port, string username, string roomkey)            // C - Single TCP to host 
        {
            try {
                clientObject = new Client {
                    username = username,
                    key = roomkey,
                    color = cmdColor.SelectedColor,                                                     // save player COLOURS to Client
                    client = new TcpClient(),
                };
                clientObject.client.Connect(ip, port);
                clientObject.stream = clientObject.client.GetStream();
                clientObject.buffer = new byte[clientObject.client.ReceiveBufferSize];
                clientObject.data = new StringBuilder();
                clientObject.handle = new EventWaitHandle(false, EventResetMode.AutoReset);
                if (Authorize()) {
                    while (clientObject.client.Connected) {
                        try {
                            clientObject.stream.BeginRead(clientObject.buffer, 0, clientObject.buffer.Length, new AsyncCallback(Read), null);
                            clientObject.handle.WaitOne();
                        } catch (Exception ex) {
                            Console(ErrorMsg("CC1: " + ex.Message));
                        }
                    }
                    clientObject.client.Close();
                    Connected(false);
                }
            } catch (Exception ex) {
                Console(ErrorMsg("CC2: " + ex.Message));
            }
        }
        private void Connection(MyPlayers obj)                                                      // H - Multi TCP to clients 
        {
            if (Authorize(obj)) {                                                                   // If authorized
                string msg = string.Format("{0} has connected.", obj.username);
                players.TryAdd(obj.id, obj);
                AddToGrid(obj.id, obj.username.ToString(), obj.color);                              // Add new client to grid
                Console(SystemMsg(msg));
                HostSendPublic(SystemMsg(msg), obj.id);                                             // Broadcast the Con
                while (obj.client.Connected) {
                    try {
                        obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(Read), obj);
                        obj.handle.WaitOne();
                    } catch (Exception ex) {
                        Console(ErrorMsg("HC: " + ex.Message));
                    }
                }
                obj.client.Close();                                                                 // DC Client
                players.TryRemove(obj.id, out MyPlayers tmp);
                RemoveFromGrid(tmp.id);                                                             // Remove from grid
                msg = string.Format("{0} has disconnected.", tmp.username);
                Console(SystemMsg(msg));
                HostSendPublic(SystemMsg(msg), tmp.id);                                             // Broadcast the DC
            } else { obj.client.Close(); }
        }
        private int GetLowestFreeID(DataGridView DGV)                                               // Lowest Free ID for clients from DGV 
        {
            int lowestFreeID = 1;               // Start with 1
            foreach (DataGridViewRow row in DGV.Rows) {
                if (int.TryParse(row.Cells[0].Value.ToString(), out int id)) {
                    if (id == lowestFreeID) {
                        lowestFreeID++;         // ID already exists, increment to the next one
                    } else if (id > lowestFreeID) {
                        break;                  // break on higher ID
                    }
                }
            }
            return lowestFreeID;
        }

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

        // Client Send
        private void Send(string msg)                                                               // Client Send 
        {
            if (!listening) {
                var encryptedString = AesOperation.EncryptString(AeS, msg);
                if (send == null || send.IsCompleted) {
                    send = Task.Factory.StartNew(() => BeginWrite(encryptedString));
                } else {
                    send.ContinueWith(antecendent => BeginWrite(encryptedString));
                }
            }
        }
        private void BeginWrite(string msg)                                                         // Client Begin 
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            if (clientObject.client.Connected) {
                try {
                    clientObject.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(Write), null);
                } catch (Exception ex) {
                    Console(ErrorMsg("CBW: " + ex.Message));
                }
            }
        }
        private void Write(IAsyncResult result)                                                     // Client Write 
        {
            if (clientObject.client.Connected) {
                try {
                    clientObject.stream.EndWrite(result);
                } catch (Exception ex) {
                    Console(ErrorMsg("CW: " + ex.Message));
                }
            }
        }

        // Host Send
        private void HostSendPrivate(string msg, MyPlayers obj)                                     // Host - send Private message 
        {
            var encryptedString = AesOperation.EncryptString(AeS, msg);
            if (send == null || send.IsCompleted) {
                send = Task.Factory.StartNew(() => HostBeginPrivate(encryptedString, obj));
            } else {
                send.ContinueWith(antecendent => HostBeginPrivate(encryptedString, obj));
            }
        }
        private void HostSendPublic(string msg, long id = -1)                                       // Host - send Public message 
        {
            var encryptedString = AesOperation.EncryptString(AeS, msg);
            if (send == null || send.IsCompleted) {
                send = Task.Factory.StartNew(() => HostBeginPublic(encryptedString, id));
            } else {
                send.ContinueWith(antecendent => HostBeginPublic(encryptedString, id));
            }
        }
        private void HostBeginPrivate(string msg, MyPlayers obj)                                    // Host - BeginWrite Private message to stream 
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            if (obj.client.Connected) {
                try {
                    obj.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(HostEndWrite), obj);
                } catch (Exception ex) {
                    Console(ErrorMsg("HBPr: " + ex.Message));
                }
            }
        }
        private void HostBeginPublic(string msg, long id = -1)                                      // Host - BeginWrite Public -- set ID to lesser than zero to send to everyone 
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            foreach (KeyValuePair<long, MyPlayers> obj in players) {
                if (id != obj.Value.id && obj.Value.client.Connected) {
                    try {
                        obj.Value.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(HostEndWrite), obj.Value);
                    } catch (Exception ex) {
                        Console(ErrorMsg("HBPu: " + ex.Message));
                    }
                }
            }
        }
        private void HostEndWrite(IAsyncResult result)                                              // Host - EndWrite 
        {
            MyPlayers obj = (MyPlayers)result.AsyncState;
            if (obj.client.Connected) {
                try {
                    obj.stream.EndWrite(result);
                } catch (Exception ex) {
                    Console(ErrorMsg("HEW: " + ex.Message));
                }
            }
        }

        // Handshake
        private bool Authorize()                                                                    // Client - Handshake 
        {
            bool success = false;
            Dictionary<string, string> handShake = new() {                                          // Collect info to send as object Handshake
                { "username", clientObject.username },
                { "roomkey", clientObject.key },
                { "color" , Convert.ToString(cmdColor.SelectedColor.A) +","+ Convert.ToString(cmdColor.SelectedColor.R) +","+ Convert.ToString(cmdColor.SelectedColor.G) +","+ Convert.ToString(cmdColor.SelectedColor.B) }, // Send COLOURS to host
            };
            JavaScriptSerializer json = new();                                                      // Format the Handshake object
            Send(json.Serialize(handShake));                                                        // Client send Handshake to Host
            while (clientObject.client.Connected) {
                try {
                    clientObject.stream.BeginRead(clientObject.buffer, 0, clientObject.buffer.Length, new AsyncCallback(ReadAuth), null);
                    clientObject.handle.WaitOne();
                    if (connected) {                                                                // on connection
                        success = true;
                        break;
                    }
                } catch (Exception ex) {
                    Console(ErrorMsg("CHS: " + ex.Message));
                }
            }
            if (!connected) {                                                                       // Connection refused
                Console(SystemMsg("Unauthorized: Confirm the RoomKey, or try another name."));
                success = false;
            }
            return success;
        }
        private bool Authorize(MyPlayers obj)                                                       // Host - Handshake 
        {
            bool success = false;
            while (obj.client.Connected) {
                try {
                    obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(ReadAuth), obj);
                    obj.handle.WaitOne();
                    foreach (DataGridViewRow row in clientsDataGridView.Rows) {
                        if (obj.username.ToString() == row.Cells[1].Value.ToString()) {
                            success = false; break;
                        } else { success = true; }
                    }
                    break;
                } catch (Exception ex) {
                    Console(ErrorMsg("HHS: " + ex.Message));
                }
            }
            return success;
        }
        private void ReadAuth(IAsyncResult result)                                                  // Handshake Readers 
        {
            if (listening) {                                                                        // Host read
                MyPlayers obj = (MyPlayers)result.AsyncState;
                int bytes = 0;
                if (obj.client.Connected) {
                    try {
                        bytes = obj.stream.EndRead(result);
                    } catch (Exception ex) {
                        Console(ErrorMsg("HRA1: " + ex.Message));
                    }
                }
                if (bytes > 0) {
                    obj.data.AppendFormat("{0}", Encoding.UTF8.GetString(obj.buffer, 0, bytes));
                    try {
                        if (obj.stream.DataAvailable) {
                            obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(ReadAuth), obj);
                        } else {
                            var decryptedString = AesOperation.DecryptString(AeS, obj.data.ToString()); // Decrypt
                            JavaScriptSerializer json = new();
                            Dictionary<string, string> data = json.Deserialize<Dictionary<string, string>>(decryptedString); // Unpack

                            if (!data.ContainsKey("username") || data["username"].Length < 1 || !data.ContainsKey("roomkey") || !data["roomkey"].Equals(txtRoomKey.Text)) {
                                obj.client.Close();
                            } else {
                                obj.username.Append(data["username"].Length > 200 ? data["username"].Substring(0, 200) : data["username"]);

                                string[] colParts = data.ElementAt(2).Value.Split(',');             // COLOURS
                                Color myColor = Color.FromArgb(Convert.ToInt32(colParts[0]), Convert.ToInt32(colParts[1]), Convert.ToInt32(colParts[2]), Convert.ToInt32(colParts[3]));
                                obj.color = myColor;
                                HostSendPrivate("{\"status\": \"authorized\"}", obj);
                            }
                            obj.data.Clear();
                            obj.handle.Set();
                        }
                    } catch (Exception ex) {
                        obj.data.Clear();
                        Console(ErrorMsg("HRA2: " + ex.Message));
                        obj.handle.Set();
                    }
                } else {
                    obj.client.Close();
                    obj.handle.Set();
                }
            } else {                                                                                // Client Read
                int bytes = 0;
                if (clientObject.client.Connected) {
                    try {
                        bytes = clientObject.stream.EndRead(result);
                    } catch (Exception ex) {
                        Console(ErrorMsg("CRA1: " + ex.Message));
                    }
                }
                if (bytes > 0) {
                    clientObject.data.AppendFormat("{0}", Encoding.UTF8.GetString(clientObject.buffer, 0, bytes));
                    try {
                        if (clientObject.stream.DataAvailable) {
                            clientObject.stream.BeginRead(clientObject.buffer, 0, clientObject.buffer.Length, new AsyncCallback(ReadAuth), null);
                        } else {
                            var decryptedString = AesOperation.DecryptString(AeS, clientObject.data.ToString()); // Decrypt
                            JavaScriptSerializer json = new();
                            Dictionary<string, string> data = json.Deserialize<Dictionary<string, string>>(decryptedString);

                            if (data.ContainsKey("status") && data["status"].Equals("authorized")) {
                                Connected(true);
                            }
                            clientObject.data.Clear();
                            clientObject.handle.Set();
                        }
                    } catch (Exception ex) {
                        clientObject.data.Clear();
                        Console(ErrorMsg("CRA2: " + ex.Message));
                        clientObject.handle.Set();
                    }
                } else {
                    clientObject.client.Close();
                    clientObject.handle.Set();
                }
            }
        }

        #region Backwards Client Drawing Server
        // Client Server - Drawing File Transfer
        private void SendDrawing()                                                                  // BW - Drawing Send Routine 
        {
            HostSendPublic("CMD:ReceiveDrawing");                                                   // CMD to make Client begin File server

            string appPath = Application.StartupPath;                                               // Prep the file
            string filePath = Path.Combine(appPath, "picDrawing.tmp");
            Bitmap btm = BM.Clone(new Rectangle(0, 0, picDrawing.Width, picDrawing.Height), BM.PixelFormat);
            btm.Save(filePath, ImageFormat.Png);

            SendFile(filePath);                                                                     // Send
            DeleteTemps(filePath);                                                                  // Clean
        }
        private void SendFile(string fn)                                                            // BW - Send file - FileName 
        {
            try {
                IPEndPoint ipEnd = new(IPAddress.Parse(txtAddress.Text), Convert.ToInt16(txtPort.Text) + 2);    // EG: 9002
                Socket clientSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

                string fileName = fn;
                byte[] fileNameByte = Encoding.ASCII.GetBytes(fileName);
                byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);
                byte[] fileData = File.ReadAllBytes(fileName);
                byte[] clientData = new byte[4 + fileNameByte.Length + fileData.Length];

                fileNameLen.CopyTo(clientData, 0);
                fileNameByte.CopyTo(clientData, 4);
                fileData.CopyTo(clientData, 4 + fileNameByte.Length);

                clientSocket.Connect(ipEnd);
                clientSocket.Send(clientData);
                clientSocket.Close();

                //[0]filenamelen[4]filenamebyte[*]filedata
            } catch (Exception ex) {
                Console(ErrorMsg("SF: " + ex.Message));
            }
        }
        private void ReceiveFile(Socket clientSocket, string n)                                     // BW - Receive / Save / Show 
        {
            Console("Incoming file....");
            byte[] clientData = new byte[1024 * 5000];
            int receivedBytesLen = clientSocket.Receive(clientData);
            int fileNameLen = BitConverter.ToInt32(clientData, 0);
            //string filePath = Encoding.ASCII.GetString(clientData, 4, fileNameLen);               // use native file path
            string appPath = Application.StartupPath;
            string filePath = Path.Combine(appPath, "pic_" + n + ".tmp");                           // set AppPath
            BinaryWriter bWrite = new(File.Open(filePath, FileMode.Create));
            bWrite.Write(clientData, 4 + fileNameLen, receivedBytesLen - 4 - fileNameLen);
            bWrite.Close();
            clientSocket.Close();

            //[0]filenamelen[4]filenamebyte[*]filedata

            Image BM = Image.FromFile(filePath); // Load image from a file
            picDrawing.Invoke((MethodInvoker)delegate {
                picDrawing.Image = BM;
                picDrawing.Refresh();
            });
            G = Graphics.FromImage(BM);

        }
        private void ListenForFile()                                                                // BW - Client - Server for image files 
        {
            IPEndPoint ipEnd = new(IPAddress.Parse(txtAddress.Text), Convert.ToInt16(txtPort.Text) + 2);    // EG: 9002
            Socket serverSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP); ;
            serverSocket.Bind(ipEnd);

            int counter = 0;
            serverSocket.Listen(Convert.ToInt16(txtPort.Text) + 2);
            Console("Picture Server Started");
            while (true) {
                counter += 1;
                Socket clientSocket = serverSocket.Accept();
                Console("Picture #:" + Convert.ToString(counter));
                Thread receiveThread = new(delegate () {
                    ReceiveFile(clientSocket, Convert.ToString(counter));
                });
                receiveThread.Start();
                receiveThread.Join();                                                               // Wait for the serveThread to complete its execution
                if (receiveThread.ThreadState == System.Threading.ThreadState.Stopped) {            // Check if the serveThread has completed its execution
                    Console("Picture Sent");
                    //break;
                }
            }
        }
        #endregion

        // Host Server
        private void Listener(IPAddress ip, int port)                                               // Host - TCP Listener 
        {
            TcpListener listener = null;
            try {
                listener = new TcpListener(ip, port);
                listener.Start();
                Listening(true);
                while (listening) {
                    if (listener.Pending()) {
                        try {
                            MyPlayers obj = new() {
                                id = GetLowestFreeID(clientsDataGridView),                          // find < available number from grid
                                username = new StringBuilder(),
                                client = listener.AcceptTcpClient(),
                                color = new Color()
                            };
                            obj.stream = obj.client.GetStream();
                            obj.buffer = new byte[obj.client.ReceiveBufferSize];
                            obj.data = new StringBuilder();
                            obj.handle = new EventWaitHandle(false, EventResetMode.AutoReset);
                            Thread th = new(() => Connection(obj)) {
                                IsBackground = true
                            };
                            th.Start();
                        } catch (Exception ex) {
                            Console(ErrorMsg("HL1: " + ex.Message));
                        }
                    } else {
                        Thread.Sleep(500);
                    }
                }
                Listening(false);
            } catch (Exception ex) {
                Console(ErrorMsg("HL2: " + ex.Message));
            } finally {
                listener?.Server.Close();
            }
        }
        // Host Server - Drawing File Transfer
        private void FileServe()                                                                    // Host - File Server 
        {
            try {
                Thread.Sleep(100);                                                                  // Prep Server
                TcpListener drawingSocket = new(IPAddress.Any, Convert.ToInt16(txtPort.Text) + 1);
                var clientSocket = default(TcpClient);
                int counter = 0;
                var source = new CancellationTokenSource();
                Task.Factory.StartNew(() => {
                    while (true) {
                        drawingSocket.Start();                                                      // Start Drawing Server
                        Console("File Server Started");
                        clientSocket = drawingSocket.AcceptTcpClient();                             // New client socket on connect
                        counter += 1;
                        var networkStream = clientSocket.GetStream();
                        Console("Connection #: " + counter.ToString() + " started.");

                        string appPath = Application.StartupPath;                                   // Prep save file
                        string filePath = Path.Combine(appPath, "picDrawing.tmp");
                        Bitmap btm = BM.Clone(new Rectangle(0, 0, picDrawing.Width, picDrawing.Height), BM.PixelFormat);
                        btm.Save(filePath, ImageFormat.Jpeg);

                        byte[] fileNameByte = Encoding.ASCII.GetBytes(filePath);                    // Create File Details
                        byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);
                        byte[] fileData = File.ReadAllBytes(filePath);
                        byte[] clientData = new byte[4 + fileNameByte.Length + fileData.Length];

                        fileNameLen.CopyTo(clientData, 0);                                          // Pack the data
                        fileNameByte.CopyTo(clientData, 4);
                        fileData.CopyTo(clientData, 4 + fileNameByte.Length);

                        networkStream.Write(clientData, 0, clientData.Length);                      // Send it

                        networkStream.Close();                                                      // Clean up
                        clientSocket.Close();
                        Console("Connection #: " + counter.ToString() + " closed.");

                        DeleteTemps(filePath);
                    }
                }, source.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            } catch (Exception w) {
                MessageBox.Show("FS: Connection error: " + w.ToString());
            }
        }
        private void FileReceive()                                                                  // Client - File Receiver 
        {
            Console("Connecting..");
            var client = new TcpClient(txtAddress.Text, Convert.ToInt16(txtPort.Text) + 1);         // Connect
            var networkStream = client.GetStream();                                                 // Receive Drawing
            Console("Receiving Drawing..");

            byte[] fileNameLenBytes = new byte[4];                                                  // Create File Details
            networkStream.Read(fileNameLenBytes, 0, 4);
            int fileNameLen = BitConverter.ToInt32(fileNameLenBytes, 0);
            byte[] fileNameBytes = new byte[fileNameLen];

            networkStream.Read(fileNameBytes, 0, fileNameLen);                                      // Read File Path/Name

            //string fileName = Encoding.ASCII.GetString(fileNameBytes);
            string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".tmp";                    // Set new Path/Name
            string filePath = Path.Combine(Application.StartupPath, fileName);

            /* From Stream (not working)
             * using (var fileStream = new FileStream(filePath, FileMode.Open)) {                      // Set Image from Stream
                            Image BM = Image.FromStream(fileStream);
                            picDrawing.Invoke((MethodInvoker)delegate {
                                picDrawing.Image = BM;
                                picDrawing.Refresh();
                                G = Graphics.FromImage(BM);
                                G.SmoothingMode = SmoothingMode.AntiAlias;
                            });
                        }
            */
            // From File - Working
            using (var fileStream = new FileStream(filePath, FileMode.Create)) {                    // New FileStream 
                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead;

                while ((bytesRead = networkStream.Read(buffer, 0, buffer.Length)) > 0) {
                    fileStream.Write(buffer, 0, bytesRead);                                         // Write the file
                    Console("Drawing Received.");
                }
            }
            Image BM = Image.FromFile(filePath);                                                    // Load image from a file
            picDrawing.Invoke((MethodInvoker)delegate {                                             // Set Image
                picDrawing.Image = BM;
                picDrawing.Refresh();
                Console("Image Set.");
                G = Graphics.FromImage(BM);                                                         // Prep Canvas for Drawing

            });
            // End From File

            Console("Closing Connection.");
            networkStream.Close();                                                                  // Clean up
            client.Close();
        }

        // Host and Client Read
        private void Read(IAsyncResult result)                                                      // Read Routine 
        {
            if (listening) {                                                                        // Host stream reader
                MyPlayers obj = (MyPlayers)result.AsyncState;
                int bytes = 0;
                if (obj.client.Connected) {
                    try {
                        bytes = obj.stream.EndRead(result);
                    } catch (Exception ex) {
                        Console(ErrorMsg("HR1: " + ex.Message));
                    }
                }
                if (bytes > 0) {
                    obj.data.AppendFormat("{0}", Encoding.UTF8.GetString(obj.buffer, 0, bytes));
                    try {
                        if (obj.stream.DataAvailable) {
                            obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(Read), obj);   // Host Receive
                        } else {
                            string clientData = AesOperation.DecryptString(AeS, obj.data.ToString());   // Decrypt
                            HostDataHandler(obj, clientData);                                       // Host Data Handler
                        }
                    } catch (Exception ex) {
                        obj.data.Clear();
                        Console(ErrorMsg("HR2: " + ex.Message));
                        obj.handle.Set();
                    }
                } else {
                    obj.client.Close();
                    obj.handle.Set();
                }
            }                                                                           // End of Host stream reader
            else {
                if (clientObject == null) { return; }
                int bytes = 0;
                if (clientObject.client.Connected) {
                    try {
                        bytes = clientObject.stream.EndRead(result);
                    } catch (Exception ex) {
                        Console(ErrorMsg("CSR1: " + ex.Message));
                    }
                }
                if (bytes > 0) {
                    clientObject.data.AppendFormat("{0}", Encoding.UTF8.GetString(clientObject.buffer, 0, bytes));
                    try {
                        if (clientObject.stream.DataAvailable) {
                            clientObject.stream.BeginRead(clientObject.buffer, 0, clientObject.buffer.Length, new AsyncCallback(Read), null);
                        } else {
                            string hostData = AesOperation.DecryptString(AeS, clientObject.data.ToString()); // Decrypt
                            ClientDataHandler(hostData);                                            // Client Data Handler
                        }
                    } catch (Exception ex) {
                        clientObject.data.Clear();
                        Console(ErrorMsg("CSR2: " + ex.Message));
                        clientObject.handle.Set();
                    }
                } else {
                    clientObject.client.Close();
                    clientObject.handle.Set();
                }
            }                                                                                   // End of CLient stream Reader
        }
        #endregion
    }
}
/*
 * Snips
 
if (listening) { ;}
else if (connected) { ;}





*/