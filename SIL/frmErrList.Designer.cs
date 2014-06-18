namespace SIL
{
    partial class frmErrList
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
            this.dgvErrors = new System.Windows.Forms.DataGridView();
            this.Number = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Line = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrors)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvErrors
            // 
            this.dgvErrors.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvErrors.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Number,
            this.Message,
            this.Line});
            this.dgvErrors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvErrors.Location = new System.Drawing.Point(0, 0);
            this.dgvErrors.Name = "dgvErrors";
            this.dgvErrors.Size = new System.Drawing.Size(634, 152);
            this.dgvErrors.TabIndex = 0;
            // 
            // Number
            // 
            this.Number.HeaderText = "#";
            this.Number.Name = "Number";
            this.Number.Width = 40;
            // 
            // Message
            // 
            this.Message.HeaderText = "Message";
            this.Message.Name = "Message";
            this.Message.Width = 500;
            // 
            // Line
            // 
            this.Line.HeaderText = "Line Number";
            this.Line.Name = "Line";
            this.Line.Width = 50;
            // 
            // frmErrList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 152);
            this.Controls.Add(this.dgvErrors);
            this.Name = "frmErrList";
            this.Text = "Error List";
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrors)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvErrors;
        private System.Windows.Forms.DataGridViewTextBoxColumn Number;
        private System.Windows.Forms.DataGridViewTextBoxColumn Message;
        private System.Windows.Forms.DataGridViewTextBoxColumn Line;
    }
}