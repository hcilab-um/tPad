using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.MTF;

namespace UofM.HCI.tPab.Network
{
  public enum StackingMessageType { StackingRequest, StackingResponse, EndNotification, LocationUpdate, StackingTouchEvent };

  public class StackingMessage : ITransportMessageContent
  {
    public const int MessageID = 1;

    int ITransportMessageContent.MessageID
    {
      get { return MessageID; }
    }

    public StackingMessageType MessageType { get; set; }
    public int SourceDeviceID { get; set; }
    public int TargetDeviceID { get; set; }

    public bool StakingRequestResponse { get; set; }

    public TPadLocation Location { get; set; }

    public System.Windows.Point TouchLocation { get; set; }
    public System.Windows.Input.TouchAction TouchAction { get; set; }


  }

}