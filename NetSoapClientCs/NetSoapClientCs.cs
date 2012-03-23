using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using System.IO;
using Microsoft.Web.Services2.Security.Tokens;
using Microsoft.Web.Services2;
using System.IO.IsolatedStorage;
using System.Windows.Forms.Design;
using NetSoapClientCs;
using Xsd2Xml;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;


namespace NetSoapClientCs
{
    public partial class NetSoapClientCs : Form
    {
        public SoapEnvelope EnvIn;
        public String LastInputFile = "";
        private int StartFormW = -1;
        private int StartFormH = -1;
        private String[] AddrNames = { "MessageID", "To", "Action" };
        private String[] UsrNames = { "UsernameToken" };
        private String[] SecNames = { "Security" };
        private bool IgnoreUpdate = false;
        private bool beingMinimized = false;
        private const String HISTORY_FILE = "NetSoapClient.txt";
        private const int HISTORY_COUNT = 32;
        private const int XSD_ELEM_COUNT = 256;
        private const String SECTION_URL = "###SECTION:URL###";
        private const String SECTION_SOAPACTION = "###SECTION:SOAPACTION###";
        private const String SECTION_WSDLXSD = "###SECTION:WSDLXSD###";
        private const String SECTION_ELEMNAME = "###SECTION:ELEMNAMES###";
        private const String SECTION_SEARCH = "###SECTION:SEARCH###";
        private const String SECTION_CONTROLS = "###SECTION:CONTROLS###";
        private const String SECTION_INPUT = "###SECTION:INPUT###";
        private const String SECTION_OUTPUT = "###SECTION:OUTPUT###";
        private const String SECTION_POSITION = "###SECTION:POSITION###";
        private const String SECTION_END = "###SECTION:END###";
        private const String NULL_STR = "<NULL>";
        private const String SEP_STR = "\t";
        public const String vbCrLf = "\r\n";
        private const String vbLf = "\n";
        private const String vbCr = "\r";
        private const String XsltFile = "NetSoapClientCs.XsltFile.xslt";
        private Boolean SearchCaseSensitive = false;
        private Boolean SearchWholeWord = false;
        private XmlSampleGenerator LastXmlGen = null;
        public const String WSDL_XSD_PROMPT = "<wsdl/xsd>";
        public const String ELEM_NAME_PROMPT = "<elem name>";
        private const String SEARCH_TEXT_PROMPT = "<text to search>";
        private bool DisableWsdlXsdUpdate = false;
        public Metrics LastMetrics = null;


        public bool IsEmptyOrPrompt(String s, String p) 
        {
            return (s == null || s.Trim().Equals("") || s.Equals(p));
        }

        public String toHtml(String xml)
        {
            String ret = "";
            XslCompiledTransform xslt = null;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(XsltFile))
            {
                if (null != stream)
                {
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        xslt = new XslCompiledTransform();
                        xslt.Load(reader);
                    }
                }
            }
            if (xslt != null)
            {
                StringWriter writer = new StringWriter();
                StringReader reader = new StringReader(xml);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ProhibitDtd = false;
                settings.IgnoreProcessingInstructions = true;
                settings.IgnoreComments = false;

//                settings.XmlResolver = new XmlProxyResolver(this.site);
                xslt.Transform(XmlReader.Create(reader, settings), XmlWriter.Create(writer));
//                xslt.Transform(XmlIncludeReader.CreateIncludeReader(context, settings, (new Uri(Application.StartupPath + "/")).AbsoluteUri), null, writer);
                reader.Close();
                writer.Close();

                ret = writer.ToString();
                                                                                                                        }
            return ret;
        }


        public void CreateEnv(String text)
        {
            try
            {
                this.EnvIn = new SoapEnvelope();
                this.EnvIn.DocumentElement.OwnerDocument.PreserveWhitespace = false;
                //this.EnvIn.LoadXml(this.RichIn.Text().Replace("<wsa:Action></wsa:Action>", ""))
                this.EnvIn.LoadXml(text);
                // We will add them anyway when send a request
                RemElements(this.EnvIn.Header, AddrNames, "http://schemas.xmlsoap.org/ws/2004/03/addressing");
                RemElements(this.EnvIn.Header, AddrNames, "http://schemas.xmlsoap.org/ws/2004/08/addressing");
                if (ChkCleanSec.Checked)
                {
                    RemSecurity();
                }
            }
            catch (Exception ex)
            {
                //            this.RichOut.Text() = ex.ToString() + vbCrLf + ex.StackTrace
                Console.Write(ex.ToString() + vbCrLf + ex.StackTrace);
                this.RichConsole.AppendText(ex.ToString() + vbCrLf + ex.StackTrace);
            }

        }
        public void CreateEnv()
        {
            CreateEnv(this.RichIn.Text);
        }

        private void RemoveUsrTkn()
        {
            if (this.EnvIn == null)
            {
                CreateEnv();
            }
            if (this.EnvIn != null)
            {
                RemElements(this.EnvIn.Header, UsrNames, "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            }

        }
        private void RemSecurity()
        {
            if (this.EnvIn == null)
            {
                CreateEnv();
            }
            if (this.EnvIn != null)
            {
                RemElements(this.EnvIn.Header, SecNames, "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            }
        }
        private bool RemElements(System.Xml.XmlElement Parent, String[] Names, String ns)
        {
            if (this.EnvIn == null)
            {
                CreateEnv();
            }
            if (this.EnvIn == null)
            {
                return false;
            }
            bool UpdEnv = false;

            if (this.EnvIn.Header == null)
            {
                return false;
            }

            foreach (String Name in Names)
            {
                System.Xml.XmlNodeList ToTkns = this.EnvIn.Header.GetElementsByTagName(Name, ns);
                int Len = ToTkns.Count;
                while (Len > 0)
                {
                    ToTkns[Len - 1].ParentNode.RemoveChild(ToTkns[Len - 1]);
                    Len -= 1;
                    UpdEnv = true;
                }
            }

            if (UpdEnv && this.EnvIn != null)
            {
                
                UpdateIn(FormatXml(this.EnvIn));
            }
            return true;

        }

        private void UpdateIn(String Text)
        {
            this.IgnoreUpdate = true;
            RichIn.Text = Text;
            this.IgnoreUpdate = false;
        }

        private void CleanHeader()
        {
            if (this.EnvIn == null)
            {
                CreateEnv();
            }
            if (this.EnvIn == null)
            {
                return;
            }
            System.Xml.XmlNodeList Hdr = this.EnvIn.GetElementsByTagName("Header", "http://schemas.xmlsoap.org/soap/envelope/");

            Boolean UpdEnv = false;
            foreach (System.Xml.XmlNode T in Hdr)
            {
                UpdEnv = true;
                T.RemoveAll();
            }
            if (UpdEnv)
            {
                UpdateIn(FormatXml(this.EnvIn));
            }
        }

        private void ResizeCombo(ComboBox Combo)
        {

            const int extras = 20;
            foreach (String s in Combo.Items)
            {
                int w = Combo.DropDownWidth;
                System.Drawing.Graphics gr = System.Drawing.Graphics.FromHwnd(Combo.Handle);
                Size size = gr.MeasureString(s, Combo.Font).ToSize();
                if (size.Width + extras > w)
                    Combo.DropDownWidth = size.Width + extras;
            }
        }

        private void AddToCombo(ComboBox Combo, int MaxCount)
        {
            if (Combo.Equals(WsdlXsd) || Combo.Equals(ElemName))
                this.DisableWsdlXsdUpdate = true;
            
            String Selected = Combo.Text;

            Selected.Trim();
            Selected.TrimEnd();
            if (!Selected.Equals(""))
            {
                Combo.Items.Insert(0, Selected);
            }

            int Ind = Combo.Items.Count - 1;
            while (Ind > 0)
            {
                if (Combo.Items[Ind].Equals(Selected) || Ind >= MaxCount)
                {
                    Combo.Items.RemoveAt(Ind);
                }
                Ind -= 1;
            }

            if (Combo.Items.Count > 0)
            {
                Combo.Text = (String)Combo.Items[0];
            }

            if (Combo.Equals(WsdlXsd) || Combo.Equals(ElemName))
                this.DisableWsdlXsdUpdate = false;
        }


        public NetSoapClientCs()
        {
            InitializeComponent();
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            (new Help()).Show();

        }
        private void GenInputFiles(int cnt)
        {
            PrgsBar.Maximum = cnt;
            PrgsBar.Step = 1;
            StsBar.Text = "Generating input files ....";
            StsBar.Refresh();
            String Dir = InputFiles.Text;
            if (Dir.Substring(Dir.Length - 1).Equals("/") || Dir.Substring(Dir.Length - 1).Equals("\\"))
            {
                Dir = Dir.Remove(Dir.Length - 1, 1);
            }

            for (int i = 0; i < cnt; i++)
            {
                String file = Dir + "\\" + "Request" + i+ DateTime.Now.ToString(".yyMMddhhmmss.") + "xml";
                StsBar.Text = "Generating file: " + file;
                StsBar.Refresh();
                try
                {
                    BtnGen_Click(BtnGen, null);
                    SendEnvelope.WriteFile(file, RichIn.Text);
                    StsBar.Text = "Stored file: " + file;
                    StsBar.Refresh();
                }
                catch (Exception) {
                    StsBar.Text = "Failed file: " + file;
                    StsBar.Refresh();
                }
                finally
                {
                    PrgsBar.PerformStep();
                }
            }
        }


        private void Send_Click(object sender, EventArgs e)
        {

            SendEnvelope Srv = new SendEnvelope();
            SoapEnvelope EnvOut = null;
            Boolean  Status = false;

            this.RichOut.ResetText();
            this.RichHeadersI.ResetText();
            this.RichHeadersO.ResetText();
            this.RichConsole.ResetText();
            this.EnvIn = null;
            this.PrgsBar.Value = 0;
            this.StsBar.Text = "";
            AddToCombo(Url,HISTORY_COUNT);
            AddToCombo(SoapAction, HISTORY_COUNT);

            ServicePointManager.CertificatePolicy = new MyCertificateValidation();
            ServicePointManager.DefaultConnectionLimit = 200;

            try
            {
                if (ChkRunAll.Checked)
                {
                    string inp = InputFiles.Text.Trim();
                    string outp = OutputDir.Text.Trim();
                    if (inp.Length == 0 || outp.Length == 0 ||
                        !Directory.Exists(inp) || !Directory.Exists(outp))
                    {
                        MessageBox.Show("You must provide valid input and output directories.", "Driectory Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }


                    int cnt = -1;
                    try
                    {
                        cnt = int.Parse(WsdlXsdCnt.Text);
                    }
                    catch (Exception ex)
                    {
                    }

                    if (cnt > 0)
                    {
                        GenInputFiles(cnt);
                    }


                    if (ChkRunConc.Checked)
                    {
                        EnvOut = null;
                        Srv.AsyncProcessFiles(Url.Text, this);
                    }
                    else
                    {
                        EnvOut = Srv.ProcessDir(Url.Text, this);
                    }
                }
                else
                {

                    StsBar.Text = "Sending Envelope ...";
                    PrgsBar.Maximum = 10;
                    PrgsBar.Step = 5;

                    Metrics met = new Metrics(1, false);
                    long start = DateTime.Now.Ticks;
                    EnvOut = Srv.Send(Url.Text, this, LastInputFile, ref Status);
                    long end = DateTime.Now.Ticks;
                    met.UpdateMetrics(start, end, Status, "Input Window");
                    met.ReportEndTime = DateTime.Now;
                    LastMetrics = met;


                    PrgsBar.Value = 10;
                    System.Threading.Thread.Sleep(150);
                    PrgsBar.Value = 0;
                    if (Status)
                    {
                        StsBar.Text = "Success!";
                    }
                    else
                    {
                        StsBar.Text = "Failed.";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: " + ex.ToString() + vbCrLf + ex.StackTrace);
                this.RichConsole.AppendText(ex.ToString() + vbCrLf + ex.StackTrace);
            }
            finally
            {
                if (EnvOut != null)
                {
//                    RichOut.Text = EnvOut.wInnerXml.ToString();
                    RichOut.Text = FormatXml(EnvOut);

                }
            }

        }
        public static String FormatXml(XmlNode node)
        {
            MemoryStream mem = new MemoryStream();
            if (node == null || node.InnerXml == null) return "";

            String ret = node.InnerXml;
            try
            {
                XmlTextWriter w = new XmlTextWriter(new StreamWriter(mem));
                w.Formatting = Formatting.Indented;
                w.Indentation = 3;
                w.IndentChar = ' ';
                node.WriteTo(w);
                w.Close();
                mem.Close();
                ret = System.Text.Encoding.Default.GetString(mem.ToArray());
            }
            catch (Exception)
            {
            }
            ret = ret.Replace("\t", "   ");
            return ret;
        }

        public static String FormatXml(String xml)
        {
            String ret = xml;
            try
            {
                SoapEnvelope env = new SoapEnvelope();
                env.LoadXml(xml);
                return FormatXml(env);
            }
            catch (Exception)
            {
            }
            return ret;
        }

        private void NetSoapClientCs_Load(object sender, EventArgs e)
        {

            MaxTime.Enabled = ChkRunAll.Checked && ChkRunConc.Checked;
            MaxTimeLabel.Enabled = ChkRunAll.Checked && ChkRunConc.Checked;
            ChkRunConc.Enabled = ChkRunAll.Checked;

            ChkVerKey.Enabled = ChkVerify.Checked;
            VerCert.Enabled = ChkVerify.Checked;
            BtnBrowseVerCert.Enabled = ChkVerify.Checked; 

            NetSoapClientCs_Resize(this, null);

            try
            {
                IsolatedStorageFile Store = IsolatedStorageFile.GetStore(IsolatedStorageScope.User
                        | IsolatedStorageScope.Assembly
                        | IsolatedStorageScope.Domain, null, null);

                String[] FileNames = Store.GetFileNames(HISTORY_FILE);
                if (FileNames.Length == 0)
                {
                    return;
                }
                IsolatedStorageFileStream Ins = new IsolatedStorageFileStream(HISTORY_FILE, FileMode.Open, Store);
                long Len = Ins.Length;
                Byte[] Bytes = new Byte[Len];
                Ins.Read(Bytes, 0, (int)Len);
                Ins.Close();
                String Res = System.Text.Encoding.Default.GetString(Bytes);
                String[] Lines = Res.Split((char[])vbCrLf.ToCharArray());
                Boolean UrlFound = false;

                foreach (String Line in Lines)
                {
                    if (Line.Equals(SECTION_URL))
                    {
                        UrlFound = true;
                    }
                    else
                    {
                        if (UrlFound)
                        {
                            if (Line.Equals(SECTION_END))
                            {
                                break;
                            }
                            else
                            {
                                Line.Trim();
                                Line.TrimEnd();
                                if (!Line.Equals(""))
                                {
                                    Url.Items.Add(Line);
                                }
                            }
                        }
                    }
                }
                Boolean SoapActionFound = false;
                foreach (String Line in Lines)
                {
                    if (Line.Equals(SECTION_SOAPACTION))
                    {
                        SoapActionFound = true;
                    }
                    else
                    {
                        if (SoapActionFound)
                        {
                            if (Line.Equals(SECTION_END))
                            {
                                break;
                            }
                            else
                            {
                                Line.Trim();
                                Line.TrimEnd();
                                if (!Line.Equals(""))
                                {
                                    SoapAction.Items.Add(Line);
                                }
                            }
                        }
                    }
                }
                Boolean SearchFound = false;
                foreach (String Line in Lines)
                {
                    if (Line.Equals(SECTION_SEARCH))
                    {
                        SearchFound = true;
                    }
                    else
                    {
                        if (SearchFound)
                        {
                            if (Line.Equals(SECTION_END))
                            {
                                break;
                            }
                            else
                            {
                                Line.Trim();
                                Line.TrimEnd();
                                if (!IsEmptyOrPrompt(Line, SEARCH_TEXT_PROMPT))
                                {
                                    SearchText.Items.Add(Line);
                                }
                            }
                        }
                    }
                }
                if (SearchText.Items.Count > 0) SearchText.SelectedIndex = 0;

                DisableWsdlXsdUpdate = true;
                Boolean WsdlXsdFound = false;
                foreach (String Line in Lines)
                {
                    if (Line.Equals(SECTION_WSDLXSD))
                    {
                        WsdlXsdFound = true;
                    }
                    else
                    {
                        if (WsdlXsdFound)
                        {
                            if (Line.Equals(SECTION_END))
                            {
                                break;
                            }
                            else
                            {
                                Line.Trim();
                                Line.TrimEnd();
                                if (!IsEmptyOrPrompt(Line,WSDL_XSD_PROMPT) )
                                {
                                    WsdlXsd.Items.Add(Line);
                                }
                            }
                        }
                    }
                }
               if (WsdlXsd.Items.Count > 0) WsdlXsd.SelectedIndex = 0;
                DisableWsdlXsdUpdate = false;

                Boolean ElemNameFound = false;
                foreach (String Line in Lines)
                {
                    if (Line.Equals(SECTION_ELEMNAME))
                    {
                        ElemNameFound = true;
                    }
                    else
                    {
                        if (ElemNameFound)
                        {
                            if (Line.Equals(SECTION_END))
                            {
                                break;
                            }
                            else
                            {
                                Line.Trim();
                                Line.TrimEnd();
                                if (!IsEmptyOrPrompt(Line, ELEM_NAME_PROMPT))
                                {
                                    ElemName.Items.Add(Line);
                                }
                            }
                        }
                    }
                }
                if (ElemName.Items.Count > 0) ElemName.SelectedIndex = 0;

                
                Boolean CtrlsFound = false;
                foreach (String Line in Lines)
                {
                    if (Line.Equals(SECTION_CONTROLS))
                    {
                        CtrlsFound = true;
                    }
                    else
                    {
                        if (CtrlsFound)
                        {
                            if (Line.Equals(SECTION_END))
                            {
                                break;
                            }
                            else
                            {
                                Line.Trim();
                                Line.TrimEnd();
                                if (!Line.Equals(""))
                                {
                                    try
                                    {
                                        String[] CtrlLines = Line.Split(SEP_STR.ToCharArray());
                                        if (CtrlLines.Length > 2)
                                        {
                                            if (CtrlLines[2].Equals(NULL_STR))
                                            {
                                                CtrlLines[2] = "";
                                            }
                                            Object Obj = this.GetType().InvokeMember(CtrlLines[0], BindingFlags.Instance
                                                | BindingFlags.GetField |
                                                BindingFlags.GetProperty | BindingFlags.NonPublic |
                                                BindingFlags.Public, null, this, null);

                                            //System.Type Typ = System.Type.GetType(CtrlLines(1))
                                            Object Val = CtrlLines[2];
                                            if (CtrlLines[1].Equals("Checked"))
                                            {
                                                Val = bool.Parse((String) Val);
                                            }
                                            Obj.GetType().InvokeMember(CtrlLines[1], BindingFlags.SetField |
                                                BindingFlags.SetProperty |
                                                BindingFlags.NonPublic |
                                                BindingFlags.Public |
                                                BindingFlags.Instance,
                                                null, Obj,
                                                new Object[] { Val });
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(String.Concat(ex.Message, vbCrLf, ex.StackTrace));
                                    }
                                }
                            }
                        }
                    }
                }

                Boolean InputFound = false;
                String InputText = "";
                foreach (String Line in Lines)
                {
                    if (Line.Equals(SECTION_INPUT))
                    {
                        InputFound = true;
                    }
                    else
                    {
                        if (InputFound)
                        {
                            if (Line.Equals(SECTION_END))
                            {
                                break;
                            }
                            else
                            {
                                if (!Line.Equals(""))
                                {
                                    InputText = String.Concat(InputText, Line, vbCrLf);
                                }
                            }
                        }
                    }
                }
                this.RichIn.Text = InputText;

                Boolean OutputFound = false;
                String OutputText = "";
                foreach (String Line in Lines)
                {
                    if (Line.Equals(SECTION_OUTPUT))
                    {
                        OutputFound = true;
                    }
                    else
                    {
                        if (OutputFound)
                        {
                            if (Line.Equals(SECTION_END))
                            {
                                break;
                            }
                            else
                            {
                                if (!Line.Equals(""))
                                {
                                    OutputText = String.Concat(OutputText, Line, vbCrLf);
                                }
                            }
                        }
                    }
                }
                this.RichOut.Text = OutputText;

                Boolean Found = false;
                foreach (String Line in Lines)
                {
                    if (Line.Equals(SECTION_POSITION))
                    {
                        Found = true;
                    }
                    else
                    {
                        if (Found)
                        {
                            if (Line.Equals(SECTION_END))
                            {
                                break;
                            }
                            else
                            {
                                if (!Line.Equals(""))
                                {
                                    String[] Pos = Line.Split(SEP_STR.ToCharArray());
                                    if (Pos.Length > 4)
                                    {
                                        FormWindowState State = FormWindowState.Normal;
                                        try
                                        {
                                            State = (FormWindowState)int.Parse(Pos[4]);
                                        }
                                        catch (Exception)
                                        { }

                                        if (State != FormWindowState.Maximized)
                                        {
                                            try
                                            {
                                                this.Left = int.Parse(Pos[0]);
                                                this.Top = int.Parse(Pos[1]);
                                                this.Width = int.Parse(Pos[2]);
                                                this.Height = int.Parse(Pos[3]);
                                            }
                                            catch (Exception) { }
                                        }
                                        this.WindowState = State;
                                        NetSoapClientCs_Resize(this, null);
                                    }
                                    if (Pos.Length > 5)
                                    {
                                        try
                                        {
                                            RichIn.Font = new Font(RichIn.Font.Name, int.Parse(Pos[5]));
                                            RichOut.Font = new Font(RichOut.Font.Name, int.Parse(Pos[5]));
                                        }
                                        catch (Exception) { }
                                    }
                                    if (Pos.Length > 6)
                                    {
                                        try
                                        {
                                            ((ToolStripMenuItem)SearchMenu.Items[0]).Checked = this.SearchCaseSensitive = bool.Parse(Pos[6]);
                                            ((ToolStripMenuItem)SearchMenu.Items[1]).Checked = this.SearchWholeWord = bool.Parse(Pos[7]);
                                        }
                                        catch (Exception) { }
                                    }
                                }
                            }
                        }
                    }
                }

                if (Url.Items.Count > 0)
                {
                    Url.Text = (String)Url.Items[0];
                }
                if (SoapAction.Items.Count > 0)
                {
                    SoapAction.Text = (String)SoapAction.Items[0];
                }
                this.HelpButton = true;
                String st = SearchText.Text.Trim();
                if (st.Length > 0) DoSearch();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private void SaveCtrls(Control Cont, Stream Out)
        {

            foreach (Control Ctrl in Cont.Controls)
            {
                if (typeof(CheckBox).IsInstanceOfType(Ctrl))
                {
                    AddCtrlValue(Ctrl, "Checked", Out);
                }
                if (typeof(RadioButton).IsInstanceOfType(Ctrl))
                {
                    AddCtrlValue(Ctrl, "Checked", Out);
                }
                if (typeof(TextBox).IsInstanceOfType(Ctrl))
                {
                    if (!((TextBox)Ctrl).ReadOnly)
                    {
                        AddCtrlValue(Ctrl, "Text", Out);
                    }
                }
                if (typeof(GroupBox).IsInstanceOfType(Ctrl))
                {
                    SaveCtrls(Ctrl, Out);
                }

            }

        }
        private void SaveRichCtrl(Control Ctrl, Stream Out, String SecHeader)
        {
            Byte[] Bytes = System.Text.Encoding.Default.GetBytes(String.Concat(SecHeader, vbCrLf));
            Out.Write(Bytes, 0, Bytes.Length);

            String Txt = String.Concat(Ctrl.Text, vbCrLf);

            Txt.Replace(vbCrLf, vbLf);
            Txt.Replace(vbLf, vbCrLf);

            Bytes = System.Text.Encoding.Default.GetBytes(Txt);
            Out.Write(Bytes, 0, Bytes.Length);

            Bytes = System.Text.Encoding.Default.GetBytes(String.Concat(SECTION_END + vbCrLf));
            Out.Write(Bytes, 0, Bytes.Length);

        }

        private void SavePositionEtc(Stream Out)
        {

            if (FormWindowState.Minimized == this.WindowState) return;


            Byte[] Bytes = System.Text.Encoding.Default.GetBytes(String.Concat(SECTION_POSITION, vbCrLf));
            Out.Write(Bytes, 0, Bytes.Length);

            String Txt = String.Concat("", this.Left, SEP_STR, this.Top, SEP_STR, this.Width, SEP_STR, this.Height, SEP_STR,
                (int)this.WindowState, SEP_STR, (int)this.RichIn.Font.SizeInPoints, SEP_STR,
                this.SearchCaseSensitive, SEP_STR, this.SearchWholeWord, vbCrLf
             );

            Bytes = System.Text.Encoding.Default.GetBytes(Txt);
            Out.Write(Bytes, 0, Bytes.Length);

            Bytes = System.Text.Encoding.Default.GetBytes(String.Concat(SECTION_END, vbCrLf));
            Out.Write(Bytes, 0, Bytes.Length);

        }

        private void AddCtrlValue(Control Ctrl, String Field, Stream Out)
        {

            String Val;

            try
            {
                Val = Ctrl.GetType().InvokeMember(Field, BindingFlags.DeclaredOnly | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.GetField,
                    null, Ctrl, null).ToString();

                if (Val == null)
                {
                    Val = NULL_STR;
                }
                else if (Val.Length == 0)
                {
                    Val = NULL_STR;
                }

                Byte[] Bytes = System.Text.Encoding.Default.GetBytes(String.Concat(Ctrl.Name, SEP_STR, Field, SEP_STR, Val, vbCrLf));
                Out.Write(Bytes, 0, Bytes.Length);
            }
            catch (Exception ex)
            {
            }
        }
        private void SaveCombo(ComboBox Combo, IsolatedStorageFileStream Out, String Section) 
        {
            Byte[] Bytes = System.Text.Encoding.Default.GetBytes(String.Concat(Section, vbCrLf));
            Out.Write(Bytes, 0, Bytes.Length);
            int i = 0;
            while (i < Combo.Items.Count) // && i < HISTORY_COUNT)
            {
                String Item = (String)Combo.Items[i];
                i += 1;
                Item.Trim();
                Item.TrimEnd();
                if (!Item.Equals(""))
                {
                    Bytes = System.Text.Encoding.Default.GetBytes(String.Concat(Item, vbCrLf));
                    Out.Write(Bytes, 0, Bytes.Length);
                }
            }
            Bytes = System.Text.Encoding.Default.GetBytes(String.Concat(SECTION_END, vbCrLf));
            Out.Write(Bytes, 0, Bytes.Length);
        }

        private void NetSoapClientCs_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {

                IsolatedStorageFile Store = IsolatedStorageFile.GetStore(IsolatedStorageScope.User
                    | IsolatedStorageScope.Assembly
                    | IsolatedStorageScope.Domain, null, null);

                String[] FileNames = Store.GetFileNames(HISTORY_FILE);
                if (FileNames.Length > 0)
                {
                    Store.DeleteFile(HISTORY_FILE);
                }
                IsolatedStorageFileStream Out = new IsolatedStorageFileStream(HISTORY_FILE, FileMode.Create, Store);

                SaveCombo(Url,Out,SECTION_URL);
                SaveCombo(SoapAction, Out, SECTION_SOAPACTION);
                SaveCombo(SearchText, Out, SECTION_SEARCH);
                SaveCombo(WsdlXsd, Out, SECTION_WSDLXSD);
                SaveCombo(ElemName, Out, SECTION_ELEMNAME);

                Byte[] Bytes = System.Text.Encoding.Default.GetBytes(String.Concat(SECTION_CONTROLS, vbCrLf));
                Out.Write(Bytes, 0, Bytes.Length);
                SaveCtrls(this, Out);
                Bytes = System.Text.Encoding.Default.GetBytes(String.Concat(SECTION_END, vbCrLf));
                Out.Write(Bytes, 0, Bytes.Length);

                SaveRichCtrl(this.RichIn, Out, SECTION_INPUT);
                SaveRichCtrl(this.RichOut, Out, SECTION_OUTPUT);
                SavePositionEtc(Out);

                Out.Close();
            }
            catch (Exception ex)
            {
                Console.Write(String.Concat(ex.Message, vbCrLf, ex.StackTrace, vbCrLf));
            }
        }

        private void BtnBrowseIn_Click(object sender, EventArgs e) {
            OpenFileDialog Dlg = new OpenFileDialog();
            DialogResult Res;
            Dlg.DefaultExt = "xml";
            Res = Dlg.ShowDialog();
            if (Res == DialogResult.OK && Dlg.FileName.Length > 0) {
                RichIn.Text = SendEnvelope.ReadFile(Dlg.FileName);
                LastInputFile = Path.GetFileName(Dlg.FileName);
                CreateEnv();
                RichIn.Text = FormatXml(this.EnvIn);
            }
        }

        private void BtnBrowseOut_Click(object sender, EventArgs e) {
            SaveFileDialog Dlg = new SaveFileDialog();
            DialogResult Res;
            Dlg.DefaultExt = "xml";
            Res = Dlg.ShowDialog();
            if (Res == DialogResult.OK && Dlg.FileName.Length > 0) {
                SendEnvelope.WriteFile(Dlg.FileName, RichOut.Text);
            }
        }

        private void RichIn_TextChanged(object sender, EventArgs e) {
           if (!this.IgnoreUpdate) {
               this.EnvIn = null;
            }
        }

        private void ChkSign_CheckedChanged(object sender, EventArgs e) {
            SignKey.Enabled = ChkSign.Checked;
            SignKeyBrowse.Enabled = ChkSign.Checked;
            CertText1.Enabled = ChkSign.Checked;
        }

        private void ChkClientCert_CheckedChanged(object sender, EventArgs e) {
            SslKey.Enabled = ChkClientCert.Checked;
            SslKeyBrowse.Enabled = ChkClientCert.Checked;
            CertText2.Enabled = ChkClientCert.Checked;
        }

        private void SslKeyBrowse_Click(object sender, EventArgs e){
            OpenFileDialog Dlg = new OpenFileDialog();
            DialogResult Res;
            Dlg.DefaultExt = ".cer";
            Res = Dlg.ShowDialog();
            if (Res == DialogResult.OK && Dlg.FileName.Length > 0) {
                this.SslKey.Text = Dlg.FileName;
            }
        }

        private void SignKeyBrowse_Click(object sender, EventArgs e) {
            OpenFileDialog Dlg = new OpenFileDialog();
            DialogResult Res;
            Dlg.DefaultExt = ".cer";
            Res = Dlg.ShowDialog();
            if (Res == DialogResult.OK && Dlg.FileName.Length > 0) {
                this.SignKey.Text = Dlg.FileName;
            }

        }

        private void BtnCleanHeader_Click(object sender, EventArgs e) {
            this.RichConsole.ResetText();
            CleanHeader();
        }

        private void NetSoapClientCs_Resize(object sender, EventArgs e) {
            Control Ctrl = (Control)sender;
            if (FormWindowState.Minimized == this.WindowState)
            {
                this.beingMinimized = true;
                return;
            }
            this.beingMinimized = true;

            if (this.StartFormW == -1) {
                this.StartFormW = this.Width;
            }
            if (this.StartFormH == -1) {
                this.StartFormH = this.Height;
            }

            int PrevStartFormW = this.StartFormW;
            int PrevStartFormH = this.StartFormH;
            this.StartFormW = this.Width;
            this.StartFormH = this.Height;


            Control[] Ctrls = new Control[] {GroupBox1, GroupBox2, GroupBox3, GroupBox5, GroupBox6, GroupBox7, 
                BtnCleanHeader, BtnRemSecurity,BtnValidate, RichConsole, RichHeadersI, 
                RichHeadersO, label7, Timeout, ChkCleanSec, ChkHdrs
            };

            int dx = this.Width - PrevStartFormW;
            int dy = this.Height - PrevStartFormH;

            //Console.WriteLine("DELTA: " + dx + " " + dy)

            if (dx == 0 && dy == 0) {
                return;
            }

            //if (this.RichIn.Size.Width + dx < 10 Or this.RichIn.Size.Height + dy < 10) {
            //Return
            //}
            //Console.WriteLine("Y:  " + this.RichOut.Size.Height)
            try {
                int OldHI = RichIn.Height;
                int OldHO = RichOut.Height;
                
                int dy1 = dy/2;
                int dy2 = dy - dy1;

                int NewHI = this.RichIn.Height + dy1;
                int NewHO = this.RichOut.Height + dy2;

                if (NewHI < 0)
                {
                    dy1 = 0;
                    NewHI = this.RichIn.Height;
                }
                if (NewHO < 0)
                {
                    dy2 = 0;
                    NewHO = this.RichOut.Height;
                }
                

//                this.RichIn.Size = new Size(this.RichIn.Width + dx, this.RichIn.Height + (dy / 2));
//                this.RichOut.Size = new Size(this.RichOut.Width + dx, this.RichOut.Height + (dy - (dy / 2)));

                this.RichIn.Size = new Size(this.RichIn.Width + dx, NewHI);
                this.RichOut.Size = new Size(this.RichOut.Width + dx, NewHO);

                //Console.WriteLine("Y1:  " + this.RichOut.Size.Height)

//                int dy1 = this.RichIn.Height - OldHI;
//                int dy2 = this.RichOut.Height - OldHO;

                MvCtrl(RichOut, 0, dy1);
                MvCtrl(BtnBrowseOut, 0, dy1);
                MvCtrl(BtnCut, 0, dy1);
                MvCtrl(BtnCopy, 0, dy1);
                MvCtrl(BtnPaste, 0, dy1);
                MvCtrl(BtnUndo, 0, dy1);
                MvCtrl(BtnRedo, 0, dy1);
                MvCtrl(BtnIncrease, 0, dy1);
                MvCtrl(BtnDecrease, 0, dy1);
                MvCtrl(BtnSearch, 0, dy1);
                MvCtrl(SearchText, 0, dy1);
                MvCtrl(BtnGen, 0, dy1);
                MvCtrl(Send, dx, 0);
                MvCtrl(SoapAction, dx / 2, 0);

                MvCtrl(BtnBrowseSoapActions, dx, 0);
                MvCtrl(BtnShowReport, dx, 0);

                MvCtrl(WsdlXsd, 0, dy1);
                WsdlXsd.Width += dx/3;
                MvCtrl(BtnPopulate, dx / 3, dy1);
                MvCtrl(WsdlXsdBrowse, dx / 3, dy1);
                MvCtrl(ElemName, dx / 3, dy1);
                ElemName.Width += dx / 3;
                MvCtrl(PrgsBar, dx, dy1);
                MvCtrl(StsBar, dx/3+dx/3, dy1);
                StsBar.Width += dx - dx/3 - dx/3;

                Url.Width = Url.Width + dx/2;
                SoapAction.Width = SoapAction.Width + dx - dx / 2;
                MvCtrl(SoapAction_Text, dx / 2, 0);
//                GroupBox4.Width = GroupBox4.Width + dx;
//                ProxyExceptions.Width = ProxyExceptions.Width + dx;


                MvCtrls(Ctrls, 0, dy1 + dy2);

                RichConsole.Width = RichConsole.Width + dx;
                MvCtrl(RichHeadersI, dx, 0);
                MvCtrl(RichHeadersO, dx, 0);
            }
            catch (Exception ex) {
            }
        }
        private void MvCtrls (Control[] ctrls, int dx, int dy) {
            foreach (Control c in ctrls) {
                c.Left = c.Left + dx;
                c.Top = c.Top + dy;
            }
        }

        private void MvCtrl (Control c, int dx, int dy) {
            c.Left = c.Left + dx;
            c.Top = c.Top + dy;
        }

        private void BtnRemSecurity_Click(object sender, EventArgs e) {
            this.RichConsole.ResetText();
            RemSecurity();
        }

        private void ChkAddUserTkn_CheckedChanged(object sender, EventArgs e) {
            User.Enabled = ChkAddUserTkn.Checked;
            Pwd.Enabled = ChkAddUserTkn.Checked;
        }

        private void BtnValidate_Click(object sender, EventArgs e) {
            this.RichConsole.ResetText();
            this.CreateEnv();
        }

        private void ChkProxy_CheckedChanged(object sender, EventArgs e) {
            ProxyUser.Enabled = ChkProxy.Checked;
            ProxyPwd.Enabled = ChkProxy.Checked;
            ProxyUri.Enabled = ChkProxy.Checked;
            ProxyExceptions.Enabled = ChkProxy.Checked;
        }

        private void ChkRunAll_CheckedChanged(object sender, EventArgs e) {
            if (ChkRunAll.Checked) {
                InputFiles.Enabled = true;
                InputFilesLabel.Enabled = true;
                BtnBrowseInputFiles.Enabled = true;
                OutputDir.Enabled = true;
                OutputDirectoryLabel.Enabled = true;
                BtnBrowseOutputDir.Enabled = true;
                RichIn.Enabled = false;
                RichOut.Enabled = false;
                BtnBrowseIn.Enabled = false;
                BtnBrowseOut.Enabled = false;
                ChkRunConc.Enabled = true;
                MaxTime.Enabled = ChkRunConc.Checked;
                MaxTimeLabel.Enabled = ChkRunConc.Checked;

                bool b = !IsEmptyOrPrompt(WsdlXsd.Text, WSDL_XSD_PROMPT) && !IsEmptyOrPrompt(ElemName.Text, ELEM_NAME_PROMPT);
                WsdlXsdCnt.Enabled = b;
                WsdlXsdCntLabel.Enabled = b;
            }
            else {
                InputFiles.Enabled = false;
                InputFilesLabel.Enabled = false;
                BtnBrowseInputFiles.Enabled = false;
                OutputDir.Enabled = false;
                OutputDirectoryLabel.Enabled = false;
                BtnBrowseOutputDir.Enabled = false;
                RichIn.Enabled = true;
                RichOut.Enabled = true;
                BtnBrowseIn.Enabled = true;
                BtnBrowseOut.Enabled = true;
                ChkRunConc.Enabled = false;
                MaxTime.Enabled = false;
                MaxTimeLabel.Enabled = false;

                WsdlXsdCnt.Enabled = false;
                WsdlXsdCntLabel.Enabled = false;
            }

        }
/*
        private void BtnBrowseInputFiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog Dlg = new OpenFileDialog();
            DialogResult Res;
            Dlg.DefaultExt = "xml";
            Dlg.Multiselect = true;
            Res = Dlg.ShowDialog();
            if (Res == DialogResult.OK && Dlg.FileNames.Length > 0)
            {
                InputFiles.Text = "";
                foreach (String File in Dlg.FileNames)
                {
                    InputFiles.Text = String.Concat(InputFiles.Text, File, ";");
                }
                if (InputFiles.Text.Length > 0)
                {
                    InputFiles.Text = InputFiles.Text.Remove(InputFiles.Text.Length - 1, 1);
                }
            }

        }
        */

        private void BtnBrowseInputFiles_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog Dlg = new FolderBrowserDialog();
            Dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            if (OutputDir.TextLength > 0)
            {
                Dlg.SelectedPath = InputFiles.Text;
            }
            DialogResult Res = Dlg.ShowDialog();
            if (Res == DialogResult.OK && Dlg.SelectedPath.Length > 0)
            {
                InputFiles.Text = Dlg.SelectedPath;
            }
        }

        private void BtnBrowseOutputDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog Dlg = new FolderBrowserDialog();
            Dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            if (OutputDir.TextLength > 0) {
                Dlg.SelectedPath = OutputDir.Text;
            }
            DialogResult Res = Dlg.ShowDialog();
            if (Res == DialogResult.OK && Dlg.SelectedPath.Length > 0) {
                OutputDir.Text = Dlg.SelectedPath;
            }
        }

        private void ChkRunConc_CheckedChanged(object sender, EventArgs e) {
            MaxTimeLabel.Enabled = ChkRunConc.Checked && ChkRunAll.Checked;
            MaxTime.Enabled = ChkRunConc.Checked && ChkRunAll.Checked;
        }

        private void ChkUseHttpAuth_CheckedChanged(object sender, EventArgs e) {
            this.HttpAuthPwd.Enabled = ChkUseHttpAuth.Checked;
            this.HttpAuthUser.Enabled = ChkUseHttpAuth.Checked;
        }

        private void ChkVerify_CheckedChanged(object sender, EventArgs e)
        {
            ChkVerKey.Enabled = ChkVerify.Checked;
            VerCert.Enabled = ChkVerKey.Enabled && ChkVerKey.Checked;
            BtnBrowseVerCert.Enabled = ChkVerKey.Enabled && ChkVerKey.Checked;
        }

        private void BtnBrowseVerCert_Click(object sender, EventArgs e)
        {
            OpenFileDialog Dlg = new OpenFileDialog();
            DialogResult Res;
            Dlg.DefaultExt = ".cer";
            Res = Dlg.ShowDialog();
            if (Res == DialogResult.OK && Dlg.FileName.Length > 0)
            {
                this.VerCert.Text = Dlg.FileName;
            }
        }

        private void ChkVerKey_CheckedChanged(object sender, EventArgs e)
        {
            VerCert.Enabled = ChkVerKey.Enabled && ChkVerKey.Checked;
            BtnBrowseVerCert.Enabled = ChkVerKey.Enabled && ChkVerKey.Checked;
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void HttpAuthUser_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void ProxyUri_TextChanged(object sender, EventArgs e)
        {

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void BtnCut_Click(object sender, EventArgs e)
        {
            if (RichIn.SelectedText != null && RichIn.SelectedText.Length > 0)
            {
                RichIn.Cut();
            }
            else if (RichOut.SelectedText != null && RichOut.SelectedText.Length > 0)
            {
                RichOut.Cut();
            }
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            if (RichIn.SelectedText != null && RichIn.SelectedText.Length > 0)
            {
                RichIn.Copy();
            }
            else if (RichOut.SelectedText != null && RichOut.SelectedText.Length > 0)
            {
                RichOut.Copy();
            }
        }

        private void BtnPaste_Click(object sender, EventArgs e)
        {
            RichIn.Paste();
        }

        private void BtnUndo_Click(object sender, EventArgs e)
        {
            RichIn.Undo();
        }

        private void BtnRedo_Click(object sender, EventArgs e)
        {
            RichIn.Redo();
        }

        private void BtnIncrease_Click(object sender, EventArgs e)
        {
            RichIn.Font = new Font(RichIn.Font.Name, RichIn.Font.SizeInPoints + 1);
            RichOut.Font = new Font(RichOut.Font.Name, RichOut.Font.SizeInPoints + 1);
        }

        private void BtnDecrease_Click(object sender, EventArgs e)
        {
            if (RichIn.Font.SizeInPoints - 1 > 0)
                RichIn.Font = new Font(RichIn.Font.Name, RichIn.Font.SizeInPoints - 1);

            if (RichOut.Font.SizeInPoints - 1 > 0)
                RichOut.Font = new Font(RichOut.Font.Name, RichOut.Font.SizeInPoints - 1);
        }
        private int FindHighlight(RichTextBox rtb)
        {
            rtb.Text = rtb.Text; // this will reset the previous search

            int pos = 0;
            Color old = rtb.SelectionColor;
            int rtblen = rtb.Text.Length;
            String str = SearchText.Text.Trim();
            int textlen = str.Length;

            if (textlen == 0)
            {
                return 0;
            }
            int cnt = 0;
            RichTextBoxFinds opts = this.SearchWholeWord ? RichTextBoxFinds.WholeWord : RichTextBoxFinds.None;
            opts |= this.SearchCaseSensitive ? RichTextBoxFinds.MatchCase : 0;

            while ((pos = rtb.Find(str, pos, rtblen, opts)) != -1)
            {
                rtb.SelectionColor = Color.Blue;
                rtb.SelectionFont = new Font(rtb.Font,FontStyle.Bold);
                rtb.Select(pos, textlen);
                pos += textlen;
                cnt++;
            }
            return cnt;
        }

        private void DoSearch() 
        {
            int cnt1 = FindHighlight(RichIn);
            int cnt2 = FindHighlight(RichOut);
            StsBar.Text = String.Format("Input Window: {0} matches; Output Window: {1} matches", cnt1, cnt2);
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            AddToCombo(SearchText,HISTORY_COUNT);
            DoSearch();
        }

        protected XmlSchemaSet GetSchemas(String file) {

            XmlSchemaSet schemas = null;
            if (file.EndsWith(".xsd", StringComparison.OrdinalIgnoreCase))
            {
                schemas = GetSchemasFromXsd(file);
            }
            else
            {
                schemas = GetSchemasFromWsdl(file);
            }
            return schemas;
        }

        
        
        
        public String[] GetElemNames(string file)
        {
            String[] elems = null;
            XmlSchemaSet schemas = GetSchemas(file);
            if (schemas == null) return null;
            elems = GetElemNames(schemas);
            return elems;
        }

        private void BtnGen_ClickXsd(object sender, EventArgs e)
        {

            
            XmlQualifiedName qname = new XmlQualifiedName("setTopicData", "http://www.openmedicalexchange.org/1.0/hp");
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add(null, "C:\\DataPowerP4\\config\\wsdl.xsd");


            MemoryStream mem = new MemoryStream();
            XmlTextWriter textWriter = new XmlTextWriter(mem, null);
            textWriter.Formatting = Formatting.Indented;
            XmlSampleGenerator genr = new XmlSampleGenerator(schemas, qname);
            genr.ListLength = 1;
            genr.MaxThreshold = 1;
            genr.WriteXml(textWriter);
            textWriter.Flush();
            XmlDocument doc = new XmlDocument();
            mem.Seek(0, SeekOrigin.Begin);
            doc.Load(mem);
            SoapEnvelope env = new SoapEnvelope();
            env.CreateBody().AppendChild(env.ImportNode(doc.GetElementsByTagName("setTopicData", "http://www.openmedicalexchange.org/1.0/hp")[0], true));
            RichIn.Text = FormatXml(env);
        }

        protected XmlSchemaSet GetSchemasFromXsd(string file)
        {
            XmlSchemaSet schemas = null;
            try
            {
                schemas = new XmlSchemaSet();
                ValidationEventHandler eh = new ValidationEventHandler(ValHand);
                schemas.ValidationEventHandler += eh;
                schemas.Add(null, file);
                schemas.Compile();
            }
            catch (Exception Ex)
            {
                this.RichConsole.Text = Ex.Message + vbCrLf + Ex.StackTrace;
            }
            return schemas;

        }

        protected bool ElemMatchesBinding(string elem, System.Web.Services.Description.OperationBinding ob)
        {
            try
            {
                System.Web.Services.Description.PortType port = ob.Binding.ServiceDescription.PortTypes[ob.Binding.Type.Name];
                foreach (System.Web.Services.Description.Operation op in port.Operations)
                {
                    if (op.Name.Equals(ob.Name)) 
                    {
                        foreach (System.Web.Services.Description.OperationMessage m in op.Messages)
                        {

                            System.Web.Services.Description.Message mm = port.ServiceDescription.Messages[m.Message.Name];
                            if (mm != null)
                            {
                                foreach (System.Web.Services.Description.MessagePart p in mm.Parts)
                                {
                                    if (p.Element.Equals(Str2Qname(elem)))
                                    {
                                        return true;
                                    }
                                }

                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public String [] GetSoapActionsFromWsdl(string file, string elem)
        {
            
            if (IsEmptyOrPrompt(file,WSDL_XSD_PROMPT) || file.EndsWith(".xsd",StringComparison.OrdinalIgnoreCase)) return null;
            
            try
            {
                List<String> actions = new List<String>();
                System.Web.Services.Description.ServiceDescription sd = System.Web.Services.Description.ServiceDescription.Read(file);
                if (sd != null && sd.Bindings != null)
                {
                    foreach (System.Web.Services.Description.Binding b in sd.Bindings)
                    {
                        if (b.Operations != null)
                        {
                            foreach (System.Web.Services.Description.OperationBinding op in b.Operations)
                            {

                                if (!IsEmptyOrPrompt(elem, ELEM_NAME_PROMPT))
                                {
                                    if (!ElemMatchesBinding(elem, op)) continue;
                                }
                                
                                if (op.Extensions != null)
                                {
                                    foreach (System.Web.Services.Description.ServiceDescriptionFormatExtension ex in op.Extensions)
                                    {
                                        System.Web.Services.Description.SoapOperationBinding sop = ex as System.Web.Services.Description.SoapOperationBinding;
                                        if (sop != null)
                                        {
                                            String act = sop.SoapAction;
                                            if (!actions.Contains(act))
                                                actions.Add(act);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (actions.Count > 0)
                {
                    actions.Sort();
                    return actions.ToArray();
                }
                
            }
            catch (Exception Ex)
            {
                this.RichConsole.Text = Ex.Message + vbCrLf + Ex.StackTrace;
            }
            return null;
        }

        protected XmlSchemaSet GetSchemasFromWsdl(string file)
        {
            System.Web.Services.Description.ServiceDescription sd = System.Web.Services.Description.ServiceDescription.Read(file);
            XmlSchemaSet schemas = new XmlSchemaSet();
            ValidationEventHandler eh = new ValidationEventHandler(ValHand);
            schemas.ValidationEventHandler += eh;
            MyResolver res = new MyResolver(new Uri(file));
            res.Credentials = CredentialCache.DefaultCredentials;
            schemas.XmlResolver = res;
            if (sd.Types.Schemas != null)
                foreach (XmlSchema s in sd.Types.Schemas)
                {
                    schemas.Add(s);
                    schemas.Reprocess(s);

                }
            schemas.Compile();

            return schemas;
        }
        protected String [] GetElemNames(XmlSchemaSet schemas)
        {
            String[] ret = null;

            XmlSchemaObjectTable tbl = schemas.GlobalElements;
            System.Collections.IEnumerator en = tbl.Names.GetEnumerator();
            ret = new String[tbl.Names.Count];

            int i = 0;
            while (en.MoveNext())
            {
                XmlQualifiedName xo = en.Current as XmlQualifiedName;
                if (xo.Namespace != null && !xo.Namespace.Trim().Equals(""))
                    ret[i++] = "{" + xo.Namespace + "}" + xo.Name;
                else
                    ret[i++] = xo.Name;
            }
            return ret;
        }

        private void GenerateXml(XmlSampleGenerator gen)
        {
            try
            {
                MemoryStream mem = new MemoryStream();
                XmlTextWriter textWriter = new XmlTextWriter(mem, null);
                textWriter.Formatting = Formatting.Indented;
                gen.ListLength = 1;
                gen.MaxThreshold = 1;
                gen.WriteXml(textWriter);
                textWriter.Flush();
                XmlDocument doc = new XmlDocument();
                mem.Seek(0, SeekOrigin.Begin);
                doc.Load(mem);
                SoapEnvelope env = new SoapEnvelope();
                env.CreateBody().AppendChild(env.ImportNode(doc.FirstChild, true));
                RichIn.Text = FormatXml(env);
            }
            catch (Exception Ex)
            {
                this.RichConsole.Text = Ex.Message + vbCrLf + Ex.StackTrace;
            }
        }

        protected XmlQualifiedName Str2Qname(string elem)
        {
            XmlQualifiedName qname = null;
            int last = elem.LastIndexOf("}");
            int first = elem.IndexOf("{");
            if (last == -1 || first == -1 || first >= last)
            {
                qname = new XmlQualifiedName(elem);
            }
            else
            {
                qname = new XmlQualifiedName(elem.Substring(last + 1), elem.Substring(first + 1, last - first - 1));
            }
            return qname;
        }



        private void BtnGen_Click(object sender, EventArgs e)
        {

            const int steps = 6;
            const int step = 2;

            const int list_cnt = 2;
            const int max_cnt = 2;

            PrgsBar.Maximum = steps;
            PrgsBar.Step = step;

            PrgsBar.PerformStep();

            AddToCombo(this.WsdlXsd, HISTORY_COUNT);
            AddToCombo(this.ElemName, XSD_ELEM_COUNT);
          
            if (LastXmlGen != null)
            {
                GenerateXml(LastXmlGen);
                PrgsBar.PerformStep();
                PrgsBar.PerformStep();
                Thread.Sleep(150);
                PrgsBar.Value = 0;
                return;
            }

            String file = WsdlXsd.Text;
            String elem = ElemName.Text;

            if (IsEmptyOrPrompt(file, WSDL_XSD_PROMPT) || IsEmptyOrPrompt(elem,ELEM_NAME_PROMPT))
            {
                MessageBox.Show("You must enter WSDL/XSD and select an XML element. To browse for WSDL/XSD: right-click on 'dice' button", "WSDL/XSD Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                PrgsBar.PerformStep();
                PrgsBar.PerformStep();
                Thread.Sleep(150);
                PrgsBar.Value = 0;
                return;
            }

            try
            {

                XmlSchemaSet schemas = GetSchemas(file);
                PrgsBar.PerformStep();

                if (file != null)
                {
                    XmlQualifiedName qname = Str2Qname(elem);
                    XmlSampleGenerator genr = new XmlSampleGenerator(schemas, qname);
                    genr.ListLength = list_cnt;
                    genr.MaxThreshold = max_cnt;

                    GenerateXml(genr);
                    LastXmlGen = genr;
                    PrgsBar.PerformStep();
                }

            }
            catch (Exception Ex)
            {
                this.RichConsole.Text = Ex.Message + vbCrLf + Ex.StackTrace;
                LastXmlGen = null;
            }
            finally
            {
                Thread.Sleep(150);
                PrgsBar.Value = 0;
            }
            
            
        }

        public void ValHand(Object obj, ValidationEventArgs args)
        {
            RichConsole.Text += vbCrLf + args.Exception;
        }

        private void BtnCaseSensitive_CheckedChanged(object sender, EventArgs e)
        {
            this.SearchCaseSensitive = ((ToolStripMenuItem)sender).Checked;
        }

        private void BtnWholeWord_CheckedChanged(object sender, EventArgs e)
        {
            this.SearchWholeWord = ((ToolStripMenuItem)sender).Checked;
        }

        private void BrowseXsd_Click(object sender, EventArgs e)
        {
            OpenFileDialog Dlg = new OpenFileDialog();
            DialogResult Res;
            Dlg.DefaultExt = "wsdl";
            Dlg.Filter = "WSDL Files(*.wsdl)|*.wsdl|XSD Files(*.xsd)|*.xsd";

            Res = Dlg.ShowDialog();
            if (Res == DialogResult.OK && Dlg.FileName.Length > 0)
            {
                this.WsdlXsd.Text = Dlg.FileName;
                AddToCombo(this.WsdlXsd, HISTORY_COUNT);
                PopulateElemNames(Dlg.FileName);
            }

        }

        private void PopulateElemNames(String file) {
            try
            {
                this.ElemName.Items.Clear();
                String[] elems = GetElemNames(file);
                if (elems != null)
                {

                    foreach (String s in elems)
                    {
                        this.ElemName.Text = s;
                        AddToCombo(this.ElemName, XSD_ELEM_COUNT);
                    }
                }
            }
            catch (Exception Ex) {
                this.RichConsole.Text = Ex.Message + vbCrLf + Ex.StackTrace;
            }
        }

        private void BtnPopulate_Click(object sender, EventArgs e)
        {
            String text = WsdlXsd.Text;
            if (IsEmptyOrPrompt(text, WSDL_XSD_PROMPT))
            {
                MessageBox.Show("You must enter WSDL or XSD. To browse for WSDL/XSD: right-click on 'dice' button", "WSDL/XSD Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            PopulateElemNames(text);

            AddToCombo(this.WsdlXsd, HISTORY_COUNT);
        }

        private void Combo_DropDown(object sender, EventArgs e)
        {
            
            ResizeCombo((ComboBox) sender);
        }

        private void WsdlXsd_TextChanged(object sender, EventArgs e)
        {
            if (!this.DisableWsdlXsdUpdate)
                LastXmlGen = null;
        }

        private void WsdlXsd_SelectionChanged(object sender, EventArgs e)
        {
            if (!this.DisableWsdlXsdUpdate)
            {
                PopulateElemNames(WsdlXsd.Text);
                LastXmlGen = null;
            }
        }

        private void ElemName_SelectionChanged(object sender, EventArgs e)
        {
            if (!this.DisableWsdlXsdUpdate)
            {
                LastXmlGen = null;
                String [] sa = GetSoapActionsFromWsdl(WsdlXsd.Text, ElemName.Text);
                if (sa != null && sa.Length > 0)
                    SoapAction.Text = sa[0];
            }
        }

        private void BtnBrowseSoapActions_Click(object sender, EventArgs e)
        {
            SoapActions form = new SoapActions(this);
            form.ShowDialog();
        }

        private void BtnShowReport_Click(object sender, EventArgs e)
        {
            if (LastMetrics != null)
            {
                PerformanceReport perf = new PerformanceReport(this);
                perf.Show();
            }
            else
            {
                MessageBox.Show("You must send request(s) to a web service before creating a report.", "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            }
        }

        private void ChkProxy_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void ChkIEProxy_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void RichConsole_TextChanged(object sender, EventArgs e)
        {

        }

        private void RichOut_TextChanged(object sender, EventArgs e)
        {

        }

        private void WsdlXsdBrowse_Click(object sender, EventArgs e)
        {
            BrowseXsd_Click(sender, e);
        }

    }
    public class MyResolver : System.Xml.XmlUrlResolver
    {
        protected Uri baseUri = null;
        public MyResolver(Uri baseUri) : base()
        {
            this.baseUri = baseUri;
        }
        
        public override Uri ResolveUri(Uri baseUri,string relativeUri)
        {
            Uri relu = null;
            try
            {
                relu = new Uri(relativeUri);
            }
            catch (Exception){ }

            Uri ret = null;
            if (relu != null && relu.IsAbsoluteUri)
                ret = base.ResolveUri(baseUri, relativeUri);
            else
                ret = base.ResolveUri(baseUri == null ? this.baseUri : baseUri, relativeUri);
            return ret;

        }
  }
}
