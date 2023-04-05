﻿using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Server
{
    public partial class GameServer : Form
    {
        public class PlayerPackage
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int[] Color { get; set; }
        }

        public class DrawPackage
        {
            public string PenColor { get; set; }
            public int PenSize { get; set; }
            public Point PT1 { get; set; }
            public Point PT2 { get; set; }
        }

        private Task send = null;

        // Host Specific
        private bool listening = false;
        private Thread listener = null;
        private Thread disconnect = null;
        private class MyPlayers
        {
            public long id;
            public Color color;
            public StringBuilder username;
            public TcpClient client;
            public NetworkStream stream;
            public byte[] buffer;
            public StringBuilder data;
            public EventWaitHandle handle;
        };
        private static readonly ConcurrentDictionary<long, MyPlayers> players = new();


        // Client Specific
        private bool connected = false;
        private Thread client = null;
        private class Client
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
        private Client clientObject;


        /* Used to enable Console Output */
        /*        [DllImport("kernel32.dll", SetLastError = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                static extern bool AllocConsole();
        */

        // Form
        public GameServer()
        {
            InitializeComponent();
            //AllocConsole();
        }
        private void GameServer_FormClosing(object sender, FormClosingEventArgs e)                  // Close connection on Exit 
        {
            if (connected) {
                clientObject.client.Close();
            } else {
                listening = false;
                Disconnect();
            }
        }

        // Console & Chats
        private void Console(string msg = "")                                                       // Console message / Clear if empty 
        {
            txtConsole.Invoke((MethodInvoker)delegate {
                if (msg.Length > 0) {
                    txtConsole.AppendText(string.Format("{2}[ {0} ] {1}", DateTime.Now.ToString("HH:mm:ss"), msg, Environment.NewLine));
                } else {
                    txtConsole.Clear();
                }
            });
        }
        private void PublicChat(string username, string msg = "")                                   // Console message / Clear if empty 
        {
            if (msg == null || msg.Length < 1) {
                txtLobby.Clear();
            } else {
                string formattedMSG = "";
                if (username.Contains("to you")) {
                    username = username.Replace("to you", "").Trim();
                    formattedMSG = string.Format("{0}[{1}] {2} to you: {3}", Environment.NewLine, DateTime.Now.ToString("HH:mm:ss"), username, msg);
                } else { formattedMSG = string.Format("{0}[{1}] {2}: {3}", Environment.NewLine, DateTime.Now.ToString("HH:mm:ss"), username, msg); }

                string playerColor = "255,0,0,0,0";
                foreach (DataGridViewRow row in clientsDataGridView.Rows) {
                    if (row.Cells[1].Value != null && row.Cells[1].Value.ToString() == username) {
                        playerColor = row.Cells[2].Value?.ToString();
                        break;
                    }
                }
                string[] colParts = playerColor.Split(',');
                Color msgColor = Color.FromArgb(Convert.ToInt32(colParts[0]), Convert.ToInt32(colParts[1]), Convert.ToInt32(colParts[2]), Convert.ToInt32(colParts[3]));
                txtLobby.Invoke((MethodInvoker)delegate {
                    txtLobby.AppendText(formattedMSG, msgColor);
                    txtLobby.Focus();
                    txtLobby.SelectionStart = txtLobby.Text.Length;
                    txtMessage.Focus();
                });
            }
        }
        private void PrivateChat(string username, string msg = "")                                  // Console message / Clear if empty 
        {
            if (msg == null || msg.Length < 1) {
                txtLobby.Clear();
            } else {

                if (username.Contains("to you")) { username = username.Replace("to you", "").Trim(); }
                string formattedMSG = string.Format("{0}[{1}] {2} to you: {3}", Environment.NewLine, DateTime.Now.ToString("HH:mm:ss"), username, msg);
                string playerColor = "255,0,0,0,0";
                foreach (DataGridViewRow row in clientsDataGridView.Rows) {
                    if (row.Cells[1].Value != null && row.Cells[1].Value.ToString() == username) {
                        playerColor = row.Cells[2].Value?.ToString();

                    }
                }
                string[] colParts = playerColor.Split(',');
                Color msgColor = Color.FromArgb(Convert.ToInt32(colParts[0]), Convert.ToInt32(colParts[1]), Convert.ToInt32(colParts[2]), Convert.ToInt32(colParts[3]));
                txtLobby.Invoke((MethodInvoker)delegate {
                    txtLobby.AppendText(formattedMSG, msgColor);
                    txtLobby.Focus();
                    txtLobby.SelectionStart = txtLobby.Text.Length;
                    txtMessage.Focus();
                });
            }
        }

        // Message formatters
        private string ErrorMsg(string msg)                                                         // Format Errors 
        {
            return string.Format("ERROR: {0}", msg);
        }
        private string SystemMsg(string msg)                                                        // Format System 
        {
            return string.Format("SYSTEM: {0}", msg);
        }

        // Message box & Send
        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)                              // Send on <Enter> 
        {
            if (e.KeyCode == Keys.Enter) {
                e.Handled = true;
                e.SuppressKeyPress = true;
                string msg = txtMessage.Text;
                if (txtMessage.Text.Length > 0 && !txtMessage.Text.StartsWith("/")) {
                    txtMessage.Clear();
                    PublicChat(txtName.Text.Trim(), msg);
                    if (listening) {
                        HostSendPublic(string.Format("{0}: {1}", txtName.Text.Trim(), msg));
                    }
                    if (connected) {
                        Send(msg);
                    }
                } else {
                    if (listening) {
                        if (msg.Contains("/msg")) {                                                 // PM - get MSG   /msg Player 1 Some text.
                            foreach (DataGridViewRow row in clientsDataGridView.Rows) {
                                if (msg.Contains(row.Cells[1].Value.ToString())) {                  // if name in grid
                                    int index = msg.IndexOf(row.Cells[1].Value.ToString());         // get start of name
                                    if (index != -1) { index += row.Cells[1].Value.ToString().Length; } // get end of name
                                    string hostPM = msg.Substring(index);                           // take text after length of name
                                    if (row.Index > 0) {
                                        if (players[row.Index].username.ToString() == row.Cells[1].Value.ToString()) {            // if dest is a player
                                            HostSendPrivate(string.Format("{0} to you: {1}", txtName.Text.Trim(), hostPM), players[row.Index]);     // PM - format string to  - Private Send
                                            Console(SystemMsg("PM Sent."));
                                        }
                                    } else {
                                        txtLobby.AppendText(string.Format("{0} to you: {1}", txtName.Text.Trim(), hostPM));
                                    }
                                }
                            }
                        }
                    }
                    if (connected) {
                        foreach (DataGridViewRow row in clientsDataGridView.Rows) {
                            if (msg.Contains(row.Cells[1].Value.ToString())) {                      // Find name in PM
                                int index = msg.IndexOf(row.Cells[1].Value.ToString());
                                if (index != -1) { index += row.Cells[1].Value.ToString().Length; }
                                string pMSG = msg.Substring(index);                                 // PM - format string to send to host
                                msg = pMSG.Trim();
                                Send(string.Format("/msg:{0}:{1}", row.Cells[1].Value.ToString(), msg));
                            }
                        }
                    }

                }
            }
        }
        private void TxtMessage_Enter(object sender, EventArgs e)                                   // MSG Note 
        {
            if (txtMessage.Text == "Type and press enter to send.") {
                txtMessage.Text = "";
            }
        }
        private void TxtMessage_Leave(object sender, EventArgs e)                                   // MSG Note 
        {
            if (txtMessage.Text == "") {
                txtMessage.Text = "Type and press enter to send.";
            }
        }

        // DataGrid management
        private void AddToGrid(long id, string name, Color color)                                   // Add Client 
        {
            int cA = color.A; int cB = color.B; int cR = color.R; int cG = color.G;         // convert COLOURS to string  - From Color to ARGB
                                                                                            //Color myColor = Color.FromArgb(cA,cR,cG,cB);                                  // From ARGB to Color
            string colorString = string.Format("{0},{1},{2},{3}", cA, cR, cG, cB);
            clientsDataGridView.Invoke((MethodInvoker)delegate {
                if (clientsDataGridView.RowCount > (int)id) {
                    DataGridViewCell cellID = clientsDataGridView.Rows[(int)id].Cells[0];
                    cellID.Value = id.ToString();
                    DataGridViewCell cellName = clientsDataGridView.Rows[(int)id].Cells[1];
                    cellName.Value = name.ToString();
                    DataGridViewCell cellCol = clientsDataGridView.Rows[(int)id].Cells[2];
                    cellCol.Value = colorString;
                    DataGridViewCell cellLat = clientsDataGridView.Rows[(int)id].Cells[3];
                    cellLat.Value = "0";
                } else {
                    string dcText = "DC";
                    if (id == 0) { dcText = "All"; }
                    string[] row = new string[] { id.ToString(), name, colorString, "0", dcText };
                    clientsDataGridView.Rows.Add(row);
                }
            });
            DataGridViewRow nrow = clientsDataGridView.Rows[(int)id];
            nrow.DefaultCellStyle.BackColor = color;
            lblConnections.Invoke((MethodInvoker)delegate {
                lblConnections.Visible = true;
                lblConnections.Text = string.Format("Total players: {0}", clientsDataGridView.Rows.Count);
            });
            if (listening) { UpdateDataContents(); }
        }
        private void RemoveFromGrid(long id)                                                        // Remove Client 
        {
            clientsDataGridView.Invoke((MethodInvoker)delegate {
                foreach (DataGridViewRow row in clientsDataGridView.Rows) {
                    if (row.Cells["identifier"].Value.ToString() == id.ToString()) {

                        clientsDataGridView.Rows.RemoveAt(row.Index);

                        break;
                    }
                }
            });
            lblConnections.Invoke((MethodInvoker)delegate {
                lblConnections.Text = string.Format("Total players: {0}", clientsDataGridView.Rows.Count);
            });
            if (listening) { UpdateDataContents(); }
        }
        private void ClearDataGrid()                                                                // Clear DataGrid 
        {
            clientsDataGridView.Invoke((MethodInvoker)delegate {
                clientsDataGridView.Rows.Clear();
            });
        }
        private void UpdateDataContents()                                                           // Host Collect, format and send DataGrid contents 
        {
            if (clientsDataGridView.RowCount > 1) {

                for (int row = 0; row < clientsDataGridView.Rows.Count; row++) {
                    string[] argbColor = clientsDataGridView.Rows[row].Cells[2].Value.ToString().Split(',');    // Seperate colour parts to INT components
                    int colA = Convert.ToInt32(argbColor[0]);
                    int colR = Convert.ToInt32(argbColor[1]);
                    int colG = Convert.ToInt32(argbColor[2]);
                    int colB = Convert.ToInt32(argbColor[3]);

                    PlayerPackage player = new() {                                                      // Create a player object to send
                        Id = row,
                        Name = clientsDataGridView.Rows[row].Cells[1].Value.ToString(),
                        Color = new int[] { colA, colR, colG, colB }
                    };
                    string json = JsonConvert.SerializeObject(player);                              // Fromat the object
                    HostSendPublic(json + "\n");                                                    // Host Send Grid row
                }
            }
        }



        /* Drawing */
        private bool Drawing = false;
        Point mouseLocA;
        Point mouseLocB;
        private void SetDraw_Click(object sender, EventArgs e)                                      // Toggles Settings and Drawings Group Boxes 
        {
            if (!gbSettings.Visible) {
                Drawing = gbDrawings.Visible = false;
                gbSettings.Visible = true;
                btnSetDraw.Text = "Drawings";
            } else {
                Drawing = gbDrawings.Visible = true;
                gbSettings.Visible = false;
                btnSetDraw.Text = "Settings";
            }
        }
        private void CmdDrawColor_Click(object sender, EventArgs e)                                 // Set Drawing Color 
        {
            using var diag = new ColorDialog();
            if (diag.ShowDialog() == DialogResult.OK)
                btnColor.BackColor = diag.Color;
        }
        private void CmdClear_Click(object sender, EventArgs e)                                     // Clears the gfx 
        {
            using var gfx = pDrawing.CreateGraphics();
            gfx.Clear(btnBGColor.BackColor);
        }
        private void CmdClearAll_Click(object sender, EventArgs e)                                  // Clears gfx and Sends Clear to Clients 
        {
            CmdClear_Click(sender, e);
            HostSendPublic("CMD:ClearDrawing");
        }

        private void DrawPanel_MouseDown(object sender, MouseEventArgs e)                           // Begin Drawing 
        {
            if (Drawing) {
                mouseLocA = e.Location;
                mouseLocB = e.Location;
                LocalDraw();
            }
        }
        private void DrawPanel_MouseMove(object sender, MouseEventArgs e)                           // Continue Drawing 
        {
            if (Drawing) {
                if (e.Button == MouseButtons.Left) {
                    mouseLocA = mouseLocB;
                    mouseLocB = e.Location;
                    LocalDraw();
                }
            }
        }

        private void LocalDraw()                                                                    // Self Draw Routine 
        {
            if (Drawing) {
                using var pen = new Pen(btnColor.BackColor, (int)nudSize.Value);                    // New Pen
                using var gfx = pDrawing.CreateGraphics();                                            // New Graphic canvas
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;                              // Circle brush
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                gfx.DrawLine(pen, mouseLocA, mouseLocB);                                            // Draw the line

                int cA = btnColor.BackColor.A; int cB = btnColor.BackColor.B; int cR = btnColor.BackColor.R; int cG = btnColor.BackColor.G;
                string colorString = string.Format("{0},{1},{2},{3}", cA, cR, cG, cB);              // convert COLOURS to string  - From Color to ARGB

                DrawPackage drawPack = new() {                                                      // Prep Draw Package for send
                    PenColor = colorString,
                    PenSize = (int)nudSize.Value,
                    PT1 = mouseLocA,
                    PT2 = mouseLocB
                };
                string json = JsonConvert.SerializeObject(drawPack);                                // Format the Package
                if (listening) {
                    HostSendPublic(json + "\n");                                                    // Host Send Draw Package
                } else if (connected) {
                    Send(json + "\n");                                                              // Client Send Draw Package
                }
            }
        }
        private void RemoteDraw(DrawPackage remoteDP)                                               // Receive Draw Routine 
        {
            if (Drawing) {
                string[] stringColor = remoteDP.PenColor.Split(',');    // Seperate colour parts to INT components
                int colA = Convert.ToInt32(stringColor[0]);
                int colR = Convert.ToInt32(stringColor[1]);
                int colG = Convert.ToInt32(stringColor[2]);
                int colB = Convert.ToInt32(stringColor[3]);
                Color argbColor = Color.FromArgb(colA, colR, colG, colB);

                using var pen = new Pen(argbColor, remoteDP.PenSize);
                using var gfx = pDrawing.CreateGraphics();
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                gfx.DrawLine(pen, remoteDP.PT1, remoteDP.PT2);
            }
        }
        /* End Drawing */



        /* Network */
        // Host, Join, and DC
        private void Connected(bool status)                                                         // Client active toggle 
        {
            cmdJoin.Invoke((MethodInvoker)delegate {
                connected = status;
                if (status) {
                    txtAddress.Enabled = false;
                    txtPort.Enabled = false;
                    txtName.Enabled = false;
                    txtRoomKey.Enabled = false;
                    cmdHost.Enabled = false;
                    clientsDataGridView.Columns["dc"].Visible = false;
                    btnColor.BackColor = cmdColor.BackColor;
                    cmdJoin.Text = "Disconnect";
                    tPing.Enabled = true;
                    Console(SystemMsg("You are now connected"));
                } else {
                    txtAddress.Enabled = true;
                    txtPort.Enabled = true;
                    txtName.Enabled = true;
                    txtRoomKey.Enabled = true;
                    cmdHost.Enabled = true;
                    cmdJoin.Text = "Connect";
                    tPing.Enabled = false;
                    ClearDataGrid();
                    Console(SystemMsg("You are now disconnected"));
                }
            });
        }
        private void Listening(bool status)                                                         // Host active toggle 
        {
            Size newSize = new();
            cmdHost.Invoke((MethodInvoker)delegate {
                listening = status;
                if (status) {
                    txtAddress.Enabled = false;
                    txtPort.Enabled = false;
                    txtName.Enabled = false;
                    txtRoomKey.Enabled = false;
                    cmdJoin.Enabled = false;
                    clientsDataGridView.Columns["dc"].Visible = true;
                    btnClearAll.Visible = true;
                    btnColor.BackColor = cmdColor.BackColor;
                    newSize = new(920, 560);
                    cmdHost.Text = "Stop";
                    tPing.Enabled = true;
                    Console(SystemMsg("Server has started"));
                } else {
                    txtAddress.Enabled = true;
                    txtPort.Enabled = true;
                    txtName.Enabled = true;
                    txtRoomKey.Enabled = true;
                    cmdJoin.Enabled = true;
                    newSize = new(600, 560);
                    cmdHost.Text = "Host";
                    tPing.Enabled = false;
                    ClearDataGrid();
                    Console(SystemMsg("Server has stopped"));
                }
            });
        }

        private void Connection(IPAddress ip, int port, string username, string roomkey)            // C - Single TCP to host 
        {
            try {
                clientObject = new Client {
                    username = username,
                    key = roomkey,
                    color = cmdColor.BackColor,                                                     // save player COLOURS to Client
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
                            Console(ErrorMsg(ex.Message));
                        }
                    }
                    clientObject.client.Close();
                    Connected(false);
                }
            } catch (Exception ex) {
                Console(ErrorMsg(ex.Message));
            }
        }
        private void Connection(MyPlayers obj)                                                      // H - Multi TCP to clients 
        {
            if (Authorize(obj)) {
                players.TryAdd(obj.id, obj);
                AddToGrid(obj.id, obj.username.ToString(), obj.color);                  // get client COLOURS add to grind
                string msg = string.Format("{0}:{1} has connected.", obj.id, obj.username);
                Console(SystemMsg(msg));
                HostSendPublic(SystemMsg(msg), obj.id);
                while (obj.client.Connected) {
                    try {
                        obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(Read), obj);
                        obj.handle.WaitOne();
                    } catch (Exception ex) {
                        Console(ErrorMsg(ex.Message));
                    }
                }
                obj.client.Close();
                players.TryRemove(obj.id, out MyPlayers tmp);
                RemoveFromGrid(tmp.id);
                msg = string.Format("{0}:{1} has disconnected.", tmp.id, tmp.username);
                Console(SystemMsg(msg));
                HostSendPublic(SystemMsg(msg), tmp.id);
            }
        }
        private void Listener(IPAddress ip, int port)                                               // H - Multi TCP to clients 
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
                            Console(ErrorMsg(ex.Message));
                        }
                    } else {
                        Thread.Sleep(500);
                    }
                }
                Listening(false);
            } catch (Exception ex) {
                Console(ErrorMsg(ex.Message));
            } finally {
                listener?.Server.Close();
            }
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

        private void Disconnect(long id = -1)                                                       // Disconnect ID / All if empty 
        {
            if (disconnect == null || !disconnect.IsAlive) {
                disconnect = new Thread(() => {
                    if (id > 0) {
                        players.TryGetValue(id, out MyPlayers obj);
                        obj.client.Close();
                        RemoveFromGrid(obj.id);
                    } else {
                        foreach (KeyValuePair<long, MyPlayers> obj in players) {
                            obj.Value.client.Close();
                            RemoveFromGrid(obj.Value.id);
                        }
                    }
                }) {
                    IsBackground = true
                };
                disconnect.Start();
            }
        }

        // Ping
        private void Ping_Tick(object sender, EventArgs e)                                          // Timer for Pings 
        {
            if (listening || connected) {
                for (int i = 1; i <= players.Count; i++) {
                    var pingCell = (DataGridViewTextBoxCell)clientsDataGridView.Rows[i].Cells[3];
                    pingCell.Value = Ping(GetPlayerAddress(i));
                }
            }
        }
        static double Ping(string address)                                                          // Perform Ping Return average of 4 as Long 
        {
            long totalTime = 0;
            Ping ping = new();

            for (int i = 0; i < 4; i++) {
                PingReply reply = ping.Send(address, 1000);
                if (reply.Status == IPStatus.Success) {
                    totalTime += reply.RoundtripTime;
                }
            }
            return totalTime / 4;
        }
        static string GetPlayerAddress(int id)                                                      // Get IP Address for Ping 
        {
            if (id != 0) {
                players.TryGetValue(id, out MyPlayers obj);
                string ipAddress = ((IPEndPoint)obj.client.Client.RemoteEndPoint).Address.ToString();
                return ipAddress;
            } else return "127.0.0.1";
        }

        // Client Send
        private void Send(string msg)                                                               // Client version 
        {
            if (!listening) {
                if (send == null || send.IsCompleted) {
                    send = Task.Factory.StartNew(() => BeginWrite(msg));
                } else {
                    send.ContinueWith(antecendent => BeginWrite(msg));
                }
            }
        }
        private void BeginWrite(string msg)                                                         // Client version 
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            if (clientObject.client.Connected) {
                try {
                    clientObject.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(Write), null);
                } catch (Exception ex) {
                    Console(ErrorMsg(ex.Message));
                }
            }
        }
        private void Write(IAsyncResult result)                                                     // Client write 
        {
            if (clientObject.client.Connected) {
                try {
                    clientObject.stream.EndWrite(result);
                } catch (Exception ex) {
                    Console(ErrorMsg(ex.Message));
                }
            }
        }

        // Host Send
        private void HostSendPrivate(string msg, MyPlayers obj)                                     // Host prepare to send Private message 
        {
            if (send == null || send.IsCompleted) {
                send = Task.Factory.StartNew(() => HostBeginPrivate(msg, obj));
            } else {
                send.ContinueWith(antecendent => HostBeginPrivate(msg, obj));
            }
        }
        private void HostSendPublic(string msg, long id = -1)                                       // Host prepare to send Public message 
        {
            if (send == null || send.IsCompleted) {
                send = Task.Factory.StartNew(() => HostBeginPublic(msg, id));
            } else {
                send.ContinueWith(antecendent => HostBeginPublic(msg, id));
            }
        }
        private void HostBeginPrivate(string msg, MyPlayers obj)                                    // Host BeginWrite Private message to stream 
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            if (obj.client.Connected) {
                try {
                    obj.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(HostEndWrite), obj);
                } catch (Exception ex) {
                    Console(ErrorMsg(ex.Message));
                }
            }
        }
        private void HostBeginPublic(string msg, long id = -1)                                      // Host BeginWrite Public -- set ID to lesser than zero to send to everyone 
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            foreach (KeyValuePair<long, MyPlayers> obj in players) {
                if (id != obj.Value.id && obj.Value.client.Connected) {
                    try {
                        obj.Value.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(HostEndWrite), obj.Value);
                    } catch (Exception ex) {
                        Console(ErrorMsg(ex.Message));
                    }
                }
            }
        }
        private void HostEndWrite(IAsyncResult result)                                              // Host EndWrite 
        {
            MyPlayers obj = (MyPlayers)result.AsyncState;
            if (obj.client.Connected) {
                try {
                    obj.stream.EndWrite(result);
                } catch (Exception ex) {
                    Console(ErrorMsg(ex.Message));
                }
            }
        }

        // Handshake
        private bool Authorize()                                                                    // Client Handshake 
        {
            bool success = false;
            Dictionary<string, string> handShake = new() {                                          // Collect info to send as object Handshake
                { "username", clientObject.username },
                { "roomkey", clientObject.key },
                { "color" , Convert.ToString(cmdColor.BackColor.A) +","+ Convert.ToString(cmdColor.BackColor.R) +","+ Convert.ToString(cmdColor.BackColor.G) +","+ Convert.ToString(cmdColor.BackColor.B) }, // Send COLOURS to host
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
                    Console(ErrorMsg(ex.Message));
                }
            }
            if (!connected) {                                                                       // Connection refused
                Console(SystemMsg("Unauthorized"));
            }
            return success;
        }
        private bool Authorize(MyPlayers obj)                                                       // Host Handshake checks 
        {
            bool success = false;
            while (obj.client.Connected) {
                try {
                    obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(ReadAuth), obj);
                    obj.handle.WaitOne();
                    if (obj.username.Length > 0) {
                        success = true;
                        break;
                    }
                } catch (Exception ex) {
                    Console(ErrorMsg(ex.Message));
                }
            }
            return success;
        }
        private void ReadAuth(IAsyncResult result)                                                  // H+C - Handshake 
        {
            if (listening) {                                                                        // Host read
                MyPlayers obj = (MyPlayers)result.AsyncState;
                int bytes = 0;
                if (obj.client.Connected) {
                    try {
                        bytes = obj.stream.EndRead(result);
                    } catch (Exception ex) {
                        Console(ErrorMsg(ex.Message));
                    }
                }
                if (bytes > 0) {
                    obj.data.AppendFormat("{0}", Encoding.UTF8.GetString(obj.buffer, 0, bytes));
                    try {
                        if (obj.stream.DataAvailable) {
                            obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(ReadAuth), obj);
                        } else {
                            JavaScriptSerializer json = new();
                            Dictionary<string, string> data = json.Deserialize<Dictionary<string, string>>(obj.data.ToString());
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
                        Console(ErrorMsg(ex.Message));
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
                        Console(ErrorMsg(ex.Message));
                    }
                }
                if (bytes > 0) {
                    clientObject.data.AppendFormat("{0}", Encoding.UTF8.GetString(clientObject.buffer, 0, bytes));
                    try {
                        if (clientObject.stream.DataAvailable) {
                            clientObject.stream.BeginRead(clientObject.buffer, 0, clientObject.buffer.Length, new AsyncCallback(ReadAuth), null);
                        } else {
                            JavaScriptSerializer json = new();
                            Dictionary<string, string> data = json.Deserialize<Dictionary<string, string>>(clientObject.data.ToString());
                            if (data.ContainsKey("status") && data["status"].Equals("authorized")) {
                                Connected(true);
                            }
                            clientObject.data.Clear();
                            clientObject.handle.Set();
                        }
                    } catch (Exception ex) {
                        clientObject.data.Clear();
                        Console(ErrorMsg(ex.Message));
                        clientObject.handle.Set();
                    }
                } else {
                    clientObject.client.Close();
                    clientObject.handle.Set();
                }
            }
        }


        // Host and Client Read
        private void Read(IAsyncResult result)                                                      // / *** READ ***/ 
        {
            if (listening) {                                                                        // Host stream reader
                MyPlayers obj = (MyPlayers)result.AsyncState;
                int bytes = 0;
                if (obj.client.Connected) {
                    try {
                        bytes = obj.stream.EndRead(result);
                    } catch (Exception ex) {
                        Console(ErrorMsg(ex.Message));
                    }
                }
                if (bytes > 0) {
                    obj.data.AppendFormat("{0}", Encoding.UTF8.GetString(obj.buffer, 0, bytes));
                    try {
                        if (obj.stream.DataAvailable) {
                            obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(Read), obj);
                        } else {
                            string clientData = obj.data.ToString();

                            // Host Receive - PM
                            if (clientData.Contains("/msg")) {
                                string[] pMSG = clientData.Split(':');
                                for (int p = 1; p <= players.Count; p++) {                          // Determine the PM Dest
                                    if (pMSG[1] == players[p].username.ToString()) {                // Dest is a player        
                                        HostSendPrivate(string.Format("{0} to you: {1}", obj.username, pMSG[2]), players[p]);   // Host - Send PM
                                        Console(SystemMsg("PM Sent."));
                                        obj.data.Clear();
                                        obj.handle.Set();
                                        return;
                                    } else if (pMSG[1] == txtName.Text.Trim()) {                    // Dest is host
                                        PrivateChat(obj.username.ToString(), pMSG[2]);              // Post Host PM 
                                        obj.data.Clear();
                                        obj.handle.Set();
                                        return;
                                    }
                                }
                                Console(SystemMsg(string.Format("PM Not Sent. From: {0}  To: {1} -- {2} --", obj.username, pMSG[1], pMSG[2])));
                                obj.data.Clear();
                                obj.handle.Set();
                                return;
                            }

                            // Host receive Drawing
                            if (clientData.StartsWith("{\"PenColor\"")) {
                                DrawPackage remoteDP = JsonConvert.DeserializeObject<DrawPackage>(clientData);
                                RemoteDraw(remoteDP);
                                HostSendPublic(clientData, obj.id);                                  // Host relay client drawing to other clients
                                obj.data.Clear();
                                obj.handle.Set();
                                return;
                            }

                            // Host Receive - Public Message
                            PublicChat(obj.username.ToString(), obj.data.ToString());               // Host post
                            HostSendPublic(obj.data.ToString(), obj.id);                            // Host relay public message to other clients
                            obj.data.Clear();
                            obj.handle.Set();
                        }
                    } catch (Exception ex) {
                        obj.data.Clear();
                        Console(ErrorMsg(ex.Message));
                        obj.handle.Set();
                    }
                } else {
                    obj.client.Close();
                    obj.handle.Set();
                }
            } else {                                                                                // Client stream reader
                if (clientObject.client == null) { return; }
                int bytes = 0;
                if (clientObject.client.Connected) {
                    try {
                        bytes = clientObject.stream.EndRead(result);
                    } catch (Exception ex) {
                        Console(ErrorMsg(ex.Message));
                    }
                }
                if (bytes > 0) {
                    clientObject.data.AppendFormat("{0}", Encoding.UTF8.GetString(clientObject.buffer, 0, bytes));
                    try {
                        if (clientObject.stream.DataAvailable) {
                            clientObject.stream.BeginRead(clientObject.buffer, 0, clientObject.buffer.Length, new AsyncCallback(Read), null);
                        } else {
                            string hostData = clientObject.data.ToString();

                            if (hostData.StartsWith("{\"Id\"")) {                                   // Client - grid update
                                string[] messages = clientObject.data.ToString().Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string message in messages) {
                                    PlayerPackage player = JsonConvert.DeserializeObject<PlayerPackage>(message);
                                    Color argbColor = Color.FromArgb(player.Color[0], player.Color[1], player.Color[2], player.Color[3]);
                                    AddToGrid(player.Id, player.Name, argbColor);
                                }
                                clientObject.data.Clear();
                                clientObject.handle.Set();
                                return;
                            }

                            if (hostData.StartsWith("SYSTEM:")) {                                   // Client - System Message
                                string[] sysParts = hostData.Split(':');
                                Console(sysParts[2]);
                                clientObject.data.Clear();
                                clientObject.handle.Set();
                                return;
                            }

                            if (hostData.StartsWith("{\"PenColor\"")) {                              // Client - Drawing
                                string[] messages = clientObject.data.ToString().Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string message in messages) {
                                    DrawPackage remoteDP = JsonConvert.DeserializeObject<DrawPackage>(message);
                                    RemoteDraw(remoteDP);
                                }
                                clientObject.data.Clear();
                                clientObject.handle.Set();
                                return;
                            }

                            if (hostData.StartsWith("CMD:")) {                                      // Client - Commands
                                string[] cmdParts = hostData.Split(':');
                                switch (cmdParts[1]) {
                                    case "ClearDrawing":
                                        Button sender = new();
                                        EventArgs e = new();
                                        CmdClearAll_Click(sender, e);
                                        break;
                                    default:
                                        break;
                                }
                                clientObject.data.Clear();
                                clientObject.handle.Set();
                                return;
                            }

                            string[] dataParts = hostData.Split(':');
                            PublicChat(dataParts[0], dataParts[1]);                                 // Client - Public Message
                            clientObject.data.Clear();
                            clientObject.handle.Set();
                        }
                    } catch (Exception ex) {
                        clientObject.data.Clear();
                        Console(ErrorMsg(ex.Message));
                        clientObject.handle.Set();
                    }
                } else {
                    clientObject.client.Close();
                    clientObject.handle.Set();
                }
            }
        }
                                                                                                    // / *** READ ***/ 
        /* END NETWORK */




        /* Controls */
        private void BGC_Click(object sender, EventArgs e)                                          // Set the background color of respective TextBox 
        {
            ColorDialog MyDialog = new() {
                AllowFullOpen = true,
                ShowHelp = true,
                //Color = cmdColor.BackColor            // Sets the initial color select to the current text color.             
            };

            if (tabSections.SelectedTab.Name == "tConsole") {
                if (MyDialog.ShowDialog() == DialogResult.OK) {                         // Update the text box color if the user clicks OK
                    txtConsole.BackColor = MyDialog.Color;
                };
            }
            if (tabSections.SelectedTab.Name == "tLobby") {
                if (MyDialog.ShowDialog() == DialogResult.OK) {                         // Update the text box color if the user clicks OK
                    txtLobby.BackColor = MyDialog.Color;
                };
            }

        }
        private void Clear_Click(object sender, EventArgs e)                                        // Clear the respective Textbox 
        {
            if (tabSections.SelectedTab.Name == "tConsole") { Console(); }
            if (tabSections.SelectedTab.Name == "tLobby") { PublicChat(null, null); }
        }
        private void CmdPlayerColor_Click(object sender, EventArgs e)                               // Player Color Chooser 
        {
            ColorDialog MyDialog = new() {
                AllowFullOpen = true,
                ShowHelp = true,
                Color = cmdColor.BackColor                                                          // Sets the initial color select to the current text color.             
            };
            if (MyDialog.ShowDialog() == DialogResult.OK) {                                         // Update the text box color if the user clicks OK
                btnColor.BackColor = cmdColor.BackColor = MyDialog.Color;
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
            if (listening) {
                listening = false;
                RemoveFromGrid(0);
            } else if (listener == null || !listener.IsAlive) {
                string address = txtAddress.Text.Trim();
                string number = txtPort.Text.Trim();
                if (txtName.Text.Trim() == "Player 1") { txtName.Text = "Host"; cmdColor.BackColor = Color.Yellow; }
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
                    AddToGrid(0, username, cmdColor.BackColor);
                    tabSections.SelectTab(1);
                    txtMessage.Focus();
                    txtMessage.SelectionStart = txtMessage.Text.Length;
                }
            }
        }
        private void CmdJoin_Click(object sender, EventArgs e)                                      // Client start 
        {
            if (connected) {
                clientObject.client.Close();
            } else if (client == null || !client.IsAlive) {
                string address = txtAddress.Text.Trim();
                string number = txtPort.Text.Trim();
                if (txtName.Text.Trim() == "Player 1") { cmdColor.BackColor = Color.Red; }
                if (txtName.Text.Trim() == "Player 2") { cmdColor.BackColor = Color.Green; }
                if (txtName.Text.Trim() == "Player 3") { cmdColor.BackColor = Color.Blue; }
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
        private void TabSections_MouseClick(object sender, MouseEventArgs e)                        // Tab Context Menu 
        {
            if (e.Button == MouseButtons.Right) {
                ContextMenu cm = new();
                cm.MenuItems.Add("BG Color");
                cm.MenuItems.Add("Clear");
                cm.MenuItems[0].Click += new EventHandler(BGC_Click);
                cm.MenuItems[1].Click += new EventHandler(Clear_Click);
                tabSections.ContextMenu = cm;
            }
        }
        /* Controls */


    }
}
