using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace UofM.HCI.tPab
{

  public interface ITPadAppController
  {
    int ActualPage { get; }
    TPadDocument ActualDocument { get; }

    float RotationAngle { get; }
    Point Location { get; }

    float WidthFactor { get; }
    float HeightFactor { get; }
  }

}

