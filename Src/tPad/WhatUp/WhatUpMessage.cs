using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.MTF;
using Jayrock.Json.Conversion;

namespace UofM.HCI.tPad.App.WhatUp
{
  public class WhatUpMessage : ITransportMessageContent
  {
    public const int MessageID = 3;

    int ITransportMessageContent.MessageID
    {
      get { return MessageID; }
    }

    public String From { get; set; }

    public String Message { get; set; }

    [JsonIgnore]
    public bool IsLocal { get; set; }

    [JsonIgnore]
    public Object Attachment { get; set; }

    public override string ToString()
    {
      return String.Format("From: {0} Message: {1}", From, Message);
    }

  }
}
