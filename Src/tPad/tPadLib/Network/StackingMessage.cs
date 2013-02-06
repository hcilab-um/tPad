using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.MTF;

namespace UofM.HCI.tPab.Network
{
  public enum StackingMessageType { StackingRequest, StackingResponse, EndNotification };

  public class StackingMessage : ITransportMessageContent
  {
    public const int StackingMessageID = 1;

    public StackingMessageType MessageType { get; set; }
    public int SourceDeviceID { get; set; }
    public int TargetDeviceID { get; set; }
    public bool StakingRequestResponse { get; set; }
  }
}