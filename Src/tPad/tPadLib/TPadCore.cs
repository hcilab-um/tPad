using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAF.ContextAdapter;
using UofM.HCI.tPab.Monitors;
using CAF.ContextService;

namespace UofM.HCI.tPab
{
  public class TPadCore : ContextService
  {

    private static log4net.ILog logger;

    public TPadDevice Device { get; set; }

    private ContextMonitor CameraMonitor { get; set; }
    private ContextMonitor FlippingMonitor { get; set; }
    private ContextMonitor StackingMonitor { get; set; }

    public void Startup(bool simulation = false)
    {

      if (simulation)
      {
        // These are the fictitious monitors used in simulation mode
        CameraMonitor = new SimCameraMonitor() { UpdateType = ContextAdapterUpdateType.Interval, UpdateInterval = 200 };
        FlippingMonitor = new SimFlippingMonitor() { UpdateType = ContextAdapterUpdateType.Continous };
        StackingMonitor = new SimStackingMonitor() { UpdateType = ContextAdapterUpdateType.Continous };
      }
      else
      {
        // Create here the actual monitors
      }

      CameraMonitor.OnNotifyContextServices += this.UpdateMonitorReading;
      FlippingMonitor.OnNotifyContextServices += this.UpdateMonitorReading;
      StackingMonitor.OnNotifyContextServices += this.UpdateMonitorReading;

      ContextMonitorContainer.AddMonitor(CameraMonitor);
      ContextMonitorContainer.AddMonitor(FlippingMonitor);
      ContextMonitorContainer.AddMonitor(StackingMonitor);

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
