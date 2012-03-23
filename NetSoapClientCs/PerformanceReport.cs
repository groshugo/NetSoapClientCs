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
    public partial class PerformanceReport : Form
    {
        NetSoapClientCs par;
        public PerformanceReport(NetSoapClientCs par)
        {
            this.par = par;
            InitializeComponent();
        }

        private void PerformanceReport_Load(object sender, EventArgs e)
        {
            TextReader reader = null;
            String rtf = null;
            try
            {
                reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("NetSoapClientCs.PerformanceReport.rtf"));
                rtf = reader.ReadToEnd();
                Metrics met = par.LastMetrics;
                String rep_date = met.ReportTime.ToString("yyyy.MM.dd HH.mm.ss");
                String rep_date_end = met.ReportEndTime.ToString("yyyy.MM.dd HH.mm.ss");
                decimal rep_delta = ((decimal)met.ReportEndTime.Ticks - (decimal)met.ReportTime.Ticks) / TimeSpan.TicksPerMillisecond / 1000;
                long avg_time = met.TotalTime / ((long)met.FileNbr);
                string conc = met.Concurrently ? "Yes" : "No";
                decimal throughput = ((decimal)met.FileNbr) / rep_delta;

                rtf = rtf.Replace("%conc%", conc);
                rtf = rtf.Replace("%report_date_time%", rep_date);
                rtf = rtf.Replace("%report_end_date_time%", rep_date_end);
                rtf = rtf.Replace("%report_processing_total%", rep_delta.ToString("N3"));
                rtf = rtf.Replace("%throughput%", throughput.ToString("N3"));

                rtf = rtf.Replace("%request_nbr%", met.FileNbr.ToString());

                rtf = rtf.Replace("%max_time%",met.MaxTime.ToString());
                rtf = rtf.Replace("%max_file%", Path.GetFileName(met.FileMaxTime));

                rtf = rtf.Replace("%min_time%",met.MinTime.ToString());
                rtf = rtf.Replace("%min_file%", Path.GetFileName(met.FileMinTime));

                rtf = rtf.Replace("%avg_time%",avg_time.ToString());
                rtf = rtf.Replace("%total_time%", ((decimal)met.TotalTime / 1000).ToString("N3"));

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
                        this.RichReport.Rtf = rtf;
                    }
                }
                catch
                {
                }
            }

        }

        private void BtnBrowseInputFiles_Click(object sender, EventArgs e)
        {
        }

        private void BtnSaveReport_Click(object sender, EventArgs e)
        {
            SaveFileDialog Dlg = new SaveFileDialog();
            DialogResult Res;
            Dlg.DefaultExt = "rtf";
            Dlg.Filter = "RTF Files (*.rtf)|*.rtf|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            Res = Dlg.ShowDialog();
            if (Res == DialogResult.OK && Dlg.FileName.Length > 0)
            {

                if (Dlg.FilterIndex == 1)
                    SendEnvelope.WriteFile(Dlg.FileName, this.RichReport.Rtf);
                else
                    SendEnvelope.WriteFile(Dlg.FileName, this.RichReport.Text);
            }


        }

    }
}