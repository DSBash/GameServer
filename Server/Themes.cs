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
        #region Theme Declarations
        private bool DarkMode { get; set; }
        private static string Theme { get; set; }
        enum ThemeColor {
            Primary,
            Secondary,
            Tertiary,
            Quaternary
        }
        private static readonly Dictionary<string, Dictionary<ThemeColor, Color>> Themes = new(StringComparer.OrdinalIgnoreCase);
        #endregion


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
            }
        }

        private static void LoadThemes()                                                            // Add themes to Dict 
        {
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
        }

        private void ChangeControlColors(Control control, Color primaryCol,
            Color secondaryCol, Color tertiaryCol, Color quaternaryCol)                             // Changes colors based on Control type  
        {
                                                                                                    // primaryCol = Border, Button, & Textbox Text
                                                                                                    // secondaryCol = Button & Textbox BackColors
                                                                                                    // tertiaryCol = Container ForeColor
                                                                                                    // quaternaryCol = Container BackColor
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
                /*
                 case "BorderedRichTextBox":                                                // This is overkill, tab Borders are enough
                    BorderedRichTextBox cBRTB = childControl as BorderedRichTextBox;
                    cBRTB.ForeColor = primaryCol;
                    cBRTB.BackColor = secondaryCol;
                    cBRTB.BorderColor = primaryCol;
                    break;
                */
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
                        col.HeaderCell.Style.BackColor = secondaryCol;                              // Change the color to your desired color
                        col.HeaderCell.Style.ForeColor = primaryCol;
                        col.HeaderCell.Style.SelectionForeColor = Color.Red;
                        col.HeaderCell.Style.SelectionBackColor = Color.Red;

                        ((System.Windows.Forms.DataGridView)childControl).GridColor = primaryCol;
                    }
                    break;
                }

                if (childControl.HasChildren) {
                    if (DebugLVL == 2) { Console("Spawn Found"); }
                    ChangeControlColors(childControl, primaryCol, secondaryCol, tertiaryCol, quaternaryCol);   // Recurse if childControl has children 
                }
            }
        }
    }
}