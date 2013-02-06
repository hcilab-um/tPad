using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jayrock.Json.Conversion;

namespace UofM.HCI.tPab.Network
{
  public class StackingMessageImporter : IImporter
  {
    public object Import(ImportContext context, Jayrock.Json.JsonReader reader)
    {
      throw new NotImplementedException();
    }

    public Type OutputType
    {
      get { throw new NotImplementedException(); }
    }
  }
}
