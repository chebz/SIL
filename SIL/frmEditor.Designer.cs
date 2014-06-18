namespace SIL
{
    partial class frmEditor
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
            this.txtCode = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // txtCode
            // 
            this.txtCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCode.Location = new System.Drawing.Point(0, 0);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(501, 414);
            this.txtCode.TabIndex = 0;
            this.txtCode.Text = "";
            // 
            // frmEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 414);
            this.Controls.Add(this.txtCode);
            this.Name = "frmEditor";
            this.Text = "Editor";
            this.Activated += new System.EventHandler(this.frmEditor_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmEditor_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtCode;
    }
}