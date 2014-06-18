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
    public partial class frmWizard : Form
    {
        public delegate void OnCreateHandler(string programName);
        public static event OnCreateHandler OnCreate;

        public frmWizard()
        {
            InitializeComponent();
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bCreate_Click(object sender, EventArgs e)
        {
            OnCreate(txtProgramName.Text);
            this.Close();
        }
    }
}
