using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace NoResolver.Core.Requesters
{
    /// <summary>
    /// ITSM's SOAP API returns XML which can not be correctly serialised in C# due to empty tags. 
    /// This method preprocesses the XML before serialising.
    /// 
    /// https://communities.bmc.com/thread/46876
    /// (nb: hotfix for this bug came out in 2011)
    /// </summary>
    internal class SoapInterceptor : IClientMessageInspector
    {

        /// <summary>
        /// Method run after XML is returned by server but before application consumes it.
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="correlationState"></param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {

            // incercept message contents
            // https://stackoverflow.com/a/40460726/7466296

            Message newMessage = null;
            MessageBuffer msgbuf = reply.CreateBufferedCopy(int.MaxValue);
            Message tmpMessage = msgbuf.CreateMessage();

            XmlDictionaryReader xdr = tmpMessage.GetReaderAtBodyContents();

            XElement bodyEl = XElement.Load(xdr.ReadSubtree());

            // difference from stackoverflow code - remove empty elements
            bodyEl.Descendants()
                    .Where(e => e.IsEmpty || String.IsNullOrWhiteSpace(e.Value))
                    .Remove();

            MemoryStream ms = new MemoryStream();
            XmlWriter xw = XmlWriter.Create(ms);
            bodyEl.Save(xw);
            xw.Flush();
            xw.Close();

            ms.Position = 0;
            XmlReader xr = XmlReader.Create(ms);

            //create new message from modified XML document
            newMessage = Message.CreateMessage(reply.Version, null, xr);
            newMessage.Headers.CopyHeadersFrom(reply);
            newMessage.Properties.CopyProperties(reply.Properties);

            reply = newMessage;

        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            return null;
        }
    }
}
