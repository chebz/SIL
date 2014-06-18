using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SIL_Core;

namespace SIL
{
    public partial class frmOutput : Form
    {
        string programName_;
        frmDrawLine frmDrawline_;

        public frmOutput()
        {
            InitializeComponent();
            frmDrawline_ = new frmDrawLine();
        }

        public void Initialize(string programName)
        {
            programName_ = programName;
        }

        public void ExecuteProgram(SIL_Program program, frmErrList errList)
        {
            this.Text = "Output - " + program.Name;
            try
            {
                program.Parse();
                program.Execute();
                
            }
            catch (Exception e)
            {
                string[] err = e.Message.Split('%');
                if (err.Length > 1)
                    errList.AddError(err[0], int.Parse(err[1]));
                else errList.AddError(err[0], 0);
            }
        }

        private void frmOutput_Load(object sender, EventArgs e)
        {
            Compiler.OnAction += new Compiler.Action(Compiler_OnAction);
        }

        void Compiler_OnAction(ActionType actionType, object[] args)
        {
            if (Compiler.ProgramName != programName_) return;
            switch (actionType)
            {
                case ActionType.PRINT:
                    txtOutput.Text += args[0];
                    break;
                case ActionType.PRINTLN:
                    txtOutput.Text += args[0] + "\n";
                    break;
                case ActionType.READ:
                    frmRead readVar = new frmRead();
                    readVar.ShowDialog((string)args[1]);
                    ((SILVariable)args[0]).Value = (SILInteger)readVar.Var;
                    break;
                case ActionType.DRAWLINE:
                    frmDrawline_.addLine((string)args[0], (int)args[1], (int)args[2], (int)args[3], (int)args[4]);
                    frmDrawline_.Show();
                    break;
            }
        }
        public void Clear()
        {
            txtOutput.Text = "";
        }

        public string ProgramName { get { return programName_; } }
    }
}
