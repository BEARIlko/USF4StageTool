
namespace USF4_Stage_Tool
{
    partial class CompressDaisyChain
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
            this.lb_Title = new System.Windows.Forms.Label();
            this.lb_SubmodelName = new System.Windows.Forms.Label();
            this.bt_Cancel = new System.Windows.Forms.Button();
            this.bt_Confirm = new System.Windows.Forms.Button();
            this.bt_StartPause = new System.Windows.Forms.Button();
            this.lb_InitCompressionTitle = new System.Windows.Forms.Label();
            this.lb_InitCompressionValue = new System.Windows.Forms.Label();
            this.lb_BestCompressionValue = new System.Windows.Forms.Label();
            this.lb_BestCompressionTitle = new System.Windows.Forms.Label();
            this.lb_AttemptsTitle = new System.Windows.Forms.Label();
            this.lb_AttemptsValue = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lb_Title
            // 
            this.lb_Title.AutoSize = true;
            this.lb_Title.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_Title.Location = new System.Drawing.Point(13, 13);
            this.lb_Title.Name = "lb_Title";
            this.lb_Title.Size = new System.Drawing.Size(82, 16);
            this.lb_Title.TabIndex = 0;
            this.lb_Title.Text = "Submodel:";
            // 
            // lb_SubmodelName
            // 
            this.lb_SubmodelName.AutoSize = true;
            this.lb_SubmodelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_SubmodelName.Location = new System.Drawing.Point(13, 29);
            this.lb_SubmodelName.Name = "lb_SubmodelName";
            this.lb_SubmodelName.Size = new System.Drawing.Size(45, 16);
            this.lb_SubmodelName.TabIndex = 1;
            this.lb_SubmodelName.Text = "label2";
            // 
            // bt_Cancel
            // 
            this.bt_Cancel.Location = new System.Drawing.Point(13, 152);
            this.bt_Cancel.Name = "bt_Cancel";
            this.bt_Cancel.Size = new System.Drawing.Size(75, 23);
            this.bt_Cancel.TabIndex = 2;
            this.bt_Cancel.Text = "Cancel";
            this.bt_Cancel.UseVisualStyleBackColor = true;
            this.bt_Cancel.Click += new System.EventHandler(this.bt_Cancel_Click);
            // 
            // bt_Confirm
            // 
            this.bt_Confirm.Location = new System.Drawing.Point(212, 152);
            this.bt_Confirm.Name = "bt_Confirm";
            this.bt_Confirm.Size = new System.Drawing.Size(75, 23);
            this.bt_Confirm.TabIndex = 3;
            this.bt_Confirm.Text = "Confirm";
            this.bt_Confirm.UseVisualStyleBackColor = true;
            this.bt_Confirm.Click += new System.EventHandler(this.bt_Confirm_Click);
            // 
            // bt_StartPause
            // 
            this.bt_StartPause.Location = new System.Drawing.Point(112, 152);
            this.bt_StartPause.Name = "bt_StartPause";
            this.bt_StartPause.Size = new System.Drawing.Size(75, 23);
            this.bt_StartPause.TabIndex = 4;
            this.bt_StartPause.Text = "Start";
            this.bt_StartPause.UseVisualStyleBackColor = true;
            this.bt_StartPause.Click += new System.EventHandler(this.bt_StartPause_Click);
            // 
            // lb_InitCompressionTitle
            // 
            this.lb_InitCompressionTitle.AutoSize = true;
            this.lb_InitCompressionTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_InitCompressionTitle.Location = new System.Drawing.Point(13, 53);
            this.lb_InitCompressionTitle.Name = "lb_InitCompressionTitle";
            this.lb_InitCompressionTitle.Size = new System.Drawing.Size(144, 16);
            this.lb_InitCompressionTitle.TabIndex = 5;
            this.lb_InitCompressionTitle.Text = "Initial Compression:";
            // 
            // lb_InitCompressionValue
            // 
            this.lb_InitCompressionValue.AutoSize = true;
            this.lb_InitCompressionValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_InitCompressionValue.Location = new System.Drawing.Point(13, 69);
            this.lb_InitCompressionValue.Name = "lb_InitCompressionValue";
            this.lb_InitCompressionValue.Size = new System.Drawing.Size(45, 16);
            this.lb_InitCompressionValue.TabIndex = 6;
            this.lb_InitCompressionValue.Text = "label3";
            // 
            // lb_BestCompressionValue
            // 
            this.lb_BestCompressionValue.AutoSize = true;
            this.lb_BestCompressionValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_BestCompressionValue.Location = new System.Drawing.Point(13, 112);
            this.lb_BestCompressionValue.Name = "lb_BestCompressionValue";
            this.lb_BestCompressionValue.Size = new System.Drawing.Size(45, 16);
            this.lb_BestCompressionValue.TabIndex = 8;
            this.lb_BestCompressionValue.Text = "label3";
            // 
            // lb_BestCompressionTitle
            // 
            this.lb_BestCompressionTitle.AutoSize = true;
            this.lb_BestCompressionTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_BestCompressionTitle.Location = new System.Drawing.Point(13, 96);
            this.lb_BestCompressionTitle.Name = "lb_BestCompressionTitle";
            this.lb_BestCompressionTitle.Size = new System.Drawing.Size(138, 16);
            this.lb_BestCompressionTitle.TabIndex = 7;
            this.lb_BestCompressionTitle.Text = "Best Compression:";
            // 
            // lb_AttemptsTitle
            // 
            this.lb_AttemptsTitle.AutoSize = true;
            this.lb_AttemptsTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_AttemptsTitle.Location = new System.Drawing.Point(205, 13);
            this.lb_AttemptsTitle.Name = "lb_AttemptsTitle";
            this.lb_AttemptsTitle.Size = new System.Drawing.Size(72, 16);
            this.lb_AttemptsTitle.TabIndex = 9;
            this.lb_AttemptsTitle.Text = "Attempts:";
            // 
            // lb_AttemptsValue
            // 
            this.lb_AttemptsValue.AutoSize = true;
            this.lb_AttemptsValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_AttemptsValue.Location = new System.Drawing.Point(205, 29);
            this.lb_AttemptsValue.Name = "lb_AttemptsValue";
            this.lb_AttemptsValue.Size = new System.Drawing.Size(15, 16);
            this.lb_AttemptsValue.TabIndex = 10;
            this.lb_AttemptsValue.Text = "0";
            // 
            // CompressDaisyChain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(299, 187);
            this.Controls.Add(this.lb_AttemptsValue);
            this.Controls.Add(this.lb_AttemptsTitle);
            this.Controls.Add(this.lb_BestCompressionValue);
            this.Controls.Add(this.lb_BestCompressionTitle);
            this.Controls.Add(this.lb_InitCompressionValue);
            this.Controls.Add(this.lb_InitCompressionTitle);
            this.Controls.Add(this.bt_StartPause);
            this.Controls.Add(this.bt_Confirm);
            this.Controls.Add(this.bt_Cancel);
            this.Controls.Add(this.lb_SubmodelName);
            this.Controls.Add(this.lb_Title);
            this.Name = "CompressDaisyChain";
            this.Text = "CompressDaisyChain";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lb_Title;
        private System.Windows.Forms.Label lb_SubmodelName;
        private System.Windows.Forms.Button bt_Cancel;
        private System.Windows.Forms.Button bt_Confirm;
        private System.Windows.Forms.Button bt_StartPause;
        private System.Windows.Forms.Label lb_InitCompressionTitle;
        private System.Windows.Forms.Label lb_InitCompressionValue;
        private System.Windows.Forms.Label lb_BestCompressionValue;
        private System.Windows.Forms.Label lb_BestCompressionTitle;
        private System.Windows.Forms.Label lb_AttemptsTitle;
        private System.Windows.Forms.Label lb_AttemptsValue;
    }
}