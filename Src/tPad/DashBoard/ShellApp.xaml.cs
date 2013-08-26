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
using UofM.HCI.tPad.App.Shell.Properties;
using UofM.HCI.tPad.App.InfSeeking;
using UofM.HCI.tPad.Controls;

namespace UofM.HCI.tPad.App.Shell
{
  /// <summary>
  /// Interaction logic for ShellApp.xaml
  /// </summary>
  public partial class ShellApp : UserControl, ITPadApp, INotifyPropertyChanged, ITransportListener
  {

    private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(ShellApp));

    public event BoolEventHandler IsTopApp;
    public event RequestUserFocus RequestFocus;

    public TPadCore Core { get; set; }
    public TPadProfile Profile { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }
    public Dictionary<String, Object> Context { get { return null; } }

    public Guid AppUUID { get { return ShellDescriptor.AppUUID; } }
    public TPadApplicationDescriptor ShellDescriptor { get; private set; }
    public TPadApplicationDescriptor NotificationDialogDescriptor { get; private set; }

    public ObservableCollection<TPadApplicationDescriptor> Applications { get; private set; }
    public ObservableCollection<TPadApplicationDescriptor> RunningApps { get; set; }

    public TPadApplicationDescriptor DefaultFlippingAppDescriptor { get; set; }

    public TPadApplicationDescriptor FaceUpAppDescriptor
    {
      get
      {
        TPadApplicationDescriptor descriptor = GetVisibleDescriptor(FlippingMode.FaceUp);
        return descriptor == null ? ShellDescriptor : descriptor;
      }
    }

    public TPadApplicationDescriptor FaceDownAppDescriptor
    {
      get
      {
        TPadApplicationDescriptor descriptor = GetVisibleDescriptor(FlippingMode.FaceDown);
        return descriptor == null ? ShellDescriptor : descriptor;
      }
    }

    public TPadApplicationDescriptor TopAppDescriptor
    {
      get
      {
        return Core.Device.FlippingSide == FlippingMode.FaceUp ? FaceUpAppDescriptor : FaceDownAppDescriptor;
      }
    }

    public ShellApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, TPadApplicationDescriptor descriptor)
    {
      Core = core;
      Container = container;
      Controller = controller;
      Profile = core.Profile;
      ShellDescriptor = descriptor;
      ShellDescriptor.Instance = this;

      NotificationDialogDescriptor = new TPadApplicationDescriptor()
      {
        AppType = typeof(NotificationDialog),
        AppUUID = Guid.NewGuid(),
        Icon = Properties.Resources.NotificationDialog
      };

      NotificationDialogDescriptor.Instance = new NotificationDialog(Core, NotificationDialogDescriptor.AppUUID);
      NotificationDialogDescriptor.Instance.Closed += application_Closed;
      NotificationDialogDescriptor.Instance.IsTopApp += application_IsTopApp;
      (NotificationDialogDescriptor.Instance as NotificationDialog).ClickedOK += new EventHandler(notification_ClickedOK);
      (NotificationDialogDescriptor.Instance as NotificationDialog).ClickedCancel += new EventHandler(notification_ClickedCancel);
      NotificationDialogDescriptor.Events.Add(TPadEvent.Flipping);

      Applications = new ObservableCollection<TPadApplicationDescriptor>();
      RunningApps = new ObservableCollection<TPadApplicationDescriptor>();
      InitializeComponent();

      core.GlyphsChanged += core_GlyphsChanged;
      core.Device.FlippingChanged += new FlippingChangedEventHandler(Device_FlippingChanged);
      core.Device.DeviceShaked += new EventHandler(Device_DeviceShaked);
      core.Device.HomePressed += new HomeButtonEventEventHandler(Device_HomePressed);
    }

    private void LaunchApp(TPadApplicationDescriptor descriptor, Dictionary<String, Object> appContext = null, bool foreground = true)
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

      if (descriptor.AppType == typeof(InfSeeking.InfSeekingApp))
      {
        (descriptor.Instance as InfSeeking.InfSeekingApp).GetTarget += ShellApp_GetTarget;
        (descriptor.Instance as InfSeeking.InfSeekingApp).SendResultOK += ShellApp_SendResultOK;
        (descriptor.Instance as InfSeeking.InfSeekingApp).SendErrorData += ShellApp_SendErrorData;
        (descriptor.Instance as InfSeeking.InfSeekingApp).SearchStarted += ShellApp_SearchStarted;
      }
      else if (descriptor.AppType == typeof(InfSeeking.InfProviderApp))
      {
        (descriptor.Instance as InfSeeking.InfProviderApp).GetTarget += ShellApp_GetTarget;
      }

      descriptor.RunningSide = Core.Device.FlippingSide;

      RunningApps.Add(descriptor);

      Container.LoadTPadApp(application, foreground);
    }

    void application_Closed(object sender, EventArgs e)
    {
      TPadApplicationDescriptor descriptor = GetRunningDescriptor((sender as ITPadApp).AppUUID);

      RunningApps.Remove(descriptor);
      FlippingMode side = descriptor.RunningSide;
      descriptor.RunningSide = Monitors.FlippingMode.Unknown;

      if (descriptor != NotificationDialogDescriptor)
      {
        descriptor.Instance.Closed -= application_Closed;
        descriptor.Instance.IsTopApp -= application_IsTopApp;
        descriptor.Instance.RequestFocus -= application_RequestFocus;
        descriptor.Instance = null;
      }

      if (side != Core.Device.FlippingSide)
        return;

      TPadApplicationDescriptor next = side == FlippingMode.FaceUp ? FaceUpAppDescriptor : FaceDownAppDescriptor;
      BringToFront(next, null);
    }

    bool application_IsTopApp(object sender, ObjectEventArgs e)
    {
      FlippingMode topSide = Core.Device.FlippingSide;
      if (e != null && e is ObjectEventArgs)
        topSide = (FlippingMode)e.Parameter;
      TPadApplicationDescriptor topDescriptor = topSide == FlippingMode.FaceUp ? FaceUpAppDescriptor : FaceDownAppDescriptor;

      ITPadApp app = sender as ITPadApp;
      TPadApplicationDescriptor descriptor = GetRunningDescriptor((sender as ITPadApp).AppUUID);
      if (descriptor == topDescriptor)
        return true;
      return false;
    }

    void application_RequestFocus(object sender, string message, string buttonOK, string buttonCancel)
    {
      TPadApplicationDescriptor descriptor = GetRunningDescriptor((sender as ITPadApp).AppUUID);

      Dictionary<String, Object> context = new Dictionary<String, Object>();
      context.Add("message", message);
      context.Add("buttonOK", buttonOK);
      context.Add("buttonCancel", buttonCancel);
      context.Add("sender", descriptor);
      context.Add("currentApp", TopAppDescriptor);

      RunningApps.Add(NotificationDialogDescriptor);
      NotificationDialogDescriptor.Instance.LoadInitContext(context);
      NotificationDialogDescriptor.RunningSide = Core.Device.FlippingSide;
      Container.LoadTPadApp(NotificationDialogDescriptor.Instance, true);
    }

    private void BringToFront(TPadApplicationDescriptor descriptor, Dictionary<string, Object> context)
    {
      descriptor.RunningSide = Core.Device.FlippingSide;
      descriptor.Instance.LoadInitContext(context);
      Container.Show(descriptor.Instance);
    }

    private void Minimize(TPadApplicationDescriptor descriptor)
    {
      Container.Hide(descriptor.Instance);
    }

    public ITPadApp GetRunningInstance(Guid appUUID)
    {
      if (appUUID == NotificationDialogDescriptor.AppUUID)
        return NotificationDialogDescriptor.Instance;

      TPadApplicationDescriptor descriptor = GetRunningDescriptor(appUUID, FlippingMode.FaceUp);
      descriptor = descriptor != null ? descriptor : GetRunningDescriptor(appUUID, FlippingMode.FaceDown);
      return descriptor == null ? null : descriptor.Instance;
    }

    public TPadApplicationDescriptor GetRunningDescriptor(Guid appUUID)
    {
      if (appUUID == NotificationDialogDescriptor.AppUUID)
        return NotificationDialogDescriptor;

      TPadApplicationDescriptor descriptor = GetRunningDescriptor(appUUID, FlippingMode.FaceUp);
      descriptor = descriptor != null ? descriptor : GetRunningDescriptor(appUUID, FlippingMode.FaceDown);
      return descriptor;
    }

    public TPadApplicationDescriptor GetRunningDescriptor(Guid appUUID, FlippingMode side)
    {
      return RunningApps.LastOrDefault(app => app.AppUUID.Equals(appUUID) && app.RunningSide == side);
    }

    private TPadApplicationDescriptor GetVisibleDescriptor(FlippingMode side)
    {
      TPadApplicationDescriptor descriptor = RunningApps.LastOrDefault(app =>
      {
        if (app.RunningSide != side)
          return false;
        if ((app.Instance as UserControl).Visibility != System.Windows.Visibility.Visible)
          return false;
        return true;
      });
      return descriptor;
    }

    private void Image_MouseUp(object sender, MouseButtonEventArgs e)
    {
      TPadApplicationDescriptor descriptor = (sender as Image).DataContext as TPadApplicationDescriptor;
      Minimize(ShellDescriptor);

      ITPadApp application = GetRunningInstance(descriptor.AppUUID);
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
            if (TopAppDescriptor.Triggers.Contains(gEvent.Glyph))
              continue;

            //The shell handles the glyph by launching the app associated to it
            var descriptor = Applications.LastOrDefault(app => app.Triggers.Exists(glyph => glyph == gEvent.Glyph));
            if (descriptor == null)
              continue;

            if (gEvent.Status == GlyphStatus.Entered)
            {
              ITPadApp application = GetRunningInstance(descriptor.AppUUID);
              if (application != null)
                BringToFront(descriptor, null);
              else
                LaunchApp(descriptor);
            }
            else if (gEvent.Status == GlyphStatus.Left)
            {
              ITPadApp application = GetRunningInstance(descriptor.AppUUID);
              if (application == null)
                continue;
              application.Close();
            }
          }
        });
    }

    void Device_FlippingChanged(object sender, FlippingEventArgs e)
    {
      if (e.Handled)
        return;

      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          if (!IsFlippingEnabled)
            return;

          TPadApplicationDescriptor currentAD = e.FlippingSide == FlippingMode.FaceUp ? FaceDownAppDescriptor : FaceUpAppDescriptor;
          TPadApplicationDescriptor targetAD = e.FlippingSide == FlippingMode.FaceUp ? FaceUpAppDescriptor : FaceDownAppDescriptor;

          //The top app handles flipping
          if (currentAD != ShellDescriptor && currentAD.Events.Contains(TPadEvent.Flipping))// || targetAD != DashboardDescriptor && targetAD.Events.Contains(TPadEvent.Flipping))
            return;

          currentAD.Instance.DeActivate();
          if (cbUserDefaultFlippingApp.IsChecked.Value && currentAD != DefaultFlippingAppDescriptor && targetAD != DefaultFlippingAppDescriptor)
          {
            if (RunningApps.Contains(DefaultFlippingAppDescriptor))
              BringToFront(DefaultFlippingAppDescriptor, null);
            else
              LaunchApp(DefaultFlippingAppDescriptor, currentAD.Instance.Context);
          }
          else
          {
            //Hides the runtime bar in case the dashboard is the next app on top
            spRunningApps.Visibility = System.Windows.Visibility.Collapsed;
            BringToFront(targetAD, currentAD.Instance.Context);
          }
        });
    }

    void Device_DeviceShaked(object sender, EventArgs e)
    {
      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          //The top app handles such glyph
          if (TopAppDescriptor.Events.Contains(TPadEvent.Shaking))
            return;
        });
    }

    void Device_HomePressed(object sender, HomeButtonEventArgs e)
    {
      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          if (e.Event == ButtonEvent.None)
            return;

          if (!IsHomeButtonEnabled && !IsRuntimeBarEnabled && TopAppDescriptor.AppType == typeof(InfSeekingApp))
            return;

          GoToPage(0);
          spRunningApps.Visibility = System.Windows.Visibility.Collapsed;
          if (IsRuntimeBarEnabled)
            spRunningApps.Visibility = System.Windows.Visibility.Visible;

          TopAppDescriptor.Instance.DeActivate();
          Minimize(TopAppDescriptor);
          BringToFront(ShellDescriptor, null);
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    public void DeActivate() { }

    public event EventHandler Closed;
    public void Close()
    {
      if (Closed != null)
        Closed(this, EventArgs.Empty);
    }

    public void LoadInitContext(Dictionary<string, Object> init) { }

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
      var networkedApps = Applications.Where(app => app.AppType.GetInterfaces().Contains(typeof(ITransportListener)));

      //Selects the ones that listen to the particular type of message
      var targetApps = networkedApps.Where(app =>
      {
        TPadLauncherSettings settings = new TPadLauncherSettings();
        settings = app.Launcher.GetSettings(settings);
        if (!settings.Context.ContainsKey("ITransportListener.MessageType"))
          return false;

        String messageType = settings.Context["ITransportListener.MessageType"] as String;
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
      BringToFront(ShellDescriptor, null);
    }

    void notification_ClickedOK(object sender, EventArgs e)
    {
      Minimize(TopAppDescriptor);

      TPadApplicationDescriptor descriptor = (NotificationDialogDescriptor.Instance as NotificationDialog).NotificationApp;
      BringToFront(descriptor, null);
    }

    void notification_ClickedCancel(object sender, EventArgs e)
    {
      Minimize(TopAppDescriptor);

      TPadApplicationDescriptor descriptor = (NotificationDialogDescriptor.Instance as NotificationDialog).ActualApp;
      BringToFront(descriptor, null);
    }

    private Dictionary<int, ObservableCollection<TPadApplicationDescriptor>> Pages = new Dictionary<int, ObservableCollection<TPadApplicationDescriptor>>();
    private ObservableCollection<TPadApplicationDescriptor> GetApplicationsPage(int pageNro)
    {
      if (Applications == null)
        return null;

      if (!Pages.ContainsKey(pageNro))
      {
        Pages.Add(pageNro, new ObservableCollection<TPadApplicationDescriptor>());

        int appsPerPage = 16;
        int startIndex = pageNro * appsPerPage;
        int endIndex = startIndex + appsPerPage; //non-inclusive
        if (Applications.Count <= startIndex)
          return null;

        for (int index = startIndex; index < endIndex; index++)
        {
          if (Applications.Count > index)
            Pages[pageNro].Add(Applications[index]);
        }
      }

      return Pages[pageNro];
    }

    private int currentPage = 0;
    private void bPrev_Click(object sender, RoutedEventArgs e)
    {
      GoToPage(Math.Max(0, currentPage - 1));
    }

    private void bNext_Click(object sender, RoutedEventArgs e)
    {
      GoToPage(Math.Min(3, currentPage + 1));
    }

    private void shellApp_Loaded(object sender, RoutedEventArgs e)
    {
      GoToPage(0);
      StartInfSeeking();
    }

    private void GoToPage(int destPage)
    {
      currentPage = destPage;
      lbAppsPage.ItemsSource = GetApplicationsPage(currentPage);
    }

    #region Information Seeking Experiments

    public bool IsFlippingEnabled
    {
      get
      {
        if (conditions == null || conditions.Count == 0 || currentCondition >= conditions.Count)
          return true;
        return conditions[currentCondition].Method == SwitchingMethod.Flipping || conditions[currentCondition].Method == SwitchingMethod.TapNFlip ? true : false;
      }
    }

    public bool IsHomeButtonEnabled
    {
      get
      {
        if (conditions == null || conditions.Count == 0 || currentCondition >= conditions.Count)
          return true;
        return conditions[currentCondition].Method == SwitchingMethod.Home ? true : false;
      }
    }

    public bool IsRuntimeBarEnabled
    {
      get
      {
        if (conditions == null || conditions.Count == 0 || currentCondition >= conditions.Count)
          return true;
        return conditions[currentCondition].Method == SwitchingMethod.RuntimeBar ? true : false;
      }
    }

    private List<InfSeeking.InfSeekingCondition> conditions;
    internal void SetInfSeekingExperiment(List<InfSeeking.InfSeekingCondition> inputC)
    {
      conditions = inputC;
    }

    private int currentCondition = 0;
    private int currentTrial = 0;
    private int currentSelection = 0;
    private void StartInfSeeking()
    {
      if (conditions == null || conditions.Count == 0)
        return;

      currentCondition = 0;
      currentTrial = 0;
      currentSelection = 0;

      MessageBox.Show(String.Format("In the following trials please use {0} for switching", conditions[currentCondition].Method));
    }

    InfSeeking.Exp1Target ShellApp_GetTarget(object sender, EventArgs e)
    {
      if (currentCondition >= conditions.Count)
      {
        MessageBox.Show("Experiment Finished");
        return null;
      }

      if (currentTrial >= conditions[currentCondition].Targets.Count / 3)
      {
        currentCondition++;
        currentTrial = 0;
        currentSelection = 0;

        if (currentCondition < conditions.Count)
          MessageBox.Show(String.Format("In the following trials please use {0} for switching", conditions[currentCondition].Method));

        return ShellApp_GetTarget(sender, e);
      }

      if (currentSelection >= 3)
      {
        CloseAllAppsExcept(typeof(InfSeekingApp));
        GoToPage(0);

        currentTrial++;
        currentSelection = 0;
        return ShellApp_GetTarget(sender, e);
      }

      Exp1Target target = conditions[currentCondition].Targets[currentTrial * 3 + currentSelection];
      if (sender is InfSeekingApp && target.FirstSeen == DateTime.MinValue)
        target.FirstSeen = DateTime.Now;
      if (sender is InfProviderApp && target.DataFound == DateTime.MinValue)
        target.DataFound = DateTime.Now;
      target.ReChecks++;
      return target;
    }

    private void CloseAllAppsExcept(Type type)
    {
      var appsToClose = RunningApps.Where(app => app.AppType != type).ToArray();
      foreach (TPadApplicationDescriptor app in appsToClose)
        RunningApps.Remove(app);
    }

    void ShellApp_SearchStarted(object sender, EventArgs e)
    {
      InfSeekingCondition condition = conditions[currentCondition];
      Exp1Target currentTarget = condition.Targets[currentTrial * 3 + currentSelection];
      if (currentTarget.SeekStarted == DateTime.MinValue)
        currentTarget.SeekStarted = DateTime.Now;
    }

    void ShellApp_SendErrorData(object sender, EventArgs e)
    {
      InfSeekingCondition condition = conditions[currentCondition];
      Exp1Target currentTarget = condition.Targets[currentTrial * 3 + currentSelection];
      currentTarget.Errors++;
    }

    void ShellApp_SendResultOK(object sender, EventArgs e)
    {
      DateTime finalTime = DateTime.Now;

      InfSeekingCondition condition = conditions[currentCondition];
      Exp1Target currentTarget = condition.Targets[currentTrial * 3 + currentSelection];

      String logLine = String.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10}",
        DateTime.Now,
        condition.Method,
        condition.AppsNumber,
        currentTrial + 1,
        currentSelection + 1,
        (int)currentTarget.SourceApp.SourceGroup,
        (finalTime - currentTarget.FirstSeen).TotalMilliseconds,
        (finalTime - currentTarget.SeekStarted).TotalMilliseconds,
        (finalTime - currentTarget.DataFound).TotalMilliseconds,
        currentTarget.ReChecks,
        currentTarget.Errors);

      logger.Info(logLine);

      currentSelection++;
    }

    #endregion

  }

}
