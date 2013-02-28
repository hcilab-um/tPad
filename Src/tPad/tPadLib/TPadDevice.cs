﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Ubicomp.Utils.NET.MTF;
using UofM.HCI.tPab.Network;
using UofM.HCI.tPab.Monitors;

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

    private float anchorToFlowAngle = 0;
    public float AnchorToFlowAngle
    {
      get { return anchorToFlowAngle; }
      set
      {
        anchorToFlowAngle = value;
        OnPropertyChanged("AnchorToFlowAngle");
      }
    }

    private StackingState state = StackingState.NotStacked;
    public StackingState State
    {
      get { return state; }
      set
      {
        state = value;
        OnPropertyChanged("State");
      }
    }

    private int deviceOnTop = -1;
    public int DeviceOnTop
    {
      get { return deviceOnTop; }
      set
      {
        deviceOnTop = value;
        OnPropertyChanged("DeviceOnTop");
      }
    }

    private int deviceBelow = -1;
    public int DeviceBelow
    {
      get { return deviceBelow; }
      set
      {
        deviceBelow = value;
        OnPropertyChanged("DeviceBelow");
      }
    }

    private FlippingMode flippingSide = FlippingMode.FaceUp;
    public FlippingMode FlippingSide
    {
      get { return flippingSide; }
      set
      {
        flippingSide = value;
        OnPropertyChanged("FlippingSide");

        if (FlippingChanged != null)
          FlippingChanged(this, new FlippingEventArgs());
      }
    }

    public TPadDevice(int deviceID)
    {
      ID = deviceID;

      State = StackingState.NotStacked;
      DeviceBelow = -1;
      DeviceOnTop = -1;

      TMessageEventSource = new EventSource(
        new Guid(String.Format("00000000-0000-0000-0000-00000000000{0}", ID)),
        String.Format("{0}-{1}", System.Environment.MachineName, ID),
        String.Format("{0}-{1}", System.Environment.MachineName, ID));

      PropertyChanged += TPadDevice_PropertyChanged;
    }

    internal void ProcessStackingUpdate(Monitors.StackingUpdate stackingUpdate)
    {
      if (stackingUpdate.Event == Monitors.StackingEvent.PhyicalStacking)
      {
        SendMessage(new StackingMessage()
        {
          MessageType = StackingMessageType.StackingRequest,
          SourceDeviceID = ID,
          TargetDeviceID = stackingUpdate.DeviceOnTopID
        });
      }
      else if (stackingUpdate.Event == Monitors.StackingEvent.PhysicalSeparation)
      {
        SendMessage(new StackingMessage()
        {
          MessageType = StackingMessageType.EndNotification,
          SourceDeviceID = ID,
          TargetDeviceID = stackingUpdate.DeviceOnTopID
        });

        State = StackingState.NotStacked;
        DeviceBelow = -1;
        DeviceOnTop = -1;

        //Notify the app that it is no longer stacked 
        if (StackingChanged != null)
          StackingChanged(this, new StackingEventArgs() { State = StackingState.NotStacked, DeviceBelow = DeviceBelow, DeviceOnTop = DeviceOnTop });
      }
    }

    internal void ProcessStackingUpdate(TransportMessage tMessage)
    {
      if (tMessage.MessageSource.ResourceId == TMessageEventSource.ResourceId)
        return;

      StackingMessage sMessage = (StackingMessage)tMessage.MessageData;
      if (sMessage.TargetDeviceID != ID)
        return;

      if (sMessage.MessageType == StackingMessageType.StackingRequest)
      {
        State = StackingState.StackedOnTop;
        DeviceBelow = sMessage.SourceDeviceID;
        DeviceOnTop = ID;

        SendMessage(new StackingMessage()
        {
          MessageType = StackingMessageType.StackingResponse,
          SourceDeviceID = ID,
          TargetDeviceID = sMessage.SourceDeviceID,
          StakingRequestResponse = true
        });

        //Notify the app that it has been stacked (on top)
        if (StackingChanged != null)
          StackingChanged(this, new StackingEventArgs() { State = StackingState.StackedOnTop, DeviceBelow = DeviceBelow, DeviceOnTop = DeviceOnTop });
      }
      else if (sMessage.MessageType == StackingMessageType.StackingResponse)
      {
        if (sMessage.StakingRequestResponse == false)
          return;

        State = StackingState.StackedBelow;
        DeviceBelow = ID;
        DeviceOnTop = sMessage.SourceDeviceID;

        //Notify the app that it has been stacked (below)
        if (StackingChanged != null)
          StackingChanged(this, new StackingEventArgs() { State = StackingState.StackedBelow, DeviceBelow = DeviceBelow, DeviceOnTop = DeviceOnTop });
      }
      else if (sMessage.MessageType == StackingMessageType.EndNotification)
      {
        State = StackingState.NotStacked;
        DeviceBelow = -1;
        DeviceOnTop = -1;

        //Notify the app that it is no longer stacked 
        if (StackingChanged != null)
          StackingChanged(this, new StackingEventArgs() { State = StackingState.NotStacked, DeviceBelow = DeviceBelow, DeviceOnTop = DeviceOnTop });
      }
      else if (sMessage.MessageType == StackingMessageType.LocationUpdate)
      {
        if (State != StackingState.StackedOnTop)
          return;

        Location = sMessage.Location;
      }
    }

    private void SendMessage(StackingMessage sMessage)
    {
      TransportComponent.Instance.Send(
        new TransportMessage()
        {
          MessageSource = TMessageEventSource,
          MessageType = StackingMessage.StackingMessageID,
          MessageData = sMessage
        });
    }

    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    void TPadDevice_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Location")
      {
        if (location == null)
          return;
        AnchorToFlowAngle = CalculateAnchoredToFlowAngle(location.RotationAngle);
      }
    }

    public void OnRegistrationChanged(TPadLocation last, TPadLocation newL)
    {
      if (RegistrationChanged != null)
        RegistrationChanged(this, new RegistrationEventArgs() { LastLocation = last, NewLocation = newL });

      if (State != StackingState.StackedBelow)
        return;

      SendMessage(new StackingMessage()
      {
        MessageType = StackingMessageType.LocationUpdate,
        Location = newL,
        SourceDeviceID = ID,
        TargetDeviceID = DeviceOnTop
      });
    }

    private float CalculateAnchoredToFlowAngle(float angle)
    {
      angle = angle % 360;
      float destAngle = 0;
      if (315 < angle || angle <= 45)
        destAngle = 0;
      else if (45 < angle && angle <= 135)
        destAngle = 270;
      else if (135 < angle && angle <= 225)
        destAngle = 180;
      else if (225 < angle && angle <= 315)
        destAngle = 90;
      return destAngle;
    }

  }

}
