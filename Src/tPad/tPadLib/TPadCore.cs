using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAF.ContextAdapter;
using UofM.HCI.tPab.Monitors;
using CAF.ContextService;
using System.Windows;
using UofM.HCI.tPab.Services;
using System.ComponentModel;

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

    public TPadCore()
    {
      Registration = new RegistrationService();
    }

    public void Configure(TPadProfile profile)
    {
      log4net.Config.XmlConfigurator.Configure();
      logger = log4net.LogManager.GetLogger(typeof(TPadCore));

      Profile = profile;
      Device = new TPadDevice() { Profile = Profile };
      Device.LoadId();

      Board = new BoardMonitor() { UpdateType = ContextAdapterUpdateType.Continous };
      SimBoard = new SimBoardMonitor() { UpdateType = ContextAdapterUpdateType.Continous };
      Camera = new CameraMonitor() { UpdateType = ContextAdapterUpdateType.Interval, UpdateInterval = 50 };
      SimCamera = new SimCameraMonitor() { UpdateType = ContextAdapterUpdateType.Interval, UpdateInterval = 50 };

      ContextMonitor flippingMonitor = new FlippingMonitor() { UpdateType = ContextAdapterUpdateType.Continous };
      ContextMonitor stackingMonitor = new StackingMonitor() { UpdateType = ContextAdapterUpdateType.Continous };

      //Wiring up the components
      Board.OnNotifyContextServices += (flippingMonitor as FlippingMonitor).UpdateMonitorReading;
      Board.OnNotifyContextServices += (stackingMonitor as StackingMonitor).UpdateMonitorReading;
      SimBoard.OnNotifyContextServices += (flippingMonitor as FlippingMonitor).UpdateMonitorReading;
      SimBoard.OnNotifyContextServices += (stackingMonitor as StackingMonitor).UpdateMonitorReading;
      Camera.OnNotifyContextServices += Registration.UpdateMonitorReading;
      SimCamera.OnNotifyContextServices += Registration.UpdateMonitorReading;
      flippingMonitor.OnNotifyContextServices += this.UpdateMonitorReading;
      stackingMonitor.OnNotifyContextServices += this.UpdateMonitorReading;
      Registration.OnNotifyContextServiceListeners += this.ContextChanged;

      //Register the monitors to the container
      ContextMonitorContainer.AddMonitor(Board);
      ContextMonitorContainer.AddMonitor(Camera);
      ContextMonitorContainer.AddMonitor(SimBoard);
      ContextMonitorContainer.AddMonitor(SimCamera);
      ContextMonitorContainer.AddMonitor(flippingMonitor);
      ContextMonitorContainer.AddMonitor(stackingMonitor);

      //Register the services to the container
      ContextServiceContainer.AddContextService(this);
      ContextServiceContainer.AddContextService(Registration);
    }

    public void CoreStart(ITPadAppContainer appContainer, ITPadAppController appController, String boardPort, String cameraPort)
    {
      ConfigurePeripherals(boardPort, cameraPort);

      //By default the system works with the simulated sources (camera, board)
      SimCamera.CameraSource = appController;
      Registration.Container = appContainer;
      Registration.Controller = appController;

      logger.Info("Starting Up Services and Monitors");
      ContextServiceContainer.StartServices();
      ContextMonitorContainer.StartMonitors();
      logger.Info("Monitors Started");
    }

    public void ConfigurePeripherals(String boardPort, String cameraPort)
    {
      //Stops everything
      Board.COMPort = null;
      SimBoard.Pause = true;
      Camera.COMPort = null;
      SimCamera.Pause = true;

      //Sets the COM port for the board and camera monitors
      Board.COMPort = boardPort;
      Camera.COMPort = cameraPort;

      //Gets everything ready to re-start
      if (Board.COMPort == null)
        SimBoard.Pause = false;
      if (Camera.COMPort == null)
        SimCamera.Pause = false;
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

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

  }

}
