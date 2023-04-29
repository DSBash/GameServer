// Copyright (c) 2012, Yves Goergen, http://unclassified.software/source/colorbutton
//
// Copying and distribution of this file, with or without modification, are permitted provided the
// copyright notice and this notice are preserved. This file is offered as-is, without any warranty.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Unclassified.Drawing;

namespace Unclassified.UI
{
    public class ColorButton : Button
    {
        private bool isPressed;
        private bool acceptClick;
        private Color selectedColor;
        private ColorSelector colorSelector;
        private ToolStripDropDown dropDown;
        private DateTime lastCloseTime;
        private Color lastOpenColor;

        public event EventHandler SelectedColorChanged;

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Black")]
        public Color SelectedColor
        {
            get {
                return selectedColor;
            }
            set {
                if (value != selectedColor) {
                    selectedColor = value;
                    Invalidate();
                    OnSelectedColorChanged();
                }
            }
        }

        #region Hidden properties
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get {
                return base.Text;
            }
            set {
                base.Text = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override System.Drawing.ContentAlignment TextAlign
        {
            get {
                return base.TextAlign;
            }
            set {
                base.TextAlign = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Font Font
        {
            get {
                return base.Font;
            }
            set {
                base.Font = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color ForeColor
        {
            get {
                return base.ForeColor;
            }
            set {
                base.ForeColor = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Image Image { get { return base.Image; } set { base.Image = value; } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new System.Drawing.ContentAlignment ImageAlign { get { return base.ImageAlign; } set { base.ImageAlign = value; } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int ImageIndex { get { return base.ImageIndex; } set { base.ImageIndex = value; } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string ImageKey { get { return base.ImageKey; } set { base.ImageKey = value; } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ImageList ImageList { get { return base.ImageList; } set { base.ImageList = value; } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new RightToLeft RightToLeft { get { return base.RightToLeft; } set { base.RightToLeft = value; } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new TextImageRelation TextImageRelation { get { return base.TextImageRelation; } set { base.TextImageRelation = value; } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool UseMnemonic { get { return base.UseMnemonic; } set { base.UseMnemonic = value; } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool AutoEllipsis { get { return base.AutoEllipsis; } set { base.AutoEllipsis = value; } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool UseCompatibleTextRendering { get { return base.UseCompatibleTextRendering; } set { base.UseCompatibleTextRendering = value; } }
        #endregion Hidden properties

        public ColorButton()
        {
            base.Text = "";
            selectedColor = Color.Black;

            PrepareColorSelector();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            colorSelector.Font = Font;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            Rectangle colorRect = ClientRectangle;

            if (isPressed && !Application.RenderWithVisualStyles) {
                colorRect.X++;
                colorRect.Y++;
            }
            colorRect.X += 0;
            colorRect.Y += 0;
            colorRect.Width -= 0;
            colorRect.Height -= 0;
            using (Brush b = new SolidBrush(selectedColor)) {
                pevent.Graphics.FillRectangle(b, colorRect);
            }
            using Pen p = new(SystemColors.ControlDark);
            colorRect.Width--;
            colorRect.Height--;
            pevent.Graphics.DrawRectangle(p, colorRect);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);

            if (mevent.Button == MouseButtons.Left) {
                isPressed = true;
                Invalidate();

                // Do not accept the click if the mouse button pressing has just closed the dropdown
                acceptClick = (DateTime.UtcNow - lastCloseTime).TotalMilliseconds > 100;
            }
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            // First do our thing to update the button before the click event does other modal things
            if (mevent.Button == MouseButtons.Left) {
                isPressed = false;
                Invalidate();
            }

            base.OnMouseUp(mevent);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            //ColorDialog cd = new ColorDialog();
            //cd.FullOpen = true;
            //cd.Color = selectedColor;
            //if (cd.ShowDialog() == DialogResult.OK)
            //{
            //    SelectedColor = cd.Color;
            //}

            if (acceptClick) {
                // Copy from ModernColorTable.ToolStripGradientEnd:
                if (ColorMath.RgbToHsl(SystemColors.Control).L > 215) {
                    // Could be Aero (L=240) or Luna (L=225-226)
                    colorSelector.BackColor = SystemColors.Control;
                } else {
                    // Could be Classic (L=206)
                    colorSelector.BackColor = ColorMath.Blend(SystemColors.Control, SystemColors.Window, 0.6);
                }

                colorSelector.SelectedColor = selectedColor;
                lastOpenColor = selectedColor;
                dropDown.Show(this, new Point(0, Height));
            }
        }

        private void ColorSelector_SelectedColorChanged(object sender, EventArgs e)
        {
            SelectedColor = colorSelector.SelectedColor;
        }

        private void ColorSelector_CloseRequested(object sender, EventArgs e)
        {
            if (dropDown != null && !dropDown.IsDisposed && dropDown.Visible) {
                dropDown.Close();
            }
        }

        private void ColorSelector_CancelRequested(object sender, EventArgs e)
        {
            if (dropDown != null && !dropDown.IsDisposed && dropDown.Visible) {
                SelectedColor = lastOpenColor;
                dropDown.Close();
            }
        }

        private void DropDown_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            lastCloseTime = DateTime.UtcNow;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (dropDown != null) {
                    dropDown.Dispose();
                    dropDown = null;
                }
            }
            base.Dispose(disposing);
        }

        private void PrepareColorSelector()
        {
            colorSelector = new ColorSelector {
                Padding = new Padding(6),
                Font = Font
            };
            colorSelector.SelectedColorChanged += ColorSelector_SelectedColorChanged;
            colorSelector.CloseRequested += ColorSelector_CloseRequested;
            colorSelector.CancelRequested += ColorSelector_CancelRequested;

            ToolStripControlHost controlHost = new(colorSelector) {
                Margin = new Padding(1)   // Preserve the dropdown border
            };

            dropDown = new ToolStripDropDown {
                Padding = Padding.Empty
            };
            dropDown.Items.Add(controlHost);
            dropDown.Closed += DropDown_Closed;
        }

        protected void OnSelectedColorChanged()
        {
            SelectedColorChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
