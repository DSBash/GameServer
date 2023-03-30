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
            this.cmdClear = new System.Windows.Forms.Button();
            this.lblConnections = new System.Windows.Forms.Label();
            this.tabSections = new System.Windows.Forms.TabControl();
            this.tConsole = new System.Windows.Forms.TabPage();
            this.txtConsole = new System.Windows.Forms.TextBox();
            this.tLobby = new System.Windows.Forms.TabPage();
            this.txtLobby = new System.Windows.Forms.RichTextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtRoomKey = new System.Windows.Forms.TextBox();
            this.cbMask = new System.Windows.Forms.CheckBox();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.lblRoomKey = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.lblPort = new System.Windows.Forms.Label();
            this.lblAddress = new System.Windows.Forms.Label();
            this.cmdHost = new System.Windows.Forms.Button();
            this.cmdJoin = new System.Windows.Forms.Button();
            this.clientsDataGridView = new System.Windows.Forms.DataGridView();
            this.identifier = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.color = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.latency = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dc = new System.Windows.Forms.DataGridViewButtonColumn();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.cmdDisconnect = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.gbSettings = new System.Windows.Forms.GroupBox();
            this.lblColor = new System.Windows.Forms.Label();
            this.cmdColor = new System.Windows.Forms.Button();
            this.cmdPing = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tPing = new System.Windows.Forms.Timer(this.components);
            this.tabSections.SuspendLayout();
            this.tConsole.SuspendLayout();
            this.tLobby.SuspendLayout();
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
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdClear
            // 
            this.cmdClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdClear.FlatAppearance.BorderSize = 0;
            this.cmdClear.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmdClear.Font = new System.Drawing.Font("Times New Roman", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdClear.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdClear.Location = new System.Drawing.Point(1073, 83);
            this.cmdClear.Margin = new System.Windows.Forms.Padding(0);
            this.cmdClear.Name = "cmdClear";
            this.cmdClear.Size = new System.Drawing.Size(40, 20);
            this.cmdClear.TabIndex = 25;
            this.cmdClear.TabStop = false;
            this.cmdClear.Text = "Clear";
            this.cmdClear.UseVisualStyleBackColor = true;
            this.cmdClear.Click += new System.EventHandler(this.CmdClear_Click);
            // 
            // lblConnections
            // 
            this.lblConnections.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblConnections.BackColor = System.Drawing.Color.Transparent;
            this.lblConnections.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblConnections.Location = new System.Drawing.Point(1, 19);
            this.lblConnections.Margin = new System.Windows.Forms.Padding(0);
            this.lblConnections.Name = "lblConnections";
            this.lblConnections.Size = new System.Drawing.Size(90, 13);
            this.lblConnections.TabIndex = 31;
            this.lblConnections.Text = "Total players: 0";
            this.lblConnections.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabSections
            // 
            this.tabSections.Controls.Add(this.tConsole);
            this.tabSections.Controls.Add(this.tLobby);
            this.tabSections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSections.HotTrack = true;
            this.tabSections.Location = new System.Drawing.Point(0, 0);
            this.tabSections.Margin = new System.Windows.Forms.Padding(0);
            this.tabSections.Name = "tabSections";
            this.tabSections.Padding = new System.Drawing.Point(0, 0);
            this.tabSections.SelectedIndex = 0;
            this.tabSections.Size = new System.Drawing.Size(1073, 103);
            this.tabSections.TabIndex = 45;
            // 
            // tConsole
            // 
            this.tConsole.Controls.Add(this.txtConsole);
            this.tConsole.Location = new System.Drawing.Point(4, 22);
            this.tConsole.Name = "tConsole";
            this.tConsole.Size = new System.Drawing.Size(1065, 77);
            this.tConsole.TabIndex = 0;
            this.tConsole.Text = "Console";
            this.tConsole.UseVisualStyleBackColor = true;
            // 
            // txtConsole
            // 
            this.txtConsole.BackColor = System.Drawing.SystemColors.Window;
            this.txtConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtConsole.Location = new System.Drawing.Point(0, 0);
            this.txtConsole.Margin = new System.Windows.Forms.Padding(0);
            this.txtConsole.Multiline = true;
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.ReadOnly = true;
            this.txtConsole.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtConsole.Size = new System.Drawing.Size(1065, 77);
            this.txtConsole.TabIndex = 25;
            this.txtConsole.TabStop = false;
            this.txtConsole.Text = "SYSTEM: An example of system text.";
            // 
            // tLobby
            // 
            this.tLobby.Controls.Add(this.txtLobby);
            this.tLobby.Location = new System.Drawing.Point(4, 22);
            this.tLobby.Name = "tLobby";
            this.tLobby.Size = new System.Drawing.Size(1065, 77);
            this.tLobby.TabIndex = 1;
            this.tLobby.Text = "Lobby";
            this.tLobby.UseVisualStyleBackColor = true;
            // 
            // txtLobby
            // 
            this.txtLobby.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLobby.Location = new System.Drawing.Point(0, 0);
            this.txtLobby.Name = "txtLobby";
            this.txtLobby.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.txtLobby.Size = new System.Drawing.Size(1065, 77);
            this.txtLobby.TabIndex = 0;
            this.txtLobby.Text = "";
            // 
            // txtPort
            // 
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
            this.txtAddress.Location = new System.Drawing.Point(5, 140);
            this.txtAddress.Margin = new System.Windows.Forms.Padding(0);
            this.txtAddress.MaxLength = 200;
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new System.Drawing.Size(132, 20);
            this.txtAddress.TabIndex = 53;
            this.txtAddress.TabStop = false;
            this.txtAddress.Text = "127.0.0.1";
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
            this.cmdHost.Location = new System.Drawing.Point(70, 95);
            this.cmdHost.Margin = new System.Windows.Forms.Padding(0);
            this.cmdHost.Name = "cmdHost";
            this.cmdHost.Size = new System.Drawing.Size(70, 23);
            this.cmdHost.TabIndex = 44;
            this.cmdHost.TabStop = false;
            this.cmdHost.Text = "Host";
            this.cmdHost.UseVisualStyleBackColor = true;
            this.cmdHost.Click += new System.EventHandler(this.CmdHost_Click);
            // 
            // cmdJoin
            // 
            this.cmdJoin.Location = new System.Drawing.Point(5, 95);
            this.cmdJoin.Margin = new System.Windows.Forms.Padding(0);
            this.cmdJoin.Name = "cmdJoin";
            this.cmdJoin.Size = new System.Drawing.Size(70, 23);
            this.cmdJoin.TabIndex = 45;
            this.cmdJoin.TabStop = false;
            this.cmdJoin.Text = "Join";
            this.cmdJoin.UseVisualStyleBackColor = true;
            this.cmdJoin.Click += new System.EventHandler(this.CmdJoin_Click);
            // 
            // clientsDataGridView
            // 
            this.clientsDataGridView.AllowUserToAddRows = false;
            this.clientsDataGridView.AllowUserToDeleteRows = false;
            this.clientsDataGridView.AllowUserToResizeColumns = false;
            this.clientsDataGridView.AllowUserToResizeRows = false;
            this.clientsDataGridView.BackgroundColor = System.Drawing.SystemColors.Window;
            this.clientsDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
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
            this.clientsDataGridView.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.clientsDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.clientsDataGridView.EnableHeadersVisualStyles = false;
            this.clientsDataGridView.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.clientsDataGridView.Location = new System.Drawing.Point(0, 31);
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
            this.clientsDataGridView.Size = new System.Drawing.Size(144, 122);
            this.clientsDataGridView.TabIndex = 33;
            this.clientsDataGridView.TabStop = false;
            this.clientsDataGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ClientsDataGridView_CellClick);
            // 
            // identifier
            // 
            this.identifier.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.identifier.HeaderText = "#";
            this.identifier.MaxInputLength = 20;
            this.identifier.MinimumWidth = 20;
            this.identifier.Name = "identifier";
            this.identifier.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.identifier.Width = 20;
            // 
            // name
            // 
            this.name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
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
            // 
            // latency
            // 
            this.latency.HeaderText = "Ping";
            this.latency.MinimumWidth = 20;
            this.latency.Name = "latency";
            this.latency.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.latency.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.latency.Width = 20;
            // 
            // dc
            // 
            this.dc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dc.HeaderText = "DC";
            this.dc.MinimumWidth = 28;
            this.dc.Name = "dc";
            this.dc.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dc.Text = "DC";
            this.dc.UseColumnTextForButtonValue = true;
            this.dc.Width = 28;
            // 
            // txtMessage
            // 
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessage.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMessage.Location = new System.Drawing.Point(0, 103);
            this.txtMessage.Margin = new System.Windows.Forms.Padding(0);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(1113, 26);
            this.txtMessage.TabIndex = 27;
            this.txtMessage.TabStop = false;
            this.txtMessage.Text = "Type and press enter to send.";
            this.txtMessage.Enter += new System.EventHandler(this.TxtMessage_Enter);
            this.txtMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtMessage_KeyDown);
            this.txtMessage.Leave += new System.EventHandler(this.TxtMessage_Leave);
            // 
            // cmdDisconnect
            // 
            this.cmdDisconnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdDisconnect.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmdDisconnect.Font = new System.Drawing.Font("Times New Roman", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdDisconnect.Location = new System.Drawing.Point(0, 0);
            this.cmdDisconnect.Margin = new System.Windows.Forms.Padding(0);
            this.cmdDisconnect.Name = "cmdDisconnect";
            this.cmdDisconnect.Size = new System.Drawing.Size(80, 18);
            this.cmdDisconnect.TabIndex = 26;
            this.cmdDisconnect.TabStop = false;
            this.cmdDisconnect.Text = "Disconnect All";
            this.cmdDisconnect.UseVisualStyleBackColor = true;
            this.cmdDisconnect.Click += new System.EventHandler(this.CmdDisconnect_Click);
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
            this.splitContainer1.Size = new System.Drawing.Size(1113, 647);
            this.splitContainer1.SplitterDistance = 514;
            this.splitContainer1.TabIndex = 47;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.splitContainer2, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1113, 514);
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
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.cmdPing);
            this.splitContainer2.Panel2.Controls.Add(this.cmdDisconnect);
            this.splitContainer2.Panel2.Controls.Add(this.lblConnections);
            this.splitContainer2.Panel2.Controls.Add(this.clientsDataGridView);
            this.splitContainer2.Size = new System.Drawing.Size(144, 508);
            this.splitContainer2.SplitterDistance = 351;
            this.splitContainer2.TabIndex = 32;
            // 
            // gbSettings
            // 
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
            this.gbSettings.Controls.Add(this.cmdHost);
            this.gbSettings.Controls.Add(this.cbMask);
            this.gbSettings.Controls.Add(this.lblAddress);
            this.gbSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbSettings.Location = new System.Drawing.Point(0, 0);
            this.gbSettings.Margin = new System.Windows.Forms.Padding(0);
            this.gbSettings.Name = "gbSettings";
            this.gbSettings.Padding = new System.Windows.Forms.Padding(0);
            this.gbSettings.Size = new System.Drawing.Size(144, 351);
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
            this.cmdColor.BackColor = System.Drawing.Color.Red;
            this.cmdColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdColor.Location = new System.Drawing.Point(110, 25);
            this.cmdColor.Margin = new System.Windows.Forms.Padding(0);
            this.cmdColor.Name = "cmdColor";
            this.cmdColor.Size = new System.Drawing.Size(25, 20);
            this.cmdColor.TabIndex = 56;
            this.cmdColor.UseVisualStyleBackColor = false;
            this.cmdColor.Click += new System.EventHandler(this.CmdColor_Click);
            // 
            // cmdPing
            // 
            this.cmdPing.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmdPing.Font = new System.Drawing.Font("Times New Roman", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdPing.Location = new System.Drawing.Point(84, 0);
            this.cmdPing.Name = "cmdPing";
            this.cmdPing.Size = new System.Drawing.Size(56, 18);
            this.cmdPing.TabIndex = 56;
            this.cmdPing.Text = "Ping All";
            this.cmdPing.UseVisualStyleBackColor = true;
            this.cmdPing.Click += new System.EventHandler(this.CmdPing_Click);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(153, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(957, 508);
            this.panel1.TabIndex = 33;
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1113, 129);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.Controls.Add(this.tabSections, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmdClear, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1113, 103);
            this.tableLayoutPanel1.TabIndex = 28;
            // 
            // tPing
            // 
            this.tPing.Interval = 1000;
            this.tPing.Tick += new System.EventHandler(this.Ping_Tick);
            // 
            // GameServer
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1113, 647);
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(500, 400);
            this.Name = "GameServer";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GameServer_FormClosing);
            this.tabSections.ResumeLayout(false);
            this.tConsole.ResumeLayout(false);
            this.tConsole.PerformLayout();
            this.tLobby.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.clientsDataGridView)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.gbSettings.ResumeLayout(false);
            this.gbSettings.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button cmdClear;
        private System.Windows.Forms.Label lblConnections;
        private System.Windows.Forms.TabControl tabSections;
        private System.Windows.Forms.TabPage tConsole;
        private System.Windows.Forms.TabPage tLobby;
        private System.Windows.Forms.TextBox txtConsole;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.DataGridView clientsDataGridView;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtRoomKey;
        private System.Windows.Forms.CheckBox cbMask;
        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.Label lblRoomKey;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblAddress;
        private System.Windows.Forms.Button cmdHost;
        private System.Windows.Forms.Button cmdJoin;
        private System.Windows.Forms.Button cmdDisconnect;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox gbSettings;
        private System.Windows.Forms.Button cmdPing;
        private System.Windows.Forms.RichTextBox txtLobby;
        private System.Windows.Forms.Timer tPing;
        private System.Windows.Forms.Label lblColor;
        private System.Windows.Forms.Button cmdColor;
        private System.Windows.Forms.DataGridViewTextBoxColumn identifier;
        private System.Windows.Forms.DataGridViewTextBoxColumn name;
        private System.Windows.Forms.DataGridViewTextBoxColumn color;
        private System.Windows.Forms.DataGridViewTextBoxColumn latency;
        private System.Windows.Forms.DataGridViewButtonColumn dc;
    }
}

