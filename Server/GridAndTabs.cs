using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Unclassified.UI;
using System.Linq;

namespace Server
{
    public partial class GameServer
    {
        #region Tab Management
        private void CreateTCM()                                                                    // Tab Control - Context Menu
        {
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
        }

        private void ChangeTXTBackColor(object sender, EventArgs e)                                 // Tabs Menu - Set the background color of respective TextBox 
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

        private void ClearTXT(object sender, EventArgs e)                                           // Tabs Menu - Clear the respective Textbox 
        {
            if (tabSections.SelectedTab.Name == "tConsole") { Console(); }
            if (tabSections.SelectedTab.Name == "tLobby") { PostChat(MessagePack("", "", "", "")); }
            if (tabSections.SelectedTab.Tag.ToString() == "PM") { ReturnFirstBoxFrom(sender).Text = ""; }
        }

        private void TabSections_TabChanged(object sender, EventArgs e)                             // Tabs - Remove the *unread* indicators from tab Text 
        {
            TabControl tabControl = (TabControl)sender;
            TabPage selectedTabPage = tabControl.SelectedTab;

            if (selectedTabPage.Text.Contains("*")) {
                selectedTabPage.Text = selectedTabPage.Text.Replace("*", "");
            }
        }

        private RichTextBox ReturnFirstBoxFrom(object sender)                                       // Tabs Menu - Get RTB from Sender(Menu) 
        {
            MenuItem menuItem = sender as MenuItem;                                                 // Get the MenuItem that triggered the event
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;                               // Get the immediate parent of the MenuItem, which should be the ContextMenu
            TabControl tabControl = contextMenu.SourceControl as TabControl;                        // Get the control on which the ContextMenu was displayed, which should be the TabControl
            TabPage selectedTab = tabControl.SelectedTab;                                           // Get the selected TabPage
            RichTextBox textBox = selectedTab.Controls.OfType<RichTextBox>().FirstOrDefault();      // Get the textbox on the selected TabPage
            return textBox;
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
        private void CDGV_CellClick(object sender, DataGridViewCellEventArgs e)                     // Grid Button Clicks / Private Message / Ping / DC 
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
        private void ClearDataGrid()                                                                // Clear DataGrid 
        {
            clientsDataGridView.Invoke((MethodInvoker)delegate {
                clientsDataGridView.Rows.Clear();
            });
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
        #endregion

    }
}