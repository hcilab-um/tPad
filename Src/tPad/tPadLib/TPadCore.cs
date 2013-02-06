using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UofM.HCI.tPab.Monitors;
using System.Windows;
using UofM.HCI.tPab.Services;
using System.ComponentModel;
using Ubicomp.Utils.NET.CAF.ContextService;
using Ubicomp.Utils.NET.CAF.ContextAdapter;
using UofM.HCI.tPab.Network;
using Ubicomp.Utils.NET.MTF;

namespace UofM.HCI.tPab
{
  public class TPadCore : ContextService, IContextServiceListener, INotifyPropertyChanged
  {

    private static log4net.ILog logger;

    //This is a shared variable among all the instances of core
    public static bool UseFeatureTracking { get; set; }

    public TPadDevice Device { get; set; }
    public TPadProfile Profile { get; set; }

    public RegistrationService Registration { get; set; }

    private BoardMonitor Board { get; set; }
    private SimBoardMonitor SimBoard { get; set; }
    private CameraMonitor Camera { get; set; }
    private SimCameraMonitor SimCamera { get; set; }

    public String BoardCOM { get; set; }
    public String CameraCOM { get; set; }

    private ContextMonitorContainer monitorsContainer = null;
    private ContextServiceContainer servicesContainer = null;

    public TPadCore()
    {
      monitorsContainer = new ContextMonitorContainer();
      servicesContainer = new ContextServiceContainer();

      Registration = new RegistrationService();
    }

    public void Configure(TPadProfile profile, int deviceID, String groupIP, int port, int TTL)
    {
      log4net.Config.XmlConfigurator.Configure();
      logger = log4net.LogManager.GetLogger(typeof(TPadCore));

      Profile = profile;
      Device = new TPadDevice(deviceID) { Profile = Profile };

      Board = new BoardMonitor() { UpdateType = ContextAdapterUpdateType.Interval, UpdateInterval = 100 };
      SimBoard = new SimBoardMonitor() { UpdateType = ContextAdapterUpdateType.OnRequest };
      Camera = new CameraMonitor() { UpdateType = ContextAdapterUpdateType.Interval, UpdateInterval = 100 };
      SimCamera = new SimCameraMonitor() { UpdateType = ContextAdapterUpdateType.Interval, UpdateInterval = 100 };

      ContextMonitor flippingMonitor = new FlippingMonitor() { UpdateType = ContextAdapterUpdateType.Continous };
      ContextMonitor stackingMonitor = new StackingMonitor() { UpdateType = ContextAdapterUpdateType.Continous };
      //MulticastMonitor multicastMonitor = new MulticastMonitor(groupIP, port, TTL);

      //Wiring up the components
      Board.OnNotifyContextServices += (flippingMonitor as FlippingMonitor).UpdateMonitorReading;
      Board.OnNotifyContextServices += (stackingMonitor as StackingMonitor).UpdateMonitorReading;
      SimBoard.OnNotifyContextServices += (flippingMonitor as FlippingMonitor).UpdateMonitorReading;
      SimBoard.OnNotifyContextServices += (stackingMonitor as StackingMonitor).UpdateMonitorReading;
      Camera.OnNotifyContextServices += Registration.UpdateMonitorReading;
      SimCamera.OnNotifyContextServices += Registration.UpdateMonitorReading;
      flippingMonitor.OnNotifyContextServices += this.UpdateMonitorReading;
      stackingMonitor.OnNotifyContextServices += this.UpdateMonitorReading;
      //multicastMonitor.OnNotifyContextServices += this.UpdateMonitorReading;
      Registration.OnNotifyContextServiceListeners += this.ContextChanged;

      //Register the monitors to the container
      monitorsContainer.AddMonitor(Board);
      monitorsContainer.AddMonitor(Camera);
      monitorsContainer.AddMonitor(SimBoard);
      monitorsContainer.AddMonitor(SimCamera);
      monitorsContainer.AddMonitor(flippingMonitor);
      monitorsContainer.AddMonitor(stackingMonitor);

      //Register the services to the container
      servicesContainer.AddContextService(this);
      servicesContainer.AddContextService(Registration);
    }

    public void CoreStart(ITPadAppContainer appContainer, ITPadAppController appController)
    {
      ConfigurePeripherals();

      //By default the system works with the simulated sources (camera, board)
      SimBoard.SimDevice = appController;
      SimCamera.CameraSource = appController;
      Registration.Container = appContainer;
      Registration.Controller = appController;

      logger.Info("Starting Up Services and Monitors");
      servicesContainer.StartServices();
      monitorsContainer.StartMonitors();
      logger.Info("Monitors Started");
    }

    public void CoreStop()
    {
      monitorsContainer.StopMonitors();
      servicesContainer.StopServices();
    }

    private void ConfigurePeripherals()
    {
      //Stops everything
      Board.COMPort = null;
      SimBoard.Pause = true;
      Camera.COMPort = null;
      SimCamera.Pause = true;

      //Sets the COM port for the board and camera monitors
      Board.COMPort = BoardCOM;
      Camera.COMPort = CameraCOM;

      if (!Board.TryPort())
        throw new ArgumentException(String.Format("Board COM port {0} could not be opened", Board.COMPort));
      if (!Camera.TryPort())
        throw new ArgumentException(String.Format("Camera COM port {0} could not be opened", Camera.COMPort));

      //Gets everything ready to re-start
      if (Board.COMPort == null)
        SimBoard.Pause = false;
      if (Camera.COMPort == null)
        SimCamera.Pause = false;
    }

    protected override void CustomUpdateMonitorReading(object sender, NotifyContextMonitorListenersEventArgs e)
    {
      if (e.Type == typeof(StackingUpdate))
      {
        Device.ProcessStackingUpdate((StackingUpdate)e.NewObject);
      }
      else if (e.Type == typeof(TransportMessage))
      { 
      }
    }

    public void ContextChanged(object sender, NotifyContextServiceListenersEventArgs e)
    {
      if (sender == Registration)
      {
        Device.Location = (TPadLocation)e.NewObject;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

  }

}
