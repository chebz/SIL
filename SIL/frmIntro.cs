using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace SIL
{
    public partial class frmIntro : Form
    {
        public frmIntro()
        {
            InitializeComponent();
        }

        private void frmIntro_Load(object sender, EventArgs e)
        {
            lblVersion.Text = GlobalSettings.VERSION;
            Thread t = new Thread(delegate() { 
                Thread.Sleep(GlobalSettings.INTRODSIPLAYCD);
                //all content loading and initialization goes here...
                this.Invoke(new MethodInvoker(delegate { this.Close(); }));
            });
            t.Start();
            
        }
    }
}
