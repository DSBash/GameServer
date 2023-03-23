using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Server
{
    public partial class GameServer : Form
    {
        //Used to enable Console Output
/*        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();*/

        // Host Specific
        private bool listening = false;
        private bool exit = false;
        private Task send = null;
        private Thread listener = null;
        private Thread disconnect = null;
        private long id = 1;
        private struct MyPlayers
        {
            public long id;
            public StringBuilder username;
            public TcpClient client;
            public NetworkStream stream;
            public byte[] buffer;
            public StringBuilder data;
            public EventWaitHandle handle;
        };
        private ConcurrentDictionary<long, MyPlayers> players = new ConcurrentDictionary<long, MyPlayers>();

        // Client Specific
        private bool connected = false;
        private Thread client = null;
        private struct MyClient
        {
            public string username;
            public string key;
            public TcpClient client;
            public NetworkStream stream;
            public byte[] buffer;
            public StringBuilder data;
            public EventWaitHandle handle;
        };
        private MyClient clientObject;

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


        private void Log(string msg = "")                                                           // Log message / Clear if empty
        {
            if (!exit) {
                txtConsole.Invoke((MethodInvoker)delegate {
                    if (msg.Length > 0) {
                        txtConsole.AppendText(string.Format("[ {0} ] {1}{2}", DateTime.Now.ToString("HH:mm"), msg, Environment.NewLine));
                    } else {
                        txtConsole.Clear();
                    }
                });
            }
        }
        private void ClearButton_Click(object sender, EventArgs e)
        {
            Log();
        }


        private string ErrorMsg(string msg)                                                         // Format MSG
        {
            return string.Format("ERROR: {0}", msg);
        }
        private string SystemMsg(string msg)
        {
            return string.Format("SYSTEM: {0}", msg);
        }
        private string CommandMsg(string msg) {
            return string.Format("CMD:{0}", msg);
        }



        private void cbMask_CheckedChanged(object sender, EventArgs e)                              // Handles Key Mask
        {
            if (txtRoomKey.PasswordChar == '*') {
                txtRoomKey.PasswordChar = '\0';
            } else {
                txtRoomKey.PasswordChar = '*';
            }
        }
        private void cmdHost_Click(object sender, EventArgs e)                                      // Host start
        {
            if (listening) {
                listening = false;
            } else if (listener == null || !listener.IsAlive) {
                string address = txtAddress.Text.Trim();
                string number = txtPort.Text.Trim();
                string username = txtName.Text.Trim();
                bool error = false;
                IPAddress ip = null;
                if (address.Length < 1) {
                    error = true;
                    Log(SystemMsg("Address is required"));
                } else {
                    try {
                        ip = Dns.GetHostEntry(address)
                                          .AddressList
                                          .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

                        //ip = Dns.Resolve(address).AddressList[0];
                    } catch {
                        error = true;
                        Log(SystemMsg("Address is not valid"));
                    }
                }
                int port = -1;
                if (number.Length < 1) {
                    error = true;
                    Log(SystemMsg("Port number is required"));
                } else if (!int.TryParse(number, out port)) {
                    error = true;
                    Log(SystemMsg("Port number is not valid"));
                } else if (port < 0 || port > 65535) {
                    error = true;
                    Log(SystemMsg("Port number is out of range"));
                }
                if (username.Length < 1) {
                    error = true;
                    Log(SystemMsg("Username is required"));
                }
                if (!error) {
                    listener = new Thread(() => Listener(ip, port)) {
                        IsBackground = true
                    };
                    listener.Start();
                    AddToGrid(0, username);
                }
            }
        }
        private void cmdJoin_Click(object sender, EventArgs e)                                      // Client start
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
                    Log(SystemMsg("Address is required"));
                } else {
                    try {
                        ip = Dns.GetHostEntry(address)
                                          .AddressList
                                          .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

                        //ip = Dns.Resolve(address).AddressList[0];
                    } catch {
                        error = true;
                        Log(SystemMsg("Address is not valid"));
                    }
                }
                int port = -1;
                if (number.Length < 1) {
                    error = true;
                    Log(SystemMsg("Port number is required"));
                } else if (!int.TryParse(number, out port)) {
                    error = true;
                    Log(SystemMsg("Port number is not valid"));
                } else if (port < 0 || port > 65535) {
                    error = true;
                    Log(SystemMsg("Port number is out of range"));
                }
                if (username.Length < 1) {
                    error = true;
                    Log(SystemMsg("Username is required"));
                }
                if (!error) {
                    client = new Thread(() => Connection(ip, port, username, txtRoomKey.Text)) {
                        IsBackground = true
                    };
                    client.Start();
                }
            }
        }
        private void cmdDisconnect_Click(object sender, EventArgs e)                                // Disconnect
        {
            Disconnect();
        }
        private void ClientsDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)      // Kick Client 
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == clientsDataGridView.Columns["dc"].Index) {
                long.TryParse(clientsDataGridView.Rows[e.RowIndex].Cells["identifier"].Value.ToString(), out long id);
                Disconnect(id);
            }
        }
        private void txtMessage_KeyDown(object sender, KeyEventArgs e)                              // Send on <Enter>
        {
            if (e.KeyCode == Keys.Enter) {
                e.Handled = true;
                e.SuppressKeyPress = true;
                if (txtMessage.Text.Length > 0) {
                    string msg = txtMessage.Text;
                    txtMessage.Clear();
                    Log(string.Format("{0} (You): {1}", txtName.Text.Trim(), msg));
                    if (listening) {
                        SSend(string.Format("{0}: {1}", txtName.Text.Trim(), msg));
                    }
                    if (connected) {
                        Send(msg);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)                                      // TEST - Update DataGrid
        {
            SendDataGridViewContents();
        }




        private void Connected(bool status)                                                         // Client Connected toggle
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
                        Log(SystemMsg("You are now connected"));
                    } else {
                        txtAddress.Enabled = true;
                        txtPort.Enabled = true;
                        txtName.Enabled = true;
                        txtRoomKey.Enabled = true;
                        cmdHost.Enabled = true;
                        cmdDisconnect.Enabled = true;
                        cmdJoin.Text = "Connect";
                        Log(SystemMsg("You are now disconnected"));
                    }
                });
            }
        }
        private void Listening(bool status)                                                         // Host Listening toggle
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
                        Log(SystemMsg("Server has started"));
                    } else {
                        txtAddress.Enabled = true;
                        txtPort.Enabled = true;
                        txtName.Enabled = true;
                        txtRoomKey.Enabled = true;
                        cmdJoin.Enabled = true;
                        newSize = new(600, 560);
                        cmdHost.Text = "Host";
                        Log(SystemMsg("Server has stopped"));
                    }
                    //this.Size = (newSize);
                });
            }
        }






        private void AddToGrid(long id, string name)                                                // Add Client
        {
            if (!exit) {
                clientsDataGridView.Invoke((MethodInvoker)delegate {
                    string[] row = new string[] { id.ToString(), name };
                    clientsDataGridView.Rows.Add(row);
                    lblConnections.Text = string.Format("Total players: {0}", clientsDataGridView.Rows.Count);
                });
/*                if (listening) {
                    SSend(CommandMsg(string.Format("{0}:{1},{2}", "AddToGrid", id, name)));
                }*/
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
        private void SendDataGridViewContents()                                                     // Collect, format and send DataGrid contents
        {
            string contents = "";
            int rCount = 0;
            foreach (DataGridViewRow row in clientsDataGridView.Rows) { // 0, name, kick, \n 1, name, kick
                foreach (DataGridViewCell cell in row.Cells) {
                    contents += cell.Value.ToString() + ",";
                }
                rCount++;
                if (rCount < clientsDataGridView.RowCount) { contents += "\n"; }
            }
            SSend(CommandMsg(string.Format("{0}:{1}", "UpdateGrid", contents)));
        }



        private void Connection(IPAddress ip, int port, string username, string roomkey)            // Single TCP to host
        {
            try {
                clientObject = new MyClient();
                clientObject.username = username;
                clientObject.key = roomkey;
                clientObject.client = new TcpClient();
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
                            Log(ErrorMsg(ex.Message));
                        }
                    }
                    clientObject.client.Close();
                    Connected(false);
                }
            } catch (Exception ex) {
                Log(ErrorMsg(ex.Message));
            }
        }

        private void Connection(MyPlayers obj)                                                      // Multi TCP to clients
        {
            if (Authorize(obj)) {
                players.TryAdd(obj.id, obj);
                AddToGrid(obj.id, obj.username.ToString());
                string msg = string.Format("{0}:{1} has connected.", obj.id, obj.username);
                Log(SystemMsg(msg));
                SSend(SystemMsg(msg), obj.id);
                while (obj.client.Connected) {
                    try {
                        obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(Read), obj);
                        obj.handle.WaitOne();
                    } catch (Exception ex) {
                        Log(ErrorMsg(ex.Message));
                    }
                }
                obj.client.Close();
                players.TryRemove(obj.id, out MyPlayers tmp);
                RemoveFromGrid(tmp.id);
                msg = string.Format("{0}:{1} has disconnected.", tmp.id, tmp.username);
                Log(SystemMsg(msg));
                SSend(SystemMsg(msg), tmp.id);
            }
        }

        private void Listener(IPAddress ip, int port)                                               // Multi TCP to clients
        {
            TcpListener listener = null;
            try {
                listener = new TcpListener(ip, port);
                listener.Start();
                Listening(true);
                while (listening) {
                    if (listener.Pending()) {
                        try {
                            MyPlayers obj = new MyPlayers();
                            obj.id = id;
                            obj.username = new StringBuilder();
                            obj.client = listener.AcceptTcpClient();
                            obj.stream = obj.client.GetStream();
                            obj.buffer = new byte[obj.client.ReceiveBufferSize];
                            obj.data = new StringBuilder();
                            obj.handle = new EventWaitHandle(false, EventResetMode.AutoReset);
                            Thread th = new Thread(() => Connection(obj)) {
                                IsBackground = true
                            };
                            th.Start();
                            id++;
                        } catch (Exception ex) {
                            Log(ErrorMsg(ex.Message));
                        }
                    } else {
                        Thread.Sleep(500);
                    }
                }
                Listening(false);
            } catch (Exception ex) {
                Log(ErrorMsg(ex.Message));
            } finally {
                if (listener != null) {
                    listener.Server.Close();
                }
            }
        }


        private void Disconnect(long id = -1)                                                       // Disconnect ID / All if empty
        {
            if (disconnect == null || !disconnect.IsAlive) {
                disconnect = new Thread(() => {
                    if (id >= 0) {
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


        private bool Authorize()                                                                    // Client request
        {
            bool success = false;
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("username", clientObject.username);
            data.Add("roomkey", clientObject.key);
            JavaScriptSerializer json = new JavaScriptSerializer(); // feel free to use JSON serializer
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
                    Log(ErrorMsg(ex.Message));
                }
            }
            if (!connected) {
                Log(SystemMsg("Unauthorized"));
            }
            return success;
        } 

        private bool Authorize(MyPlayers obj)                                                       // Host replies
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
                    Log(ErrorMsg(ex.Message));
                }
            }
            return success;
        } 






        private void Read(IAsyncResult result)                                                      // Stream Readers
        {
            if (listening) {                                                                        // Host stream reader
                MyPlayers obj = (MyPlayers)result.AsyncState;  
                int bytes = 0;
                if (obj.client.Connected) {
                    try {
                        bytes = obj.stream.EndRead(result);
                    } catch (Exception ex) {
                        Log(ErrorMsg(ex.Message));
                    }
                }
                if (bytes > 0) {
                    obj.data.AppendFormat("{0}", Encoding.UTF8.GetString(obj.buffer, 0, bytes));
                    try {
                        if (obj.stream.DataAvailable) {
                            obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(Read), obj);
                        } else {
                            if (connected) {
                                Log(obj.data.ToString());
                                obj.data.Clear();
                            } else {
                                if (listening) {
                                    string msg = string.Format("{0}: {1}", obj.username, obj.data);
                                    Log(msg);
                                    SSend(msg, obj.id);
                                } else {
                                    Log(obj.data.ToString());
                                }
                            } 
                            obj.data.Clear();
                            obj.handle.Set();
                        }
                    } catch (Exception ex) {
                        obj.data.Clear();
                        Log(ErrorMsg(ex.Message));
                        obj.handle.Set();
                    }
                } else {
                    obj.client.Close();
                    obj.handle.Set();
                }
            } else {                                                                                // Client stream reader
                if (clientObject.client == null) { return;}
                int bytes = 0;
                if (clientObject.client.Connected) {
                    try {
                        bytes = clientObject.stream.EndRead(result);
                    } catch (Exception ex) {
                        Log(ErrorMsg(ex.Message));
                    }
                }
                if (bytes > 0) {
                    clientObject.data.AppendFormat("{0}", Encoding.UTF8.GetString(clientObject.buffer, 0, bytes));
                    try {
                        if (clientObject.stream.DataAvailable) {
                            clientObject.stream.BeginRead(clientObject.buffer, 0, clientObject.buffer.Length, new AsyncCallback(Read), null);
                        } else {
                            if (clientObject.data.ToString().StartsWith("CMD:")) {
                                ReceiveCommand(clientObject.data.ToString());
                                clientObject.data.Clear();
                                clientObject.handle.Set();
                                return;
                            }
/*                            if (clientObject.data.ToString().StartsWith("SYSTEM:")) {
                                if (clientObject.data.ToString().EndsWith(" connected.")) {
                                    string[] addParts = clientObject.data.ToString().Split(':');
                                    string addID = addParts[1].Trim();
                                    string addName = addParts[2].Trim();                                            // DON'T DO THIS ON CLIENTS ANYMORE
                                    AddToGrid(Convert.ToInt64(addID), addName);
                                }
                                if (clientObject.data.ToString().EndsWith(" disconnected.")) {
                                    string[] dcParts = clientObject.data.ToString().Split(':');
                                    string dcID = dcParts[1].Trim();
                                    RemoveFromGrid(Convert.ToInt64(dcID));
                                }
                            }*/
                            Log(clientObject.data.ToString());
                            clientObject.data.Clear();
                            clientObject.handle.Set();
                        }
                    } catch (Exception ex) {
                        clientObject.data.Clear();
                        Log(ErrorMsg(ex.Message));
                        clientObject.handle.Set();
                    }
                } else {
                    clientObject.client.Close();
                    clientObject.handle.Set();
                }
            }
        }

        private void ReceiveCommand(string Command)                                                 // String enters "CMD:NameOfCommand:Details"
        {
            string[] CmdParts = Command.Split(':');
            string CmdName = CmdParts[1];
            string CmdDetails = CmdParts[2];

            switch (CmdName) {
                case "AddToGrid":
                    string[] addCell = CmdDetails.Split(',');                  // Seperate the Cells
                    long addID = Convert.ToInt64(addCell[0]);
                    string addName = addCell[1];
                    AddToGrid(addID, addName);
                    break;

                case "UpdateGrid":
                    //"0,B,Kick,\n"
                    //
                    ClearDataGrid();
                                                   // Clear the Grid

                    string[] rows = CmdDetails.Split('\n');                      // Seperate the Rows from the Details
                    int i = 0;
                    foreach (string row in rows) {                        
                        string[] ugCell = CmdDetails.Split(',');                  // Seperate the Cells
                        long ugID = Convert.ToInt64(ugCell[i]);
                        string ugName = ugCell[++i];
                        i++;
                        i++;
                        AddToGrid(ugID, ugName);
                    }
                    break;

                default:
                    Log("Unreconized Command: " + CmdName + " --- " + CmdDetails);
                    break;
            }
        }





        private void ReadAuth(IAsyncResult result)                                                  // Stream Readers
        {
            if (listening) {                                                                        // Host read
                MyPlayers obj = (MyPlayers)result.AsyncState;
                int bytes = 0;
                if (obj.client.Connected) {
                    try {
                        bytes = obj.stream.EndRead(result);
                    } catch (Exception ex) {
                        Log(ErrorMsg(ex.Message));
                    }
                }
                if (bytes > 0) {
                    obj.data.AppendFormat("{0}", Encoding.UTF8.GetString(obj.buffer, 0, bytes));
                    try {
                        if (obj.stream.DataAvailable) {
                            obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(ReadAuth), obj);
                        } else {
                            JavaScriptSerializer json = new JavaScriptSerializer();
                            Dictionary<string, string> data = json.Deserialize<Dictionary<string, string>>(obj.data.ToString());
                            if (!data.ContainsKey("username") || data["username"].Length < 1 || !data.ContainsKey("roomkey") || !data["roomkey"].Equals(txtRoomKey.Text)) {
                                obj.client.Close();
                            } else {
                                obj.username.Append(data["username"].Length > 200 ? data["username"].Substring(0, 200) : data["username"]);
                                SSend("{\"status\": \"authorized\"}", obj);
                            }
                            obj.data.Clear();
                            obj.handle.Set();
                        }
                    } catch (Exception ex) {
                        obj.data.Clear();
                        Log(ErrorMsg(ex.Message));
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
                        Log(ErrorMsg(ex.Message));
                    }
                }
                if (bytes > 0) {
                    clientObject.data.AppendFormat("{0}", Encoding.UTF8.GetString(clientObject.buffer, 0, bytes));
                    try {
                        if (clientObject.stream.DataAvailable) {
                            clientObject.stream.BeginRead(clientObject.buffer, 0, clientObject.buffer.Length, new AsyncCallback(ReadAuth), null);
                        } else {
                            JavaScriptSerializer json = new JavaScriptSerializer();
                            Dictionary<string, string> data = json.Deserialize<Dictionary<string, string>>(clientObject.data.ToString());
                            if (data.ContainsKey("status") && data["status"].Equals("authorized")) {
                                Connected(true);
                            }
                            clientObject.data.Clear();
                            clientObject.handle.Set();
                        }
                    } catch (Exception ex) {
                        clientObject.data.Clear();
                        Log(ErrorMsg(ex.Message));
                        clientObject.handle.Set();
                    }
                } else {
                    clientObject.client.Close();
                    clientObject.handle.Set();
                }
            }
        }

        private void Write(IAsyncResult result)                                                     // Client write
        {
            if (clientObject.client.Connected) {
                try {
                    clientObject.stream.EndWrite(result);
                } catch (Exception ex) {
                    Log(ErrorMsg(ex.Message));
                }
            }
        }
       
        private void SWrite(IAsyncResult result)                                                    // Host write
        {            
            MyPlayers obj = (MyPlayers)result.AsyncState;
            if (obj.client.Connected) {
                try {
                    obj.stream.EndWrite(result);
                } catch (Exception ex) {
                    Log(ErrorMsg(ex.Message));
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
                        Log(ErrorMsg(ex.Message));
                    }
                }
        }

        private void SBeginWrite(string msg, MyPlayers obj)                                         // Private message
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            if (obj.client.Connected) {
                try {
                    obj.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(SWrite), obj);
                } catch (Exception ex) {
                    Log(ErrorMsg(ex.Message));
                }
            }
        }

        private void SBeginWrite(string msg, long id = -1)                                          // Public -- set ID to lesser than zero to send to everyone
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            foreach (KeyValuePair<long, MyPlayers> obj in players) {
                if (id != obj.Value.id && obj.Value.client.Connected) {
                    try {
                        obj.Value.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(SWrite), obj.Value);
                    } catch (Exception ex) {
                        Log(ErrorMsg(ex.Message));
                    }
                }
            }
        }

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

        private void SSend(string msg, MyPlayers obj)                                               // Private message
        {
            if (send == null || send.IsCompleted) {
                send = Task.Factory.StartNew(() => SBeginWrite(msg, obj));
            } else {
                send.ContinueWith(antecendent => SBeginWrite(msg, obj));
            }
        }

        private void SSend(string msg, long id = -1)                                                // Public message
        {
            if (send == null || send.IsCompleted) {
                send = Task.Factory.StartNew(() => SBeginWrite(msg, id));
            } else {
                send.ContinueWith(antecendent => SBeginWrite(msg, id));
            }
        }







    }
}
