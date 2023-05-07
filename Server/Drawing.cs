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
        #region Drawing Delclarations
        public class DrawPackage                                                                    // Details needed to Draw and Broadcast 
        {
            public string PenColor { get; set; }
            public string BrushColor { get; set; }
            public string DrawType { get; set; }
            public Point[] PPath { get; set; }
            public bool Fill { get; set; }
            public int PenSize { get; set; }
            public Point PT1 { get; set; }
            public Point PT2 { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int PT1X { get; set; }
            public int PT1Y { get; set; }
            public int PT2X { get; set; }
            public int PT2Y { get; set; }
        }

        public class FillPackage                                                                    // Details needed to Fill and Broadcast 
        {
            public string FillColor { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }

        readonly Bitmap BM;                                                                         // The Drawing
        Graphics G;                                                                                 // The Canvas
        GraphicsPath mPath;                                                                         // Pen Path
        Point PT2, PT1;                                                                             // Points for Drawing
        int x, y, PT1X, PT1Y, PT2X, PT2Y;                                                           // Math Points
        private bool Drawing = false;                                                               // Local Drawing Toggle
        private bool remoteDraw = false;                                                            // Remote Drawing Toggle
        private Color _preBC;                                                                       // BG replace
        #endregion


        #region Drawing
        private DrawPackage PrepareDrawPackage()                                                    // Draw Pakage Constructor 
        {
            Color color = btnColor.SelectedColor;
            Color fillcolor = btnFillColor.SelectedColor;

            DrawPackage drawPack = new() {                                                          // Create the Draw Package
                PenColor = ColorToString(ref color),
                PenSize = (int)nudSize.Value,
                BrushColor = ColorToString(ref fillcolor),
                DrawType = cbBType.Text,
                Fill = cbFillDraw.Checked,

                PT1 = PT1,
                PT2 = PT2,
                X = x,
                Y = y,
                PT1X = PT1X,
                PT1Y = PT1Y,
                PT2X = PT2X,
                PT2Y = PT2Y
            };
            if (mPath != null) {
                drawPack.PPath ??= new Point[mPath.PointCount];                                     // Create a path for the package
                for (int i = 0; i < mPath.PointCount; i++) {
                    drawPack.PPath[i] = new((int)mPath.PathPoints[i].X, (int)mPath.PathPoints[i].Y);    // Add path points to package
                }
            }

            mPath.Reset();

            return drawPack;                                                                        // Return Package to caller
        }

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
        private void CmdClear_Click(object sender, EventArgs e)                                     // Clears the gfx 
        {
            G.Clear(btnBGColor.SelectedColor);
            picDrawing.Invoke((MethodInvoker)delegate {
                picDrawing.Refresh();
            });
        }
        private void CmdClearAll_Click(object sender, EventArgs e)                                  // Clears gfx and Sends Clear to Clients 
        {
            CmdClear_Click(sender, e);
            HostSendPublic("CMD:ClearDrawing");
        }

        private void Drawing_MouseDown(object sender, MouseEventArgs e)                             // Start Drawing points 
        {
            if (Drawing && e.Button == MouseButtons.Left) {
                PT1 = e.Location;
                PT1X = e.X;
                PT1Y = e.Y;
            }
        }
        private void Drawing_MouseMove(object sender, MouseEventArgs e)                             // Continue Drawing 
        {
            if (Drawing && e.Button == MouseButtons.Left) {
                PT2 = e.Location;                                                                   // Update PT2 for Mouse up event                 
                switch (cbBType.Text) {
                    case "Circle":
                    case "Fill Tool":
                    case "Line":
                    case "Rectangle":
                    case "Triangle":
                        break;
                    default:
                        mPath ??= new();                                                            // New Path if needed
                        Point mP = new(e.Location.X, e.Location.Y);
                        mPath.AddLines(new Point[] {                                                // Add points to Path
                            mP
                        });
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
        private void Drawing_MouseUp(object sender, MouseEventArgs e)                               // Draw shape on Mouse Up 
        {
            if (Drawing && e.Button == MouseButtons.Left) {
                PT2X = x - PT1X;                                                                    // Eclipse Width
                PT2Y = y - PT1Y;                                                                    // Eclipse Height
                DrawPackage localDP = PrepareDrawPackage();                                         // Create the Package 
                DrawShape(localDP);                                                                 // Draw it
                x = e.Location.X; y = e.Location.Y;
            }
        }
        private void Drawing_Paint(object sender, PaintEventArgs e)                                 // Shape Preview 
        {
            if (Drawing) {
                Graphics G = e.Graphics;
                Pen pen = new(btnColor.SelectedColor, (int)nudSize.Value);
                SolidBrush brush = new(btnFillColor.SelectedColor);
                switch (cbBType.Text) {
                    case "Circle":
                        G.DrawEllipse(pen, PT1X, PT1Y, PT2X, PT2Y);                                 // Draw
                        if (cbFillDraw.Checked) {
                            G.FillEllipse(brush, PT1X, PT1Y, PT2X, PT2Y);                           // Fill
                        }
                        break;
                    case "Fill Tool":
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
                    case "Pen w/ Close":
                        G.DrawLine(pen, PT1X, PT1Y, x, y);
                        goto default;
                    default:
                        mPath ??= new();
                        G.DrawPath(pen, mPath);
                        break;
                }
            }
        }
        private void Drawing_Resize(object sender, EventArgs e)                                     // Canvas resize 
        {
            UpdateCanvas();
        }
        private void UpdateCanvas()                                                                 // Update the PictureBox and Graphics 
        {
            Bitmap savedImage = (Bitmap)picDrawing.Image;                                           // Save the current image to a Bitmap object
            Bitmap BM = new(picDrawing.Width, picDrawing.Height);                                   // Create a new Bitmap object with the resized dimensions
            using (Graphics G = Graphics.FromImage(BM)) {                                           // Draw the saved image onto the resized Bitmap
                G.DrawImage(savedImage, new Rectangle(0, 0, picDrawing.Width, picDrawing.Height));
            }
            picDrawing.Image = BM;                                                                  // Set the resized Bitmap as the new image for the PictureBox
            G = Graphics.FromImage(BM);                                                             // Set the canvas
        }

        private void Drawing_MouseClick(object sender, MouseEventArgs e)                            // Fill Tool 
        {
            if (Drawing && cbBType.Text == "Fill Tool" && e.Button == MouseButtons.Left) {
                Color fillcolor = btnFillColor.SelectedColor;
                Point canvas = Set_Point(picDrawing, e.Location);                                   // Get location of canvas
                FillTool(BM, canvas.X, canvas.Y, fillcolor);                                        // Do the Fill

                FillPackage fillPack = new() {                                                      // Details for Broadcast
                    FillColor = ColorToString(ref fillcolor),                                       // Convert colours to strings of their ARGB
                    X = canvas.X,
                    Y = canvas.Y
                };

                if (listening) {
                    string json = JsonConvert.SerializeObject(fillPack);                            // Format the Package 
                    HostSendPublic(json);                                                           // Host Send Fill Package
                } else if (connected) {
                    string json = JsonConvert.SerializeObject(fillPack);                            // Format the Package 
                    Send(json);                                                                     // Client Send Fill Package
                }
            }
        }
        static Point Set_Point(PictureBox pb, Point pt)                                             // Calculare Fill Point on Canvas 
        {
            float pX = 1f * pb.Image.Width / pb.Width;
            float pY = 1f * pb.Image.Height / pb.Height;
            return new Point((int)(pt.X * pX), (int)(pt.Y * pY));
        }
        private void FillTool(Bitmap bm, int x, int y, Color c2)                                    // Fill Process 
        {
            Color c1 = bm.GetPixel(x, y);                                                           // Get color of clicked pixel
            Stack<Point> pixel = new();
            pixel.Push(new Point(x, y));
            bm.SetPixel(x, y, c2);                                                                  // Replaced clicked pixel color
            if (c1 == c2) return;
            while (pixel.Count > 0) {
                Point pt = (Point)pixel.Pop();
                if (pt.X > 0 && pt.Y > 0) {                                                         // check 4 pixels around XY
                    Validate(bm, pixel, pt.X - 1, pt.Y, c1, c2);
                    Validate(bm, pixel, pt.X, pt.Y - 1, c1, c2);
                    Validate(bm, pixel, pt.X + 1, pt.Y, c1, c2);
                    Validate(bm, pixel, pt.X, pt.Y + 1, c1, c2);
                }
            }
        }
        private void Validate(Bitmap bm, Stack<Point> sp, int x, int y, Color c1, Color c2)         // Fill 
        {
            if (x < bm.Width && y < bm.Height) {
                Color cx = bm.GetPixel(x, y);                                                       // Get pixel color
                if (cx == c1) {
                    sp.Push(new Point(x, y));
                    bm.SetPixel(x, y, c2);                                                          // Change pixel color
                }
            }
        }

        private void ReplaceTargetColor(Bitmap BM, Color tCol, Color rCol)                          // Replace target pixel colour 
        {
            if (tCol == Color.Empty) { tCol = Color.LightGoldenrodYellow; }                         // If Empty, replaces TrueTransP Key Color 
            if (rCol == Color.Empty) { rCol = Color.Transparent; }                                  // with Color.Transparancy

            for (int x = 0; x < BM.Width; x++) {
                for (int y = 0; y < BM.Height; y++) {                                               // Loop over image 
                    if (BM.GetPixel(x, y) == tCol) {                                                // When mathching target caolour
                        BM.SetPixel(x, y, rCol);                                                    // Change pixel to replacement colour
                    }
                }
            }
        }
        private void BGColorChange(object sender, EventArgs e)                                      // Background - Uses Replace - This happens 3 Times and I'm not sure why 
        {
            //ReplaceTargetColor(BM, _preBC, ((ColorButton)sender).SelectedColor);                  // BUG - faster but only works the second time

            for (int x = 0; x < BM.Width; x++) {                                                    // BUG Slow - Iterate over the pixels in the bitmap
                for (int y = 0; y < BM.Height; y++) {
                    if (BM.GetPixel(x, y).A == _preBC.A
                        && BM.GetPixel(x, y).R == _preBC.R
                        && BM.GetPixel(x, y).G == _preBC.G
                        && BM.GetPixel(x, y).B == _preBC.B) {                                       // Check if the current pixel has the original color                        
                        BM.SetPixel(x, y, ((ColorButton)sender).SelectedColor);                     // Replace the color of the current pixel with the replacement color
                    }
                }
            }
            _preBC = ((ColorButton)sender).SelectedColor;
            picDrawing.Image = BM;
            G = Graphics.FromImage(BM);
        }

        private void DeleteTemps(string filePath = "")                                              // Remove tmp image file 
        {
            if (filePath.Length > 0) {
                try {
                    File.Delete(filePath);                                                          // Delete specified temp file
                    Console("Temp file successfully deleted.");
                } catch (Exception ex) {
                    Console(ErrorMsg("DF1: " + ex.Message));
                }
            } else {
                string runPath = Application.StartupPath;                                           // Directory to delete from
                string[] tempFiles = Directory.GetFiles(runPath, "*.tmp");                          // Filetype to delete
                foreach (string tempFile in tempFiles) {
                    try {
                        File.Delete(tempFile);                                                      // Delete those found
                        Console("Temp file successfully deleted.");
                    } catch (Exception ex) {
                        Console(ErrorMsg("DF2: " + ex.Message));
                    }
                }
            }
        }
        private void SaveDrawing()                                                                  // Drawing Save Routine 
        {
            SaveFileDialog saveFileDialog = new() {                                                 // Prompt user for Save File
                Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp|GIF Image|*.gif"
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                string fileName = saveFileDialog.FileName;                                          // What and Where
                string extension = Path.GetExtension(fileName);
                ImageFormat imageFormat;
                switch (extension.ToLower()) {                                                      // ImageFormat based on the file extension
                    case ".jpg":
                    case ".jpeg":
                        imageFormat = ImageFormat.Jpeg;
                        break;
                    case ".png":
                        imageFormat = ImageFormat.Png;
                        break;
                    case ".bmp":
                        imageFormat = ImageFormat.Bmp;
                        break;
                    case ".gif":
                        imageFormat = ImageFormat.Gif;
                        break;
                    default:
                        Console(ErrorMsg("Invalid file format."));
                        return;
                }
                Bitmap btm = BM.Clone(new Rectangle(0, 0, picDrawing.Width, picDrawing.Height), BM.PixelFormat);
                if (imageFormat == ImageFormat.Png) { ReplaceTargetColor(btm, Color.Empty, Color.Empty); }    // TrueTransP Key colour conversion
                btm.Save(saveFileDialog.FileName, imageFormat);                                     // Save the image in the selected format                
                Console("Image Saved: " + saveFileDialog.FileName);
            }
        }
        private void DrawShape(DrawPackage drawPackage)                                             // Draw the shape from package 
        {
            picDrawing.Invoke((MethodInvoker)delegate {
                using var pen = new Pen(ArgbColor(drawPackage.PenColor), drawPackage.PenSize);      // Set Pen
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                SolidBrush brush = new(ArgbColor(drawPackage.BrushColor));                          // Set Brush

                GraphicsPath pPath = new();                                                         // Set Path
                if (drawPackage.PPath != null && drawPackage.PPath.Length > 0) {
                    for (int i = 0; i < drawPackage.PPath.Length; i++) {                            // Add each Point
                        pPath.AddLines(new PointF[] {                                               // to Draw Package
                                drawPackage.PPath[i]
                        });
                    }
                }

                switch (drawPackage.DrawType) {
                    case "Fill Tool":
                        break;
                    case "Line":
                        G.DrawLine(pen, drawPackage.PT1.X, drawPackage.PT1.Y, drawPackage.X, drawPackage.Y);
                        break;
                    case "Circle":
                        if (drawPackage.Fill) {
                            G.FillEllipse(brush, drawPackage.PT1.X, drawPackage.PT1.Y, drawPackage.PT2X, drawPackage.PT2Y);
                        }
                        G.DrawEllipse(pen, drawPackage.PT1.X, drawPackage.PT1.Y, drawPackage.PT2X, drawPackage.PT2Y);
                        break;
                    case "Rectangle":
                        var rc = new Rectangle(
                            Math.Min(drawPackage.PT1.X, drawPackage.PT2.X),
                            Math.Min(drawPackage.PT1.Y, drawPackage.PT2.Y),
                            Math.Abs(drawPackage.PT2.X - drawPackage.PT1.X),
                            Math.Abs(drawPackage.PT2.Y - drawPackage.PT1.Y));
                        if (drawPackage.Fill) {
                            G.FillRectangle(brush, rc);                                             // Fill
                        }
                        G.DrawRectangle(pen, rc);                                                   // Draw
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
                            G.FillPath(brush, tPath);                                               // Fill
                        }
                        G.DrawPath(pen, tPath);                                                     // Draw
                        break;
                    case "Pen w/ Close":
                        if (drawPackage.Fill) {
                            G.FillPath(brush, pPath);
                        }
                        G.DrawLine(pen, drawPackage.PT1X, drawPackage.PT1Y, drawPackage.X, drawPackage.Y);
                        goto default;
                    default:
                        G.DrawPath(pen, pPath);
                        break;
                }

                if (listening && !remoteDraw) {
                    string json = JsonConvert.SerializeObject(drawPackage) + "\n";                  // Format the Package 
                    HostSendPublic(json);                                                           // Host Send Draw Package
                } else if (connected && !remoteDraw) {
                    string json = JsonConvert.SerializeObject(drawPackage) + "\n";                  // Format the Package 
                    Send(json);                                                                     // Client Send Draw Package
                }
                picDrawing.Refresh();                                                               // Update the Canvas
                pPath.Reset();                                                                      // Clean up
                pen.Dispose();
                brush.Dispose();
                remoteDraw = false;
            });
        }
        #endregion
    }
}