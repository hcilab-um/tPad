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
using UofM.HCI.tPab.Applications;

namespace UofM.HCI.tPab
{
  public class TPadCore : ContextService, IContextServiceListener, INotifyPropertyChanged
  {

    private static log4net.ILog logger;

    //This is a shared variable among all the instances of core
    public static bool UseFeatureTracking { get; set; }

    public event GlyphsChangedEventHandler GlyphsChanged;
    private List<Glyph> currentGlyphs = new List<Glyph>();

    public TPadDevice Device { get; set; }
    public TPadProfile Profile { get; set; }

    public RegistrationService Registration { get; set; }
    public GlyphDetectionService GlyphDetection { get; set; }

    private BoardMonitor Board { get; set; }
    private SimBoardMonitor SimBoard { get; set; }
    private SimCameraMonitor SimCamera { get; set; }

    public String BoardCOM { get; set; }
    public bool UseCamera { get; set; }

    private ContextMonitorContainer monitorsContainer = null;
    private ContextServiceContainer servicesContainer = null;

    public TPadCore()
    {
      monitorsContainer = new ContextMonitorContainer();
      servicesContainer = new ContextServiceContainer();
    }

    public void Configure(TPadProfile profile, int deviceID, String groupIP, int port, int TTL)
    {
      log4net.Config.XmlConfigurator.Configure();
      logger = log4net.LogManager.GetLogger(typeof(TPadCore));

      Profile = profile;
      Device = new TPadDevice(deviceID) { Profile = Profile };
      Registration = new RegistrationService(UseCamera, Device);
      GlyphDetection = new GlyphDetectionService(Device);

      Board = new BoardMonitor() { UpdateType = ContextAdapterUpdateType.Interval, UpdateInterval = 100 };
      SimBoard = new SimBoardMonitor() { UpdateType = ContextAdapterUpdateType.OnRequest };
      SimCamera = new SimCameraMonitor() { UpdateType = ContextAdapterUpdateType.Interval, UpdateInterval = 50 };

      ContextMonitor flippingMonitor = new FlippingMonitor() { UpdateType = ContextAdapterUpdateType.Continous };
      ContextMonitor stackingMonitor = new StackingMonitor() { UpdateType = ContextAdapterUpdateType.Continous };
      MulticastMonitor multicastMonitor = new MulticastMonitor(groupIP, port, TTL);

      //Wiring up the components
      Board.OnNotifyContextServices += (flippingMonitor as FlippingMonitor).UpdateMonitorReading;
      Board.OnNotifyContextServices += (stackingMonitor as StackingMonitor).UpdateMonitorReading;
      SimBoard.OnNotifyContextServices += (flippingMonitor as FlippingMonitor).UpdateMonitorReading;
      SimBoard.OnNotifyContextServices += (stackingMonitor as StackingMonitor).UpdateMonitorReading;
      SimCamera.OnNotifyContextServices += Registration.UpdateMonitorReading;
      SimCamera.OnNotifyContextServices += GlyphDetection.UpdateMonitorReading;
      flippingMonitor.OnNotifyContextServices += this.UpdateMonitorReading;
      stackingMonitor.OnNotifyContextServices += this.UpdateMonitorReading;
      multicastMonitor.OnNotifyContextServices += this.UpdateMonitorReading;
      Registration.OnNotifyContextServiceListeners += this.ContextChanged;
      GlyphDetection.OnNotifyContextServiceListeners += this.ContextChanged;

      //Register the monitors to the container
      monitorsContainer.AddMonitor(Board);
      monitorsContainer.AddMonitor(SimBoard);
      monitorsContainer.AddMonitor(SimCamera);
      monitorsContainer.AddMonitor(flippingMonitor);
      monitorsContainer.AddMonitor(stackingMonitor);
      monitorsContainer.AddMonitor(multicastMonitor);

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
      Registration.Pause();
      monitorsContainer.StopMonitors();
      servicesContainer.StopServices();
    }

    private void ConfigurePeripherals()
    {
      //Stops everything
      Board.COMPort = null;
      SimBoard.Pause = true;
      SimCamera.Pause = true;

      //Sets the COM port for the board and camera monitors
      Board.COMPort = BoardCOM;

      if (!Board.TryPort())
        SimBoard.Pause = false;

      if (!UseCamera)
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
        Device.ProcessStackingUpdate((TransportMessage)e.NewObject);
      }
      else if (e.Type == typeof(FlippingMode))
      {
        Device.FlippingSide = (FlippingMode)e.NewObject;
      }
    }

    public void ContextChanged(object sender, NotifyContextServiceListenersEventArgs e)
    {
      if (sender == Registration)
      {
        Device.Location = (TPadLocation)e.NewObject;
      }
      if (sender == GlyphDetection)
      {
        List<GlyphEvent> events = new List<GlyphEvent>();
        List<Glyph> glyphs = (List<Glyph>)e.NewObject;

        foreach (Glyph glyph in glyphs)
        {
          if (currentGlyphs.Exists(tmp => tmp == glyph))
            continue;
          events.Add(new GlyphEvent() { Glyph = glyph, Status = GlyphStatus.Entered });
        }

        foreach (Glyph glyph in currentGlyphs)
        {
          if (glyphs.Exists(tmp => tmp == glyph))
            continue;
          events.Add(new GlyphEvent() { Glyph = glyph, Status = GlyphStatus.Left });
        }

        currentGlyphs = glyphs;
        if (GlyphsChanged != null)
          GlyphsChanged(this, new GlyphsEventArgs() { GlyphEvents = events });
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