using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPab
{

  public interface ITPadApp
  {
    TPadProfile Profile { get; set; }
    TPadDevice Device { get; set; }
    ITPadAppContainer Container { get; set; }

    double WidthScalingFactor { get; set; }
    double HeightScalingFactor { get; set; }
  }

}
