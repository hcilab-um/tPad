using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAF.ContextAdapter;
using UofM.HCI.tPab.Monitors;
using CAF.ContextService;
using System.Windows;
using UofM.HCI.tPab.Services;

namespace UofM.HCI.tPab
{
  public class TPadCore : ContextService, IContextServiceListener
  {

    private static log4net.ILog logger;

    public bool IsSimulation { get; set; }
    public TPadDevice Device { get; set; }
    public TPadProfile Profile { get; set; }

    public RegistrationService Registration { get; set; }

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
      Registration = new RegistrationService();
    }

    public void Configure(TPadProfile profile, bool isSimulation = false)
    {
      log4net.Config.XmlConfigurator.Configure();
      logger = log4net.LogManager.GetLogger(typeof(TPadCore));

      IsSimulation = isSimulation;
      Profile = profile;
      Device = new TPadDevice() { Profile = Profile };
      Device.LoadId();

      ArduinoMonitor arduino = null;
      ContextMonitor cameraMonitor = null, flippingMonitor = null, stackingMonitor = null;
      if (IsSimulation)
      {
        // These are the fictitious monitors used in simulation mode
        cameraMonitor = new SimCameraMonitor() { UpdateType = ContextAdapterUpdateType.Interval, UpdateInterval = 200 };
        flippingMonitor = new SimFlippingMonitor() { UpdateType = ContextAdapterUpdateType.Continous };
        stackingMonitor = new SimStackingMonitor() { UpdateType = ContextAdapterUpdateType.Continous };
      }
      else
      {
        // Create here the actual monitors
        arduino = new ArduinoMonitor() { UpdateType = ContextAdapterUpdateType.Continous };
        cameraMonitor = new CameraMonitor() { UpdateType = ContextAdapterUpdateType.Interval, UpdateInterval = 50 };
        flippingMonitor = new FlippingMonitor() { UpdateType = ContextAdapterUpdateType.Continous };
        stackingMonitor = new StackingMonitor() { UpdateType = ContextAdapterUpdateType.Continous };

        // Arduino wiring
        arduino.OnNotifyContextServices += (flippingMonitor as FlippingMonitor).UpdateMonitorReading;
        arduino.OnNotifyContextServices += (stackingMonitor as StackingMonitor).UpdateMonitorReading;
        // Arduino embedding
        ContextMonitorContainer.AddMonitor(arduino);
      }

      //Wiring up the components
      cameraMonitor.OnNotifyContextServices += Registration.UpdateMonitorReading;
      flippingMonitor.OnNotifyContextServices += this.UpdateMonitorReading;
      stackingMonitor.OnNotifyContextServices += this.UpdateMonitorReading;
      Registration.OnNotifyContextServiceListeners += this.ContextChanged;

      //Register the monitors to the container
      ContextMonitorContainer.AddMonitor(cameraMonitor);
      ContextMonitorContainer.AddMonitor(flippingMonitor);
      ContextMonitorContainer.AddMonitor(stackingMonitor);

      //Register the services to the container
      ContextServiceContainer.AddContextService(this);
      ContextServiceContainer.AddContextService(Registration);
    }

    public void CoreStart(ITPadAppContainer cameraSource = null)
    {
      if(IsSimulation && cameraSource != null)
      {
        SimCameraMonitor cameraMonitor = (SimCameraMonitor)ContextMonitorContainer.GetContextMonitor(typeof(SimCameraMonitor));
        cameraMonitor.CameraSource = cameraSource;
        Registration.Container = cameraSource;
      }

      logger.Info("Starting Up Services and Monitors");
      ContextServiceContainer.StartServices();
      ContextMonitorContainer.StartMonitors();
      logger.Info("Monitors Started");
    }

    protected override void CustomUpdateMonitorReading(object sender, NotifyContextMonitorListenersEventArgs e)
    {
    }

    public void ContextChanged(object sender, NotifyContextServiceListenersEventArgs e)
    {
      if (sender == Registration)
      {
        Device.Location = (TPadLocation)e.NewObject;
      }
    }

  }

}
