using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace NetSoapClientCs
{
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
        }

        private void Help_Load(object sender, EventArgs e)
        {
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            System.IO.FileInfo inf = new System.IO.FileInfo(Assembly.GetExecutingAssembly().Location);
            DateTime dt = inf.LastWriteTime;
//          appDate = dt.ToString("d MMM yyyy")
            TextReader reader = null;
            String rtf = null;
            try
            {
                reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("NetSoapClientCs.Help.rtf"));
                rtf = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.InnerException);
            }
            finally
            {
                try
                {
                    if (reader != null) reader.Close();
                    if (rtf != null)
                    {
                        rtf = rtf.Replace("%ver%", ver.ToString());
                        rtf = rtf.Replace("%date%", dt.ToString("dd/MM/yyyTHH:mm:ss"));
                        this.richTextBox1.Rtf = rtf;
                    }
                }
                catch
                {
                }
            }

        }
    }
}