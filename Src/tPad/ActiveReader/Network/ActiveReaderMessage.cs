using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.MTF;
using System.Windows.Ink;
using Jayrock.Json.Conversion;
using System.Windows.Markup;
using System.IO;
using System.Xml;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace UofM.HCI.tPab.App.ActiveReader.Network
{
  public class ActiveReaderMessage : ITransportMessageContent
  {
    public const int MessageID = 2;

    int ITransportMessageContent.MessageID
    {
      get { return MessageID; }
    }

    public ActiveReaderMessageType MessageType { get; set; }
    public int SourceDeviceID { get; set; }
    public int TargetDeviceID { get; set; }

    public int DocumentID { get; set; }
    public int PageIndex { get; set; }

    public Highlight ContentHL { get; set; }
    public Note ContentNT { get; set; }

    [JsonIgnore]
    public Stroke Stroke { get; set; }

    public String StrokeXAML
    {
      get {
        if (Stroke == null)
          return String.Empty;

        StrokeCollection wrapper = new StrokeCollection();
        wrapper.Add(Stroke);

        String result = XamlWriter.Save(wrapper);
        return result;
      }
      set
      {
        String strokeXaml = value;
        if (strokeXaml == null || strokeXaml == String.Empty)
          return;

        StringReader stringReader = new StringReader(strokeXaml);
        XmlReader xmlReader = XmlReader.Create(stringReader);

        StrokeCollection wrapper = (StrokeCollection)XamlReader.Load(xmlReader);
        Stroke = wrapper[0];
      }
    }
  }
}
