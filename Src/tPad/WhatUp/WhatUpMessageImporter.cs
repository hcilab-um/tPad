using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jayrock.Json.Conversion;
using System.Windows.Threading;

namespace UofM.HCI.tPad.App.WhatUp
{
  public class WhatUpMessageImporter : IImporter
  {
    public object Import(ImportContext context, Jayrock.Json.JsonReader reader)
    {
      WhatUpMessage pMessage = (WhatUpMessage)context.Import<WhatUpMessage>(reader); ;
      return pMessage;
    }

    public Type OutputType
    {
      get { return typeof(WhatUpMessage); }
    }
  }
}
