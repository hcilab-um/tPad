using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.MTF;

namespace UofM.HCI.tPad.App.PhotoAlbum.Network
{
  public class PhotoMessage : ITransportMessageContent
  {
    public const int MessageID = 2;

    int ITransportMessageContent.MessageID
    {
      get { return MessageID; }
    }

    public PhotoMessageType Type { get; set; }

    public Guid Transaction { get; set; }
    public String FileName { get; set; }
    public int TotalParts { get; set; }
    public int ActualPart { get; set; }
    public int ContentSize { get; set; }
    public byte[] Content { get; set; }

    public override string ToString()
    {
      return String.Format("Transaction: {0} FileName: {1} Part: {2}", Transaction, FileName, ActualPart);
    }

    public enum PhotoMessageType { FilePart, DragStarted, DragFinished };

  }
}
