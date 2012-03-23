namespace NetSoapClientCs
{
    partial class PerformanceReport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PerformanceReport));
            this.RichReport = new System.Windows.Forms.RichTextBox();
            this.ToolBar = new System.Windows.Forms.ToolStrip();
            this.BtnSaveReport = new System.Windows.Forms.ToolStripButton();
            this.ToolBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // RichReport
            // 
            this.RichReport.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.RichReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RichReport.Location = new System.Drawing.Point(1, 26);
            this.RichReport.Name = "RichReport";
            this.RichReport.Size = new System.Drawing.Size(685, 407);
            this.RichReport.TabIndex = 3;
            this.RichReport.Text = "";
            // 
            // ToolBar
            // 
            this.ToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BtnSaveReport});
            this.ToolBar.Location = new System.Drawing.Point(0, 0);
            this.ToolBar.Name = "ToolBar";
            this.ToolBar.Size = new System.Drawing.Size(686, 25);
            this.ToolBar.TabIndex = 5;
            this.ToolBar.Text = "toolStrip1";
            // 
            // BtnSaveReport
            // 
            this.BtnSaveReport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnSaveReport.Image = global::NetSoapClientCs.Properties.Resources.filesave;
            this.BtnSaveReport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnSaveReport.Name = "BtnSaveReport";
            this.BtnSaveReport.Size = new System.Drawing.Size(23, 22);
            this.BtnSaveReport.Text = "toolStripButton1";
            this.BtnSaveReport.Click += new System.EventHandler(this.BtnSaveReport_Click);
            // 
            // PerformanceReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(686, 436);
            this.Controls.Add(this.ToolBar);
            this.Controls.Add(this.RichReport);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.Desktop;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximumSize = new System.Drawing.Size(694, 470);
            this.Name = "PerformanceReport";
            this.Text = "Last Performance Report";
            this.Load += new System.EventHandler(this.PerformanceReport_Load);
            this.ToolBar.ResumeLayout(false);
            this.ToolBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox RichReport;
        private System.Windows.Forms.ToolStrip ToolBar;
        private System.Windows.Forms.ToolStripButton BtnSaveReport;
    }
}