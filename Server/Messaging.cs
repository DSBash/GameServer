using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Server
{
    public partial class GameServer
    {
        #region Message Declarations
        private bool remoteMsg = false;
        private TabPage pmTab;
        private readonly List<string> MSGHistory = new();                                           // CLI History
        private int HistoryIndex = -1;
        #endregion


        private class MessagePackage                                                                // Details needed to Draw and Broadcast 
        {
            public string Msg { get; set; }
            public string From { get; set; }
            public string To { get; set; }
            public string MsgType { get; set; }
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
    #region Public Messages
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
    #endregion
                string json = JsonConvert.SerializeObject(msgPack) + "\n";                          // Format the Package 
                if (listening && !remoteMsg) {
                    HostSendPublic(json);                                                           // Host Send Draw Package
                } else if (connected && !remoteMsg) {
                    Send(json);                                                                     // Client Send Draw Package
                }
                remoteMsg = false;
            }
        }


        #region Message formatters
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
        #endregion


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

        private void SetMessageCursor()                                                             // Puts the cursor at the end of the CLI History 
        {
            txtMessage.SelectionStart = txtMessage.Text.Length;
            txtMessage.SelectionLength = 0;
            txtMessage.Focus();
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
      
    }
}