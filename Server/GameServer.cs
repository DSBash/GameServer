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

namespace Server
{
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
        #endregion

        #region Message Settings
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
        private void TabSections_DrawItem(object sender, DrawItemEventArgs e)
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
                    textBox.BackColor = MyDialog.Color;                                             // Update the text box color if the user clicks OK
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