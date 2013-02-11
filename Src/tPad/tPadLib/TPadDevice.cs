using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Ubicomp.Utils.NET.MTF;
using UofM.HCI.tPab.Network;

namespace UofM.HCI.tPab
{

  public class TPadDevice : INotifyPropertyChanged
  {
    //events
    public event PropertyChangedEventHandler PropertyChanged;
    public event RegistrationChangedEventHandler RegistrationChanged;
    public event FlippingChangedEventHandler FlippingChanged;
    public event StackingChangedEventHandler StackingChanged;

    //This id is written in the device's firmware (arduino)
    public int ID { get; private set; }
    public TPadProfile Profile { get; set; }
    private EventSource TMessageEventSource { get; set; }

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

    private int DeviceOnTop { get; set; }
    private int DeviceBelow { get; set; }

    public TPadDevice(int deviceID)
    {
      ID = deviceID;
      TMessageEventSource = new EventSource(
        new Guid(String.Format("00000000-0000-0000-0000-00000000000{0}", ID)),
        String.Format("{0}-{1}", System.Environment.MachineName, ID),
        String.Format("{0}-{1}", System.Environment.MachineName, ID));
    }

    internal void ProcessStackingUpdate(Monitors.StackingUpdate stackingUpdate)
    {
      StackingMessage messageToSend = new StackingMessage()
      {
        MessageType = StackingMessageType.StackingRequest,
        SourceDeviceID = ID,
        TargetDeviceID = stackingUpdate.DeviceOnTopID
      };
      TransportComponent.Instance.Send(
        new TransportMessage()
        {
          MessageSource = TMessageEventSource,
          MessageType = StackingMessage.StackingMessageID,
          MessageData = messageToSend
        });
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
