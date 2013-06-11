using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jayrock.Json.Conversion;

namespace UofM.HCI.tPad.Network
{
  public class StackingMessageImporter : IImporter
  {
    public object Import(ImportContext context, Jayrock.Json.JsonReader reader)
    {
      return (StackingMessage)context.Import<StackingMessage>(reader);
    }

    public Type OutputType
    {
      get { return typeof(StackingMessage); }
    }
  }
}
