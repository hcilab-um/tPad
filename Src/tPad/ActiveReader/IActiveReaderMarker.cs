using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace UofM.HCI.tPad.App.ActiveReader
{
  public interface IActiveReaderMarker
  {
    Guid ID { get; }

    double X { get; }

    double Y { get; }
    
    Point Position { get; }

    IActiveReaderMarker Clone();
  }
}
