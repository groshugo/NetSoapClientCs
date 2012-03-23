using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NetSoapClientCs
{
    public partial class SoapActions : Form
    {
        NetSoapClientCs par;
        public SoapActions()
        {
            InitializeComponent();
        }
        public SoapActions(NetSoapClientCs par)
        {
            InitializeComponent();
            this.par = (NetSoapClientCs) par;
        }

        private  void Populate(string file) 
        {
            if (file == null || file.Trim().Length == 0) return;
            
            try
            {
                String[] actions = par.GetSoapActionsFromWsdl(file, null);
                if (actions != null)
                {
                    ListSoapActions.Items.Clear();
                    foreach (String action in actions)
                    {
                        ListSoapActions.Items.Add(action);
                    }
                }
            }
            catch (Exception Ex)
            {
                par.RichConsole.Text = Ex.Message + NetSoapClientCs.vbCrLf + Ex.StackTrace;
            }
        }


        private void BtnBrowseForWsdl_Click(object sender, EventArgs e)
        {
            OpenFileDialog Dlg = new OpenFileDialog();
            DialogResult Res;
            Dlg.DefaultExt = "wsdl";
            Dlg.Filter = "WSDL Files(*.wsdl)|*.wsdl|All Files(*.*)|*.*";

            Res = Dlg.ShowDialog();
            if (Res == DialogResult.OK && Dlg.FileName.Length > 0)
            {
                this.WsdlPath.Text = Dlg.FileName;
                Populate(Dlg.FileName);
            }
        }

        private void BtnPopulateSoapActions_Click(object sender, EventArgs e)
        {
            Populate(WsdlPath.Text);
        }

        private void WsdlPath_TextChanged(object sender, EventArgs e)
        {
            BtnPopulateSoapActions.Enabled = (WsdlPath.Text.Trim().Length > 0);
        }

        private void SaveClose()
        {
            if (ListSoapActions.SelectedItem != null)
            {
                String s = ListSoapActions.SelectedItem.ToString();
                if (s != null && s.Trim().Length > 0)
                {
                    par.SoapAction.Text = s;
                }
            }
            this.Close();
        }


        private void ListSoapActions_DoubleClick(object sender, EventArgs e)
        {
            SaveClose();
        }

        private void BtnOkSoapActions_Click(object sender, EventArgs e)
        {
            SaveClose();
        }

        private void InputFilesLabel_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void SoapActions_Load(object sender, EventArgs e)
        {

            string file = par.WsdlXsd.Text;

            if (par.IsEmptyOrPrompt(file, NetSoapClientCs.WSDL_XSD_PROMPT) || file.EndsWith(".xsd", StringComparison.OrdinalIgnoreCase)) return;

            WsdlPath.Text = file;
            Populate(file);
            BtnPopulateSoapActions.Enabled = (WsdlPath.Text.Trim().Length > 0);
        }
    }
}
