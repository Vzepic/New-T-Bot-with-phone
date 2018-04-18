namespace New_T_Bot
{
    partial class ConnectingDialog
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
            this.ConnectingProgressBar = new System.Windows.Forms.ProgressBar();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ConnectingProgressBar
            // 
            this.ConnectingProgressBar.Location = new System.Drawing.Point(2, 4);
            this.ConnectingProgressBar.Name = "ConnectingProgressBar";
            this.ConnectingProgressBar.Size = new System.Drawing.Size(191, 23);
            this.ConnectingProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.ConnectingProgressBar.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(60, 33);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 22);
            this.button1.TabIndex = 1;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ConnectingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(195, 55);
            this.ControlBox = false;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ConnectingProgressBar);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConnectingDialog";
            this.Text = "Connecting";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar ConnectingProgressBar;
        private System.Windows.Forms.Button button1;
    }
}