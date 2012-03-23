namespace NetSoapClientCs
{
    partial class SoapActions
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
            this.BtnBrowseForWsdl = new System.Windows.Forms.Button();
            this.InputFilesLabel = new System.Windows.Forms.Label();
            this.WsdlPath = new System.Windows.Forms.TextBox();
            this.ListSoapActions = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BtnPopulateSoapActions = new System.Windows.Forms.Button();
            this.BtnOkSoapActions = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnBrowseForWsdl
            // 
            this.BtnBrowseForWsdl.Location = new System.Drawing.Point(412, 18);
            this.BtnBrowseForWsdl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.BtnBrowseForWsdl.Name = "BtnBrowseForWsdl";
            this.BtnBrowseForWsdl.Size = new System.Drawing.Size(29, 20);
            this.BtnBrowseForWsdl.TabIndex = 2;
            this.BtnBrowseForWsdl.Text = "...";
            this.BtnBrowseForWsdl.UseVisualStyleBackColor = true;
            this.BtnBrowseForWsdl.Click += new System.EventHandler(this.BtnBrowseForWsdl_Click);
            // 
            // InputFilesLabel
            // 
            this.InputFilesLabel.AutoSize = true;
            this.InputFilesLabel.Location = new System.Drawing.Point(-2, 3);
            this.InputFilesLabel.Name = "InputFilesLabel";
            this.InputFilesLabel.Size = new System.Drawing.Size(148, 13);
            this.InputFilesLabel.TabIndex = 14;
            this.InputFilesLabel.Text = "Path to WSDL or WSDL URI:";
            this.InputFilesLabel.Click += new System.EventHandler(this.InputFilesLabel_Click);
            // 
            // WsdlPath
            // 
            this.WsdlPath.Location = new System.Drawing.Point(0, 18);
            this.WsdlPath.Name = "WsdlPath";
            this.WsdlPath.Size = new System.Drawing.Size(411, 20);
            this.WsdlPath.TabIndex = 1;
            this.WsdlPath.TextChanged += new System.EventHandler(this.WsdlPath_TextChanged);
            // 
            // ListSoapActions
            // 
            this.ListSoapActions.FormattingEnabled = true;
            this.ListSoapActions.Location = new System.Drawing.Point(1, 54);
            this.ListSoapActions.Name = "ListSoapActions";
            this.ListSoapActions.Size = new System.Drawing.Size(411, 264);
            this.ListSoapActions.Sorted = true;
            this.ListSoapActions.TabIndex = 4;
            this.ListSoapActions.DoubleClick += new System.EventHandler(this.ListSoapActions_DoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-2, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "SOAP Actions:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // BtnPopulateSoapActions
            // 
            this.BtnPopulateSoapActions.Image = global::NetSoapClientCs.Properties.Resources.gen_run_narrow_left;
            this.BtnPopulateSoapActions.Location = new System.Drawing.Point(412, 54);
            this.BtnPopulateSoapActions.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.BtnPopulateSoapActions.Name = "BtnPopulateSoapActions";
            this.BtnPopulateSoapActions.Size = new System.Drawing.Size(29, 20);
            this.BtnPopulateSoapActions.TabIndex = 3;
            this.BtnPopulateSoapActions.UseVisualStyleBackColor = true;
            this.BtnPopulateSoapActions.Click += new System.EventHandler(this.BtnPopulateSoapActions_Click);
            // 
            // BtnOkSoapActions
            // 
            this.BtnOkSoapActions.Location = new System.Drawing.Point(412, 297);
            this.BtnOkSoapActions.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.BtnOkSoapActions.Name = "BtnOkSoapActions";
            this.BtnOkSoapActions.Size = new System.Drawing.Size(29, 20);
            this.BtnOkSoapActions.TabIndex = 5;
            this.BtnOkSoapActions.Text = "Ok";
            this.BtnOkSoapActions.UseVisualStyleBackColor = true;
            this.BtnOkSoapActions.Click += new System.EventHandler(this.BtnOkSoapActions_Click);
            // 
            // SoapActions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(441, 318);
            this.Controls.Add(this.BtnOkSoapActions);
            this.Controls.Add(this.BtnPopulateSoapActions);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ListSoapActions);
            this.Controls.Add(this.BtnBrowseForWsdl);
            this.Controls.Add(this.InputFilesLabel);
            this.Controls.Add(this.WsdlPath);
            this.Name = "SoapActions";
            this.Text = "SoapActions";
            this.Load += new System.EventHandler(this.SoapActions_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Button BtnBrowseForWsdl;
        public System.Windows.Forms.Label InputFilesLabel;
        public System.Windows.Forms.TextBox WsdlPath;
        private System.Windows.Forms.ListBox ListSoapActions;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Button BtnPopulateSoapActions;
        public System.Windows.Forms.Button BtnOkSoapActions;
    }
}