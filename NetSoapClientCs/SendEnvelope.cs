
using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Xml;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System.Web.Services;
using Microsoft.Web.Services2;
using Microsoft.Web.Services2.Addressing ;
using Microsoft.Web.Services2.Security;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using Microsoft.Web.Services2.Security.Tokens;
using Microsoft.Web.Services2.Security.X509;
using NetSoapClientCs;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using System.Reflection;

namespace NetSoapClientCs
{
    public class SendEnvelope
    {
        public const int MAX_BUF_SIZE = (128 * 1024);
        public const int DEF_TIMEOUT = 180000;    // 3 minute
        public const int MAX_TIMEOUT = 3600000;   // 1 hour
        public const int ASYNC_CHECK_TIME = 10;  // 10 milis
        public const int MAX_ASYNC_WAIT_TIME = (24 * 3600000); // 1 hour
        public const int DEF_ASYNC_WAIT_TIME = 3600000;  // 24 hours
        public const int MAX_CONC_FILES = 200;
        public const String vbCrLf = "\r\n";
        public const String vbLf = "\n";
        public const String vbCr = "\r";
        public const String ADDRESSING_TO = "soap://to";

        public SendEnvelope()
        {
        }

        public static int GetTimeout(String Milis)
        {
        int Ret = DEF_TIMEOUT;
        try {
            Ret = int.Parse(Milis);
        }
        catch (Exception ex) {
        }
        if (Ret <= 0 || Ret >= MAX_TIMEOUT) {
            Ret = DEF_TIMEOUT;
        }
        return Ret;

    }

    public static int GetMaxWaitTime(String Milis) {
        int Ret = DEF_ASYNC_WAIT_TIME;
        try {
            Ret = int.Parse(Milis);
        }
        catch (Exception ex) {
        }

        if (Ret < 0  || Ret >= MAX_ASYNC_WAIT_TIME) {
            Ret = DEF_ASYNC_WAIT_TIME;
        }
        return Ret;
    }

    public void SupressSoapHeaders(NetSoapClientCs Frm, SoapWebRequest Req)
    {

        if (!Frm.ChkSupressAction.Checked && !Frm.ChkSupressMessageID.Checked &&
            !Frm.ChkSupressReplyTo.Checked && !Frm.ChkSupressTimestamp.Checked && !Frm.ChkSupressTo.Checked)
            return;

        Req.Pipeline.OutputFilters.Add(new CustomFilter(Frm, Req));
        try
        {
//            Req.Pipeline.ProcessOutputMessage(Req.SoapContext.Envelope);
        }
        catch (Exception ex)
        {
//            Frm.RichConsole.AppendText("EXCEPTION: " + ex.Message + vbCrLf + ex.StackTrace + vbCrLf);
        }


        if (Frm.ChkSupressTimestamp.Checked && Req.SoapContext.Envelope.Header != null)
        {
            XmlNode sec = Req.SoapContext.Envelope.Header["Security", CustomFilter.WSSE_NS];
            if (sec != null)
            {
                XmlNode ts = sec["Timestamp", CustomFilter.WSU_NS];
                if (ts != null)
                    sec.RemoveChild(ts);
            }
        }

        BindingFlags bf = BindingFlags.NonPublic;
        bf |= BindingFlags.FlattenHierarchy;
        bf |= BindingFlags.SetField;
        bf |= BindingFlags.GetField;
        bf |= BindingFlags.Instance;
        Req.SoapContext.GetType().GetField("_processed", bf).SetValue(Req.SoapContext, false);
        Req.SoapContext.Addressing.Clear();
        Req.SoapContext.Security.Clear();

        Req.Pipeline.InputFilters.Clear();
        Req.Pipeline.OutputFilters.Clear();
    }



    public SoapEnvelope Send(String U, NetSoapClientCs Frm, String LastFile, ref Boolean Status)
        {

        Status = false;

        SoapWebRequest Req = null;
        SoapWebResponse Res = null;


        System.IO.Stream Out= null;
        System.IO.Stream Ins = null;
        SoapEnvelope EnvOut = null;

        Frm.CreateEnv();

        Req = new SoapWebRequest(U);
        HttpWebRequest HttpR = (HttpWebRequest) Req.Request;
        HttpWebResponse WebRes = null;


        if (Frm.ChkProxy.Checked) {
            System.Net.NetworkCredential Cred = new System.Net.NetworkCredential(Frm.ProxyUser.Text, Frm.ProxyPwd.Text);
            System.Net.WebProxy Proxy = new System.Net.WebProxy(new Uri(Frm.ProxyUri.Text));
            Proxy.Credentials = Cred;
            String Reg = Frm.ProxyExceptions.Text;
            Reg = Reg.Replace(".", "\\.");
            Reg = Reg.Replace("*", ".*");
            Proxy.BypassList = Reg.Split(';');
            Req.Request.Proxy = Proxy;
        }
        if (Frm.ChkNoProxy.Checked) {
            Req.Request.Proxy = GlobalProxySelection.GetEmptyWebProxy();
        }

        Req.SoapContext.Envelope = Frm.EnvIn;
        Req.Method = "POST";
 
        Req.ContentType = "text/xml;charset=\"utf-8\"";
        Req.SoapContext.Addressing.Destination = new EndpointReference(new Uri(ADDRESSING_TO));

// TODO: Need to add an action here taken from GUI: some web services need it !!!            

        Req.SoapContext.Addressing.Action = new Microsoft.Web.Services2.Addressing.Action(Frm.SoapAction.Text);
        Req.SoapContext.Addressing.To = new To(new Uri(ADDRESSING_TO));
        //Req.SoapContext.Addressing.To = New Addressing.To(New Uri("/EDE/Zoot/DecisioningService/Decision"))
        //Req.SoapContext.Addressing.ReplyTo = New Addressing.ReplyTo(New Uri("http://schemas.xmlsoap.org/ws/2004/03/addressing/role/anonymous"))
        //Req.SoapContext.Addressing.To = New Addressing.To(New Uri("http://www.wamu.net"))

        if ((Frm.ChkClientCert.Checked && (Frm.SslKey.Text != null) && Frm.SslKey.Text.Length > 0)) {
            HttpR.ClientCertificates.Add(System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromCertFile(Frm.SslKey.Text));

            //X509CertificateStore KeyStore = X509CertificateStore.LocalMachineStore(X509CertificateStore.MyStore)
            //           KeyStore.
            //KeyStore.OpenRead()
            //X509Certificate c
            //For Each c In KeyStore.Certificates()
            //String s = ""
            //Byte b
            //For Each b In c.GetKeyIdentifier()
            //s = s + " " + Hex(b)
            //}
            //Console.WriteLine(c.FriendlyDisplayName() + " " + s + " " + c.PrincipalName + " " + c.SubjectAlternativeName)
            //}
            //Dim bytes() As Byte = New Byte() {&H6F, &HE, &HE7, &HF7, &H45, &HFF, &HF1, &H8C, &H54, &HFC, &HA3, &H47, &H3F, &HA2, &H6, &HA, &H92, &H14, &H1A, &HF2}
            //X509Certificate cert = KeyStore.FindCertificateBySubjectName("workstation, ADM, Accenture, CA, US")(0)
            //X509Certificate cert = KeyStore.FindCertificateBySubjectString("CN=workstation, OU=ADM, O=Accenture, ST=CA, C=US")(0)
            //X509Certificate cert = KeyStore.FindCertificateByHash(bytes)(0)
            //HttpR.ClientCertificates.Add(cert)
            //bytes = New Byte() {&H49, &H9C, &H12, &H35, &H2D, &H54, &HEE, &H45, &H1C, &HE3, &H6C, &H18, &HA7, &HB, &HD2, &H3D, &HBD, &HB, &H4B, &HAF}
            //cert = KeyStore.FindCertificateByHash(bytes)(0)
            //HttpR.ClientCertificates.Add(cert)
            //KeyStore.Close()
        }
        bool EnvMod = false;

        if ((Frm.ChkAddUserTkn.Checked)) {
            if (Frm.User.Text != null  && Frm.User.TextLength != 0  && Frm.Pwd.Text != null  && Frm.Pwd.TextLength != 0) {
                UsernameToken Tkn = new UsernameToken(Frm.User.Text, Frm.Pwd.Text, PasswordOption.SendPlainText);
                if ((Tkn != null)) {
                    Req.SoapContext.Security.Tokens.Add(Tkn);
                    Req.SoapContext.Security.MustUnderstand = false;
                    EnvMod = true;
                }
            }
        }

        

        if ((Frm.ChkSign.Checked && Frm.SignKey.Text != null && Frm.SignKey.Text.Length > 0)) {
            Microsoft.Web.Services2.Security.X509.X509Certificate cert = Microsoft.Web.Services2.Security.X509.X509Certificate.CreateCertFromFile(Frm.SignKey.Text);
            X509SecurityToken BinTkn = new X509SecurityToken(cert);
            Req.SoapContext.Security.Tokens.Add(BinTkn);
            MessageSignature sign = new MessageSignature(BinTkn);
            sign.SignatureOptions = SignatureOptions.IncludeSoapBody;
            Req.SoapContext.Security.Elements.Add(sign);
            Req.SoapContext.Security.MustUnderstand = false;
            EnvMod = true;
        }


        System.Net.WebHeaderCollection Hdrs;
        Byte [] InBytes = null;
        //int i = 0;
        bool StepPerformed = false;
        int Ind = 0;
        try {
            if ((Frm.ChkUseHttpAuth.Checked)) {
                string cre = String.Format("{0}:{1}", Frm.HttpAuthUser.Text, Frm.HttpAuthPwd.Text);
                byte[] bytes = Encoding.ASCII.GetBytes(cre);
                string base64 = Convert.ToBase64String(bytes);
                Req.Headers.Add("Authorization", "basic " + base64);

//                WebResponse resp = req.GetResponse(); NetworkCredential Crds = new NetworkCredential(Frm.HttpAuthUser.Text, Frm.HttpAuthPwd.Text);
//                ICredentials ICrds = Crds.GetCredential(new Uri(Frm.Url.Text), "Basic");
//                Req.Credentials = ICrds;
//                Req.PreAuthenticate = false;
                
            }

            Req.Timeout = GetTimeout(Frm.Timeout.Text);

            if (Frm.ChkHdrs.Checked)
            {
                Req.Headers.Add("SoapAction", Frm.SoapAction.Text);
            }

            SupressSoapHeaders(Frm, Req);
            Out = Req.Request.GetRequestStream();
            Req.SoapContext.Envelope.Save(Out);
            Out.Close();

            Frm.PrgsBar.PerformStep();
            StepPerformed = true;

            Hdrs = Req.Headers;
            Frm.RichHeadersI.Text = String.Concat("----- INPUT HTTP HEADERS ----- " ,vbCrLf);
            Frm.RichHeadersI.AppendText( String.Concat(Hdrs.ToString(),vbCrLf) );

            try {
                WebRes = (HttpWebResponse) Req.Request.GetResponse();
            }
            catch (WebException wex) {
                Frm.RichConsole.AppendText("EXCEPTION (" + LastFile + "): " + wex.Message + vbCrLf + wex.StackTrace + vbCrLf);
                //if (wex.Status = WebExceptionStatus.ProtocolError) {
                WebRes = (HttpWebResponse) wex.Response;
                //}
            }
            if (WebRes != null) {
                Ins = WebRes.GetResponseStream();
                Hdrs = WebRes.Headers;

                EnvOut = new SoapEnvelope();
                ArrayList DynArray = new ArrayList();
                int b = Ins.ReadByte();
                while (b != -1 && Ind < MAX_BUF_SIZE) {
                    DynArray.Add((Byte) b);
                    Ind += 1;
                    b = Ins.ReadByte();
                }
                Ins.Close();
                InBytes = (Byte[])DynArray.ToArray(typeof(Byte));

                if (Ind >= MAX_BUF_SIZE) {
                    Frm.RichConsole.AppendText("ERROR(" + LastFile + "): RESPONSE IS BIGGER THAN " + MAX_BUF_SIZE + vbCrLf);
                }
                else {
                    EnvOut.LoadXml(System.Text.Encoding.Default.GetString(InBytes, 0, Ind));
                }
            }
            else {
                Frm.RichConsole.AppendText("UNEXPECTED ERROR(" + LastFile + "): NO RESPONSE RETURNED FROM A SERVER" + vbCrLf);
            }
        }
        catch (Exception ex) {
            Frm.RichConsole.AppendText("EXCEPTION (" + LastFile + "): " + ex.Message + vbCrLf + ex.StackTrace + vbCrLf);
            if (Ind > 0) {
                Frm.RichOut.Text = NetSoapClientCs.FormatXml(System.Text.Encoding.Default.GetString(InBytes, 0, Ind));
                EnvOut = null;
            }
        }
        finally {
            if (!StepPerformed) {
                Frm.PrgsBar.PerformStep();
            }
        }

//        Frm.RichIn.Text = Req.SoapContext.Envelope.InnerXml;
        Frm.RichIn.Text = NetSoapClientCs.FormatXml(Req.SoapContext.Envelope);

        if (WebRes != null) {
            Hdrs = WebRes.Headers;
            Frm.RichHeadersO.Text = String.Concat("----- OUTPUT HTTP HEADERS -----", vbCrLf);
            Frm.RichHeadersO.AppendText(String.Concat("HTTP " , WebRes.ProtocolVersion.ToString() , ", Status " , WebRes.StatusCode , ":" , WebRes.StatusDescription + vbCrLf));
            Frm.RichHeadersO.AppendText(String.Concat(Hdrs.ToString() , vbCrLf));
            if (WebRes.StatusCode == HttpStatusCode.OK) {
                Status = true;
            }
        }
/*
        if (EnvOut != null && Frm.ChkVerify.Checked )
        {
            try
            {

                SignedXml signedXml = new SignedXml(EnvOut.DocumentElement );
//                RSA key = new RSACryptoServiceProvider();
//                AsymmetricAlgorithm signingKey;
                bool bVerified = false;

                XmlElement sign = (XmlElement)EnvOut.Envelope.GetElementsByTagName("Signature",
                    "http://www.w3.org/2000/09/xmldsig#")[0];

                if (sign == null)
                {
                    throw new Exception("No Digital Signature Found !");
                }
                signedXml.LoadXml(sign);

                if (Frm.ChkVerKey.Checked && Frm.VerCert != null && Frm.VerCert.Text.Length > 0)
                {
                    String certFile = Frm.VerCert.Text.Trim();
                    X509Certificate2 cert = new X509Certificate2(certFile);
                    bVerified = signedXml.CheckSignature(cert, true);
                }
                else
                {
                    bVerified = signedXml.CheckSignature();
                }
                if (!bVerified)
                {
                    throw new Exception("Digital Signature found but was not verified !");
                }
            }
            catch (Exception ex)
            {
                Frm.RichConsole.AppendText("EXCEPTION (" + LastFile + "): " + ex.Message + vbCrLf + ex.StackTrace + vbCrLf);
            }
        }
*/
        return EnvOut;
    }

    public  SoapEnvelope ProcessDir(String U, NetSoapClientCs Frm) {
        SoapEnvelope EnvOut = null;
//        String[] Files = Frm.InputFiles.Text.Split(';');
        String[] Files = GetFilesFromDir(Frm.InputFiles.Text,0);
        int OkCnt = 0;
        int FailCnt = 0;
        bool CurStat = false;
        Frm.PrgsBar.Maximum = Files.Length * 4;
        Frm.PrgsBar.Step = 2;

        Metrics met = new Metrics(Files.Length, false);

        foreach (String CurFile in Files) {
            try {
                String FileName = Path.GetFileName(CurFile);
                Frm.RichIn.Text = ReadFile(CurFile);

                Frm.StsBar.Text = "Processing File: " + CurFile;
                Frm.StsBar.Refresh();

                long start = DateTime.Now.Ticks;
                EnvOut = Send(U, Frm, FileName, ref CurStat);
                long end = DateTime.Now.Ticks;

                met.UpdateMetrics(start, end, CurStat, CurFile);
/*
                if (CurStat) {
                    OkCnt += 1;
                }
                else {
                    FailCnt += 1;
                }
                */
//                Frm.RichOut.Text = EnvOut.InnerXml;
                Frm.RichOut.Text = NetSoapClientCs.FormatXml(EnvOut);

                String Dir = Frm.OutputDir.Text;
                if (Dir.Substring(Dir.Length - 1).Equals("/")  || Dir.Substring(Dir.Length - 1).Equals("\\")) {
                    Dir = Dir.Remove(Dir.Length - 1, 1);
                }
                FileName = Dir + "\\Response." + FileName;
                WriteFile(FileName, Frm.RichOut.Text);
            }
            catch (Exception Ex) {
                Frm.RichConsole.AppendText("EXCEPTION(" + CurFile + ")" + Ex.Message + vbCrLf + Ex.StackTrace + vbCrLf);
            }
            finally {
                Frm.StsBar.Text = "File " + CurFile + " processed!";
                Frm.StsBar.Refresh();
                Frm.PrgsBar.PerformStep();
                met.ReportEndTime = DateTime.Now;
                Frm.LastMetrics = met;
            }
        }
        System.Threading.Thread.Sleep(150);
//        Frm.StsBar.Text = "Successfully Processed (HTTP 200): " + OkCnt + ", Failed: " + FailCnt + ", Total: " + (FailCnt + OkCnt);
        Frm.StsBar.Text = "Successfully Processed (HTTP 200): " + met.SuccededNbr + ", Failed: " + met.FailedNbr + ", Total: " + (met.FailedNbr+met.SuccededNbr);
        Frm.StsBar.Refresh();
        Frm.PrgsBar.Value = 0;
        return EnvOut;
    }
    public String [] GetFilesFromDir(String dir, int max)
    {
        String[] r = Directory.GetFiles(dir);
        String[] ret;
        if (r.Length > max && max != 0)
        {
            ret = new String [max];
            Array.Copy(r, ret, max);
        }
        else
            ret = r;

        return ret;
    }
    public void AsyncProcessFiles(String U, NetSoapClientCs Frm) {

        
        
        ThreadPool.SetMaxThreads(MAX_CONC_FILES+10, MAX_CONC_FILES+10);
        Frm.Send.Enabled = false;
        Metrics met = null;
        try
        {
            if (Frm.OutputDir.Text == null || Frm.OutputDir.Text.Length == 0){
                throw (new Exception("\"Output Directory\" field is empty"));
            }

            FileInfo DirInfo = new FileInfo(Frm.OutputDir.Text);
            if (DirInfo == null || ( (int) DirInfo.Attributes & (int)FileAttributes.Directory )==0 ||  
                ((int) DirInfo.Attributes & (int) FileAttributes.ReadOnly ) != 0 ) {
                throw (new Exception(String.Concat("Can't write to \"Output Directory\": ", Frm.OutputDir.Text)));
            }
            String[] Files = GetFilesFromDir(Frm.InputFiles.Text, MAX_CONC_FILES);

/*
            String [] Files = Frm.InputFiles.Text.Split(';');
            if (Files.Length > MAX_CONC_FILES) {
                String  [] Fls = new String [MAX_CONC_FILES - 1];
                Array.Copy(Files, Fls, MAX_CONC_FILES);
            }
            */

            int OkCnt = 0;
            int FailCnt = 0;
            bool CurStat = false;
            ArrayList States = new  ArrayList();
            Frm.PrgsBar.Maximum = Files.Length * 8;
            Frm.PrgsBar.Step = 2;

            Frm.StsBar.Text = "Reading Files ....";
            List <KeyValuePair<String,String>> files = new List <KeyValuePair<String,String>>();

            met = new Metrics(Files.Length, true);

            foreach (String CurFile in Files)
            {
                try
                {
                    String FileName = Path.GetFileName(CurFile);
                    files.Add(new KeyValuePair<String,String>(CurFile,ReadFile(CurFile)));
                }
                catch (Exception ex)
                {
                    Frm.RichConsole.AppendText("EXCEPTION(" + CurFile + ")" + ex.Message + vbCrLf + ex.StackTrace + vbCrLf);
                    Frm.StsBar.Text = "Can't read file: " + CurFile;
                    Frm.StsBar.Refresh();
                }
                finally
                {
                    Frm.PrgsBar.PerformStep();
                }
            }
            met.FileNbr = files.Count;
            if (met.FileNbr == 0) return;

            foreach (KeyValuePair<String,String> pair in files) {
                String CurFile = pair.Key;
                try
                {

                    Frm.RichIn.Text = pair.Value;

                    Frm.StsBar.Text = "Processing File: " + CurFile + " ...";
                    Frm.StsBar.Refresh();
                    CallState st = AsyncSend(U, Frm, Path.GetFileName(CurFile), ref CurStat);
                    if (st != null) {
                        States.Add(st);
                    }
                }
                catch (Exception ex) {
                    Frm.RichConsole.AppendText("EXCEPTION(" + CurFile + ")" + ex.Message + vbCrLf + ex.StackTrace + vbCrLf);
                    Frm.StsBar.Text = "File " + CurFile + " Failed!";
                    Frm.StsBar.Refresh();
                }
                finally {
                    Frm.PrgsBar.PerformStep();
                }
            }

            int Count = (GetMaxWaitTime(Frm.MaxTime.Text) / ASYNC_CHECK_TIME) + 1;
            WaitHandle [] WaitHandles = {new ManualResetEvent(false)} ; //new System.Threading.WaitHandle[1];
            

            while (Count-- > 0) {
                WaitHandle.WaitAny(WaitHandles, ASYNC_CHECK_TIME, true);
                bool Completed = true;

                foreach (CallState st in States) {
                    if (st.MvBar) {
                        st.MvBar = false;
                        Frm.PrgsBar.PerformStep();
                        Frm.StsBar.Text = "Finished sending file: " + st.InputFile;
                        Frm.StsBar.Refresh();

                    }
                    if (st.Completed) {
                        if (!st.Saved) {
//                            try {
                                st.EndTime = DateTime.Now;
                                met.UpdateMetrics(st.StartTime.Ticks, st.EndTime.Ticks, st.Success, st.InputFile);
                                st.Saved = true;
/*
                                if (st.Success) {
                                    OkCnt += 1;
                                }
                                else {
                                    FailCnt += 1;
                                }

                                String Dir = Frm.OutputDir.Text;
                                if (Dir.Substring(Dir.Length - 1).Equals("/")  || Dir.Substring(Dir.Length - 1).Equals("\\")) {
                                    Dir = Dir.Remove(Dir.Length - 1, 1);
                                }
                                String FileName = Path.GetFileName(st.InputFile);
                                FileName = Dir + "\\Response." + FileName;
                                if (st.InBytes.Count > 0) {
                                    String str = System.Text.Encoding.Default.GetString((Byte[])st.InBytes.ToArray(typeof(Byte)));
                                    Frm.RichOut.Text = NetSoapClientCs.FormatXml(str);
                                    WriteFile(FileName, str);
                                }
*/
                                Frm.PrgsBar.PerformStep();
                                Frm.StsBar.Text = "File: " + st.InputFile + " completed!";
                                Frm.StsBar.Refresh();
/*
                            }
                            catch (Exception ex) {
                                Frm.RichConsole.AppendText("EXCEPTION(" + st.InputFile + ")" + ex.Message + vbCrLf + ex.StackTrace + vbCrLf);
                                Frm.StsBar.Text = "File " + st.InputFile + " Failed!";
                                Frm.StsBar.Refresh();
                            }
                            finally {
                                Frm.RichConsole.AppendText(st.Errors);
                                Frm.PrgsBar.PerformStep();
                                st.Saved = true;
//                                Frm.RichIn.Text = st.InputTxt;
                            } 
*/
                        }
                    }
                    else {
                        Completed = false;
                    }

                }
                if (Completed) {
                    break;
                }
            }

            Frm.StsBar.Text = "Saving Files ...";
            Frm.StsBar.Refresh();
            System.Threading.Thread.Sleep(100);
            Frm.PrgsBar.Value = 0;
            Frm.PrgsBar.Maximum = Files.Length * 2;
            Frm.PrgsBar.Step = 2;

            foreach (CallState st in States)
            {
                if (st.Completed)
                {
                    try
                    {
                        String Dir = Frm.OutputDir.Text;
                        if (Dir.Substring(Dir.Length - 1).Equals("/") || Dir.Substring(Dir.Length - 1).Equals("\\"))
                        {
                            Dir = Dir.Remove(Dir.Length - 1, 1);
                        }
                        String FileName = Path.GetFileName(st.InputFile);
                        FileName = Dir + "\\Response." + FileName;
                        if (st.InBytes.Count > 0)
                        {
                            String str = System.Text.Encoding.Default.GetString((Byte[])st.InBytes.ToArray(typeof(Byte)));
                            Frm.RichOut.Text = NetSoapClientCs.FormatXml(str);
                            WriteFile(FileName, str);
                        }
                        Frm.StsBar.Text = "File: " + st.InputFile + " completed!";
                        Frm.StsBar.Refresh();
                    }
                    catch (Exception ex)
                    {
                        Frm.RichConsole.AppendText("EXCEPTION(" + st.InputFile + ")" + ex.Message + vbCrLf + ex.StackTrace + vbCrLf);
                        Frm.StsBar.Text = "File " + st.InputFile + " Failed!";
                        Frm.StsBar.Refresh();
                    }
                    finally
                    {
                        Frm.RichConsole.AppendText(st.Errors);
                        Frm.PrgsBar.PerformStep();
                        st.Saved = true;
                    }
                }
            }


            System.Threading.Thread.Sleep(150);
            Frm.PrgsBar.Value = 0;
            if (Count > 0) {
                Frm.StsBar.Text = "Successfully Processed (HTTP 200): " + met.SuccededNbr + ", Failed: " + met.FailedNbr + ", Total: " + (met.FailedNbr + met.SuccededNbr);
                Frm.StsBar.Refresh();
            }
            else {
                Frm.StsBar.Text = "Timed-Out. Successfully Processed (HTTP 200): " + met.SuccededNbr + ", Failed: " + met.FailedNbr + ", Total: " + (met.FailedNbr+met.SuccededNbr);
                Frm.StsBar.Refresh();
            }
        }
        catch (Exception Ex) {
            Frm.StsBar.Text = "Failed! Check Console output for details";
            Frm.StsBar.Refresh();
            Frm.RichConsole.Text = Ex.Message + vbCrLf + Ex.StackTrace;
        }
        finally {
            Frm.Send.Enabled = true;
            if (met != null)
            {
                met.ReportEndTime = DateTime.Now;
                Frm.LastMetrics = met;
            }
        }
    }



    public static String ReadFile(String F) {
        String Ret;
        long Len = (new FileInfo(F)).Length;
        byte [] Bytes = new byte[Len];
        FileStream Inp = File.OpenRead(F);
        Inp.Read(Bytes, 0, (int)Len);
        Inp.Close();

        Ret = System.Text.Encoding.Default.GetString(Bytes);
        Ret = Ret.Replace(vbCrLf, vbLf);
        Ret = Ret.Replace(vbLf, vbCrLf);
        return Ret;
    }

     public static void WriteFile(String F, String Text) {
        FileStream Out = File.Create(F);
        Text = Text.Replace(vbCrLf, vbLf);
        Text = Text.Replace(vbLf, vbCrLf);
        Byte [] Bytes = System.Text.Encoding.Default.GetBytes(Text);
        Out.Write(Bytes, 0, Bytes.Length);
        Out.Close();
    }

    public static CallState AsyncSend(String U, NetSoapClientCs Frm, String LastFile, ref Boolean Status) {
        Status = true;
        SoapWebRequest Req;
        SoapWebResponse Res;
        Frm.CreateEnv();
        Req = new SoapWebRequest(U);
        HttpWebResponse WebRes;
        CallState CallSt=null;

        try {
            if ((Frm.ChkUseHttpAuth.Checked)) {
                NetworkCredential Crds = new NetworkCredential(Frm.HttpAuthUser.Text, Frm.HttpAuthPwd.Text);
                ICredentials ICrds = Crds.GetCredential(new Uri(Frm.Url.Text), "Basic");
                Req.Credentials = ICrds;
                Req.PreAuthenticate = true;
            }

            if (Frm.ChkProxy.Checked) {
                System.Net.NetworkCredential Cred = new System.Net.NetworkCredential(Frm.ProxyUser.Text, Frm.ProxyPwd.Text);
                System.Net.WebProxy Proxy = new System.Net.WebProxy(new Uri(Frm.ProxyUri.Text));
                Proxy.Credentials = Cred;
                String Reg = Frm.ProxyExceptions.Text;
                Reg = Reg.Replace(".", "\\.");
                Reg = Reg.Replace("*", ".*");
                Proxy.BypassList = Reg.Split(';');
                Req.Request.Proxy = Proxy;
            }
            if (Frm.ChkNoProxy.Checked) {
                Req.Request.Proxy = GlobalProxySelection.GetEmptyWebProxy();
            }

            Req.SoapContext.Envelope = Frm.EnvIn;
            Req.Method = "POST";
            Req.ContentType = "text/xml;charset=\"utf-8\"";
            Req.SoapContext.Addressing.Destination = new EndpointReference(new Uri(ADDRESSING_TO));
            Req.SoapContext.Addressing.Action = new  Microsoft.Web.Services2.Addressing.Action(Frm.SoapAction.Text);
            Req.SoapContext.Addressing.To = new  To(new Uri(ADDRESSING_TO));

            if ((Frm.ChkClientCert.Checked && Frm.SslKey.Text != null && Frm.SslKey.Text.Length > 0)) {
                ((HttpWebRequest)Req.Request).ClientCertificates.Add(System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromCertFile(Frm.SslKey.Text));
            }

            if ((Frm.ChkAddUserTkn.Checked)) {
                if (Frm.User.Text != null  && Frm.User.TextLength != 0  && Frm.Pwd.Text != null  && Frm.Pwd.TextLength != 0 ) {
                    UsernameToken Tkn = new UsernameToken(Frm.User.Text, Frm.Pwd.Text, PasswordOption.SendPlainText);
                    if (Tkn != null) {
                        Req.SoapContext.Security.Tokens.Add(Tkn);
                        Req.SoapContext.Security.MustUnderstand = false;
                    }
                }
            }

            if ((Frm.ChkSign.Checked && Frm.SignKey.Text != null && Frm.SignKey.Text.Length > 0)) {
                Microsoft.Web.Services2.Security.X509.X509Certificate cert = Microsoft.Web.Services2.Security.X509.X509Certificate.CreateCertFromFile(Frm.SignKey.Text);
                X509SecurityToken BinTkn = new X509SecurityToken(cert);
                Req.SoapContext.Security.Tokens.Add(BinTkn);
                MessageSignature sign = new MessageSignature(BinTkn);
                sign.SignatureOptions = SignatureOptions.IncludeSoapBody;
                Req.SoapContext.Security.Elements.Add(sign);
                Req.SoapContext.Security.MustUnderstand = false;
            }

            Req.Timeout = GetTimeout(Frm.Timeout.Text);

            CallSt = new CallState(Req, U, Req.SoapContext.Envelope.InnerXml, LastFile, Frm, DateTime.Now);
            Req.BeginGetRequestStream(CallState.ProcessRequest, CallSt);
        }
        catch (Exception Ex) {
            if (CallSt != null)
            {
                CallSt.Frm.RichConsole.AppendText("EXCEPTION (" + CallSt.InputFile + "): " + Ex.Message + vbCrLf + Ex.StackTrace + vbCrLf);
            }
            Status = false;
        } 

        return CallSt;

      }
    }

    public class Metrics
    {
        public DateTime ReportTime = DateTime.Now;
        public DateTime ReportEndTime;
        public long MaxTime = 0;
        public long MinTime = long.MaxValue;
        public long AvgTime = 0;
        public long TotalTime = 0;
        public int FileNbr = 0;
        public int FailedNbr = 0;
        public int SuccededNbr = 0;
        public bool Concurrently = false;

        public String FileMinTime = "";
        public String FileMaxTime = "";

        public Metrics(int FileNbr, bool concur)
        {
            this.FileNbr = FileNbr;
            this.Concurrently = concur;
        }
        public void UpdateMetrics(long start, long end, bool success, String file)
        {
            long d = (end - start) / TimeSpan.TicksPerMillisecond;
            if (d > MaxTime)
            {
                MaxTime = d;
                FileMaxTime = file;
            }
            if (d < MinTime)
            {
                MinTime = d;
                FileMinTime = file;
            }
            TotalTime += d;
            if (success)
            {
                SuccededNbr++;
            }
            else
            {
                FailedNbr++;
            }
        }
    }
    
    public class CallState {
    public SoapWebRequest WebReq;
    public NetSoapClientCs Frm;
    public HttpWebResponse WebRes;
    public String InputFile;
    public String Url;
    public int CurInd;
    public String InputTxt;
    public bool Completed;
    public bool Saved;
    public bool Success;
    public ArrayList InBytes;
    public String Errors;
    public bool MvBar;
    public DateTime StartTime;
    public DateTime EndTime;

    public CallState(WebRequest WebReq, String Url, String InputTxt, String InputFile, NetSoapClientCs Frm, DateTime start){
        this.Url = Url;
        this.InputTxt = InputTxt;
        this.InputFile = InputFile;
        this.Frm = Frm;
        this.WebReq = (SoapWebRequest)WebReq;
        this.Success = false;
        this.Completed = false;
        this.Saved = false;
        this.InBytes = new ArrayList();
        this.Errors = "";
        this.MvBar = false;
        this.StartTime = start;
    }

    public static void ProcessRequest(IAsyncResult Res) {
        //System.Threading.Thread Thr = System.Threading.Thread.CurrentThread

        CallState CallSt = (CallState )Res.AsyncState;
        //CallSt.Frm.RichIn.Text = CallSt.InputTxt;

        try {
            Stream Out = CallSt.WebReq.EndGetRequestStream(Res);
            Byte [] Bytes = System.Text.Encoding.Default.GetBytes(CallSt.InputTxt);
            Out.Write(Bytes, 0, Bytes.Length);
            Out.Close();

            CallSt.WebReq.Request.BeginGetResponse(ProcessResponse, CallSt);
        }
        catch (Exception Ex) {
            CallSt.Errors = String.Concat(CallSt.Errors,"EXCEPTION (", CallSt.InputFile, "): ", Ex.Message, SendEnvelope.vbCrLf, Ex.StackTrace, SendEnvelope.vbCrLf);
            CallSt.Completed = true;
        }
        finally {
            CallSt.MvBar = true;
        } 


    }

    public static void ProcessResponse(IAsyncResult Res) {
        CallState CallSt = (CallState) Res.AsyncState;
        int Ind = 0;

        try {

            try {
                CallSt.WebRes = (HttpWebResponse)CallSt.WebReq.Request.EndGetResponse(Res);
            }
            catch (WebException wex) {
                CallSt.WebRes = (HttpWebResponse )wex.Response;
            } 


            if (CallSt.WebRes != null) {
                Stream Ins = CallSt.WebRes.GetResponseStream();

                int b = Ins.ReadByte();
                while (b != -1 && Ind < SendEnvelope.MAX_BUF_SIZE) {
                    CallSt.InBytes.Add((Byte) b);
                    Ind += 1;
                    b = Ins.ReadByte();
                }
                Ins.Close();

                if (Ind >= SendEnvelope.MAX_BUF_SIZE) {
                    CallSt.Errors = String.Concat(CallSt.Errors,"ERROR(" , CallSt.InputFile, "): RESPONSE IS BIGGER THAN ", SendEnvelope.MAX_BUF_SIZE, SendEnvelope.vbCrLf);
                }
            }
            else {
                CallSt.Errors = String.Concat(CallSt.Errors,"UNEXPECTED ERROR(", CallSt.InputFile, "): NO RESPONSE RETURNED FROM A SERVER", SendEnvelope.vbCrLf);
            }
        }
        catch (Exception ex) {
            CallSt.Errors = String.Concat(CallSt.Errors, "EXCEPTION (", CallSt.InputFile, "): ", ex.Message, SendEnvelope.vbCrLf, ex.StackTrace, SendEnvelope.vbCrLf);
        }
        finally {
            if (Ind > 0) {
                //CallSt.Frm.RichOut.Text = System.Text.Encoding.Default.GetString(CallSt.InBytes.ToArray(GetType(Byte)), 0, Ind)
            }

            if (CallSt.WebRes != null) {
                if (CallSt.WebRes.StatusCode == HttpStatusCode.OK) {
                    CallSt.Success = true;
                }
            }
            if ((CallSt.Success)) {
                //CallSt.Frm.StsBar.Text = "File " + CallSt.InputFile + " processed successfully (HTTP 200)"
            }
            else {
                //CallSt.Frm.StsBar.Text = "File " + CallSt.InputFile + " failed (HTTP " + CallSt.WebRes.StatusCode + ")"
            }
            CallSt.Completed = true;
        }

    }

 }






    public class MyCertificateValidation : ICertificatePolicy
    {

        public enum CertificateProblem : long {
            CertEXPIRED = 2148204801,                      // 0x800B0101
            CertVALIDITYPERIODNESTING = 2148204802,        // 0x800B0102
            CertROLE = 2148204803,                         // 0x800B0103
            CertPATHLENCONST = 2148204804,                 // 0x800B0104
            CertCRITICAL = 2148204805,                     // 0x800B0105
            CertPURPOSE = 2148204806,                      // 0x800B0106
            CertISSUERCHAINING = 2148204807,               // 0x800B0107
            CertMALFORMED = 2148204808,                    // 0x800B0108
            CertUNTRUSTEDROOT = 2148204809,                // 0x800B0109
            CertCHAINING = 2148204810,                     // 0x800B010A
            CertREVOKED = 2148204812,                      // 0x800B010C
            CertUNTRUSTEDTESTROOT = 2148204813,            // 0x800B010D       
            CertREVOCATION_FAILURE = 2148204814,           // 0x800B010E
            CertCN_NO_MATCH = 2148204815,                  // 0x800B010F
            CertWRONG_USAGE = 2148204816,                  // 0x800B0110
            CertUNTRUSTEDCA = 2148204818                   // 0x800B0112
    }
        public static bool DefaultValidate = true;

        public bool CheckValidationResult(ServicePoint srvPoint, System.Security.Cryptography.X509Certificates.X509Certificate  cert, WebRequest request, int problem)
        {

            bool ValidationResult = false;
            Console.WriteLine(("Certificate Problem with accessing " +
               request.RequestUri.ToString()));

            Console.Write("Problem code 0x{0:X8},", problem);
            Console.WriteLine(GetProblemMessage((CertificateProblem)problem));
            ValidationResult = DefaultValidate;
            return ValidationResult;
        }

        private String GetProblemMessage(CertificateProblem Problem) {
        String ProblemMessage = "";
        CertificateProblem problemList;
        String ProblemCodeName = Problem.ToString();
        if (ProblemCodeName != null) {
            ProblemMessage = ProblemMessage + "-Certificateproblem:" + ProblemCodeName;
        }
        else {
            ProblemMessage = "Unknown Certificate Problem";
        }
        return ProblemMessage;
    }
  }

}
