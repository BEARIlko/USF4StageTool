﻿namespace USF4_Stage_Tool
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.diagOpenOBJ = new System.Windows.Forms.OpenFileDialog();
            this.pnlOBJECTS = new System.Windows.Forms.Panel();
            this.pSelectedTreeNodeData = new System.Windows.Forms.Panel();
            this.pnlEO_MaterialEdit = new System.Windows.Forms.Panel();
            this.btnCalculateFloat = new System.Windows.Forms.Button();
            this.SPOutFloat = new System.Windows.Forms.TextBox();
            this.SPINFloat = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.cbShaderProperties = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.lvShaderProperties = new System.Windows.Forms.TextBox();
            this.btnEO_ShaderEditSave = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.cbShaders = new System.Windows.Forms.ComboBox();
            this.lbShader = new System.Windows.Forms.Label();
            this.pnlEO_SUBMOD = new System.Windows.Forms.Panel();
            this.bntEO_SubModSave = new System.Windows.Forms.Button();
            this.tbEO_SubModName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.pnlEO_MOD = new System.Windows.Forms.Panel();
            this.bntEO_ModSave = new System.Windows.Forms.Button();
            this.tbEOMod_TextureIndex = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.pnlEO_EMG = new System.Windows.Forms.Panel();
            this.bntEO_EMGSave = new System.Windows.Forms.Button();
            this.tbEMGRootBone = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lbSelNODE_ListData = new System.Windows.Forms.ListBox();
            this.lbSelNODE_Title = new System.Windows.Forms.Label();
            this.pbPreviewDDS = new System.Windows.Forms.PictureBox();
            this.pOBJProperties = new System.Windows.Forms.Panel();
            this.lbOBJNameProperty = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tbTextureIndex = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.chkTextureFlipX = new System.Windows.Forms.CheckBox();
            this.chkTextureFlipY = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbScaleV = new System.Windows.Forms.TextBox();
            this.tbScaleU = new System.Windows.Forms.TextBox();
            this.tvTree = new System.Windows.Forms.TreeView();
            this.lbLoadSteps = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lvStatus = new System.Windows.Forms.ListBox();
            this.btnSaveTEXEMZ = new System.Windows.Forms.Button();
            this.btnSaveEMZ = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnOpenEMZ = new System.Windows.Forms.Button();
            this.chkGeometryFlipX = new System.Windows.Forms.CheckBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.btnOpenOBJ = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.consoleMessagesOutputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveEncodedOBJToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveEncodedOBJToHEXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.informationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uSF4ModdingDocumentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emgContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.emgContextInjectOBJ = new System.Windows.Forms.ToolStripMenuItem();
            this.insertOBJAsNewEMGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteEMGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.injectSMDAsEMGExperimentalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duplicateEMGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duplicateModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emzContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveEMZToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeEMZToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.injectFileExperimentalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exctractTEXEMZTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duplicateUSAMAN01BToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmEmpty = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.emmContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addNewMaterialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.embContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.injectDDSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addDDSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exctractAllDDSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.luaContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.injectLUAScriptToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.extractLUAScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.injectLUAScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractLUABytecodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addLUAScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ddsContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addDDSToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.injectDDSToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.renameDDSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractDDSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteDDSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.matContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addMaterialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteMaterialToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.csbContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.injectCSBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractCSBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animationContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.InjectAnimationtoolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.AddAnimationtoolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.DeleteAnimaiontoolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.dumpRefPoseToSMDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rawDumpEMAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emoContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.insertOBJAsNewEMGToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.InjectEMO = new System.Windows.Forms.ToolStripMenuItem();
            this.rawDumpEMOAsSMDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rawDumpEMOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlOBJECTS.SuspendLayout();
            this.pSelectedTreeNodeData.SuspendLayout();
            this.pnlEO_MaterialEdit.SuspendLayout();
            this.pnlEO_SUBMOD.SuspendLayout();
            this.pnlEO_MOD.SuspendLayout();
            this.pnlEO_EMG.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPreviewDDS)).BeginInit();
            this.pOBJProperties.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.emgContext.SuspendLayout();
            this.emzContext.SuspendLayout();
            this.emmContext.SuspendLayout();
            this.embContext.SuspendLayout();
            this.luaContext.SuspendLayout();
            this.ddsContext.SuspendLayout();
            this.matContext.SuspendLayout();
            this.csbContext.SuspendLayout();
            this.animationContext.SuspendLayout();
            this.emoContext.SuspendLayout();
            this.SuspendLayout();
            // 
            // diagOpenOBJ
            // 
            this.diagOpenOBJ.RestoreDirectory = true;
            // 
            // pnlOBJECTS
            // 
            this.pnlOBJECTS.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlOBJECTS.BackColor = System.Drawing.Color.AliceBlue;
            this.pnlOBJECTS.Controls.Add(this.pSelectedTreeNodeData);
            this.pnlOBJECTS.Controls.Add(this.pOBJProperties);
            this.pnlOBJECTS.Controls.Add(this.tvTree);
            this.pnlOBJECTS.Controls.Add(this.lbLoadSteps);
            this.pnlOBJECTS.Controls.Add(this.progressBar1);
            this.pnlOBJECTS.Location = new System.Drawing.Point(-5, 154);
            this.pnlOBJECTS.Name = "pnlOBJECTS";
            this.pnlOBJECTS.Size = new System.Drawing.Size(1062, 479);
            this.pnlOBJECTS.TabIndex = 13;
            // 
            // pSelectedTreeNodeData
            // 
            this.pSelectedTreeNodeData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pSelectedTreeNodeData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pSelectedTreeNodeData.Controls.Add(this.pnlEO_MaterialEdit);
            this.pSelectedTreeNodeData.Controls.Add(this.pnlEO_SUBMOD);
            this.pSelectedTreeNodeData.Controls.Add(this.pnlEO_MOD);
            this.pSelectedTreeNodeData.Controls.Add(this.pnlEO_EMG);
            this.pSelectedTreeNodeData.Controls.Add(this.lbSelNODE_ListData);
            this.pSelectedTreeNodeData.Controls.Add(this.lbSelNODE_Title);
            this.pSelectedTreeNodeData.Controls.Add(this.pbPreviewDDS);
            this.pSelectedTreeNodeData.Location = new System.Drawing.Point(357, 19);
            this.pSelectedTreeNodeData.Name = "pSelectedTreeNodeData";
            this.pSelectedTreeNodeData.Size = new System.Drawing.Size(451, 407);
            this.pSelectedTreeNodeData.TabIndex = 24;
            // 
            // pnlEO_MaterialEdit
            // 
            this.pnlEO_MaterialEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlEO_MaterialEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlEO_MaterialEdit.Controls.Add(this.btnCalculateFloat);
            this.pnlEO_MaterialEdit.Controls.Add(this.SPOutFloat);
            this.pnlEO_MaterialEdit.Controls.Add(this.SPINFloat);
            this.pnlEO_MaterialEdit.Controls.Add(this.label15);
            this.pnlEO_MaterialEdit.Controls.Add(this.cbShaderProperties);
            this.pnlEO_MaterialEdit.Controls.Add(this.label14);
            this.pnlEO_MaterialEdit.Controls.Add(this.lvShaderProperties);
            this.pnlEO_MaterialEdit.Controls.Add(this.btnEO_ShaderEditSave);
            this.pnlEO_MaterialEdit.Controls.Add(this.label13);
            this.pnlEO_MaterialEdit.Controls.Add(this.cbShaders);
            this.pnlEO_MaterialEdit.Controls.Add(this.lbShader);
            this.pnlEO_MaterialEdit.Location = new System.Drawing.Point(3, 31);
            this.pnlEO_MaterialEdit.Name = "pnlEO_MaterialEdit";
            this.pnlEO_MaterialEdit.Size = new System.Drawing.Size(443, 371);
            this.pnlEO_MaterialEdit.TabIndex = 59;
            this.pnlEO_MaterialEdit.Visible = false;
            // 
            // btnCalculateFloat
            // 
            this.btnCalculateFloat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCalculateFloat.Location = new System.Drawing.Point(188, 275);
            this.btnCalculateFloat.Name = "btnCalculateFloat";
            this.btnCalculateFloat.Size = new System.Drawing.Size(25, 20);
            this.btnCalculateFloat.TabIndex = 14;
            this.btnCalculateFloat.Text = ">";
            this.btnCalculateFloat.UseVisualStyleBackColor = true;
            this.btnCalculateFloat.Click += new System.EventHandler(this.btnCalculateFloat_Click);
            // 
            // SPOutFloat
            // 
            this.SPOutFloat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SPOutFloat.Location = new System.Drawing.Point(219, 275);
            this.SPOutFloat.Name = "SPOutFloat";
            this.SPOutFloat.Size = new System.Drawing.Size(214, 20);
            this.SPOutFloat.TabIndex = 13;
            // 
            // SPINFloat
            // 
            this.SPINFloat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SPINFloat.Location = new System.Drawing.Point(87, 275);
            this.SPINFloat.Name = "SPINFloat";
            this.SPINFloat.Size = new System.Drawing.Size(95, 20);
            this.SPINFloat.TabIndex = 11;
            // 
            // label15
            // 
            this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(7, 278);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(77, 13);
            this.label15.TabIndex = 10;
            this.label15.Text = "Calculate Float";
            // 
            // cbShaderProperties
            // 
            this.cbShaderProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbShaderProperties.FormattingEnabled = true;
            this.cbShaderProperties.Location = new System.Drawing.Point(10, 333);
            this.cbShaderProperties.Name = "cbShaderProperties";
            this.cbShaderProperties.Size = new System.Drawing.Size(217, 21);
            this.cbShaderProperties.TabIndex = 9;
            this.cbShaderProperties.SelectedIndexChanged += new System.EventHandler(this.cbShaderProperties_SelectedIndexChanged);
            // 
            // label14
            // 
            this.label14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(7, 315);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(68, 13);
            this.label14.TabIndex = 8;
            this.label14.Text = "Add Property";
            // 
            // lvShaderProperties
            // 
            this.lvShaderProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvShaderProperties.Location = new System.Drawing.Point(6, 63);
            this.lvShaderProperties.Multiline = true;
            this.lvShaderProperties.Name = "lvShaderProperties";
            this.lvShaderProperties.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.lvShaderProperties.Size = new System.Drawing.Size(427, 202);
            this.lvShaderProperties.TabIndex = 7;
            // 
            // btnEO_ShaderEditSave
            // 
            this.btnEO_ShaderEditSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEO_ShaderEditSave.Location = new System.Drawing.Point(239, 315);
            this.btnEO_ShaderEditSave.Name = "btnEO_ShaderEditSave";
            this.btnEO_ShaderEditSave.Size = new System.Drawing.Size(194, 49);
            this.btnEO_ShaderEditSave.TabIndex = 6;
            this.btnEO_ShaderEditSave.Text = "Save Shader Edits";
            this.btnEO_ShaderEditSave.UseVisualStyleBackColor = true;
            this.btnEO_ShaderEditSave.Click += new System.EventHandler(this.btnEO_ShaderEditSave_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(7, 47);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(54, 13);
            this.label13.TabIndex = 3;
            this.label13.Text = "Properties";
            // 
            // cbShaders
            // 
            this.cbShaders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbShaders.FormattingEnabled = true;
            this.cbShaders.Location = new System.Drawing.Point(46, 7);
            this.cbShaders.Name = "cbShaders";
            this.cbShaders.Size = new System.Drawing.Size(388, 21);
            this.cbShaders.TabIndex = 1;
            this.cbShaders.SelectedIndexChanged += new System.EventHandler(this.cbShaders_SelectedIndexChanged);
            // 
            // lbShader
            // 
            this.lbShader.AutoSize = true;
            this.lbShader.Location = new System.Drawing.Point(3, 10);
            this.lbShader.Name = "lbShader";
            this.lbShader.Size = new System.Drawing.Size(41, 13);
            this.lbShader.TabIndex = 0;
            this.lbShader.Text = "Shader";
            // 
            // pnlEO_SUBMOD
            // 
            this.pnlEO_SUBMOD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlEO_SUBMOD.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlEO_SUBMOD.Controls.Add(this.bntEO_SubModSave);
            this.pnlEO_SUBMOD.Controls.Add(this.tbEO_SubModName);
            this.pnlEO_SUBMOD.Controls.Add(this.label1);
            this.pnlEO_SUBMOD.Controls.Add(this.label12);
            this.pnlEO_SUBMOD.Location = new System.Drawing.Point(237, 31);
            this.pnlEO_SUBMOD.Name = "pnlEO_SUBMOD";
            this.pnlEO_SUBMOD.Size = new System.Drawing.Size(209, 369);
            this.pnlEO_SUBMOD.TabIndex = 54;
            this.pnlEO_SUBMOD.Visible = false;
            // 
            // bntEO_SubModSave
            // 
            this.bntEO_SubModSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bntEO_SubModSave.Location = new System.Drawing.Point(8, 323);
            this.bntEO_SubModSave.Name = "bntEO_SubModSave";
            this.bntEO_SubModSave.Size = new System.Drawing.Size(191, 38);
            this.bntEO_SubModSave.TabIndex = 52;
            this.bntEO_SubModSave.Text = "Save Sub Model Edits";
            this.bntEO_SubModSave.UseVisualStyleBackColor = true;
            this.bntEO_SubModSave.Click += new System.EventHandler(this.BntEO_SubModSave_Click);
            // 
            // tbEO_SubModName
            // 
            this.tbEO_SubModName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbEO_SubModName.Location = new System.Drawing.Point(13, 52);
            this.tbEO_SubModName.Name = "tbEO_SubModName";
            this.tbEO_SubModName.Size = new System.Drawing.Size(186, 20);
            this.tbEO_SubModName.TabIndex = 51;
            this.tbEO_SubModName.TabStop = false;
            this.tbEO_SubModName.WordWrap = false;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(202, 16);
            this.label1.TabIndex = 50;
            this.label1.Text = "Edit Options";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(10, 36);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(79, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "Rename Model";
            // 
            // pnlEO_MOD
            // 
            this.pnlEO_MOD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlEO_MOD.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlEO_MOD.Controls.Add(this.bntEO_ModSave);
            this.pnlEO_MOD.Controls.Add(this.tbEOMod_TextureIndex);
            this.pnlEO_MOD.Controls.Add(this.label10);
            this.pnlEO_MOD.Controls.Add(this.label11);
            this.pnlEO_MOD.Location = new System.Drawing.Point(237, 31);
            this.pnlEO_MOD.Name = "pnlEO_MOD";
            this.pnlEO_MOD.Size = new System.Drawing.Size(209, 369);
            this.pnlEO_MOD.TabIndex = 53;
            // 
            // bntEO_ModSave
            // 
            this.bntEO_ModSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bntEO_ModSave.Location = new System.Drawing.Point(8, 323);
            this.bntEO_ModSave.Name = "bntEO_ModSave";
            this.bntEO_ModSave.Size = new System.Drawing.Size(191, 38);
            this.bntEO_ModSave.TabIndex = 52;
            this.bntEO_ModSave.Text = "Save Model Edits";
            this.bntEO_ModSave.UseVisualStyleBackColor = true;
            this.bntEO_ModSave.Click += new System.EventHandler(this.BntEO_ModSave_Click);
            // 
            // tbEOMod_TextureIndex
            // 
            this.tbEOMod_TextureIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbEOMod_TextureIndex.Location = new System.Drawing.Point(88, 33);
            this.tbEOMod_TextureIndex.Name = "tbEOMod_TextureIndex";
            this.tbEOMod_TextureIndex.Size = new System.Drawing.Size(43, 20);
            this.tbEOMod_TextureIndex.TabIndex = 51;
            this.tbEOMod_TextureIndex.TabStop = false;
            this.tbEOMod_TextureIndex.WordWrap = false;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label10.Location = new System.Drawing.Point(3, 5);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(202, 16);
            this.label10.TabIndex = 50;
            this.label10.Text = "Edit Options";
            this.label10.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(10, 36);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(72, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Texture Index";
            // 
            // pnlEO_EMG
            // 
            this.pnlEO_EMG.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlEO_EMG.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlEO_EMG.Controls.Add(this.bntEO_EMGSave);
            this.pnlEO_EMG.Controls.Add(this.tbEMGRootBone);
            this.pnlEO_EMG.Controls.Add(this.label9);
            this.pnlEO_EMG.Controls.Add(this.label6);
            this.pnlEO_EMG.Location = new System.Drawing.Point(237, 31);
            this.pnlEO_EMG.Name = "pnlEO_EMG";
            this.pnlEO_EMG.Size = new System.Drawing.Size(209, 369);
            this.pnlEO_EMG.TabIndex = 44;
            // 
            // bntEO_EMGSave
            // 
            this.bntEO_EMGSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bntEO_EMGSave.Location = new System.Drawing.Point(8, 323);
            this.bntEO_EMGSave.Name = "bntEO_EMGSave";
            this.bntEO_EMGSave.Size = new System.Drawing.Size(191, 38);
            this.bntEO_EMGSave.TabIndex = 52;
            this.bntEO_EMGSave.Text = "Save EMG Edits";
            this.bntEO_EMGSave.UseVisualStyleBackColor = true;
            this.bntEO_EMGSave.Click += new System.EventHandler(this.BntEO_EMGSave_Click);
            // 
            // tbEMGRootBone
            // 
            this.tbEMGRootBone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbEMGRootBone.Location = new System.Drawing.Point(74, 33);
            this.tbEMGRootBone.Name = "tbEMGRootBone";
            this.tbEMGRootBone.Size = new System.Drawing.Size(43, 20);
            this.tbEMGRootBone.TabIndex = 51;
            this.tbEMGRootBone.TabStop = false;
            this.tbEMGRootBone.WordWrap = false;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label9.Location = new System.Drawing.Point(3, 5);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(202, 16);
            this.label9.TabIndex = 50;
            this.label9.Text = "Edit Options";
            this.label9.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 36);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Root Bone";
            // 
            // lbSelNODE_ListData
            // 
            this.lbSelNODE_ListData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbSelNODE_ListData.BackColor = System.Drawing.Color.AliceBlue;
            this.lbSelNODE_ListData.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lbSelNODE_ListData.FormattingEnabled = true;
            this.lbSelNODE_ListData.Location = new System.Drawing.Point(7, 31);
            this.lbSelNODE_ListData.Name = "lbSelNODE_ListData";
            this.lbSelNODE_ListData.Size = new System.Drawing.Size(227, 364);
            this.lbSelNODE_ListData.TabIndex = 43;
            // 
            // lbSelNODE_Title
            // 
            this.lbSelNODE_Title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbSelNODE_Title.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbSelNODE_Title.Location = new System.Drawing.Point(3, 9);
            this.lbSelNODE_Title.Name = "lbSelNODE_Title";
            this.lbSelNODE_Title.Size = new System.Drawing.Size(443, 20);
            this.lbSelNODE_Title.TabIndex = 42;
            this.lbSelNODE_Title.Text = "selectedTreeNodeData";
            this.lbSelNODE_Title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pbPreviewDDS
            // 
            this.pbPreviewDDS.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbPreviewDDS.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pbPreviewDDS.Location = new System.Drawing.Point(3, 3);
            this.pbPreviewDDS.Name = "pbPreviewDDS";
            this.pbPreviewDDS.Size = new System.Drawing.Size(443, 399);
            this.pbPreviewDDS.TabIndex = 58;
            this.pbPreviewDDS.TabStop = false;
            this.pbPreviewDDS.Visible = false;
            // 
            // pOBJProperties
            // 
            this.pOBJProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pOBJProperties.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pOBJProperties.Controls.Add(this.lbOBJNameProperty);
            this.pOBJProperties.Controls.Add(this.label8);
            this.pOBJProperties.Controls.Add(this.tbTextureIndex);
            this.pOBJProperties.Controls.Add(this.label7);
            this.pOBJProperties.Controls.Add(this.chkTextureFlipX);
            this.pOBJProperties.Controls.Add(this.chkTextureFlipY);
            this.pOBJProperties.Controls.Add(this.label5);
            this.pOBJProperties.Controls.Add(this.label3);
            this.pOBJProperties.Controls.Add(this.label2);
            this.pOBJProperties.Controls.Add(this.tbScaleV);
            this.pOBJProperties.Controls.Add(this.tbScaleU);
            this.pOBJProperties.Location = new System.Drawing.Point(810, 18);
            this.pOBJProperties.Name = "pOBJProperties";
            this.pOBJProperties.Size = new System.Drawing.Size(234, 407);
            this.pOBJProperties.TabIndex = 23;
            // 
            // lbOBJNameProperty
            // 
            this.lbOBJNameProperty.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbOBJNameProperty.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbOBJNameProperty.Location = new System.Drawing.Point(3, 42);
            this.lbOBJNameProperty.Name = "lbOBJNameProperty";
            this.lbOBJNameProperty.Size = new System.Drawing.Size(226, 16);
            this.lbOBJNameProperty.TabIndex = 49;
            this.lbOBJNameProperty.Text = "No OBJ Loaded";
            this.lbOBJNameProperty.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label8.Location = new System.Drawing.Point(11, 135);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(198, 16);
            this.label8.TabIndex = 48;
            this.label8.Text = "Override EMG Texture Data";
            // 
            // tbTextureIndex
            // 
            this.tbTextureIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTextureIndex.Location = new System.Drawing.Point(90, 158);
            this.tbTextureIndex.Name = "tbTextureIndex";
            this.tbTextureIndex.Size = new System.Drawing.Size(43, 20);
            this.tbTextureIndex.TabIndex = 47;
            this.tbTextureIndex.TabStop = false;
            this.tbTextureIndex.WordWrap = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 161);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(75, 13);
            this.label7.TabIndex = 46;
            this.label7.Text = "Texture Index:";
            // 
            // chkTextureFlipX
            // 
            this.chkTextureFlipX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkTextureFlipX.AutoSize = true;
            this.chkTextureFlipX.BackColor = System.Drawing.Color.Transparent;
            this.chkTextureFlipX.Location = new System.Drawing.Point(14, 105);
            this.chkTextureFlipX.Name = "chkTextureFlipX";
            this.chkTextureFlipX.Size = new System.Drawing.Size(70, 17);
            this.chkTextureFlipX.TabIndex = 43;
            this.chkTextureFlipX.Text = "X Flip UV";
            this.chkTextureFlipX.UseVisualStyleBackColor = false;
            // 
            // chkTextureFlipY
            // 
            this.chkTextureFlipY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkTextureFlipY.AutoSize = true;
            this.chkTextureFlipY.BackColor = System.Drawing.Color.Transparent;
            this.chkTextureFlipY.Checked = true;
            this.chkTextureFlipY.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTextureFlipY.Location = new System.Drawing.Point(93, 105);
            this.chkTextureFlipY.Name = "chkTextureFlipY";
            this.chkTextureFlipY.Size = new System.Drawing.Size(70, 17);
            this.chkTextureFlipY.TabIndex = 42;
            this.chkTextureFlipY.Text = "Y Flip UV";
            this.chkTextureFlipY.UseVisualStyleBackColor = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(25, 3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(180, 20);
            this.label5.TabIndex = 41;
            this.label5.Text = "OBJ Inject Properties";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 216);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 40;
            this.label3.Text = "Scale V:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 190);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 39;
            this.label2.Text = "Scale U:";
            // 
            // tbScaleV
            // 
            this.tbScaleV.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbScaleV.Location = new System.Drawing.Point(90, 213);
            this.tbScaleV.Name = "tbScaleV";
            this.tbScaleV.Size = new System.Drawing.Size(43, 20);
            this.tbScaleV.TabIndex = 38;
            this.tbScaleV.TabStop = false;
            this.tbScaleV.Text = "1";
            this.tbScaleV.WordWrap = false;
            // 
            // tbScaleU
            // 
            this.tbScaleU.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbScaleU.Location = new System.Drawing.Point(90, 187);
            this.tbScaleU.Name = "tbScaleU";
            this.tbScaleU.Size = new System.Drawing.Size(43, 20);
            this.tbScaleU.TabIndex = 37;
            this.tbScaleU.TabStop = false;
            this.tbScaleU.Text = "1";
            this.tbScaleU.WordWrap = false;
            // 
            // tvTree
            // 
            this.tvTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tvTree.Location = new System.Drawing.Point(11, 18);
            this.tvTree.Name = "tvTree";
            this.tvTree.Size = new System.Drawing.Size(334, 407);
            this.tvTree.TabIndex = 22;
            this.tvTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TvTree_AfterSelect);
            this.tvTree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TvTree_NodeMouseClick);
            // 
            // lbLoadSteps
            // 
            this.lbLoadSteps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbLoadSteps.BackColor = System.Drawing.Color.Transparent;
            this.lbLoadSteps.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbLoadSteps.Location = new System.Drawing.Point(16, 432);
            this.lbLoadSteps.Name = "lbLoadSteps";
            this.lbLoadSteps.Size = new System.Drawing.Size(1028, 16);
            this.lbLoadSteps.TabIndex = 21;
            this.lbLoadSteps.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(16, 445);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(1028, 23);
            this.progressBar1.TabIndex = 20;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.Moccasin;
            this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.lvStatus);
            this.panel1.Controls.Add(this.btnSaveTEXEMZ);
            this.panel1.Controls.Add(this.btnSaveEMZ);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.btnOpenEMZ);
            this.panel1.Controls.Add(this.chkGeometryFlipX);
            this.panel1.Controls.Add(this.pictureBox2);
            this.panel1.Controls.Add(this.btnOpenOBJ);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.menuStrip1);
            this.panel1.Location = new System.Drawing.Point(-5, -3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1062, 172);
            this.panel1.TabIndex = 12;
            // 
            // lvStatus
            // 
            this.lvStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvStatus.BackColor = System.Drawing.Color.AntiqueWhite;
            this.lvStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lvStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lvStatus.ForeColor = System.Drawing.Color.Black;
            this.lvStatus.FormattingEnabled = true;
            this.lvStatus.ItemHeight = 16;
            this.lvStatus.Location = new System.Drawing.Point(136, 116);
            this.lvStatus.Name = "lvStatus";
            this.lvStatus.Size = new System.Drawing.Size(760, 50);
            this.lvStatus.TabIndex = 36;
            this.lvStatus.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lvStatus_MouseDown);
            // 
            // btnSaveTEXEMZ
            // 
            this.btnSaveTEXEMZ.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSaveTEXEMZ.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnSaveTEXEMZ.Location = new System.Drawing.Point(432, 33);
            this.btnSaveTEXEMZ.Name = "btnSaveTEXEMZ";
            this.btnSaveTEXEMZ.Size = new System.Drawing.Size(142, 64);
            this.btnSaveTEXEMZ.TabIndex = 35;
            this.btnSaveTEXEMZ.Tag = "TEX";
            this.btnSaveTEXEMZ.Text = "Save TEX.EMZ";
            this.btnSaveTEXEMZ.UseVisualStyleBackColor = true;
            this.btnSaveTEXEMZ.Click += new System.EventHandler(this.btnSaveTEXEMZ_Click);
            // 
            // btnSaveEMZ
            // 
            this.btnSaveEMZ.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSaveEMZ.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnSaveEMZ.Location = new System.Drawing.Point(284, 33);
            this.btnSaveEMZ.Name = "btnSaveEMZ";
            this.btnSaveEMZ.Size = new System.Drawing.Size(142, 64);
            this.btnSaveEMZ.TabIndex = 34;
            this.btnSaveEMZ.Tag = "EMZ";
            this.btnSaveEMZ.Text = "Save EMZ";
            this.btnSaveEMZ.UseVisualStyleBackColor = true;
            this.btnSaveEMZ.Click += new System.EventHandler(this.btnSaveEMZ_Click_1);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(618, 31);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(128, 66);
            this.button1.TabIndex = 33;
            this.button1.Text = "SMD Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnOpenEMZ
            // 
            this.btnOpenEMZ.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenEMZ.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnOpenEMZ.Location = new System.Drawing.Point(136, 33);
            this.btnOpenEMZ.Name = "btnOpenEMZ";
            this.btnOpenEMZ.Size = new System.Drawing.Size(142, 64);
            this.btnOpenEMZ.TabIndex = 31;
            this.btnOpenEMZ.Text = "Open EMZ/TEX.EMZ";
            this.btnOpenEMZ.UseVisualStyleBackColor = true;
            this.btnOpenEMZ.Click += new System.EventHandler(this.BtnOpenEMZ_Click);
            // 
            // chkGeometryFlipX
            // 
            this.chkGeometryFlipX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkGeometryFlipX.AutoSize = true;
            this.chkGeometryFlipX.BackColor = System.Drawing.Color.Transparent;
            this.chkGeometryFlipX.Checked = true;
            this.chkGeometryFlipX.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGeometryFlipX.Location = new System.Drawing.Point(760, 98);
            this.chkGeometryFlipX.Name = "chkGeometryFlipX";
            this.chkGeometryFlipX.Size = new System.Drawing.Size(100, 17);
            this.chkGeometryFlipX.TabIndex = 19;
            this.chkGeometryFlipX.Text = "X Flip Geometry";
            this.chkGeometryFlipX.UseVisualStyleBackColor = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox2.BackgroundImage")));
            this.pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox2.Location = new System.Drawing.Point(904, 29);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(144, 125);
            this.pictureBox2.TabIndex = 16;
            this.pictureBox2.TabStop = false;
            // 
            // btnOpenOBJ
            // 
            this.btnOpenOBJ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenOBJ.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenOBJ.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnOpenOBJ.Location = new System.Drawing.Point(752, 31);
            this.btnOpenOBJ.Name = "btnOpenOBJ";
            this.btnOpenOBJ.Size = new System.Drawing.Size(142, 64);
            this.btnOpenOBJ.TabIndex = 0;
            this.btnOpenOBJ.Text = "Encode OBJ";
            this.btnOpenOBJ.UseVisualStyleBackColor = true;
            this.btnOpenOBJ.Click += new System.EventHandler(this.BtnOpenOBJ_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.BackgroundImage = global::USF4_Stage_Tool.Properties.Resources.SakuraStageTool;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Location = new System.Drawing.Point(9, 29);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(117, 125);
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.SandyBrown;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuToolStripMenuItem,
            this.debugToolStripMenuItem,
            this.informationToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1060, 24);
            this.menuStrip1.TabIndex = 11;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // menuToolStripMenuItem
            // 
            this.menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            this.menuToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.menuToolStripMenuItem.Text = "Menu";
            this.menuToolStripMenuItem.Visible = false;
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.BackColor = System.Drawing.Color.Transparent;
            this.debugToolStripMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.debugToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.consoleMessagesOutputToolStripMenuItem,
            this.saveEncodedOBJToolStripMenuItem,
            this.saveEncodedOBJToHEXToolStripMenuItem});
            this.debugToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // consoleMessagesOutputToolStripMenuItem
            // 
            this.consoleMessagesOutputToolStripMenuItem.BackColor = System.Drawing.Color.SandyBrown;
            this.consoleMessagesOutputToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.consoleMessagesOutputToolStripMenuItem.Name = "consoleMessagesOutputToolStripMenuItem";
            this.consoleMessagesOutputToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.consoleMessagesOutputToolStripMenuItem.Text = "Open Debug Output";
            this.consoleMessagesOutputToolStripMenuItem.Click += new System.EventHandler(this.ConsoleMessagesOutputToolStripMenuItem_Click);
            // 
            // saveEncodedOBJToolStripMenuItem
            // 
            this.saveEncodedOBJToolStripMenuItem.BackColor = System.Drawing.Color.SandyBrown;
            this.saveEncodedOBJToolStripMenuItem.Name = "saveEncodedOBJToolStripMenuItem";
            this.saveEncodedOBJToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.saveEncodedOBJToolStripMenuItem.Text = "Save Encoded OBJ";
            this.saveEncodedOBJToolStripMenuItem.Click += new System.EventHandler(this.SaveEncodedOBJToolStripMenuItem_Click);
            // 
            // saveEncodedOBJToHEXToolStripMenuItem
            // 
            this.saveEncodedOBJToHEXToolStripMenuItem.BackColor = System.Drawing.Color.SandyBrown;
            this.saveEncodedOBJToHEXToolStripMenuItem.Name = "saveEncodedOBJToHEXToolStripMenuItem";
            this.saveEncodedOBJToHEXToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.saveEncodedOBJToHEXToolStripMenuItem.Text = "Save Encoded OBJ to HEX";
            this.saveEncodedOBJToHEXToolStripMenuItem.Click += new System.EventHandler(this.SaveEncodedOBJToHEXToolStripMenuItem_Click);
            // 
            // informationToolStripMenuItem
            // 
            this.informationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.uSF4ModdingDocumentToolStripMenuItem});
            this.informationToolStripMenuItem.Name = "informationToolStripMenuItem";
            this.informationToolStripMenuItem.Size = new System.Drawing.Size(82, 20);
            this.informationToolStripMenuItem.Text = "Information";
            // 
            // uSF4ModdingDocumentToolStripMenuItem
            // 
            this.uSF4ModdingDocumentToolStripMenuItem.BackColor = System.Drawing.Color.SandyBrown;
            this.uSF4ModdingDocumentToolStripMenuItem.Name = "uSF4ModdingDocumentToolStripMenuItem";
            this.uSF4ModdingDocumentToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.uSF4ModdingDocumentToolStripMenuItem.Text = "USF4 Modding Document";
            this.uSF4ModdingDocumentToolStripMenuItem.Click += new System.EventHandler(this.USF4ModdingDocumentToolStripMenuItem_Click);
            // 
            // emgContext
            // 
            this.emgContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.emgContextInjectOBJ,
            this.insertOBJAsNewEMGToolStripMenuItem,
            this.deleteEMGToolStripMenuItem,
            this.injectSMDAsEMGExperimentalToolStripMenuItem,
            this.duplicateEMGToolStripMenuItem,
            this.duplicateModelToolStripMenuItem});
            this.emgContext.Name = "treeContext";
            this.emgContext.Size = new System.Drawing.Size(254, 136);
            this.emgContext.Opening += new System.ComponentModel.CancelEventHandler(this.emgContext_Opening);
            // 
            // emgContextInjectOBJ
            // 
            this.emgContextInjectOBJ.Name = "emgContextInjectOBJ";
            this.emgContextInjectOBJ.Size = new System.Drawing.Size(253, 22);
            this.emgContextInjectOBJ.Text = "Inject Encoded OBJ";
            this.emgContextInjectOBJ.Click += new System.EventHandler(this.TreeContextInjectOBJ_Click);
            // 
            // insertOBJAsNewEMGToolStripMenuItem
            // 
            this.insertOBJAsNewEMGToolStripMenuItem.Name = "insertOBJAsNewEMGToolStripMenuItem";
            this.insertOBJAsNewEMGToolStripMenuItem.Size = new System.Drawing.Size(253, 22);
            this.insertOBJAsNewEMGToolStripMenuItem.Text = "Insert OBJ as new EMG";
            this.insertOBJAsNewEMGToolStripMenuItem.Click += new System.EventHandler(this.InsertOBJAsNewEMGToolStripMenuItem_Click);
            // 
            // deleteEMGToolStripMenuItem
            // 
            this.deleteEMGToolStripMenuItem.Name = "deleteEMGToolStripMenuItem";
            this.deleteEMGToolStripMenuItem.Size = new System.Drawing.Size(253, 22);
            this.deleteEMGToolStripMenuItem.Text = "Delete EMG";
            this.deleteEMGToolStripMenuItem.Click += new System.EventHandler(this.DeleteEMGToolStripMenuItem_Click);
            // 
            // injectSMDAsEMGExperimentalToolStripMenuItem
            // 
            this.injectSMDAsEMGExperimentalToolStripMenuItem.Name = "injectSMDAsEMGExperimentalToolStripMenuItem";
            this.injectSMDAsEMGExperimentalToolStripMenuItem.Size = new System.Drawing.Size(253, 22);
            this.injectSMDAsEMGExperimentalToolStripMenuItem.Text = "Inject SMD as EMG (Experimental)";
            this.injectSMDAsEMGExperimentalToolStripMenuItem.Click += new System.EventHandler(this.injectSMDAsEMGExperimentalToolStripMenuItem_Click);
            // 
            // duplicateEMGToolStripMenuItem
            // 
            this.duplicateEMGToolStripMenuItem.Name = "duplicateEMGToolStripMenuItem";
            this.duplicateEMGToolStripMenuItem.Size = new System.Drawing.Size(253, 22);
            this.duplicateEMGToolStripMenuItem.Text = "Duplicate EMG";
            this.duplicateEMGToolStripMenuItem.Click += new System.EventHandler(this.duplicateEMGToolStripMenuItem_Click);
            // 
            // duplicateModelToolStripMenuItem
            // 
            this.duplicateModelToolStripMenuItem.Name = "duplicateModelToolStripMenuItem";
            this.duplicateModelToolStripMenuItem.Size = new System.Drawing.Size(253, 22);
            this.duplicateModelToolStripMenuItem.Text = "Duplicate Model";
            this.duplicateModelToolStripMenuItem.Click += new System.EventHandler(this.duplicateModelToolStripMenuItem_Click);
            // 
            // emzContext
            // 
            this.emzContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveEMZToolStripMenuItem,
            this.closeEMZToolStripMenuItem,
            this.injectFileExperimentalToolStripMenuItem,
            this.toolStripSeparator1,
            this.exctractTEXEMZTexturesToolStripMenuItem,
            this.duplicateUSAMAN01BToolStripMenuItem});
            this.emzContext.Name = "emzContext";
            this.emzContext.Size = new System.Drawing.Size(212, 120);
            this.emzContext.Opening += new System.ComponentModel.CancelEventHandler(this.emzContext_Opening);
            // 
            // saveEMZToolStripMenuItem
            // 
            this.saveEMZToolStripMenuItem.Name = "saveEMZToolStripMenuItem";
            this.saveEMZToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.saveEMZToolStripMenuItem.Text = "Save EMZ";
            this.saveEMZToolStripMenuItem.Click += new System.EventHandler(this.SaveEMZToolStripMenuItem_Click);
            // 
            // closeEMZToolStripMenuItem
            // 
            this.closeEMZToolStripMenuItem.Name = "closeEMZToolStripMenuItem";
            this.closeEMZToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.closeEMZToolStripMenuItem.Text = "Close EMZ";
            this.closeEMZToolStripMenuItem.Click += new System.EventHandler(this.CloseEMZToolStripMenuItem_Click);
            // 
            // injectFileExperimentalToolStripMenuItem
            // 
            this.injectFileExperimentalToolStripMenuItem.Name = "injectFileExperimentalToolStripMenuItem";
            this.injectFileExperimentalToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.injectFileExperimentalToolStripMenuItem.Text = "Inject File (Experimental)";
            this.injectFileExperimentalToolStripMenuItem.Click += new System.EventHandler(this.injectFileExperimentalToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(208, 6);
            // 
            // exctractTEXEMZTexturesToolStripMenuItem
            // 
            this.exctractTEXEMZTexturesToolStripMenuItem.Name = "exctractTEXEMZTexturesToolStripMenuItem";
            this.exctractTEXEMZTexturesToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.exctractTEXEMZTexturesToolStripMenuItem.Text = "Exctract TEX.EMZ Textures";
            this.exctractTEXEMZTexturesToolStripMenuItem.Click += new System.EventHandler(this.exctractTEXEMZTexturesToolStripMenuItem_Click);
            // 
            // duplicateUSAMAN01BToolStripMenuItem
            // 
            this.duplicateUSAMAN01BToolStripMenuItem.Name = "duplicateUSAMAN01BToolStripMenuItem";
            this.duplicateUSAMAN01BToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.duplicateUSAMAN01BToolStripMenuItem.Text = "Duplicate USA_MAN01_B";
            this.duplicateUSAMAN01BToolStripMenuItem.Click += new System.EventHandler(this.dupliacteUSAMAN01BToolStripMenuItem_Click);
            // 
            // cmEmpty
            // 
            this.cmEmpty.Name = "cmEmpty";
            this.cmEmpty.Size = new System.Drawing.Size(61, 4);
            // 
            // emmContext
            // 
            this.emmContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNewMaterialToolStripMenuItem});
            this.emmContext.Name = "emmContext";
            this.emmContext.Size = new System.Drawing.Size(170, 26);
            this.emmContext.Opening += new System.ComponentModel.CancelEventHandler(this.emmContext_Opening);
            // 
            // addNewMaterialToolStripMenuItem
            // 
            this.addNewMaterialToolStripMenuItem.Name = "addNewMaterialToolStripMenuItem";
            this.addNewMaterialToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.addNewMaterialToolStripMenuItem.Text = "Add New Material";
            this.addNewMaterialToolStripMenuItem.Click += new System.EventHandler(this.addNewMaterialToolStripMenuItem_Click);
            // 
            // embContext
            // 
            this.embContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.injectDDSToolStripMenuItem,
            this.addDDSToolStripMenuItem,
            this.exctractAllDDSToolStripMenuItem});
            this.embContext.Name = "embContext";
            this.embContext.Size = new System.Drawing.Size(183, 70);
            this.embContext.Opening += new System.ComponentModel.CancelEventHandler(this.embContext_Opening);
            // 
            // injectDDSToolStripMenuItem
            // 
            this.injectDDSToolStripMenuItem.Name = "injectDDSToolStripMenuItem";
            this.injectDDSToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.injectDDSToolStripMenuItem.Text = "Add DDS";
            this.injectDDSToolStripMenuItem.Click += new System.EventHandler(this.injectDDSToolStripMenuItem_Click);
            // 
            // addDDSToolStripMenuItem
            // 
            this.addDDSToolStripMenuItem.Name = "addDDSToolStripMenuItem";
            this.addDDSToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.addDDSToolStripMenuItem.Text = "Delete EMB (danger)";
            // 
            // exctractAllDDSToolStripMenuItem
            // 
            this.exctractAllDDSToolStripMenuItem.Name = "exctractAllDDSToolStripMenuItem";
            this.exctractAllDDSToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.exctractAllDDSToolStripMenuItem.Text = "Exctract All DDS";
            this.exctractAllDDSToolStripMenuItem.Click += new System.EventHandler(this.exctractAllDDSToolStripMenuItem_Click);
            // 
            // luaContext
            // 
            this.luaContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.injectLUAScriptToolStripMenuItem1,
            this.extractLUAScriptToolStripMenuItem,
            this.injectLUAScriptToolStripMenuItem,
            this.extractLUABytecodeToolStripMenuItem,
            this.addLUAScriptToolStripMenuItem});
            this.luaContext.Name = "luaContext";
            this.luaContext.Size = new System.Drawing.Size(188, 114);
            // 
            // injectLUAScriptToolStripMenuItem1
            // 
            this.injectLUAScriptToolStripMenuItem1.Name = "injectLUAScriptToolStripMenuItem1";
            this.injectLUAScriptToolStripMenuItem1.Size = new System.Drawing.Size(187, 22);
            this.injectLUAScriptToolStripMenuItem1.Text = "Inject LUA Script";
            this.injectLUAScriptToolStripMenuItem1.Click += new System.EventHandler(this.injectLUAScriptToolStripMenuItem1_Click);
            // 
            // extractLUAScriptToolStripMenuItem
            // 
            this.extractLUAScriptToolStripMenuItem.Name = "extractLUAScriptToolStripMenuItem";
            this.extractLUAScriptToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.extractLUAScriptToolStripMenuItem.Text = "Extract LUA Script";
            this.extractLUAScriptToolStripMenuItem.Click += new System.EventHandler(this.extractLUAScriptToolStripMenuItem_Click);
            // 
            // injectLUAScriptToolStripMenuItem
            // 
            this.injectLUAScriptToolStripMenuItem.Name = "injectLUAScriptToolStripMenuItem";
            this.injectLUAScriptToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.injectLUAScriptToolStripMenuItem.Text = "Inject LUA Bytecode";
            this.injectLUAScriptToolStripMenuItem.Click += new System.EventHandler(this.injectLUAScriptToolStripMenuItem_Click);
            // 
            // extractLUABytecodeToolStripMenuItem
            // 
            this.extractLUABytecodeToolStripMenuItem.Name = "extractLUABytecodeToolStripMenuItem";
            this.extractLUABytecodeToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.extractLUABytecodeToolStripMenuItem.Text = "Extract LUA Bytecode";
            this.extractLUABytecodeToolStripMenuItem.Click += new System.EventHandler(this.extractLUABytecodeToolStripMenuItem_Click);
            // 
            // addLUAScriptToolStripMenuItem
            // 
            this.addLUAScriptToolStripMenuItem.Name = "addLUAScriptToolStripMenuItem";
            this.addLUAScriptToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.addLUAScriptToolStripMenuItem.Text = "Add LUA Script";
            this.addLUAScriptToolStripMenuItem.Click += new System.EventHandler(this.addLUAScriptToolStripMenuItem_Click);
            // 
            // ddsContext
            // 
            this.ddsContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addDDSToolStripMenuItem1,
            this.injectDDSToolStripMenuItem1,
            this.renameDDSToolStripMenuItem,
            this.extractDDSToolStripMenuItem,
            this.deleteDDSToolStripMenuItem});
            this.ddsContext.Name = "ddsContext";
            this.ddsContext.Size = new System.Drawing.Size(143, 114);
            // 
            // addDDSToolStripMenuItem1
            // 
            this.addDDSToolStripMenuItem1.Name = "addDDSToolStripMenuItem1";
            this.addDDSToolStripMenuItem1.Size = new System.Drawing.Size(142, 22);
            this.addDDSToolStripMenuItem1.Text = "Add DDS";
            this.addDDSToolStripMenuItem1.Click += new System.EventHandler(this.addDDSToolStripMenuItem1_Click);
            // 
            // injectDDSToolStripMenuItem1
            // 
            this.injectDDSToolStripMenuItem1.Name = "injectDDSToolStripMenuItem1";
            this.injectDDSToolStripMenuItem1.Size = new System.Drawing.Size(142, 22);
            this.injectDDSToolStripMenuItem1.Text = "Inject DDS";
            this.injectDDSToolStripMenuItem1.Click += new System.EventHandler(this.injectDDSToolStripMenuItem1_Click);
            // 
            // renameDDSToolStripMenuItem
            // 
            this.renameDDSToolStripMenuItem.Name = "renameDDSToolStripMenuItem";
            this.renameDDSToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.renameDDSToolStripMenuItem.Text = "Rename DDS";
            this.renameDDSToolStripMenuItem.Click += new System.EventHandler(this.renameDDSToolStripMenuItem_Click);
            // 
            // extractDDSToolStripMenuItem
            // 
            this.extractDDSToolStripMenuItem.Name = "extractDDSToolStripMenuItem";
            this.extractDDSToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.extractDDSToolStripMenuItem.Text = "Extract DDS";
            this.extractDDSToolStripMenuItem.Click += new System.EventHandler(this.extractDDSToolStripMenuItem_Click);
            // 
            // deleteDDSToolStripMenuItem
            // 
            this.deleteDDSToolStripMenuItem.Name = "deleteDDSToolStripMenuItem";
            this.deleteDDSToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.deleteDDSToolStripMenuItem.Text = "Delete DDS";
            this.deleteDDSToolStripMenuItem.Click += new System.EventHandler(this.deleteDDSToolStripMenuItem_Click);
            // 
            // matContext
            // 
            this.matContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addMaterialToolStripMenuItem,
            this.deleteMaterialToolStripMenuItem1});
            this.matContext.Name = "matContext";
            this.matContext.Size = new System.Drawing.Size(154, 48);
            // 
            // addMaterialToolStripMenuItem
            // 
            this.addMaterialToolStripMenuItem.Name = "addMaterialToolStripMenuItem";
            this.addMaterialToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.addMaterialToolStripMenuItem.Text = "Add Material";
            this.addMaterialToolStripMenuItem.Click += new System.EventHandler(this.addMaterialToolStripMenuItem_Click);
            // 
            // deleteMaterialToolStripMenuItem1
            // 
            this.deleteMaterialToolStripMenuItem1.Name = "deleteMaterialToolStripMenuItem1";
            this.deleteMaterialToolStripMenuItem1.Size = new System.Drawing.Size(153, 22);
            this.deleteMaterialToolStripMenuItem1.Text = "Delete Material";
            this.deleteMaterialToolStripMenuItem1.Click += new System.EventHandler(this.deleteMaterialToolStripMenuItem1_Click);
            // 
            // csbContext
            // 
            this.csbContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.injectCSBToolStripMenuItem,
            this.extractCSBToolStripMenuItem});
            this.csbContext.Name = "csbContext";
            this.csbContext.Size = new System.Drawing.Size(135, 48);
            this.csbContext.Opening += new System.ComponentModel.CancelEventHandler(this.csbContext_Opening);
            // 
            // injectCSBToolStripMenuItem
            // 
            this.injectCSBToolStripMenuItem.Name = "injectCSBToolStripMenuItem";
            this.injectCSBToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.injectCSBToolStripMenuItem.Text = "Inject CSB";
            this.injectCSBToolStripMenuItem.Click += new System.EventHandler(this.injectCSBToolStripMenuItem_Click);
            // 
            // extractCSBToolStripMenuItem
            // 
            this.extractCSBToolStripMenuItem.Name = "extractCSBToolStripMenuItem";
            this.extractCSBToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.extractCSBToolStripMenuItem.Text = "Extract CSB";
            this.extractCSBToolStripMenuItem.Click += new System.EventHandler(this.extractCSBToolStripMenuItem_Click);
            // 
            // animationContext
            // 
            this.animationContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InjectAnimationtoolStripMenuItem1,
            this.AddAnimationtoolStripMenuItem2,
            this.DeleteAnimaiontoolStripMenuItem3,
            this.dumpRefPoseToSMDToolStripMenuItem,
            this.rawDumpEMAToolStripMenuItem});
            this.animationContext.Name = "treeContext";
            this.animationContext.Size = new System.Drawing.Size(198, 136);
            this.animationContext.Text = "animationContext";
            this.animationContext.Opening += new System.ComponentModel.CancelEventHandler(this.animationContext_Opening);
            // 
            // InjectAnimationtoolStripMenuItem1
            // 
            this.InjectAnimationtoolStripMenuItem1.Name = "InjectAnimationtoolStripMenuItem1";
            this.InjectAnimationtoolStripMenuItem1.Size = new System.Drawing.Size(197, 22);
            this.InjectAnimationtoolStripMenuItem1.Text = "Inject Animation";
            this.InjectAnimationtoolStripMenuItem1.Click += new System.EventHandler(this.InjectAnimationtoolStripMenuItem1_Click);
            // 
            // AddAnimationtoolStripMenuItem2
            // 
            this.AddAnimationtoolStripMenuItem2.Name = "AddAnimationtoolStripMenuItem2";
            this.AddAnimationtoolStripMenuItem2.Size = new System.Drawing.Size(197, 22);
            this.AddAnimationtoolStripMenuItem2.Text = "Add Animation";
            this.AddAnimationtoolStripMenuItem2.Click += new System.EventHandler(this.AddAnimationtoolStripMenuItem2_Click);
            // 
            // DeleteAnimaiontoolStripMenuItem3
            // 
            this.DeleteAnimaiontoolStripMenuItem3.Name = "DeleteAnimaiontoolStripMenuItem3";
            this.DeleteAnimaiontoolStripMenuItem3.Size = new System.Drawing.Size(197, 22);
            this.DeleteAnimaiontoolStripMenuItem3.Text = "Delete Animation";
            this.DeleteAnimaiontoolStripMenuItem3.Click += new System.EventHandler(this.DeleteAnimaiontoolStripMenuItem3_Click);
            // 
            // dumpRefPoseToSMDToolStripMenuItem
            // 
            this.dumpRefPoseToSMDToolStripMenuItem.Name = "dumpRefPoseToSMDToolStripMenuItem";
            this.dumpRefPoseToSMDToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.dumpRefPoseToSMDToolStripMenuItem.Text = "Dump Ref Pose to SMD";
            this.dumpRefPoseToSMDToolStripMenuItem.Click += new System.EventHandler(this.dumpRefPoseToSMDToolStripMenuItem_Click);
            // 
            // rawDumpEMAToolStripMenuItem
            // 
            this.rawDumpEMAToolStripMenuItem.Name = "rawDumpEMAToolStripMenuItem";
            this.rawDumpEMAToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.rawDumpEMAToolStripMenuItem.Text = "Raw Dump EMA";
            this.rawDumpEMAToolStripMenuItem.Click += new System.EventHandler(this.rawDumpEMAToolStripMenuItem_Click);
            // 
            // emoContext
            // 
            this.emoContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertOBJAsNewEMGToolStripMenuItem1,
            this.expandAllToolStripMenuItem,
            this.collapseAllToolStripMenuItem,
            this.InjectEMO,
            this.rawDumpEMOAsSMDToolStripMenuItem,
            this.rawDumpEMOToolStripMenuItem});
            this.emoContext.Name = "luaContext";
            this.emoContext.Size = new System.Drawing.Size(259, 136);
            this.emoContext.Opening += new System.ComponentModel.CancelEventHandler(this.emoContext_Opening);
            // 
            // insertOBJAsNewEMGToolStripMenuItem1
            // 
            this.insertOBJAsNewEMGToolStripMenuItem1.Name = "insertOBJAsNewEMGToolStripMenuItem1";
            this.insertOBJAsNewEMGToolStripMenuItem1.Size = new System.Drawing.Size(258, 22);
            this.insertOBJAsNewEMGToolStripMenuItem1.Text = "Insert OBJ As New EMG";
            this.insertOBJAsNewEMGToolStripMenuItem1.Click += new System.EventHandler(this.insertOBJAsNewEMGToolStripMenuItem1_Click);
            // 
            // expandAllToolStripMenuItem
            // 
            this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
            this.expandAllToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.expandAllToolStripMenuItem.Text = "Expand All";
            this.expandAllToolStripMenuItem.Click += new System.EventHandler(this.expandAllToolStripMenuItem_Click);
            // 
            // collapseAllToolStripMenuItem
            // 
            this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            this.collapseAllToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.collapseAllToolStripMenuItem.Text = "Collapse All";
            this.collapseAllToolStripMenuItem.Click += new System.EventHandler(this.collapseAllToolStripMenuItem_Click);
            // 
            // InjectEMO
            // 
            this.InjectEMO.Name = "InjectEMO";
            this.InjectEMO.Size = new System.Drawing.Size(258, 22);
            this.InjectEMO.Text = "Inject SMD as EMO (Experimental)";
            this.InjectEMO.Click += new System.EventHandler(this.InjectEMO_Click);
            // 
            // rawDumpEMOAsSMDToolStripMenuItem
            // 
            this.rawDumpEMOAsSMDToolStripMenuItem.Name = "rawDumpEMOAsSMDToolStripMenuItem";
            this.rawDumpEMOAsSMDToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.rawDumpEMOAsSMDToolStripMenuItem.Text = "Dump EMO as SMD (Experimental)";
            this.rawDumpEMOAsSMDToolStripMenuItem.Click += new System.EventHandler(this.rawDumpEMOAsSMDToolStripMenuItem_Click);
            // 
            // rawDumpEMOToolStripMenuItem
            // 
            this.rawDumpEMOToolStripMenuItem.Name = "rawDumpEMOToolStripMenuItem";
            this.rawDumpEMOToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.rawDumpEMOToolStripMenuItem.Text = "Raw Dump EMO";
            this.rawDumpEMOToolStripMenuItem.Click += new System.EventHandler(this.rawDumpEMOToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1051, 631);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pnlOBJECTS);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "USF4 Stage Tool by BEAR & JingoJungle";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.pnlOBJECTS.ResumeLayout(false);
            this.pSelectedTreeNodeData.ResumeLayout(false);
            this.pnlEO_MaterialEdit.ResumeLayout(false);
            this.pnlEO_MaterialEdit.PerformLayout();
            this.pnlEO_SUBMOD.ResumeLayout(false);
            this.pnlEO_SUBMOD.PerformLayout();
            this.pnlEO_MOD.ResumeLayout(false);
            this.pnlEO_MOD.PerformLayout();
            this.pnlEO_EMG.ResumeLayout(false);
            this.pnlEO_EMG.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPreviewDDS)).EndInit();
            this.pOBJProperties.ResumeLayout(false);
            this.pOBJProperties.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.emgContext.ResumeLayout(false);
            this.emzContext.ResumeLayout(false);
            this.emmContext.ResumeLayout(false);
            this.embContext.ResumeLayout(false);
            this.luaContext.ResumeLayout(false);
            this.ddsContext.ResumeLayout(false);
            this.matContext.ResumeLayout(false);
            this.csbContext.ResumeLayout(false);
            this.animationContext.ResumeLayout(false);
            this.emoContext.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOpenOBJ;
        private System.Windows.Forms.OpenFileDialog diagOpenOBJ;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel pnlOBJECTS;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem consoleMessagesOutputToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem informationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uSF4ModdingDocumentToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lbLoadSteps;
        private System.Windows.Forms.TreeView tvTree;
        private System.Windows.Forms.Button btnOpenEMZ;
        private System.Windows.Forms.ContextMenuStrip emgContext;
        private System.Windows.Forms.ToolStripMenuItem emgContextInjectOBJ;
        private System.Windows.Forms.ToolStripMenuItem insertOBJAsNewEMGToolStripMenuItem;
        private System.Windows.Forms.Panel pOBJProperties;
        private System.Windows.Forms.TextBox tbTextureIndex;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox chkTextureFlipX;
        private System.Windows.Forms.CheckBox chkTextureFlipY;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbScaleV;
        private System.Windows.Forms.TextBox tbScaleU;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel pSelectedTreeNodeData;
        private System.Windows.Forms.Label lbSelNODE_Title;
        private System.Windows.Forms.ListBox lbSelNODE_ListData;
        private System.Windows.Forms.Label lbOBJNameProperty;
        private System.Windows.Forms.Panel pnlEO_EMG;
        private System.Windows.Forms.TextBox tbEMGRootBone;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button bntEO_EMGSave;
        private System.Windows.Forms.Panel pnlEO_MOD;
        private System.Windows.Forms.Button bntEO_ModSave;
        private System.Windows.Forms.TextBox tbEOMod_TextureIndex;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ToolStripMenuItem deleteEMGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveEncodedOBJToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveEncodedOBJToHEXToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkGeometryFlipX;
        private System.Windows.Forms.Panel pnlEO_SUBMOD;
        private System.Windows.Forms.Button bntEO_SubModSave;
        private System.Windows.Forms.TextBox tbEO_SubModName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ContextMenuStrip emzContext;
        private System.Windows.Forms.ToolStripMenuItem saveEMZToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeEMZToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip cmEmpty;
        private System.Windows.Forms.ContextMenuStrip emmContext;
        private System.Windows.Forms.ContextMenuStrip embContext;
        private System.Windows.Forms.ContextMenuStrip luaContext;
        private System.Windows.Forms.ToolStripMenuItem addNewMaterialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem injectDDSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addDDSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem injectLUAScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractLUAScriptToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip ddsContext;
        private System.Windows.Forms.ToolStripMenuItem injectDDSToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem addDDSToolStripMenuItem1;
        private System.Windows.Forms.PictureBox pbPreviewDDS;
        private System.Windows.Forms.ContextMenuStrip matContext;
        private System.Windows.Forms.ToolStripMenuItem addMaterialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteMaterialToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem deleteDDSToolStripMenuItem;
        private System.Windows.Forms.Button btnSaveEMZ;
        private System.Windows.Forms.Button btnSaveTEXEMZ;
        private System.Windows.Forms.Panel pnlEO_MaterialEdit;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox cbShaders;
        private System.Windows.Forms.Label lbShader;
        private System.Windows.Forms.Button btnEO_ShaderEditSave;
        private System.Windows.Forms.TextBox lvShaderProperties;
        private System.Windows.Forms.ComboBox cbShaderProperties;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button btnCalculateFloat;
        private System.Windows.Forms.TextBox SPOutFloat;
        private System.Windows.Forms.TextBox SPINFloat;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ToolStripMenuItem renameDDSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractDDSToolStripMenuItem;
        private System.Windows.Forms.ListBox lvStatus;
        private System.Windows.Forms.ToolStripMenuItem exctractAllDDSToolStripMenuItem;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ToolStripMenuItem exctractTEXEMZTexturesToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip csbContext;
        private System.Windows.Forms.ToolStripMenuItem injectCSBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractCSBToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip animationContext;
        private System.Windows.Forms.ToolStripMenuItem InjectAnimationtoolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem AddAnimationtoolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem DeleteAnimaiontoolStripMenuItem3;
        private System.Windows.Forms.ContextMenuStrip emoContext;
        private System.Windows.Forms.ToolStripMenuItem InjectEMO;
        private System.Windows.Forms.ToolStripMenuItem dumpRefPoseToSMDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rawDumpEMOAsSMDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractLUABytecodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertOBJAsNewEMGToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem injectSMDAsEMGExperimentalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem injectLUAScriptToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem injectFileExperimentalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addLUAScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rawDumpEMOToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rawDumpEMAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem duplicateEMGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem duplicateModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem duplicateUSAMAN01BToolStripMenuItem;
    }
}

