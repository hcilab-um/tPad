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

  public class StackingEventArgs : EventArgs
  {
  }

  public delegate void StackingChangedEventHandler(object sender, StackingEventArgs e);

  public class RegistrationEventArgs : EventArgs
  {
    public TPadLocation LastLocation { get; set; }
    public TPadLocation NewLocation { get; set; }
  }

  public delegate void RegistrationChangedEventHandler(object sender, RegistrationEventArgs e);

}
