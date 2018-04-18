using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace New_T_Bot
{
    public partial class Login : Form
    {
        public string username;
        public bool CloseForm = false;
        public Login()
        {
            InitializeComponent();
        }

        private void Ok_bt_Click(object sender, EventArgs e)
        {
            if (Username_tb.Text !="")
            {
                char[] letters = Username_tb.Text.ToCharArray();
                letters[0] = char.ToUpper(letters[0]);
                username = new string(letters);
                CloseForm = true;
                this.Hide();
            }
        }

        private void Username_tb_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (Username_tb.Text != "")
                {
                    char[] letters = Username_tb.Text.ToCharArray();
                    letters[0] = char.ToUpper(letters[0]);
                    username = new string(letters);
                    CloseForm = true;
                    this.Hide();
                }
            }
        }

        
    }
}
