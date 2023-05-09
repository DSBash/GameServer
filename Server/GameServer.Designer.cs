using System.Drawing;
using Unclassified.UI;

namespace Server
{
    partial class GameServer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabSections = new Server.GameServer.BorderedTabControl();
            this.tConsole = new System.Windows.Forms.TabPage();
            this.txtConsole = new System.Windows.Forms.RichTextBox();
            this.tLobby = new System.Windows.Forms.TabPage();
            this.txtLobby = new System.Windows.Forms.RichTextBox();
            this.tPM = new System.Windows.Forms.TabPage();
            this.txtPM = new System.Windows.Forms.RichTextBox();
            this.txtPort = new Server.GameServer.BorderedTextBox();
            this.txtRoomKey = new Server.GameServer.BorderedTextBox();
            this.cbMask = new System.Windows.Forms.CheckBox();
            this.txtAddress = new Server.GameServer.BorderedTextBox();
            this.lblRoomKey = new System.Windows.Forms.Label();
            this.txtName = new Server.GameServer.BorderedTextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.lblPort = new System.Windows.Forms.Label();
            this.lblAddress = new System.Windows.Forms.Label();
            this.cmdHost = new Server.GameServer.BorderedButton();
            this.cmdJoin = new Server.GameServer.BorderedButton();
            this.clientsDataGridView = new System.Windows.Forms.DataGridView();
            this.identifier = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.color = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.latency = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dc = new System.Windows.Forms.DataGridViewButtonColumn();
            this.txtMessage = new Server.GameServer.BorderedTextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.gbSettings = new Server.GameServer.BorderedGroupBox();
            this.lblColor = new System.Windows.Forms.Label();
            this.cmdColor = new Unclassified.UI.ColorButton();
            this.gbDrawings = new System.Windows.Forms.GroupBox();
            this.gbBG = new System.Windows.Forms.GroupBox();
            this.cbTrans = new System.Windows.Forms.CheckBox();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnBGColor = new Unclassified.UI.ColorButton();
            this.gbBrush = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnFillColor = new Unclassified.UI.ColorButton();
            this.cbFillDraw = new System.Windows.Forms.CheckBox();
            this.cbBType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnColor = new Unclassified.UI.ColorButton();
            this.nudSize = new System.Windows.Forms.NumericUpDown();
            this.btnSetDraw = new Server.GameServer.BorderedButton();
            this.picDrawing = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tPing = new System.Windows.Forms.Timer(this.components);
            this.tabSections.SuspendLayout();
            this.tConsole.SuspendLayout();
            this.tLobby.SuspendLayout();
            this.tPM.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.clientsDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.gbSettings.SuspendLayout();
            this.gbDrawings.SuspendLayout();
            this.gbBG.SuspendLayout();
            this.gbBrush.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDrawing)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabSections
            // 
            this.tabSections.Controls.Add(this.tConsole);
            this.tabSections.Controls.Add(this.tLobby);
            this.tabSections.Controls.Add(this.tPM);
            this.tabSections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSections.HotTrack = true;
            this.tabSections.ItemSize = new System.Drawing.Size(0, 21);
            this.tabSections.Location = new System.Drawing.Point(150, 0);
            this.tabSections.Margin = new System.Windows.Forms.Padding(0);
            this.tabSections.Name = "tabSections";
            this.tabSections.Padding = new System.Drawing.Point(0, 0);
            this.tabSections.SelectedIndex = 0;
            this.tabSections.Size = new System.Drawing.Size(576, 121);
            this.tabSections.TabIndex = 45;
            this.tabSections.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.TabSections_DrawItem);
            this.tabSections.SelectedIndexChanged += new System.EventHandler(this.TabSections_TabChanged);
            // 
            // tConsole
            // 
            this.tConsole.Controls.Add(this.txtConsole);
            this.tConsole.Location = new System.Drawing.Point(4, 25);
            this.tConsole.Name = "tConsole";
            this.tConsole.Size = new System.Drawing.Size(568, 92);
            this.tConsole.TabIndex = 0;
            this.tConsole.Tag = "Console";
            this.tConsole.Text = "Console";
            this.tConsole.UseVisualStyleBackColor = true;
            // 
            // txtConsole
            // 
            this.txtConsole.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtConsole.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtConsole.Location = new System.Drawing.Point(0, 0);
            this.txtConsole.Margin = new System.Windows.Forms.Padding(0);
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.ReadOnly = true;
            this.txtConsole.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtConsole.Size = new System.Drawing.Size(568, 92);
            this.txtConsole.TabIndex = 25;
            this.txtConsole.TabStop = false;
            this.txtConsole.Text = "";
            // 
            // tLobby
            // 
            this.tLobby.Controls.Add(this.txtLobby);
            this.tLobby.Location = new System.Drawing.Point(4, 25);
            this.tLobby.Name = "tLobby";
            this.tLobby.Size = new System.Drawing.Size(568, 92);
            this.tLobby.TabIndex = 1;
            this.tLobby.Tag = "Lobby";
            this.tLobby.Text = "Lobby";
            this.tLobby.UseVisualStyleBackColor = true;
            // 
            // txtLobby
            // 
            this.txtLobby.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtLobby.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLobby.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLobby.Location = new System.Drawing.Point(0, 0);
            this.txtLobby.Name = "txtLobby";
            this.txtLobby.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtLobby.Size = new System.Drawing.Size(568, 92);
            this.txtLobby.TabIndex = 0;
            this.txtLobby.Text = "";
            // 
            // tPM
            // 
            this.tPM.Controls.Add(this.txtPM);
            this.tPM.Location = new System.Drawing.Point(4, 25);
            this.tPM.Margin = new System.Windows.Forms.Padding(0);
            this.tPM.Name = "tPM";
            this.tPM.Size = new System.Drawing.Size(568, 92);
            this.tPM.TabIndex = 2;
            this.tPM.Tag = "PM";
            this.tPM.Text = "#";
            this.tPM.UseVisualStyleBackColor = true;
            // 
            // txtPM
            // 
            this.txtPM.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtPM.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPM.Location = new System.Drawing.Point(0, 0);
            this.txtPM.Name = "txtPM";
            this.txtPM.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtPM.Size = new System.Drawing.Size(568, 92);
            this.txtPM.TabIndex = 1;
            this.txtPM.Tag = "PM";
            this.txtPM.Text = "";
            // 
            // txtPort
            // 
            this.txtPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPort.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtPort.Location = new System.Drawing.Point(5, 180);
            this.txtPort.Margin = new System.Windows.Forms.Padding(0);
            this.txtPort.MaxLength = 10;
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(132, 20);
            this.txtPort.TabIndex = 46;
            this.txtPort.TabStop = false;
            this.txtPort.Text = "9000";
            this.txtPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtRoomKey
            // 
            this.txtRoomKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRoomKey.Location = new System.Drawing.Point(5, 65);
            this.txtRoomKey.Margin = new System.Windows.Forms.Padding(0);
            this.txtRoomKey.MaxLength = 200;
            this.txtRoomKey.Name = "txtRoomKey";
            this.txtRoomKey.Size = new System.Drawing.Size(75, 20);
            this.txtRoomKey.TabIndex = 52;
            this.txtRoomKey.TabStop = false;
            this.txtRoomKey.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cbMask
            // 
            this.cbMask.BackColor = System.Drawing.Color.Transparent;
            this.cbMask.Checked = true;
            this.cbMask.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbMask.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbMask.Location = new System.Drawing.Point(85, 65);
            this.cbMask.Margin = new System.Windows.Forms.Padding(0);
            this.cbMask.Name = "cbMask";
            this.cbMask.Size = new System.Drawing.Size(53, 20);
            this.cbMask.TabIndex = 54;
            this.cbMask.Text = "Show";
            this.cbMask.UseVisualStyleBackColor = false;
            this.cbMask.CheckedChanged += new System.EventHandler(this.CbMask_CheckedChanged);
            // 
            // txtAddress
            // 
            this.txtAddress.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAddress.Location = new System.Drawing.Point(5, 140);
            this.txtAddress.Margin = new System.Windows.Forms.Padding(0);
            this.txtAddress.MaxLength = 200;
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new System.Drawing.Size(130, 20);
            this.txtAddress.TabIndex = 53;
            this.txtAddress.TabStop = false;
            this.txtAddress.Text = "192.168.0.81";
            this.txtAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblRoomKey
            // 
            this.lblRoomKey.AutoSize = true;
            this.lblRoomKey.BackColor = System.Drawing.Color.Transparent;
            this.lblRoomKey.Location = new System.Drawing.Point(5, 50);
            this.lblRoomKey.Margin = new System.Windows.Forms.Padding(0);
            this.lblRoomKey.Name = "lblRoomKey";
            this.lblRoomKey.Size = new System.Drawing.Size(59, 13);
            this.lblRoomKey.TabIndex = 51;
            this.lblRoomKey.Text = "Room Key:";
            // 
            // txtName
            // 
            this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtName.Location = new System.Drawing.Point(5, 25);
            this.txtName.Margin = new System.Windows.Forms.Padding(0);
            this.txtName.MaxLength = 50;
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(100, 20);
            this.txtName.TabIndex = 50;
            this.txtName.TabStop = false;
            this.txtName.Text = "Player 1";
            this.txtName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.BackColor = System.Drawing.Color.Transparent;
            this.lblName.Location = new System.Drawing.Point(5, 10);
            this.lblName.Margin = new System.Windows.Forms.Padding(0);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(38, 13);
            this.lblName.TabIndex = 49;
            this.lblName.Text = "Name:";
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.BackColor = System.Drawing.Color.Transparent;
            this.lblPort.Location = new System.Drawing.Point(5, 165);
            this.lblPort.Margin = new System.Windows.Forms.Padding(0);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(29, 13);
            this.lblPort.TabIndex = 48;
            this.lblPort.Text = "Port:";
            // 
            // lblAddress
            // 
            this.lblAddress.AutoSize = true;
            this.lblAddress.BackColor = System.Drawing.Color.Transparent;
            this.lblAddress.Location = new System.Drawing.Point(5, 125);
            this.lblAddress.Margin = new System.Windows.Forms.Padding(0);
            this.lblAddress.Name = "lblAddress";
            this.lblAddress.Size = new System.Drawing.Size(48, 13);
            this.lblAddress.TabIndex = 47;
            this.lblAddress.Text = "Address:";
            // 
            // cmdHost
            // 
            this.cmdHost.Location = new System.Drawing.Point(71, 95);
            this.cmdHost.Margin = new System.Windows.Forms.Padding(0);
            this.cmdHost.Name = "cmdHost";
            this.cmdHost.Size = new System.Drawing.Size(70, 23);
            this.cmdHost.TabIndex = 44;
            this.cmdHost.TabStop = false;
            this.cmdHost.Text = "Host";
            this.cmdHost.UseVisualStyleBackColor = true;
            this.cmdHost.Click += new System.EventHandler(this.StartHosting);
            // 
            // cmdJoin
            // 
            this.cmdJoin.Location = new System.Drawing.Point(4, 95);
            this.cmdJoin.Margin = new System.Windows.Forms.Padding(0);
            this.cmdJoin.Name = "cmdJoin";
            this.cmdJoin.Size = new System.Drawing.Size(70, 23);
            this.cmdJoin.TabIndex = 45;
            this.cmdJoin.TabStop = false;
            this.cmdJoin.Text = "Join";
            this.cmdJoin.UseVisualStyleBackColor = true;
            this.cmdJoin.Click += new System.EventHandler(this.StartClienting);
            // 
            // clientsDataGridView
            // 
            this.clientsDataGridView.AllowUserToAddRows = false;
            this.clientsDataGridView.AllowUserToDeleteRows = false;
            this.clientsDataGridView.AllowUserToResizeColumns = false;
            this.clientsDataGridView.AllowUserToResizeRows = false;
            this.clientsDataGridView.BackgroundColor = System.Drawing.SystemColors.ScrollBar;
            this.clientsDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.clientsDataGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this.clientsDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.clientsDataGridView.ColumnHeadersHeight = 24;
            this.clientsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.clientsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.identifier,
            this.name,
            this.color,
            this.latency,
            this.dc});
            this.clientsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clientsDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.clientsDataGridView.EnableHeadersVisualStyles = false;
            this.clientsDataGridView.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.clientsDataGridView.Location = new System.Drawing.Point(0, 0);
            this.clientsDataGridView.Margin = new System.Windows.Forms.Padding(0);
            this.clientsDataGridView.MinimumSize = new System.Drawing.Size(75, 0);
            this.clientsDataGridView.MultiSelect = false;
            this.clientsDataGridView.Name = "clientsDataGridView";
            this.clientsDataGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.clientsDataGridView.RowHeadersVisible = false;
            this.clientsDataGridView.RowHeadersWidth = 40;
            this.clientsDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.clientsDataGridView.RowTemplate.Height = 24;
            this.clientsDataGridView.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.clientsDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.clientsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.clientsDataGridView.ShowCellErrors = false;
            this.clientsDataGridView.ShowCellToolTips = false;
            this.clientsDataGridView.ShowEditingIcon = false;
            this.clientsDataGridView.ShowRowErrors = false;
            this.clientsDataGridView.Size = new System.Drawing.Size(150, 121);
            this.clientsDataGridView.TabIndex = 33;
            this.clientsDataGridView.TabStop = false;
            this.clientsDataGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.CDGV_CellClick);
            // 
            // identifier
            // 
            this.identifier.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.Transparent;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            this.identifier.DefaultCellStyle = dataGridViewCellStyle1;
            this.identifier.HeaderText = "#";
            this.identifier.MinimumWidth = 28;
            this.identifier.Name = "identifier";
            this.identifier.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.identifier.Visible = false;
            this.identifier.Width = 28;
            // 
            // name
            // 
            this.name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Transparent;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            this.name.DefaultCellStyle = dataGridViewCellStyle2;
            this.name.HeaderText = "Name";
            this.name.MaxInputLength = 20;
            this.name.MinimumWidth = 50;
            this.name.Name = "name";
            this.name.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // color
            // 
            this.color.HeaderText = "";
            this.color.Name = "color";
            this.color.Visible = false;
            // 
            // latency
            // 
            this.latency.HeaderText = "Ping";
            this.latency.MinimumWidth = 28;
            this.latency.Name = "latency";
            this.latency.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.latency.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.latency.Visible = false;
            this.latency.Width = 28;
            // 
            // dc
            // 
            this.dc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dc.HeaderText = "DC";
            this.dc.MinimumWidth = 28;
            this.dc.Name = "dc";
            this.dc.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dc.Text = "DC";
            this.dc.Visible = false;
            this.dc.Width = 28;
            // 
            // txtMessage
            // 
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessage.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMessage.Location = new System.Drawing.Point(0, 121);
            this.txtMessage.Margin = new System.Windows.Forms.Padding(0);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(726, 26);
            this.txtMessage.TabIndex = 27;
            this.txtMessage.TabStop = false;
            this.txtMessage.Text = "Type and press enter to send.";
            this.txtMessage.Enter += new System.EventHandler(this.TxtMessage_Enter);
            this.txtMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtMessage_KeyDown);
            this.txtMessage.Leave += new System.EventHandler(this.TxtMessage_Leave);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel2);
            this.splitContainer1.Size = new System.Drawing.Size(726, 569);
            this.splitContainer1.SplitterDistance = 418;
            this.splitContainer1.TabIndex = 47;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.splitContainer2, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.picDrawing, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(726, 418);
            this.tableLayoutPanel3.TabIndex = 45;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.gbSettings);
            this.splitContainer2.Panel1.Controls.Add(this.gbDrawings);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.btnSetDraw);
            this.splitContainer2.Size = new System.Drawing.Size(144, 412);
            this.splitContainer2.SplitterDistance = 378;
            this.splitContainer2.TabIndex = 32;
            // 
            // gbSettings
            // 
            this.gbSettings.Controls.Add(this.cmdHost);
            this.gbSettings.Controls.Add(this.lblColor);
            this.gbSettings.Controls.Add(this.cmdColor);
            this.gbSettings.Controls.Add(this.txtPort);
            this.gbSettings.Controls.Add(this.lblName);
            this.gbSettings.Controls.Add(this.txtRoomKey);
            this.gbSettings.Controls.Add(this.lblRoomKey);
            this.gbSettings.Controls.Add(this.txtAddress);
            this.gbSettings.Controls.Add(this.cmdJoin);
            this.gbSettings.Controls.Add(this.txtName);
            this.gbSettings.Controls.Add(this.lblPort);
            this.gbSettings.Controls.Add(this.cbMask);
            this.gbSettings.Controls.Add(this.lblAddress);
            this.gbSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbSettings.Location = new System.Drawing.Point(0, 0);
            this.gbSettings.Margin = new System.Windows.Forms.Padding(0);
            this.gbSettings.Name = "gbSettings";
            this.gbSettings.Padding = new System.Windows.Forms.Padding(0);
            this.gbSettings.Size = new System.Drawing.Size(144, 378);
            this.gbSettings.TabIndex = 0;
            this.gbSettings.TabStop = false;
            // 
            // lblColor
            // 
            this.lblColor.AutoSize = true;
            this.lblColor.BackColor = System.Drawing.Color.Transparent;
            this.lblColor.Location = new System.Drawing.Point(107, 10);
            this.lblColor.Margin = new System.Windows.Forms.Padding(0);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(34, 13);
            this.lblColor.TabIndex = 57;
            this.lblColor.Text = "Color:";
            this.lblColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmdColor
            // 
            this.cmdColor.BackColor = System.Drawing.Color.Yellow;
            this.cmdColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdColor.Location = new System.Drawing.Point(110, 25);
            this.cmdColor.Margin = new System.Windows.Forms.Padding(0);
            this.cmdColor.Name = "cmdColor";
            this.cmdColor.SelectedColor = System.Drawing.Color.Yellow;
            this.cmdColor.Size = new System.Drawing.Size(25, 20);
            this.cmdColor.TabIndex = 56;
            this.cmdColor.UseVisualStyleBackColor = false;
            // 
            // gbDrawings
            // 
            this.gbDrawings.Controls.Add(this.gbBG);
            this.gbDrawings.Controls.Add(this.gbBrush);
            this.gbDrawings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbDrawings.Location = new System.Drawing.Point(0, 0);
            this.gbDrawings.Margin = new System.Windows.Forms.Padding(0);
            this.gbDrawings.Name = "gbDrawings";
            this.gbDrawings.Padding = new System.Windows.Forms.Padding(0);
            this.gbDrawings.Size = new System.Drawing.Size(144, 378);
            this.gbDrawings.TabIndex = 0;
            this.gbDrawings.TabStop = false;
            // 
            // gbBG
            // 
            this.gbBG.Controls.Add(this.cbTrans);
            this.gbBG.Controls.Add(this.btnClearAll);
            this.gbBG.Controls.Add(this.label4);
            this.gbBG.Controls.Add(this.btnClear);
            this.gbBG.Controls.Add(this.btnBGColor);
            this.gbBG.Location = new System.Drawing.Point(1, 105);
            this.gbBG.Margin = new System.Windows.Forms.Padding(0);
            this.gbBG.Name = "gbBG";
            this.gbBG.Size = new System.Drawing.Size(143, 76);
            this.gbBG.TabIndex = 6;
            this.gbBG.TabStop = false;
            this.gbBG.Text = "Background";
            // 
            // cbTrans
            // 
            this.cbTrans.AutoSize = true;
            this.cbTrans.BackColor = System.Drawing.Color.Transparent;
            this.cbTrans.Location = new System.Drawing.Point(90, 19);
            this.cbTrans.Margin = new System.Windows.Forms.Padding(0);
            this.cbTrans.Name = "cbTrans";
            this.cbTrans.Size = new System.Drawing.Size(53, 17);
            this.cbTrans.TabIndex = 4;
            this.cbTrans.Text = "Trans";
            this.cbTrans.UseVisualStyleBackColor = false;
            this.cbTrans.CheckedChanged += new System.EventHandler(this.TransToggle);
            // 
            // btnClearAll
            // 
            this.btnClearAll.Location = new System.Drawing.Point(70, 40);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(68, 25);
            this.btnClearAll.TabIndex = 3;
            this.btnClearAll.Text = "Clear All";
            this.btnClearAll.UseVisualStyleBackColor = true;
            this.btnClearAll.Visible = false;
            this.btnClearAll.Click += new System.EventHandler(this.CmdClearAll_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(50, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Colour";
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(4, 40);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(67, 25);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Clear Own";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.CmdClear_Click);
            // 
            // btnBGColor
            // 
            this.btnBGColor.BackColor = System.Drawing.Color.Linen;
            this.btnBGColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBGColor.Location = new System.Drawing.Point(5, 16);
            this.btnBGColor.Margin = new System.Windows.Forms.Padding(0);
            this.btnBGColor.Name = "btnBGColor";
            this.btnBGColor.SelectedColor = System.Drawing.Color.Linen;
            this.btnBGColor.Size = new System.Drawing.Size(42, 20);
            this.btnBGColor.TabIndex = 0;
            this.btnBGColor.UseVisualStyleBackColor = false;
            this.btnBGColor.SelectedColorChanged += new System.EventHandler(this.BGColorChange);
            // 
            // gbBrush
            // 
            this.gbBrush.Controls.Add(this.label3);
            this.gbBrush.Controls.Add(this.btnFillColor);
            this.gbBrush.Controls.Add(this.cbFillDraw);
            this.gbBrush.Controls.Add(this.cbBType);
            this.gbBrush.Controls.Add(this.label2);
            this.gbBrush.Controls.Add(this.label1);
            this.gbBrush.Controls.Add(this.btnColor);
            this.gbBrush.Controls.Add(this.nudSize);
            this.gbBrush.Location = new System.Drawing.Point(1, 0);
            this.gbBrush.Margin = new System.Windows.Forms.Padding(0);
            this.gbBrush.Name = "gbBrush";
            this.gbBrush.Size = new System.Drawing.Size(143, 100);
            this.gbBrush.TabIndex = 5;
            this.gbBrush.TabStop = false;
            this.gbBrush.Text = "Brush";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(88, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Fill Colour";
            // 
            // btnFillColor
            // 
            this.btnFillColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnFillColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFillColor.Location = new System.Drawing.Point(58, 72);
            this.btnFillColor.Margin = new System.Windows.Forms.Padding(0);
            this.btnFillColor.Name = "btnFillColor";
            this.btnFillColor.SelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnFillColor.Size = new System.Drawing.Size(28, 20);
            this.btnFillColor.TabIndex = 6;
            this.btnFillColor.UseVisualStyleBackColor = false;
            // 
            // cbFillDraw
            // 
            this.cbFillDraw.AutoSize = true;
            this.cbFillDraw.Location = new System.Drawing.Point(7, 75);
            this.cbFillDraw.Name = "cbFillDraw";
            this.cbFillDraw.Size = new System.Drawing.Size(38, 17);
            this.cbFillDraw.TabIndex = 5;
            this.cbFillDraw.Text = "Fill";
            this.cbFillDraw.UseVisualStyleBackColor = true;
            this.cbFillDraw.CheckedChanged += new System.EventHandler(this.FillToggle);
            // 
            // cbBType
            // 
            this.cbBType.FormattingEnabled = true;
            this.cbBType.Items.AddRange(new object[] {
            "Line",
            "Circle",
            "Fill Tool",
            "Pen",
            "Pen w/ Close",
            "Rectangle",
            "Triangle"});
            this.cbBType.Location = new System.Drawing.Point(7, 45);
            this.cbBType.Margin = new System.Windows.Forms.Padding(0);
            this.cbBType.Name = "cbBType";
            this.cbBType.Size = new System.Drawing.Size(129, 21);
            this.cbBType.TabIndex = 4;
            this.cbBType.Text = "Shape / Style";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(115, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Size";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Colour";
            // 
            // btnColor
            // 
            this.btnColor.BackColor = System.Drawing.Color.Green;
            this.btnColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnColor.Location = new System.Drawing.Point(5, 15);
            this.btnColor.Margin = new System.Windows.Forms.Padding(0);
            this.btnColor.Name = "btnColor";
            this.btnColor.SelectedColor = System.Drawing.Color.Green;
            this.btnColor.Size = new System.Drawing.Size(28, 20);
            this.btnColor.TabIndex = 0;
            this.btnColor.UseVisualStyleBackColor = false;
            // 
            // nudSize
            // 
            this.nudSize.Location = new System.Drawing.Point(75, 15);
            this.nudSize.Margin = new System.Windows.Forms.Padding(0);
            this.nudSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSize.Name = "nudSize";
            this.nudSize.Size = new System.Drawing.Size(40, 20);
            this.nudSize.TabIndex = 1;
            this.nudSize.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // btnSetDraw
            // 
            this.btnSetDraw.Location = new System.Drawing.Point(3, 3);
            this.btnSetDraw.Name = "btnSetDraw";
            this.btnSetDraw.Size = new System.Drawing.Size(138, 23);
            this.btnSetDraw.TabIndex = 0;
            this.btnSetDraw.Text = "Drawings";
            this.btnSetDraw.UseVisualStyleBackColor = true;
            this.btnSetDraw.Click += new System.EventHandler(this.SetDraw_Click);
            // 
            // picDrawing
            // 
            this.picDrawing.BackColor = System.Drawing.Color.Linen;
            this.picDrawing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picDrawing.Location = new System.Drawing.Point(150, 0);
            this.picDrawing.Margin = new System.Windows.Forms.Padding(0);
            this.picDrawing.Name = "picDrawing";
            this.picDrawing.Size = new System.Drawing.Size(576, 418);
            this.picDrawing.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picDrawing.TabIndex = 0;
            this.picDrawing.TabStop = false;
            this.picDrawing.Visible = false;
            this.picDrawing.Paint += new System.Windows.Forms.PaintEventHandler(this.Drawing_Paint);
            this.picDrawing.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Drawing_MouseClick);
            this.picDrawing.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Drawing_MouseDown);
            this.picDrawing.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Drawing_MouseMove);
            this.picDrawing.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Drawing_MouseUp);
            this.picDrawing.Resize += new System.EventHandler(this.Drawing_Resize);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.txtMessage, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(726, 147);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tabSections, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.clientsDataGridView, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(726, 121);
            this.tableLayoutPanel1.TabIndex = 28;
            // 
            // tPing
            // 
            this.tPing.Interval = 5000;
            this.tPing.Tick += new System.EventHandler(this.Ping_Tick);
            // 
            // GameServer
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(726, 569);
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(500, 400);
            this.Name = "GameServer";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GameServer_FormClosing);
            this.Load += new System.EventHandler(this.GameServer_Load);
            this.tabSections.ResumeLayout(false);
            this.tConsole.ResumeLayout(false);
            this.tLobby.ResumeLayout(false);
            this.tPM.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.clientsDataGridView)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.gbSettings.ResumeLayout(false);
            this.gbSettings.PerformLayout();
            this.gbDrawings.ResumeLayout(false);
            this.gbBG.ResumeLayout(false);
            this.gbBG.PerformLayout();
            this.gbBrush.ResumeLayout(false);
            this.gbBrush.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDrawing)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabPage tConsole;
        private System.Windows.Forms.TabPage tLobby;
        private System.Windows.Forms.RichTextBox txtConsole;
        private BorderedTextBox txtMessage;
        private System.Windows.Forms.DataGridView clientsDataGridView;
        private BorderedTextBox txtPort;
        private BorderedTextBox txtRoomKey;
        private System.Windows.Forms.CheckBox cbMask;
        private BorderedTextBox txtAddress;
        private System.Windows.Forms.Label lblRoomKey;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblAddress;
        private BorderedButton cmdHost;
        private BorderedButton cmdJoin;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private BorderedGroupBox gbSettings;
        private System.Windows.Forms.GroupBox gbDrawings;
        private System.Windows.Forms.RichTextBox txtLobby;
        private System.Windows.Forms.Timer tPing;
        private System.Windows.Forms.Label lblColor;
        private ColorButton cmdColor;
        private BorderedButton btnSetDraw;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.NumericUpDown nudSize;
        private ColorButton btnColor;
        private System.Windows.Forms.GroupBox gbBG;
        private System.Windows.Forms.Label label4;
        private ColorButton btnBGColor;
        private System.Windows.Forms.GroupBox gbBrush;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.ComboBox cbBType;
        private System.Windows.Forms.Label label3;
        private ColorButton btnFillColor;
        private System.Windows.Forms.CheckBox cbFillDraw;
        private System.Windows.Forms.PictureBox picDrawing;
        private System.Windows.Forms.CheckBox cbTrans;
        private System.Windows.Forms.TabPage tPM;
        private System.Windows.Forms.DataGridViewTextBoxColumn identifier;
        private System.Windows.Forms.DataGridViewTextBoxColumn name;
        private System.Windows.Forms.DataGridViewTextBoxColumn color;
        private System.Windows.Forms.DataGridViewTextBoxColumn latency;
        private System.Windows.Forms.DataGridViewButtonColumn dc;
        private System.Windows.Forms.RichTextBox txtPM;
        private BorderedTextBox txtName;
        private BorderedTabControl tabSections;
    }
}

