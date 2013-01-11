using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace UofM.HCI.tPab
{

  public interface ITPadAppContainer
  {
    float WidthFactor { get; }
    float HeightFactor { get; }
    float RotationAngle { get; }
    Point Location { get; }
    int ActualPage { get; set; }

    //This is the ratio between the page displayed in the simulator, and the page file stored in disk
    float SimCaptureToSourceImageRatio { get; }
  }

}

