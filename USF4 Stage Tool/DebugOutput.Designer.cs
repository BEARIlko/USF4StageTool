namespace USF4_Stage_Tool
{
	partial class DebugOutput
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DebugOutput));
			this.tbVisualConsole = new System.Windows.Forms.ListBox();
			this.btnSaveVC = new System.Windows.Forms.Button();
			this.btnClearVC = new System.Windows.Forms.Button();
			this.saveFileDialogDebug = new System.Windows.Forms.SaveFileDialog();
			this.SuspendLayout();
			// 
			// tbVisualConsole
			// 
			this.tbVisualConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbVisualConsole.BackColor = System.Drawing.Color.Black;
			this.tbVisualConsole.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.tbVisualConsole.ForeColor = System.Drawing.Color.Lime;
			this.tbVisualConsole.FormattingEnabled = true;
			this.tbVisualConsole.ItemHeight = 16;
			this.tbVisualConsole.Location = new System.Drawing.Point(3, 37);
			this.tbVisualConsole.Name = "tbVisualConsole";
			this.tbVisualConsole.Size = new System.Drawing.Size(583, 548);
			this.tbVisualConsole.TabIndex = 3;
			// 
			// btnSaveVC
			// 
			this.btnSaveVC.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.btnSaveVC.Location = new System.Drawing.Point(3, 3);
			this.btnSaveVC.Name = "btnSaveVC";
			this.btnSaveVC.Size = new System.Drawing.Size(136, 28);
			this.btnSaveVC.TabIndex = 0;
			this.btnSaveVC.Text = "Save Output";
			this.btnSaveVC.UseVisualStyleBackColor = true;
			this.btnSaveVC.Click += new System.EventHandler(this.BtnSaveVC_Click);
			// 
			// btnClearVC
			// 
			this.btnClearVC.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.btnClearVC.Location = new System.Drawing.Point(145, 3);
			this.btnClearVC.Name = "btnClearVC";
			this.btnClearVC.Size = new System.Drawing.Size(136, 28);
			this.btnClearVC.TabIndex = 1;
			this.btnClearVC.Text = "Clear";
			this.btnClearVC.UseVisualStyleBackColor = true;
			this.btnClearVC.Click += new System.EventHandler(this.BtnClearVC_Click);
			// 
			// DebugOutput
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(588, 593);
			this.Controls.Add(this.btnClearVC);
			this.Controls.Add(this.btnSaveVC);
			this.Controls.Add(this.tbVisualConsole);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "DebugOutput";
			this.Text = "Debug Output";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DebugOutput_FormClosing);
			this.Shown += new System.EventHandler(this.DebugOutput_Shown);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox tbVisualConsole;
		private System.Windows.Forms.Button btnSaveVC;
		private System.Windows.Forms.Button btnClearVC;
		private System.Windows.Forms.SaveFileDialog saveFileDialogDebug;
	}
}