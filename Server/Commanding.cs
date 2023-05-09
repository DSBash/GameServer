using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Unclassified.UI;

namespace Server
{
    public partial class GameServer
    {
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
    }
}