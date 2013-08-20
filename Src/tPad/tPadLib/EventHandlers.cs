using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UofM.HCI.tPad.Monitors;
using UofM.HCI.tPad.App;

namespace UofM.HCI.tPad
{

  public class FlippingEventArgs : EventArgs
  {
    public FlippingMode FlippingSide { get; set; }
    public bool Handled { get; set; }
  }

  public delegate void FlippingChangedEventHandler(object sender, FlippingEventArgs e);

  public enum StackingState { NotStacked, StackedOnTop, StackedBelow };

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

  public class GlyphsEventArgs : EventArgs
  {
    public List<GlyphEvent> GlyphEvents { get; set; }
  }

  public delegate void GlyphsChangedEventHandler(object sender, GlyphsEventArgs e);

  public class StackingTouchEventArgs : EventArgs
  {
    public System.Windows.Point Location { get; set; }
    public System.Windows.Input.TouchAction Action { get; set; }
  }

  public delegate void StackingTouchEventEventHandler(object sender, StackingTouchEventArgs e);

  public class HomeButtonEventArgs : EventArgs 
  {
    public ButtonEvent Event { get; set; }
  }

  public delegate void HomeButtonEventEventHandler(object sender, HomeButtonEventArgs e);

}
