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
    public partial class frmErrList : Form
    {
        string programName_;

        public frmErrList()
        {
            InitializeComponent();
        }

        public void Initialize(string programName)
        {
            programName_ = programName;            
        }

        public void AddError(string message, int lineNumber)
        {
            object[] parameters = new object[3];
            parameters[0] = dgvErrors.Rows.Count;
            parameters[1] = message;
            parameters[2] = lineNumber;
            dgvErrors.Rows.Add(parameters);
        }

        public void Clear()
        {
            dgvErrors.Rows.Clear();
        }

        public string ProgramName { get { return programName_; } }
    }
}
