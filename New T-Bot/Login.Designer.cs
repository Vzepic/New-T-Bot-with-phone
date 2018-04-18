namespace New_T_Bot
{
    partial class Login
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Username_tb = new System.Windows.Forms.TextBox();
            this.Password_tb = new System.Windows.Forms.TextBox();
            this.Ok_bt = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Please enter your username";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Username";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Password";
            this.label3.Visible = false;
            // 
            // Username_tb
            // 
            this.Username_tb.Location = new System.Drawing.Point(73, 40);
            this.Username_tb.Name = "Username_tb";
            this.Username_tb.Size = new System.Drawing.Size(146, 20);
            this.Username_tb.TabIndex = 3;
            this.Username_tb.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Username_tb_KeyPress);
            // 
            // Password_tb
            // 
            this.Password_tb.Location = new System.Drawing.Point(73, 76);
            this.Password_tb.Name = "Password_tb";
            this.Password_tb.Size = new System.Drawing.Size(146, 20);
            this.Password_tb.TabIndex = 4;
            this.Password_tb.UseSystemPasswordChar = true;
            this.Password_tb.Visible = false;
            // 
            // Ok_bt
            // 
            this.Ok_bt.Location = new System.Drawing.Point(225, 76);
            this.Ok_bt.Name = "Ok_bt";
            this.Ok_bt.Size = new System.Drawing.Size(42, 20);
            this.Ok_bt.TabIndex = 5;
            this.Ok_bt.Text = "OK";
            this.Ok_bt.UseVisualStyleBackColor = true;
            this.Ok_bt.Click += new System.EventHandler(this.Ok_bt_Click);
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(276, 124);
            this.ControlBox = false;
            this.Controls.Add(this.Ok_bt);
            this.Controls.Add(this.Password_tb);
            this.Controls.Add(this.Username_tb);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Login";
            this.Text = "Login";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox Username_tb;
        private System.Windows.Forms.TextBox Password_tb;
        private System.Windows.Forms.Button Ok_bt;
    }
}