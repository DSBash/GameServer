using Microsoft.SqlServer.Server;
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
using static System.Net.Mime.MediaTypeNames;

namespace Server
{


    public partial class GameServer : Form
    {

        private bool exit = false;
        private Task send = null;

        // Host Specific
        private bool listening = false;
        private Thread listener = null;
        private Thread disconnect = null;
        private struct MyPlayers
        {
            public long id;
            //public Color color;
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
        private struct MyClient
        {
            public string username;
            public string key;
            //public Color color;
            public TcpClient client;
            public NetworkStream stream;
            public byte[] buffer;
            public StringBuilder data;
            public EventWaitHandle handle;
        };
        private MyClient clientObject;


        //Used to enable Console Output
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
            exit = true;
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
            if (!exit) {
                txtConsole.Invoke((MethodInvoker)delegate {
                    if (msg.Length > 0) {
                        txtConsole.AppendText(string.Format("{2}[ {0} ] {1}", DateTime.Now.ToString("HH:mm"), msg, Environment.NewLine));
                    } else {
                        txtConsole.Clear();
                    }
                });
            }
        }
        private void PublicChat(string msg = "")                                                    // Console message / Clear if empty 
        {
            if (!exit) {
                txtLobby.Invoke((MethodInvoker)delegate {
                    if (msg.Length > 0) {
                        string formattedMSG = string.Format("{2}[ {0} ] {1}", DateTime.Now.ToString("HH:mm"), msg, Environment.NewLine);
                        //msg
                        txtLobby.AppendText(formattedMSG, cmdColor.BackColor);
                    } else {
                        txtLobby.Clear();
                    }
                });
            }
        }
        private void CmdClear_Click(object sender, EventArgs e)
        {
            if (tabSections.SelectedTab.Name == "tConsole") { Console(); }
            if (tabSections.SelectedTab.Name == "tLobby") { PublicChat(); }
        }

        // Message formatters
        private string ErrorMsg(string msg)                                                         // Format MSG 
        {
            return string.Format("ERROR: {0}", msg);
        }
        private string SystemMsg(string msg)
        {
            return string.Format("SYSTEM: {0}", msg);
        }
        private string CommandMsg(string msg)
        {
            return string.Format("CMD:{0}", msg);
        }



        // Controls
        private void CmdColor_Click(object sender, EventArgs e)                                     // Color Chooser
        {
            ColorDialog MyDialog = new() {
                AllowFullOpen = true,
                ShowHelp = true,
                Color = cmdColor.BackColor            // Sets the initial color select to the current text color.             
            };
            if (MyDialog.ShowDialog() == DialogResult.OK) {                         // Update the text box color if the user clicks OK
                cmdColor.BackColor = MyDialog.Color;
            }
        }
        private void CbMask_CheckedChanged(object sender, EventArgs e)                              // Handles Key Mask 
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
                    AddToGrid(0, username/*, cmdColor.BackColor*/);
                    tabSections.SelectTab(1);
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
        private void CmdDisconnect_Click(object sender, EventArgs e)                                // Disconnect 
        {
            Disconnect();
        }
        private void ClientsDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)      // Grid Button CLicks / Ping & DC 
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == clientsDataGridView.Columns["dc"].Index) {
                long.TryParse(clientsDataGridView.Rows[e.RowIndex].Cells["identifier"].Value.ToString(), out long id);
                Disconnect(id);
            }
            /*            if (e.RowIndex >= 0 && e.ColumnIndex == clientsDataGridView.Columns["latency"].Index) {
                            var BtnCell = (DataGridViewTextBoxCell)clientsDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                            BtnCell.Value = Ping(GetPlayerAddress(e.RowIndex));
                        }*/
        }



        // Message box & Send
        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)                              // Send on <Enter> 
        {
            if (e.KeyCode == Keys.Enter) {
                e.Handled = true;
                e.SuppressKeyPress = true;
                if (txtMessage.Text.Length > 0) {
                    string msg = txtMessage.Text;
                    txtMessage.Clear();
                    PublicChat(string.Format("{0} (You): {1}", txtName.Text.Trim(), msg));
                    if (listening) {
                        HostSendPublic(string.Format("{0}: {1}", txtName.Text.Trim(), msg));
                    }
                    if (connected) {
                        Send(msg);
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
        private void AddToGrid(long id, string name/*, Color color*/)                                                // Add Client 
        {
            if (!exit) {
                clientsDataGridView.Invoke((MethodInvoker)delegate {
                    
                    //int cA = color.A; int cB = color.B; int cR = color.R; int cG = color.G;         // convert COLOURS to string  - From Color to ARGB
                    //Color myColor = Color.FromArgb(cA,cR,cG,cB);                                  // From ARGB to Color
                    //string colorString = string.Format("{0},{1},{2},{3}", cA,cR,cG,cB);

                    string[] row = new string[] { id.ToString(), name, "colorString", "0" };
                    clientsDataGridView.Rows.Add(row);
                    //clientsDataGridView.DefaultCellStyle.BackColor = color;

                    lblConnections.Visible = true;
                    lblConnections.Text = string.Format("Total players: {0}", clientsDataGridView.Rows.Count);
                });

                if (listening) { UpdateDataContents(); }
            }
        }
        private void RemoveFromGrid(long id)                                                        // Remove Client 
        {
            if (!exit) {
                clientsDataGridView.Invoke((MethodInvoker)delegate {
                    foreach (DataGridViewRow row in clientsDataGridView.Rows) {
                        if (row.Cells["identifier"].Value.ToString() == id.ToString()) {
                            clientsDataGridView.Rows.RemoveAt(row.Index);
                            break;
                        }
                    }
                    if (listening) { UpdateDataContents(); }
                    lblConnections.Text = string.Format("Total players: {0}", clientsDataGridView.Rows.Count);
                });
            }
        }
        private void ClearDataGrid()                                                                // Clear DataGrid 
        {
            clientsDataGridView.Invoke((MethodInvoker)delegate {
                clientsDataGridView.Rows.Clear();
            });


            /*            if (clientsDataGridView.InvokeRequired) {
                            clientsDataGridView.BeginInvoke(() =>
                            {
                                clientsDataGridView.Rows.Clear();
                            });
                        } else {
                            clientsDataGridView.Rows.Clear();
                        }*/
        }
        private void UpdateDataContents()                                                           // Host Collect, format and send DataGrid contents 
        {
            if (clientsDataGridView.RowCount > 1) {
                string contents = "";
                int rCount = 0;
                foreach (DataGridViewRow row in clientsDataGridView.Rows) {
                    contents += row.Cells[0].Value?.ToString() + "," ?? string.Empty;   // ID
                    contents += row.Cells[1].Value?.ToString() + "," ?? string.Empty;   // Name
                    contents += row.Cells[2].Value?.ToString() ?? string.Empty;         // Color    // get COLOURS to send
                    rCount++;
                    if (rCount < clientsDataGridView.RowCount) { contents += ';'; }
                }

                JavaScriptSerializer json = new();
                var msg = CommandMsg(string.Format("{0}:{1}", "uGrid", contents));
                HostSendPublic(json.Serialize(msg));
            }
        }



        // Host, Join, and DC
        private void Connected(bool status)                                                         // isClient toggle 
        {
            if (!exit) {
                cmdJoin.Invoke((MethodInvoker)delegate {
                    connected = status;
                    if (status) {
                        txtAddress.Enabled = false;
                        txtPort.Enabled = false;
                        txtName.Enabled = false;
                        txtRoomKey.Enabled = false;
                        cmdHost.Enabled = false;
                        cmdDisconnect.Enabled = false;
                        clientsDataGridView.Columns["dc"].Visible = false;
                        cmdJoin.Text = "Disconnect";
                        tPing.Enabled = true;
                        Console(SystemMsg("You are now connected"));
                    } else {
                        txtAddress.Enabled = true;
                        txtPort.Enabled = true;
                        txtName.Enabled = true;
                        txtRoomKey.Enabled = true;
                        cmdHost.Enabled = true;
                        cmdDisconnect.Enabled = true;
                        cmdJoin.Text = "Connect";
                        tPing.Enabled = false;
                        Console(SystemMsg("You are now disconnected"));
                    }
                });
            }
        }
        private void Listening(bool status)                                                         // isHost toggle 
        {
            if (!exit) {
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
                        Console(SystemMsg("Server has stopped"));
                    }
                });
            }
        }

        private void Connection(IPAddress ip, int port, string username, string roomkey)            // C - Single TCP to host 
        {
            try {
                clientObject = new MyClient {
                    username = username,
                    key = roomkey,
                    //color = cmdColor.BackColor,                                                     // save player COLOURS to MyClient
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
                AddToGrid(obj.id, obj.username.ToString()/*, obj.color*/);                  // get client COLOURS add to grind
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
                                client = listener.AcceptTcpClient()
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
        private void Ping_Tick(object sender, EventArgs e)
        {
/*            if (listening || connected) {
                for (int i = 0; i <= players.Count; i++) {
                    var pingCell = (DataGridViewTextBoxCell)clientsDataGridView.Rows[i].Cells[2];
                    pingCell.Value = Ping(GetPlayerAddress(i));
                }
            }*/
        }
        static double Ping(string address)
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
        private void CmdPing_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= players.Count; i++) {
                players.TryGetValue(i, out MyPlayers obj);
                var pingTest = Ping(GetPlayerAddress(i));
                Console(string.Format("Ping for {1}:{0}", pingTest, obj.username));
            }
        }
        static string GetPlayerAddress(int id)
        {
            if (id != 0) {
                players.TryGetValue(id, out MyPlayers obj);
                string ipAddress = ((IPEndPoint)obj.client.Client.RemoteEndPoint).Address.ToString();
                return ipAddress;
            } else return "127.0.0.1";
        }



        // Room Key management
        private bool Authorize()                                                                    // Client request 
        {
            bool success = false;
            Dictionary<string, string> data = new() {
                { "username", clientObject.username },
                { "roomkey", clientObject.key },
                { "color" , Convert.ToString(cmdColor.BackColor.A) +","+ Convert.ToString(cmdColor.BackColor.R) +","+ Convert.ToString(cmdColor.BackColor.G) +","+ Convert.ToString(cmdColor.BackColor.B) }, // Send COLOURS to host
            };
            JavaScriptSerializer json = new();
            Send(json.Serialize(data));
            while (clientObject.client.Connected) {
                try {
                    clientObject.stream.BeginRead(clientObject.buffer, 0, clientObject.buffer.Length, new AsyncCallback(ReadAuth), null);
                    clientObject.handle.WaitOne();
                    if (connected) {
                        success = true;
                        break;
                    }
                } catch (Exception ex) {
                    Console(ErrorMsg(ex.Message));
                }
            }
            if (!connected) {
                Console(SystemMsg("Unauthorized"));
            }
            return success;
        }
        private bool Authorize(MyPlayers obj)                                                       // Host reply 
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
        
        
        // TCP Read
        private void ReadAuth(IAsyncResult result)                                                  // H+C - Private MSG Readers 
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
                                
                                //string[] colParts = data.ElementAt(2).Value.Split(',');             // COLOURS
                                //Color myColor = Color.FromArgb(Convert.ToInt32(colParts[0]), Convert.ToInt32(colParts[1]), Convert.ToInt32(colParts[2]), Convert.ToInt32(colParts[3]));


                                // host gets COLOURS and adds to MyPlayers
                                //int cA = data["color"].color.A; int cR = data["color"].color.R; int cG = data["color"].color.G; int cB = data["color"].color.B;         // From Color to ARGB
                                // From ARGB to Color
                                //.ElementAt(2).Value
                                
                                //obj.color = myColor;





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
        private void Read(IAsyncResult result)                                                      // H+C - Public Readers 
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
                            string msg = string.Format("{0}: {1}", obj.username, obj.data);
                            PublicChat(msg);
                            HostSendPublic(msg, obj.id);
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
                            if (clientObject.data.ToString().StartsWith("CMD:")) {
                                ReceiveCommand(clientObject.data);
                                clientObject.data.Clear();
                                clientObject.handle.Set();
                                return;
                            }
                            PublicChat(clientObject.data.ToString());
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
        private void ReceiveCommand(StringBuilder Command)                                                 // C - String enters {CMD:uGrid:0,H,colorString;1,Player 1,colorString}
        {                                                               
            string[] CmdParts = Convert.ToString(Command).Split(':');
            string CmdName = CmdParts[1];
            string CmdDetails = CmdParts[2];

            switch (CmdName) {
                case "dGrid":

                    break;

                case "uGrid":
                    ClearDataGrid();
                    string[] rows = CmdDetails.Split(';');                      // Seperate the Rows from the Details
                    foreach (string row in rows) {
                        string[] Cell = row.Split(',');                  // Seperate the Cells
                        long ID = Convert.ToInt64(Cell[0]);
                        string Name = Cell[1];
                        //string colorName = Cell[2].Substring(Cell[2].IndexOf("[") + 1, Cell[2].IndexOf("]") - Cell[2].IndexOf("[") - 1).Trim(); // extract color name
                        //Color color = Color.FromName(colorName);
                        AddToGrid(ID, Name/*, color*/);
                    }
                    break;

                default:
                    Console("Unreconized Command: " + CmdName + " --- " + CmdDetails);
                    break;
            }
        }



        // TCP Send, Begin & End Writes
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


/*        private void clientsDataGridView_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e) {
            if (e.RowIndex > -1 && e.ColumnIndex > -1) { 
            clientsDataGridView.DefaultCellStyle.BackColor = Color.FromName(clientsDataGridView.Rows[e.RowIndex].Cells[2].Value.ToString().Substring(clientsDataGridView.Rows[e.RowIndex].Cells[2].Value.ToString().IndexOf("[") + 1, clientsDataGridView.Rows[e.RowIndex].Cells[2].Value.ToString().IndexOf("]") - clientsDataGridView.Rows[e.RowIndex].Cells[2].Value.ToString().IndexOf("[") - 1).Trim()); // extract color name);
            }
        }*/
    }
}
