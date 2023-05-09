using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static Server.Encryption;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Linq;
using System.Web.Script.Serialization;

namespace Server
{
    public partial class GameServer
    {
        #region Host Specific Declarations
        public class PlayerPackage                                                                  // To Broadcast to Clients 
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int[] Color { get; set; }
        }

        private bool listening = false;                                                             // Host / Client Mode Marker
        private Thread listener = null;                                                             // Host TCP Listener
        private Thread disconnect = null;                                                           // Manage Disconnects

        #pragma warning disable IDE1006                             // Naming Styles
        public class MyPlayers                                                                      // Handles Connections 
        {
            public long id { get; set; }
            public Color color { get; set; }
            public StringBuilder username { get; set; }
            public TcpClient client { get; set; }
            public NetworkStream stream { get; set; }
            public byte[] buffer { get; set; }
            public StringBuilder data { get; set; }
            public EventWaitHandle handle { get; set; }
        };
        #pragma warning restore IDE1006                             // Naming Styles

        private static readonly ConcurrentDictionary<long, MyPlayers> players = new();
        #endregion

        // Host DC & C
        private void StartHosting(object sender, EventArgs e)                                                                 // Host - Start 
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


        // Host Receive
        private void HostAuthReader(IAsyncResult result)                                            // Handles Async Handshake from Client 
        {
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

        private void HostStreamReader(IAsyncResult result)                                          // Stream Reader 
        {
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



        // Host Routines
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

        private void FileServe()                                                                    // Host - Drawing File Server 
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
    }
}