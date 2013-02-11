using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextAdapter;
using Ubicomp.Utils.NET.MTF;
using System.Net;
using UofM.HCI.tPab.Network;

namespace UofM.HCI.tPab.Monitors
{
  public class MulticastMonitor : ContextMonitor, ITransportListener
  {

    log4net.ILog logger = log4net.LogManager.GetLogger(typeof(MulticastMonitor));

    public MulticastMonitor(String groupIP, int port, int TTL)
    {
      TransportComponent.Instance.MulticastGroupAddress = IPAddress.Parse(groupIP);
      TransportComponent.Instance.Port = port;
      TransportComponent.Instance.UDPTTL = TTL;
      updateType = ContextAdapterUpdateType.OnRequest;
    }

    protected override void CustomStart()
    {
      base.CustomStart();

      TransportComponent.Instance.TransportListeners.Add(this);
      if (!TransportMessageExporter.Exporters.ContainsKey(StackingMessage.StackingMessageID))
        TransportMessageExporter.Exporters.Add(StackingMessage.StackingMessageID, new StackingMessageExporter());
      if (!TransportMessageImporter.Importers.ContainsKey(StackingMessage.StackingMessageID))
        TransportMessageImporter.Importers.Add(StackingMessage.StackingMessageID, new StackingMessageImporter());

      TransportComponent.Instance.Init();
    }

    public int MessageType
    {
      get { return StackingMessage.StackingMessageID; }
    }

    public void MessageReceived(TransportMessage message, string rawMessage)
    {
      logger.Debug(String.Format("Message Received - {0} & {1}:{2}", message.MessageType, (message.MessageData as StackingMessage).MessageType, rawMessage));

      NotifyContextServices(this, new NotifyContextMonitorListenersEventArgs(typeof(TransportMessage), message));
    }
  }
}
