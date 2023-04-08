using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using static Server.GameServer;

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
            public string BrushColor { get; set; }
            public string DrawType { get; set; }
            public bool Fill { get; set; }
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


        Bitmap BM;
        Graphics G;
        Point PT2, PT1;
/*        bool paint;*/
        int x, y, PT1X, PT1Y, PT2X, PT2Y;
        /*
                private void picDrawing_MouseDown(object sender, MouseEventArgs e)
                {
                    paint = true;
                    PT1 = e.Location;

                    PT1X = e.X;
                    PT1Y = e.Y;
                }
        */
        /*
                private void picDrawing_MouseMove(object sender, MouseEventArgs e)
                {
                    if (paint) {
                        Pen pen = new(btnColor.BackColor,(int)nudSize.Value);

                        switch (cbBType.Text) {
                            case "Circle":
                            case "Line":
                            case "Rectangle":
                                break;
                            default:                        
                                PT2 = e.Location;
                                G.DrawLine(pen,PT2,PT1);
                                PT1 = PT2;
                                break;
                        }

                    }
                    picDrawing.Refresh();

                    x = e.X;
                    y = e.Y;
                    PT2X = e.X - PT1X;
                    PT2Y = e.Y - PT1Y;

                }
        */
        /*
                private void picDrawing_MouseUp(object sender, MouseEventArgs e)
                {
                    paint = false;
                    Pen pen = new(btnColor.BackColor, (int)nudSize.Value);

                    PT2X = x - PT1X;
                    PT2Y = y - PT1Y;

                    switch (cbBType.Text) {
                        case "Circle":
                            G.DrawEllipse(pen,PT1X,PT1Y,PT2X,PT2Y);
                            break;
                        case "Line":
                            G.DrawLine(pen,PT1X,PT1Y,x,y);
                            break;
                        case "Rectangle":
                            G.DrawRectangle(pen, PT1X, PT1Y, PT2X, PT2Y);
                            break;
                        default:
                            break;
                    }
                }
        */
        /*
                private void picDrawing_Paint(object sender, PaintEventArgs e)
                {
                    Graphics G = e.Graphics;
                    Pen pen = new(btnColor.BackColor, (int)nudSize.Value);
                    if (paint) {
                        switch (cbBType.Text) {
                            case "Circle":
                                G.DrawEllipse(pen, PT1.X, PT1.Y, PT2.X, PT2.Y);
                                break;
                            case "Line":
                                G.DrawLine(pen, PT1.X, PT1.Y, x, y);
                                break;
                            case "Rectangle":
                                G.DrawRectangle(pen, PT1.X, PT1.Y, PT2.X, PT2.Y);
                                break;
                            default:
                                break;
                        }
                    }
                }
        */


















        // Form
        public GameServer()
        {
            InitializeComponent();
            //AllocConsole();
            BM = new(picDrawing.Width, picDrawing.Height);
            G = Graphics.FromImage(BM);
            G.SmoothingMode = SmoothingMode.AntiAlias;
            G.Clear(Color.Transparent);
            picDrawing.Image = BM;
        }
        private void GameServer_FormClosing(object sender, FormClosingEventArgs e)                  // Close connection on Exit 
        {
            listening = false;
            if (connected) {
                clientObject.client.Close();
            }
            Disconnect();
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
        private void PostChat(string username, string msg = "")                                     // Post the message / Clear if empty 
        {
            if (msg == null || msg.Length < 1) {
                txtLobby.Clear();
            } else {
                string formattedMSG = "";                                                           // Format Messages
                if (username.Contains("to you")) {                                                  // Private Messages
                    username = username.Replace("to you", "").Trim();
                    formattedMSG = string.Format("{0}[{1}] {2} to you: {3}", Environment.NewLine, DateTime.Now.ToString("HH:mm:ss"), username, msg);
                    if (username == txtName.Text.Trim()) { formattedMSG = string.Format("{0}[{1}] You say to yourself: {3}", Environment.NewLine, DateTime.Now.ToString("HH:mm:ss"), username, msg); }
                } else {                                                                            // Public Messages
                    formattedMSG = string.Format("{0}[{1}] {2}: {3}", Environment.NewLine, DateTime.Now.ToString("HH:mm:ss"), username, msg);
                }

                string playerColor = "255,0,0,0,0";                                                 // Determine color for post using Grid
                foreach (DataGridViewRow row in clientsDataGridView.Rows) {
                    if (row.Cells[1].Value != null && row.Cells[1].Value.ToString() == username) {
                        playerColor = row.Cells[2].Value?.ToString();
                        break;
                    }
                }
                string[] colParts = playerColor.Split(',');                                         // Format string to Color
                Color msgColor = Color.FromArgb(Convert.ToInt32(colParts[0]), Convert.ToInt32(colParts[1]), Convert.ToInt32(colParts[2]), Convert.ToInt32(colParts[3]));
                txtLobby.Invoke((MethodInvoker)delegate {                                           // Post actual msg
                    txtLobby.AppendText(formattedMSG, msgColor);
                    txtLobby.Focus();                                                               // Set Focus to lobby to
                    txtLobby.SelectionStart = txtLobby.Text.Length;                                 // scroll lobby to end
                    txtMessage.Focus();                                                             // Leave focus for next message
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
                txtMessage.Clear();
                if (txtMessage.Text.Length > 0 && !txtMessage.Text.StartsWith("/")) {
                    PostChat(txtName.Text.Trim(), msg);
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
                                        PostChat(txtName.Text.Trim() + "to you", hostPM);
                                        //txtLobby.AppendText(string.Format("{0}{1} to you: {2}", Environment.NewLine, txtName.Text.Trim(), hostPM));
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
            int cA = color.A; int cB = color.B; int cR = color.R; int cG = color.G;                 // convert COLOURS to string  - From Color to ARGB
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

                    PlayerPackage player = new() {                                                  // Create a player object to send
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
        private void DrawColor_Click(object sender, EventArgs e)                                    // Set Drawing Colour 
        {
            SetColour(sender);
        }
        private void FillColor_Click(object sender, EventArgs e)                                    // Set Fill Colour 
        {
            SetColour(sender);
        }
        private void CmdClear_Click(object sender, EventArgs e)                                     // Clears the gfx 
        {
            //using var G = picDrawing.CreateGraphics();
            G.Clear(btnBGColor.BackColor);
            picDrawing.Refresh();
        }
        private void CmdClearAll_Click(object sender, EventArgs e)                                  // Clears gfx and Sends Clear to Clients 
        {
            CmdClear_Click(sender, e);
            HostSendPublic("CMD:ClearDrawing");
        }

/*private void DrawPanel_MouseDown(object sender, MouseEventArgs e) 
        {
            DrawPackage localDP = PrepareDrawPackage();                                             // Create the Package
            if (Drawing && e.Button == MouseButtons.Left) {
                switch (cbBType.Text) {
                    case "Line":
                    case "Rectangle":
                    case "Triangle":
                    case "Circle":
                        ShapeDrawing = true;                                                        // Shape Toggle
                        mousePT1 = e.Location;
                        break;
                    default:
                        mousePT1 = e.Location;
                        mousePT2 = e.Location;
                        DrawShape(localDP);                                                         // Draw it
                        SendShape(localDP);                                                         // Send it
                        break;
                }
            }
        }*/
        private void picDrawing_MouseDown(object sender, MouseEventArgs e)                          // Start Drawing points
        {
            if (Drawing && e.Button == MouseButtons.Left) {
                PT1 = e.Location;  
                PT1X = e.X;
                PT1Y = e.Y;
            }
        }

/*        private void DrawPanel_MouseMove(object sender, MouseEventArgs e) 
        {
            if (Drawing && e.Button == MouseButtons.Left) {
                DrawPackage localDP = PrepareDrawPackage();
                switch (cbBType.Text) {
                    case "Line":
                    case "Rectangle":
                    case "Triangle":
                    case "Circle":
                        mousePT2 = e.Location;
                        break;
                    default:
                        mousePT1 = mousePT2;
                        mousePT2 = e.Location;
                        DrawShape(localDP);                                                         // Draw it
                        SendShape(localDP);                                                         // Send it
                        break;
                }
            }
        }*/
        private void picDrawing_MouseMove(object sender, MouseEventArgs e)                          // Continue Drawing 
        {
            if (Drawing && e.Button == MouseButtons.Left) {                
                switch (cbBType.Text) {
                    case "Circle":
                    case "Line":
                    case "Rectangle":
                    case "Triangle":
                        PT2 = e.Location;
                        break;
                    default:
                        PT2 = e.Location;
                        DrawPackage localDP = PrepareDrawPackage();                                 // Create the Package 
                        DrawShape(localDP);                                                         // Draw it                           
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

/*        private void DrawPanel_MouseUp(object sender, MouseEventArgs e) 
        {
            if (Drawing && ShapeDrawing && e.Button == MouseButtons.Left) {
                DrawPackage localDP = PrepareDrawPackage();                                             // Create the Package            
                switch (cbBType.Text) {
                    case "Line":
                    case "Circle":
                    case "Rectangle":
                    case "Triangle":
                        ShapeDrawing = false;                                                       // Shape Toggle
                        goto default;
                    default:
                        DrawShape(localDP);                                                         // Draw it
                        SendShape(localDP);                                                         // Send it
                        break;
                }
            }
        }*/
        private void picDrawing_MouseUp(object sender, MouseEventArgs e)                            // Draw shape on Mouse Up 
        {
            if (Drawing && e.Button == MouseButtons.Left) {                               
                PT2X = x - PT1X;                                                                    // Eclipse Width
                PT2Y = y - PT1Y;                                                                    // Eclipse Height
                DrawPackage localDP = PrepareDrawPackage();                                         // Create the Package 
                DrawShape(localDP);                                                                 // Draw it
            }
        }

        private void picDrawing_Paint(object sender, PaintEventArgs e)                              // Draw shape Preview
        { 
            if (Drawing) {
                Graphics G = e.Graphics;
                Pen pen = new(btnColor.BackColor, (int)nudSize.Value);
                SolidBrush brush = new(btnFillColor.BackColor);
                switch (cbBType.Text) {
                    case "Circle":
                        G.DrawEllipse(pen, PT1X, PT1Y, PT2X, PT2Y);                                 // Draw
                        if (cbFillDraw.Checked) {
                            G.FillEllipse(brush, PT1X, PT1Y, PT2X, PT2Y);                           // Fill
                        }
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
                    default:
                        break;
                }
            }
        }

        private DrawPackage PrepareDrawPackage()                                                    // Create a Draw Pakage 
        {
            int cA = btnColor.BackColor.A; int cB = btnColor.BackColor.B; int cR = btnColor.BackColor.R; int cG = btnColor.BackColor.G;
            string pcString = string.Format("{0},{1},{2},{3}", cA, cR, cG, cB);                     // convert COLOURS to string  - From Color to ARGB
            int bA = btnFillColor.BackColor.A; int bB = btnFillColor.BackColor.B; int bR = btnFillColor.BackColor.R; int bG = btnFillColor.BackColor.G;
            string bcString = string.Format("{0},{1},{2},{3}", bA, bR, bG, bB);                     // convert COLOURS to string  - From Color to ARGB

            DrawPackage drawPack = new() {                                                          // Prep Draw Package for send
                PenColor = pcString,
                PenSize = (int)nudSize.Value,
                PT1 = PT1,
                PT2 = PT2,
                BrushColor = bcString,
                DrawType = cbBType.Text,
                Fill = cbFillDraw.Checked
            };

            return drawPack;                                                                            // Return Package to caller
        }
        private Color ArgbColor(string wholeColor)                                                  // Converts String in "A,R,G,B" to Color 
        {
            string[] ColorParts = wholeColor.Split(',');                                            // Seperate colour parts for Pen
            int ColA = Convert.ToInt32(ColorParts[0]);
            int ColR = Convert.ToInt32(ColorParts[1]);
            int ColG = Convert.ToInt32(ColorParts[2]);
            int ColB = Convert.ToInt32(ColorParts[3]);
            Color argbColor = Color.FromArgb(ColA, ColR, ColG, ColB);
            return argbColor;
        }
        private void DrawShape(DrawPackage drawPackage)                                             // Draw the shape from package 
        {
            using var pen = new Pen(ArgbColor(drawPackage.PenColor), drawPackage.PenSize);          // Set Pen
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;

            SolidBrush brush = new(ArgbColor(drawPackage.BrushColor));                              // Set Brush
            
            switch (drawPackage.DrawType) {
                case "Line":
                    G.DrawLine(pen, drawPackage.PT1.X, drawPackage.PT1.Y, x, y);
                    break;
                case "Circle":
                    if (drawPackage.Fill) {
                        G.FillEllipse(brush, drawPackage.PT1.X, drawPackage.PT1.Y, PT2X, PT2Y);
                    }
                    G.DrawEllipse(pen, drawPackage.PT1.X, drawPackage.PT1.Y, PT2X, PT2Y);
                    break;
                case "Rectangle":                   
                    var rc = new Rectangle(
                        Math.Min(drawPackage.PT1.X, drawPackage.PT2.X),
                        Math.Min(drawPackage.PT1.Y, drawPackage.PT2.Y),
                        Math.Abs(drawPackage.PT2.X - drawPackage.PT1.X),
                        Math.Abs(drawPackage.PT2.Y - drawPackage.PT1.Y));
                    if (drawPackage.Fill) {
                        G.FillRectangle(brush, rc);                                                   // Fill
                    }
                    G.DrawRectangle(pen, rc);                                                         // Draw
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
                        G.FillPath(brush, tPath);                                                   // Fill
                    }
                    G.DrawPath(pen, tPath);                                                         // Draw
                    break;
                default:
                    picDrawing.Invoke((MethodInvoker)delegate {
                    G.DrawLine(pen, drawPackage.PT2, drawPackage.PT1);                              // Remote Pen Draw
                    });
                    break;
            }
            string json = JsonConvert.SerializeObject(drawPackage);                                 // Format the Package 
            if (listening) {
                HostSendPublic(json + "\n");                                                        // Host Send Draw Package
            } else if (connected) {
                Send(json + "\n");                                                                  // Client Send Draw Package
            }
            picDrawing.Invoke((MethodInvoker)delegate {
                picDrawing.Refresh();
            });
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
                PingReply reply = ping.Send(address, 999);
                if (reply.Status == IPStatus.Success) {
                    totalTime += reply.RoundtripTime;
                }
            }
            return totalTime / 4;
        }
        private string GetPlayerAddress(int id)                                                      // Get IP Address for Ping 
        {
            if (id != 0) {
                try {
                    players.TryGetValue(id, out MyPlayers obj);
                    string ipAddress = ((IPEndPoint)obj.client.Client.RemoteEndPoint).Address.ToString();
                    return ipAddress;
                } catch (Exception ex) {
                    Console(ErrorMsg(ex.Message));
                    return "127.0.0.1";
                }

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
                                        PostChat(obj.username.ToString() + "to you", pMSG[2]);
                                        /*PrivateChat(obj.username.ToString(), pMSG[2]);              // Post Host PM */
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
                                DrawShape(remoteDP);
                                HostSendPublic(clientData, obj.id);                                  // Host relay client drawing to other clients
                                obj.data.Clear();
                                obj.handle.Set();
                                return;
                            }

                            // Host Receive - Public Message
                            PostChat(obj.username.ToString(), obj.data.ToString());               // Host post
                            HostSendPublic(string.Format("{0}:{1}", obj.username, obj.data.ToString()), obj.id);                            // Host relay public message to other clients

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
                if (clientObject == null) { return; }
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
                                    DrawShape(remoteDP);
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
                                        CmdClear_Click(sender, e);
                                        break;
                                    default:
                                        break;
                                }
                                clientObject.data.Clear();
                                clientObject.handle.Set();
                                return;
                            }

                            string[] dataParts = hostData.Split(':');
                            PostChat(dataParts[0], dataParts[1]);                                 // Client - Public Message
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



        /* Classes and Routines */
        private void SetColour(object sender)                                                       // Set's all Colour related Buttons 
        {
            Button clickedButton = (Button)sender;
            ColorDialog diag = new() {
                AllowFullOpen = true,
                ShowHelp = true,
                Color = clickedButton.BackColor                                                     // Sets initial color to current button backcolor             
            };
            //using var diag = new ColorDialog();
            if (diag.ShowDialog() == DialogResult.OK)
                clickedButton.BackColor = diag.Color;
        }


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
            if (tabSections.SelectedTab.Name == "tLobby") { PostChat(null, null); }
        }



        private void CmdPlayerColor_Click(object sender, EventArgs e)                               // Player Color Chooser / set starting brush to same
        {
            SetColour(sender);
            btnColor.BackColor = cmdColor.BackColor;
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
