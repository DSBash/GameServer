using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static Server.Encryption;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Server
{
    public partial class GameServer
    {
        #region Client Specific Declarations
        private bool connected = false;                                                             // Client Mode Marker
        private Thread client = null;                                                               // The Client TCP
        private Client clientObject;                                                                // The Client Object

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
        #endregion

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
        private void FileReceive()                                                                  // Client - Drawing File Receiver 
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

    }
}