using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Server
{
    public partial class GameServer
    {
        private class BorderedTabControl                                                            /* Custom Borders */ : TabControl
        {
            public Color BorderColor { get; internal set; }
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == 0xf) {
                    using var g = Graphics.FromHwnd(Handle);
                    using var p = new Pen(BorderColor, 4);
                    g.DrawRectangle(p, new Rectangle(2, 23, Width - 4, Height - 25));               // - 26 Will remove the White line below the Tab Control
                }
            }
        }

        private class BorderedGroupBox                                                              /* Custom Borders */ : System.Windows.Forms.GroupBox
        {
            public static Color BorderColor { get; internal set; }
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                if (Theme != null) {
                    if (Themes.TryGetValue(Theme, out Dictionary<ThemeColor, Color> matchingDictionary)) {  // If a matching dictionary was found
                        Color SecondaryColor = matchingDictionary[ThemeColor.Secondary];
                        BorderColor = SecondaryColor;
                    }
                } else { BorderColor = Color.Transparent; }

                using Pen pen = new(BorderColor, 2);                                                // Create a new Pen with the desired border color
                Rectangle rect = new(1, 7, Width - 2, Height - 8);                                  // Draw a rectangle around the group box
                e.Graphics.DrawRectangle(pen, rect);
            }
        }
        private class BorderedButton                                                                /* Custom Borders */ : Button
        {
            public Color BorderColor { get; internal set; }
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == 0xf) {
                    using var g = Graphics.FromHwnd(Handle);
                    using var p = new Pen(BorderColor, 3);
                    g.DrawRectangle(p, new Rectangle(2, 2, Width - 5, Height - 5));
                }
            }
        }
        private class BorderedTextBox                                                               /* Custom Borders */ : TextBox
        {
            public Color BorderColor { get; internal set; }
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == 0xf) {
                    using var g = Graphics.FromHwnd(Handle);
                    using var p = new Pen(BorderColor, 4);
                    g.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));
                }
            }
        }
        private class BorderedRichTextBox : RichTextBox
        {
            public Color BorderColor { get; internal set; }
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == 0xf) {
                    using var g = Graphics.FromHwnd(Handle);
                    using var p = new Pen(BorderColor, 5);
                    g.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));

                }
            }
        }

    }
}