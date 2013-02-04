﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextAdapter;
using System.Windows.Media.Media3D;

namespace UofM.HCI.tPab.Monitors
{

  public class SimBoardMonitor : ContextMonitor
  {

    public bool Pause { get; set; }

    private SimulatorDevice sDevice = null;
    public ITPadAppController SimDevice
    {
      get { return sDevice; }
      set
      {
        if (!(value is SimulatorDevice))
          return;

        sDevice = (value as SimulatorDevice);
        sDevice.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(sDevice_PropertyChanged);
      }
    }

    public SimBoardMonitor()
    {
      Pause = false;
    }

    protected override void CustomRun()
    {
      if (Pause)
        return;
    }

    void sDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName != "DeviceOnTopID")
        return;

      BoardUpdate update = new BoardUpdate() { DeviceOnTopID = sDevice.DeviceOnTopID, Orientation = new Point3D(0, 0, 0) };
      NotifyContextServices(this, new NotifyContextMonitorListenersEventArgs(typeof(BoardUpdate), update));
    }

  }

}
