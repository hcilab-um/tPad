using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UofM.HCI.tPab.App.ActiveReader.Network;
using Ubicomp.Utils.NET.MTF;
using UofM.HCI.tPab.App.ActiveReader;
using System.Windows.Threading;
using System.Windows.Ink;

namespace UofM.HCI.tPab.Network
{
  public class SynchHelper : ITransportListener
  {

    public event SynchContentEventHandler SynchContent;

    private TPadCore Core { get; set; }

    public SynchHelper(TPadCore core, Dispatcher dispatcher)
    {
      Core = core;

      TransportComponent.Instance.TransportListeners.Add(this);
      if (!TransportMessageExporter.Exporters.ContainsKey(ActiveReaderMessage.MessageID))
        TransportMessageExporter.Exporters.Add(ActiveReaderMessage.MessageID, new ActiveReaderMessageExporter());
      if (!TransportMessageImporter.Importers.ContainsKey(ActiveReaderMessage.MessageID))
        TransportMessageImporter.Importers.Add(ActiveReaderMessage.MessageID, new ActiveReaderMessageImporter(dispatcher));
    }

    public int MessageType
    {
      get { return ActiveReaderMessage.MessageID; }
    }

    public void MessageReceived(TransportMessage message, string rawMessage)
    {
      if (message.MessageSource.ResourceId == Core.Device.TMessageEventSource.ResourceId)
        return;

      ActiveReaderMessage arMessage = (ActiveReaderMessage)message.MessageData;
      if (arMessage.TargetDeviceID != Core.Device.ID)
        return;

      if (arMessage.MessageType == ActiveReaderMessageType.RequestCopyCurrentPage)
      {
        if (Core.Device.State != StackingState.StackedBelow)
          return;

        if (SynchContent != null)
          SynchContent(this, new SynchContentEventArgs() { Type = SynchContentEventType.RequestCopyCurrentPage });
      }
      else if (arMessage.MessageType == ActiveReaderMessageType.RequestCopyAll)
      {
        if (Core.Device.State != StackingState.StackedBelow)
          return;

        if (SynchContent != null)
          SynchContent(this, new SynchContentEventArgs() { Type = SynchContentEventType.RequestCopyAll });
      }
      else if (arMessage.MessageType == ActiveReaderMessageType.Highlight)
      {
        if (Core.Device.State != StackingState.StackedOnTop)
          return;

        if (SynchContent != null)
        {
          SynchContent(this, new SynchContentEventArgs()
          {
            Type = SynchContentEventType.Highlight,
            DocumentID = arMessage.DocumentID,
            PageIndex = arMessage.PageIndex,
            Content = arMessage.ContentHL
          });
        }
      }
      else if (arMessage.MessageType == ActiveReaderMessageType.Stroke)
      {
        if (Core.Device.State != StackingState.StackedOnTop)
          return;

        if (SynchContent != null)
        {
          SynchContent(this, new SynchContentEventArgs()
          {
            Type = SynchContentEventType.Stroke,
            DocumentID = arMessage.DocumentID,
            PageIndex = arMessage.PageIndex,
            Content = arMessage.Stroke
          });
        }
      }
      else if (arMessage.MessageType == ActiveReaderMessageType.Note)
      {
        if (Core.Device.State != StackingState.StackedOnTop)
          return;

        if (SynchContent != null)
        {
          SynchContent(this, new SynchContentEventArgs()
          {
            Type = SynchContentEventType.Note,
            DocumentID = arMessage.DocumentID,
            PageIndex = arMessage.PageIndex,
            Content = arMessage.ContentNT
          });
        }
      }
    }

    public void RequestCopyAll()
    {
      Core.Device.SendMessage(new ActiveReaderMessage()
      {
        MessageType = ActiveReaderMessageType.RequestCopyAll,
        SourceDeviceID = Core.Device.ID,
        TargetDeviceID = Core.Device.DeviceBelow
      });
    }

    public void RequestCopyCurrentPage()
    {
      Core.Device.SendMessage(new ActiveReaderMessage()
      {
        MessageType = ActiveReaderMessageType.RequestCopyCurrentPage,
        SourceDeviceID = Core.Device.ID,
        TargetDeviceID = Core.Device.DeviceBelow
      });
    }


    internal void SendContent(int documentID, int pageIndex, App.ActiveReader.Highlight highlight)
    {
      Core.Device.SendMessage(new ActiveReaderMessage()
      {
        MessageType = ActiveReaderMessageType.Highlight,
        SourceDeviceID = Core.Device.ID,
        TargetDeviceID = Core.Device.DeviceOnTop,
        DocumentID = documentID,
        PageIndex = pageIndex,
        ContentHL = highlight
      });
    }

    internal void SendContent(int documentID, int pageIndex, App.ActiveReader.ScribbleCollection collection)
    {
      foreach (Stroke stroke in collection.Scribbles)
      {
        Core.Device.SendMessage(new ActiveReaderMessage()
        {
          MessageType = ActiveReaderMessageType.Stroke,
          SourceDeviceID = Core.Device.ID,
          TargetDeviceID = Core.Device.DeviceOnTop,
          DocumentID = documentID,
          PageIndex = pageIndex,
          Stroke = stroke
        });
      }
    }

    internal void SendContent(int documentID, int pageIndex, App.ActiveReader.Note note)
    {
      Core.Device.SendMessage(new ActiveReaderMessage()
      {
        MessageType = ActiveReaderMessageType.Note,
        SourceDeviceID = Core.Device.ID,
        TargetDeviceID = Core.Device.DeviceOnTop,
        DocumentID = documentID,
        PageIndex = pageIndex,
        ContentNT = note
      });
    }
  }

  public enum SynchContentEventType { RequestCopyAll, RequestCopyCurrentPage, Highlight, Stroke, Note };

  public class SynchContentEventArgs : EventArgs
  {
    public SynchContentEventType Type { get; set; }
    public int DocumentID { get; set; }
    public int PageIndex { get; set; }
    public Object Content { get; set; }
  }

  public delegate void SynchContentEventHandler(object sender, SynchContentEventArgs e);

}
