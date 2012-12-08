using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAF.ContextAdapter;
using UofM.HCI.tPab.Monitors;
using CAF.ContextService;
using System.Windows;

namespace UofM.HCI.tPab
{
  public class TPadCore : ContextService
  {

    private static log4net.ILog logger ;

    public TPadDevice Device { get; set; }
    public TPadProfile Profile { get; set; }

    private static TPadCore instance = null;
    public static TPadCore Instance
    {
      get 
      {
        if (instance == null)
          instance = new TPadCore();
        return instance;
      }
    }

    private TPadCore()
    { 
    }

    public void Startup(TPadProfile profile, bool simulation = false, UIElement cameraSource = null)
    {
      log4net.Config.XmlConfigurator.Configure();
      logger = log4net.LogManager.GetLogger(typeof(TPadCore));

      Profile = profile;
      Device = new TPadDevice() { Profile = Profile };
      Device.LoadId();

      ContextMonitor cameraMonitor = null, flippingMonitor = null, stackingMonitor = null;
      if (simulation)
      {
        // These are the fictitious monitors used in simulation mode
        cameraMonitor = new SimCameraMonitor() { UpdateType = ContextAdapterUpdateType.Interval, UpdateInterval = 200, CameraSource = cameraSource };
        flippingMonitor = new SimFlippingMonitor() { UpdateType = ContextAdapterUpdateType.Continous };
        stackingMonitor = new SimStackingMonitor() { UpdateType = ContextAdapterUpdateType.Continous };
      }
      else
      {
        // Create here the actual monitors
      }

      cameraMonitor.OnNotifyContextServices += this.UpdateMonitorReading;
      flippingMonitor.OnNotifyContextServices += this.UpdateMonitorReading;
      stackingMonitor.OnNotifyContextServices += this.UpdateMonitorReading;

      ContextMonitorContainer.AddMonitor(cameraMonitor);
      ContextMonitorContainer.AddMonitor(flippingMonitor);
      ContextMonitorContainer.AddMonitor(stackingMonitor);

      ContextServiceContainer.AddContextService(this);

      logger.Info("Starting Up Monitors");
      ContextServiceContainer.StartServices();
      ContextMonitorContainer.StartMonitors();
      logger.Info("Monitors Started");
    }

    protected override void CustomUpdateMonitorReading(object sender, NotifyContextMonitorListenersEventArgs e)
    {

    }

  }

}
