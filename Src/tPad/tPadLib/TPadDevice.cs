using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPab
{

  public class TPadDevice
  {
    //events
    public event RegistrationChangedEventHandler OnRegistrationChanged;
    public event FlippingChangedEventHandler OnFlipping;
    public event StackingChangedEventHandler OnStacking;

    //This id is written in the device's firmware (arduino)
    public int DeviceId { get; set; }
    public TPadProfile Profile { get; set; }

    // Loads the device ID from the arduino firmware
    internal void LoadId()
    {
      DeviceId = -1;
    }
  }

}
