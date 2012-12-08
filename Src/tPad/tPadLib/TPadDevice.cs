using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace UofM.HCI.tPab
{

  public class TPadDevice : INotifyPropertyChanged
  {
    //events
    public event PropertyChangedEventHandler PropertyChanged;
    public event RegistrationChangedEventHandler RegistrationChanged;
    public event FlippingChangedEventHandler Flipping;
    public event StackingChangedEventHandler Stacking;

    //This id is written in the device's firmware (arduino)
    public int DeviceId { get; set; }
    public TPadProfile Profile { get; set; }

    private TPadLocation location;
    public TPadLocation Location 
    {
      get { return location; }
      set 
      {
        TPadLocation lastLocation = location;
        location = value;
        OnPropertyChanged("Location");
        OnRegistrationChanged(lastLocation, location);
      }
    }

    // Loads the device ID from the arduino firmware
    internal void LoadId()
    {
      DeviceId = -1;
    }
    
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    public void OnRegistrationChanged(TPadLocation last, TPadLocation newL)
    {
      if (RegistrationChanged != null)
        RegistrationChanged(this, new RegistrationEventArgs() { LastLocation = last, NewLocation = newL });
    }

  }

}
