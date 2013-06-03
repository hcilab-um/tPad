﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jayrock.Json.Conversion;

namespace UofM.HCI.tPab.Network
{
  class StackingMessageExporter : IExporter
  {
    public void Export(ExportContext context, object value, Jayrock.Json.JsonWriter writer)
    {
      context.Export(value, writer);
    }

    public Type InputType
    {
      get { return typeof(StackingMessage); }
    }
  }
}