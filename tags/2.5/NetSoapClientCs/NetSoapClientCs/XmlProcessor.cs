using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.IO;
using System.Net;
using System.Reflection;

using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using System.Web.Configuration;
using Microsoft.Web.Services2;
using Microsoft.Web.Services2.Security;
using Microsoft.Web.Services2.Security.Utility;


namespace NetSoapClientCs
{
   public class CustomFilter : SoapOutputFilter
   {

       public const string WSA_NS = "http://schemas.xmlsoap.org/ws/2004/03/addressing";
       public const string WSU_NS = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
       public const string WSSE_NS = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

       private NetSoapClientCs frm = null;
       SoapWebRequest req = null;

       public CustomFilter(NetSoapClientCs Frm, SoapWebRequest Req)
       {
           this.frm = Frm;
           this.req = Req;
       }
       
       
      public override void ProcessMessage(SoapEnvelope envelope)
      {


          if (this.frm.ChkSupressAction.Checked)
          {
              XmlNode actionNode = envelope.Header["Action", WSA_NS];
              while (actionNode != null)
              {
                  envelope.Header.RemoveChild(actionNode);
                  actionNode = envelope.Header["Action", WSA_NS];
              }
          }
          else
          {
              if (this.frm.SoapAction.Text != null && this.frm.SoapAction.Text.Length > 0)
              {
                  this.req.Headers.Add("SoapAction", this.frm.SoapAction.Text);
              }
          }

          if (this.frm.ChkSupressMessageID.Checked)
          {
              XmlNode messageNode =
                  envelope.Header["MessageID", WSA_NS];

              while (messageNode != null)
              {
                  envelope.Header.RemoveChild(messageNode);
                  messageNode =
                      envelope.Header["MessageID", WSA_NS];
              }
          }

          if (this.frm.ChkSupressReplyTo.Checked)
          {
              XmlNode replyToNode =
                  envelope.Header["ReplyTo", WSA_NS];
              while (replyToNode != null)
              {
                  envelope.Header.RemoveChild(replyToNode);
                  replyToNode =
                      envelope.Header["ReplyTo", WSA_NS];
              }
          }

          if (this.frm.ChkSupressTo.Checked)
          {
              XmlNode toNode = envelope.Header["To", WSA_NS];
              while (toNode != null)
              {
                  envelope.Header.RemoveChild(toNode);
                  toNode = envelope.Header["To", WSA_NS];
              }
          }

      }
   }
}





