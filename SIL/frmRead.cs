using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SIL
{
    public partial class frmRead : Form
    {
        int var_;

        public frmRead()
        {
            InitializeComponent();
        }

        public DialogResult ShowDialog(string name)
        {
            lblText.Text = name;
            return this.ShowDialog();
        }

        private void bSubmit_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtInput.Text, out var_))
            {
                MessageBox.Show("Incorrect input, must be numeric!", "Input error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            this.Close();
        }

        public int Var { get { return var_; } }
    }
}
