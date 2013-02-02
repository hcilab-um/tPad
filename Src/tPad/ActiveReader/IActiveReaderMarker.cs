using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace UofM.HCI.tPab.App.ActiveReader
{
  public interface IActiveReaderMarker
  {
    double X { get; }

    double Y { get; }

    PointF Position { get; }

    IActiveReaderMarker Clone();
  }
}
