using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;
using UofM.HCI.tPad.Monitors;
using Ubicomp.Utils.NET.MTF;

namespace UofM.HCI.tPad.App.Dashboard
{
  /// <summary>
  /// Interaction logic for Dashboard.xaml
  /// </summary>
  public partial class DashboardApp : UserControl, ITPadApp, INotifyPropertyChanged, ITransportListener
  {

    public event BoolEventHandler IsTopApp;
    public event RequestUserFocus RequestFocus;

    public TPadCore Core { get; set; }
    public TPadProfile Profile { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }
    public Dictionary<String, String> Context { get { return null; } }

    public TPadApplicationDescriptor DashboardDescriptor { get; private set; }
    public TPadApplicationDescriptor NotificationDialogDescriptor { get; private set; }

    public ObservableCollection<TPadApplicationDescriptor> Applications { get; private set; }
    public ObservableCollection<TPadApplicationDescriptor> RunningApps { get; set; }

    public TPadApplicationDescriptor DefaultFlippingAppDescriptor { get; set; }

    public TPadApplicationDescriptor FaceUpAppDescriptor
    {
      get
      {
        TPadApplicationDescriptor descriptor = RunningApps.LastOrDefault(app => app.RunningSide == FlippingMode.FaceUp);
        return descriptor == null ? DashboardDescriptor : descriptor;
      }
    }

    public TPadApplicationDescriptor FaceDownAppDescriptor
    {
      get
      {
        TPadApplicationDescriptor descriptor = RunningApps.LastOrDefault(app => app.RunningSide == FlippingMode.FaceDown);
        return descriptor == null ? DashboardDescriptor : descriptor;
      }
    }

    public TPadApplicationDescriptor TopAppDescriptor
    {
      get
      {
        TPadApplicationDescriptor descriptor = RunningApps.FirstOrDefault(app => 
          {
            if (app.RunningSide != Core.Device.FlippingSide)
              return false;
            if ((app.Instance as UserControl).Visibility != System.Windows.Visibility.Visible)
              return false;
            return true;
          });
        return descriptor == null ? DashboardDescriptor : descriptor;
      }
    }

    public DashboardApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, TPadApplicationDescriptor descriptor)
    {
      Core = core;
      Container = container;
      Controller = controller;
      Profile = core.Profile;
      DashboardDescriptor = descriptor;
      DashboardDescriptor.Instance = this;

      NotificationDialogDescriptor = new TPadApplicationDescriptor() { AppClass = typeof(NotificationDialog) };
      NotificationDialogDescriptor.Instance = new NotificationDialog();
      NotificationDialogDescriptor.Instance.Closed += application_Closed;
      NotificationDialogDescriptor.Instance.IsTopApp += application_IsTopApp;

      Applications = new ObservableCollection<TPadApplicationDescriptor>();
      RunningApps = new ObservableCollection<TPadApplicationDescriptor>();
      InitializeComponent();

      core.GlyphsChanged += core_GlyphsChanged;
      core.Device.FlippingChanged += new FlippingChangedEventHandler(Device_FlippingChanged);
      core.Device.DeviceShaked += new EventHandler(Device_DeviceShaked);
      core.Device.HomePressed += new EventHandler(Device_HomePressed);
    }

    private void LaunchApp(TPadApplicationDescriptor descriptor, Dictionary<String, String> appContext = null, bool foreground = true)
    {
      if (descriptor == null)
        return;

      TPadLauncherSettings settings = new TPadLauncherSettings();
      settings.Context = appContext;

      ITPadApp application = descriptor.Launcher.GetAppInstance(descriptor, Container, Controller, Core, settings);
      descriptor.Instance = application;
      descriptor.Instance.Closed += application_Closed;
      descriptor.Instance.IsTopApp += application_IsTopApp;
      descriptor.Instance.RequestFocus += application_RequestFocus;
      descriptor.RunningSide = Core.Device.FlippingSide;

      RunningApps.Add(descriptor);

      Container.LoadTPadApp(application, foreground);
    }

    void application_Closed(object sender, EventArgs e)
    {
      TPadApplicationDescriptor descriptor = GetRunningDescriptor(sender.GetType());

      descriptor.Instance.Closed -= application_Closed;
      descriptor.Instance.IsTopApp -= application_IsTopApp;
      descriptor.Instance.RequestFocus -= application_RequestFocus;

      RunningApps.Remove(descriptor);
      FlippingMode side = descriptor.RunningSide;
      descriptor.RunningSide = Monitors.FlippingMode.Unknown;
      descriptor.Instance = null;

      if (side != Core.Device.FlippingSide)
        return;

      TPadApplicationDescriptor next = side == FlippingMode.FaceUp ? FaceUpAppDescriptor : FaceDownAppDescriptor;
      BringToFront(next, null);
    }

    bool application_IsTopApp(object sender, EventArgs e)
    {
      ITPadApp app = sender as ITPadApp;
      TPadApplicationDescriptor descriptor = GetRunningDescriptor(app.GetType());
      if (descriptor == TopAppDescriptor)
        return true;
      return false;
    }

    void application_RequestFocus(object sender, string message, string buttonOK, string buttonCancel)
    {
      Dictionary<String, String> context = new Dictionary<String,String>();
      context.Add("message", message);
      context.Add("buttonOK", buttonOK);
      context.Add("buttonCancel", buttonCancel);

      NotificationDialogDescriptor.Instance.LoadInitContext(context);
      NotificationDialogDescriptor.RunningSide = Core.Device.FlippingSide;
      Container.LoadTPadApp(NotificationDialogDescriptor.Instance, true);
    }

    private void BringToFront(TPadApplicationDescriptor descriptor, Dictionary<string, string> context)
    {
      descriptor.Instance.LoadInitContext(context);
      Container.Show(descriptor.Instance);
    }

    private void Minimize(TPadApplicationDescriptor descriptor)
    {
      Container.Hide(descriptor.Instance);
    }

    public ITPadApp GetRunningInstance(Type appType)
    {
      TPadApplicationDescriptor descriptor = GetRunningDescriptor(appType, FlippingMode.FaceUp);
      descriptor = descriptor != null ? descriptor : GetRunningDescriptor(appType, FlippingMode.FaceDown);
      return descriptor == null ? null : descriptor.Instance;
    }

    public TPadApplicationDescriptor GetRunningDescriptor(Type appType)
    {
      TPadApplicationDescriptor descriptor = GetRunningDescriptor(appType, FlippingMode.FaceUp);
      descriptor = descriptor != null ? descriptor : GetRunningDescriptor(appType, FlippingMode.FaceDown);
      return descriptor;
    }

    public TPadApplicationDescriptor GetRunningDescriptor(Type appType, FlippingMode side)
    {
      return RunningApps.LastOrDefault(app => app.AppClass.Equals(appType) && app.RunningSide == side);
    }

    private void Image_MouseUp(object sender, MouseButtonEventArgs e)
    {
      TPadApplicationDescriptor descriptor = (sender as Image).DataContext as TPadApplicationDescriptor;
      Minimize(DashboardDescriptor);

      ITPadApp application = GetRunningInstance(descriptor.AppClass);
      if (application != null)
        BringToFront(descriptor, null);
      else
        LaunchApp(descriptor);
    }

    void core_GlyphsChanged(object sender, GlyphsEventArgs e)
    {
      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          foreach (GlyphEvent gEvent in e.GlyphEvents)
          {
            //The top app handles such glyph
            if (TopAppDescriptor != null && TopAppDescriptor.Triggers.Contains(gEvent.Glyph))
              continue;

            //The dashboard handles the glyph by launching the app associated to it
            var descriptor = Applications.LastOrDefault(app => app.Triggers.Exists(glyph => glyph == gEvent.Glyph));
            if (descriptor == null)
              continue;

            if (gEvent.Status == GlyphStatus.Entered)
            {
              ITPadApp application = GetRunningInstance(descriptor.AppClass);
              if (application != null)
                BringToFront(descriptor, null);
              else
                LaunchApp(descriptor);
            }
            else if (gEvent.Status == GlyphStatus.Left)
            {
              ITPadApp application = GetRunningInstance(descriptor.AppClass);
              if (application == null)
                continue;
              application.Close();
            }
          }
        });
    }

    void Device_FlippingChanged(object sender, FlippingEventArgs e)
    {
      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          TPadApplicationDescriptor currentAD = e.FlippingSide == FlippingMode.FaceUp ? FaceDownAppDescriptor : FaceUpAppDescriptor;
          TPadApplicationDescriptor targetAD = e.FlippingSide == FlippingMode.FaceUp ? FaceUpAppDescriptor : FaceDownAppDescriptor;

          //The top app handles such glyph
          if (currentAD != DashboardDescriptor && currentAD.Events.Contains(TPadEvent.Flipping) ||
            targetAD != DashboardDescriptor && targetAD.Events.Contains(TPadEvent.Flipping))
            return;

          Minimize(currentAD);

          if (cbUserDefaultFlippingApp.IsChecked.Value && currentAD != DefaultFlippingAppDescriptor && targetAD != DefaultFlippingAppDescriptor)
            LaunchApp(DefaultFlippingAppDescriptor, TopAppDescriptor == null ? null : TopAppDescriptor.Instance.Context);
          else
            BringToFront(targetAD, currentAD == null ? null : currentAD.Instance.Context);
        });
    }

    void Device_DeviceShaked(object sender, EventArgs e)
    {
      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          //The top app handles such glyph
          if (TopAppDescriptor != null && TopAppDescriptor.Events.Contains(TPadEvent.Shaking))
            return;
        });
    }

    void Device_HomePressed(object sender, EventArgs e)
    {
      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          if (TopAppDescriptor == DashboardDescriptor)
            return;

          Minimize(TopAppDescriptor);
          BringToFront(DashboardDescriptor, null);
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    public event EventHandler Closed;
    public void Close()
    {
      if (Closed != null)
        Closed(this, EventArgs.Empty);
    }

    public void LoadInitContext(Dictionary<string, string> init) { }

    private void cbUserDefaultFlippingApp_Unchecked(object sender, RoutedEventArgs e)
    {
      if (DefaultFlippingAppDescriptor != null && DefaultFlippingAppDescriptor.Instance != null)
        DefaultFlippingAppDescriptor.Instance.Close();
    }

    public int MessageType
    {
      get { throw new NotImplementedException(); }
    }

    //Receives messages for all the possible applications and checks there is a running version of the app
    // This class acts as the pre-delivery listener
    public void MessageReceived(TransportMessage message, string rawMessage)
    {
      //Searchers all the networked apps by seeing if they implement the ITransportListener class
      var networkedApps = Applications.Where(app => app.AppClass.GetInterfaces().Contains(typeof(ITransportListener)));

      //Selects the ones that listen to the particular type of message
      var targetApps = networkedApps.Where(app =>
      {
        TPadLauncherSettings settings = new TPadLauncherSettings();
        settings = app.Launcher.GetSettings(settings);
        if (!settings.Context.ContainsKey("ITransportListener.MessageType"))
          return false;

        String messageType = settings.Context["ITransportListener.MessageType"];
        if (messageType == null)
          return false;

        int type = Int32.Parse(messageType);
        if (type == message.MessageType)
          return true;

        return false;
      });

      //Checks whether there is a runtime version of the app
      var notRunning = targetApps.Where(app => !RunningApps.Contains(app));

      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          //Launches all the instances that are not already running
          foreach (TPadApplicationDescriptor descriptor in notRunning)
            LaunchApp(descriptor, null, false);
        });
    }

    private void imgRunningApp_MouseUp(object sender, MouseButtonEventArgs e)
    {
      TPadApplicationDescriptor descriptor = (sender as Ellipse).DataContext as TPadApplicationDescriptor;
      descriptor.Instance.Close();
      BringToFront(DashboardDescriptor, null);
    }
  }

}
