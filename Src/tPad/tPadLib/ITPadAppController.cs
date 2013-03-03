using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

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

    //This is the ratio between the page displayed in the simulator, and the page file stored in disk
    float SimCaptureToSourceImageRatio { get; }
  }

}

