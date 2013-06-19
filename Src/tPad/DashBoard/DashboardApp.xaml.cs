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

namespace UofM.HCI.tPad.App.Dashboard
{
  /// <summary>
  /// Interaction logic for Dashboard.xaml
  /// </summary>
  public partial class DashboardApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public TPadCore Core { get; set; }
    public TPadProfile Profile { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }
    public Dictionary<String, String> Context { get { return null; } }

    public ObservableCollection<TPadApplicationDescriptor> Applications { get; private set; }
    public Stack<TPadApplicationDescriptor> RunningApps { get; set; }

    public TPadApplicationDescriptor DefaultFlippingAppDescriptor { get; set; }

    public TPadApplicationDescriptor FaceUpAppDescriptor
    {
      get { return RunningApps.FirstOrDefault(app => app.RunningSide == FlippingMode.FaceUp); }
    }

    public TPadApplicationDescriptor FaceDownAppDescriptor
    {
      get { return RunningApps.FirstOrDefault(app => app.RunningSide == FlippingMode.FaceDown); }
    }

    public TPadApplicationDescriptor TopAppDescriptor
    {
      get { return RunningApps.FirstOrDefault(app => app.RunningSide == Core.Device.FlippingSide); }
    }

    public DashboardApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller)
    {
      Core = core;
      Container = container;
      Controller = controller;
      Profile = core.Profile;

      Applications = new ObservableCollection<TPadApplicationDescriptor>();
      RunningApps = new Stack<TPadApplicationDescriptor>();
      InitializeComponent();

      core.GlyphsChanged += core_GlyphsChanged;
      core.Device.FlippingChanged += new FlippingChangedEventHandler(Device_FlippingChanged);
      core.Device.DeviceShaked += new EventHandler(Device_DeviceShaked);
    }

    private void LaunchApp(TPadApplicationDescriptor descriptor, Dictionary<String, String> topAppContext = null)
    {
      if (descriptor == null)
        return;

      TPadLauncherSettings settings = new TPadLauncherSettings();
      settings.Context = topAppContext;

      ITPadApp application = descriptor.Launcher.GetAppInstance(descriptor, Container, Controller, Core, settings);
      descriptor.Instance = application;
      descriptor.Instance.Closed += new EventHandler(application_Closed);
      descriptor.RunningSide = Core.Device.FlippingSide;
      RunningApps.Push(descriptor);
      Container.LoadTPadApp(application);
    }

    void application_Closed(object sender, EventArgs e)
    {
      TPadApplicationDescriptor descriptor = RunningApps.Pop();
      descriptor.RunningSide = Monitors.FlippingMode.Unknown;
      descriptor.Instance = null;
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
      TPadApplicationDescriptor descriptor = RunningApps.SingleOrDefault(app => app.AppClass.Equals(appType));
      if (descriptor == null)
        return null;
      return descriptor.Instance;
    }

    private void Image_MouseUp(object sender, MouseButtonEventArgs e)
    {
      TPadApplicationDescriptor descriptor = (sender as Image).DataContext as TPadApplicationDescriptor;
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
            var descriptor = Applications.SingleOrDefault(app => app.Triggers.Exists(glyph => glyph == gEvent.Glyph));
            if (descriptor == null)
              continue;

            if (gEvent.Status == GlyphStatus.Entered)
            {
              ITPadApp application = GetRunningInstance(descriptor.AppClass);
              if (application != null)
                continue;
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
          if (currentAD != null && currentAD.Events.Contains(TPadEvent.Flipping) ||
            targetAD != null && targetAD.Events.Contains(TPadEvent.Flipping))
            return;

          if (currentAD != null)
            Minimize(currentAD);

          if (targetAD != null)
            BringToFront(targetAD, currentAD == null ? null : currentAD.Instance.Context);
          else if (cbUserDefaultFlippingApp.IsChecked.Value && currentAD != DefaultFlippingAppDescriptor)
            LaunchApp(DefaultFlippingAppDescriptor, TopAppDescriptor == null ? null : TopAppDescriptor.Instance.Context);
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

  }

}
