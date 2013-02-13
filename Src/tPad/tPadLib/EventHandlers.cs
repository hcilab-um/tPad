using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPab
{

  public class FlippingEventArgs : EventArgs
  { 
  }

  public delegate void FlippingChangedEventHandler(object sender, FlippingEventArgs e);

  public enum StackingState { StackedOnTop, StackedBelow, NotStacked };

  public class StackingEventArgs : EventArgs
  {
    public StackingState State { get; set; }
    public int DeviceOnTop { get; set; }
    public int DeviceBelow { get; set; }

    public StackingEventArgs()
    {
      State = StackingState.NotStacked;
      DeviceOnTop = -1;
      DeviceBelow = -1;
    }
  }

  public delegate void StackingChangedEventHandler(object sender, StackingEventArgs e);

  public class RegistrationEventArgs : EventArgs
  {
    public TPadLocation LastLocation { get; set; }
    public TPadLocation NewLocation { get; set; }
  }

  public delegate void RegistrationChangedEventHandler(object sender, RegistrationEventArgs e);

}
