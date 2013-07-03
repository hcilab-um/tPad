using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jayrock.Json.Conversion;
using Ubicomp.Utils.NET.MTF;

namespace UofM.HCI.tPad.Network
{
  public class FakeWhatUpMessage : ITransportMessageContent, IJsonExportable
  {

    public void Export(ExportContext context, Jayrock.Json.JsonWriter writer)
    {
      //"from":"Device-1","message":"JUAN"
      writer.WriteStartObject();
      writer.WriteMember("from");
      context.Export("Simulator", writer);
      writer.WriteMember("message");
      context.Export("Hello World!", writer);
      writer.WriteEndObject();
    }

    public int MessageID
    {
      get { throw new NotImplementedException(); }
    }
  }
}
