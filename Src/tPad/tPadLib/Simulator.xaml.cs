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
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;
using UofM.HCI.tPad.Util;
using UofM.HCI.tPad.Services;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Threading;
using UofM.HCI.tPad.App;
using System.Collections.ObjectModel;
using UofM.HCI.tPad.Network;

namespace UofM.HCI.tPad
{

  /// <summary>
  /// Interaction logic for Simulatorç.xaml
  /// </summary>
  public partial class Simulator : Window, INotifyPropertyChanged
  {

    private double deviceWidth, deviceHeight;
    private double appWidth, appHeight;
    private double frameWidth, frameHeight;

    private double startPageX = 0;
    private float widthFactor, heightFactor;
    private float simCaptureToSourceImageRatio;

    private int deviceCount = 1;

    private TPadProfile Profile { get; set; }
    private ITPadAppLauncher Launcher { get; set; }

    private TPadDocument actualDocument = null;
    public TPadDocument ActualDocument
    {
      get { return actualDocument; }
      set
      {
        actualDocument = value;
        ActualPage = 0;
        OnPropertyChanged("ActualDocument");
        OnPropertyChanged("TotalPages");
        OnPropertyChanged("ActualPage");
      }
    }

    public int TotalPages
    {
      get
      {
        if (ActualDocument == null || ActualDocument.Pages == null)
          return 0;
        return ActualDocument.Pages.Length;
      }
    }

    public int ActualPage { get; set; }

    public float WidthFactor
    {
      get { return widthFactor; }
      set
      {
        widthFactor = value;
        OnPropertyChanged("WidthFactor");
      }
    }

    public float HeightFactor
    {
      get { return heightFactor; }
      set
      {
        heightFactor = value;
        OnPropertyChanged("HeightFactor");
      }
    }

    public float SimCaptureToSourceImageRatio
    {
      get { return simCaptureToSourceImageRatio; }
      set
      {
        simCaptureToSourceImageRatio = value;
        OnPropertyChanged("SimCaptureToSourceImageRatio");
      }
    }

    public bool UseFeatureTracking
    {
      get { return TPadCore.UseFeatureTracking; }
      set { TPadCore.UseFeatureTracking = value; }
    }

    public double StartPageX
    {
      get { return startPageX; }
      set
      {
        startPageX = value;
        OnPropertyChanged("StartPageX");
      }
    }

    public double DeviceWidth
    {
      get { return deviceWidth; }
      set
      {
        deviceWidth = value;
        OnPropertyChanged("DeviceWidth");
      }
    }


    public double DeviceHeight
    {
      get { return deviceHeight; }
      set
      {
        deviceHeight = value;
        OnPropertyChanged("DeviceHeight");
      }
    }

    public double AppWidth
    {
      get { return appWidth; }
      set
      {
        appWidth = value;
        OnPropertyChanged("AppWidth");
      }
    }

    public double AppHeight
    {
      get { return appHeight; }
      set
      {
        appHeight = value;
        OnPropertyChanged("AppHeight");
      }
    }

    public double FrameWidth
    {
      get { return frameWidth; }
      set
      {
        frameWidth = value;
        OnPropertyChanged("FrameWidth");
      }
    }

    public double FrameHeight
    {
      get { return frameHeight; }
      set
      {
        frameHeight = value;
        OnPropertyChanged("FrameHeight");
      }
    }

    public ObservableCollection<ITPadApp> AppInstances { get; set; }

    public Simulator(ITPadAppLauncher launcher, TPadProfile profile)
    {
      Launcher = launcher;
      Profile = profile;
      AppInstances = new ObservableCollection<ITPadApp>();

      InitializeComponent();
      iDocument.SizeChanged += new SizeChangedEventHandler(iDocument_SizeChanged);

      ActualDocument = GetDocument("Blank", 0);
    }

    private void wSimulator_Loaded(object sender, RoutedEventArgs e)
    {
    }

    void iDocument_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      CalculateFactors();
    }

    private void CalculateFactors()
    {
      // This is the number of pixels per centimeter on the height.
      HeightFactor = (float)(iDocument.ActualHeight / ActualDocument.DocumentSize.Height);
      // This is the number of pixels per centimeter on the width
      WidthFactor = (float)(iDocument.ActualWidth / ActualDocument.DocumentSize.Width);

      // These two values should be nearly the same
      if (Math.Abs(HeightFactor - WidthFactor) >= 0.5)
        throw new ArgumentException("The document image does not match the specified document profile");

      //Calculates the X=0 for the device simulator 
      Rect docBounds = iDocument.TransformToAncestor(gTop).TransformBounds(VisualTreeHelper.GetDescendantBounds(iDocument));
      StartPageX = docBounds.Left;

      // The ratio between the capture and the source image is calculated.
      SimCaptureToSourceImageRatio = (float)((iDocument.Source as BitmapFrame).PixelWidth / iDocument.ActualWidth);

      //Resize the device
      DeviceWidth = WidthFactor * Profile.DeviceSize.Width;
      DeviceHeight = HeightFactor * Profile.DeviceSize.Height;
      //Adjusts the screen size to the device size
      AppWidth = WidthFactor * Profile.ScreenSize.Width;
      AppHeight = HeightFactor * Profile.ScreenSize.Height;
      //Adjusts the borders 
      FrameWidth = (DeviceWidth - AppWidth) / 2;
      FrameHeight = (DeviceHeight - AppHeight) / 2;
    }

    private void tbRunCore_Click(object sender, RoutedEventArgs e)
    {
      if (deviceCount == 4)
      {
        MessageBox.Show("The current implementation only supports 3 devices.");
        return;
      }

      if (cbSimCamera.IsSelected || cbComCamera.IsSelected)
      {
        foreach (ITPadApp appInstance in AppInstances)
        {
          if (appInstance.Core.UseCamera)
          {
            MessageBox.Show("You cannot use the camera in more than one device if a simulation is running.");
            return;
          }
        }
      }

      try
      {
        TPadLauncherSettings settings = new TPadLauncherSettings() { DeviceID = deviceCount++, BoardPort = null, UseCamera = false };
        if (cbSimCamera.IsSelected || cbComCamera.IsSelected)
        {
          chbUseFeatureTracking.IsChecked = true;
          chbUseFeatureTracking.IsEnabled = false;
          settings.UseCamera = true;
        }

        if (cbComSim.IsSelected || cbComCamera.IsSelected)
          settings.BoardPort = (cbComPort.SelectedItem as ComboBoxItem).Tag as String;
        settings = Launcher.GetSettings(settings);

        TPadCore core = new TPadCore();
        core.BoardCOM = settings.BoardPort;
        core.UseCamera = settings.UseCamera;
        core.Dispatcher = Dispatcher;
        core.Configure(Profile, settings.DeviceID, settings.MulticastGroup, settings.MulticastPort, settings.MulticastTTL);

        SimulatorDevice simDevice = new SimulatorDevice(this, core);
        simDevice.OnStackingControl += simDevice_OnStackingControl;
        simDevice.PropertyChanged += simDevice_PropertyChanged;
        simDevice.VerticalAlignment = System.Windows.VerticalAlignment.Top;
        simDevice.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
        BindingOperations.SetBinding(simDevice, SimulatorDevice.InitialXProperty, new Binding("StartPageX") { Source = this });
        BindingOperations.SetBinding(simDevice, SimulatorDevice.WidthProperty, new Binding("DeviceWidth") { Source = this });
        BindingOperations.SetBinding(simDevice, SimulatorDevice.HeightProperty, new Binding("DeviceHeight") { Source = this });
        BindingOperations.SetBinding(simDevice, SimulatorDevice.AppWidthProperty, new Binding("AppWidth") { Source = this });
        BindingOperations.SetBinding(simDevice, SimulatorDevice.AppHeightProperty, new Binding("AppHeight") { Source = this });
        BindingOperations.SetBinding(simDevice, SimulatorDevice.FrameWidthProperty, new Binding("FrameWidth") { Source = this });
        BindingOperations.SetBinding(simDevice, SimulatorDevice.FrameHeightProperty, new Binding("FrameHeight") { Source = this });

        //*************** TO RUN ON TPAD WINDOW *********************
        if (ckbSeparateWindow.IsChecked.Value)
        {
          simDevice.LoadTPadApp(new MockApp(Profile, simDevice, simDevice) { Core = core });
          gTop.Children.Add(simDevice);

          TPadWindow deviceWindow = new TPadWindow(Profile, Launcher, core);
          deviceWindow.Closed += deviceWindow_Closed;

          core.CoreStart(deviceWindow, simDevice);

          TPadApplicationDescriptor defaultAppDescriptor = Launcher.GetApplicationDescriptor();
          ITPadApp defatultApp = Launcher.GetAppInstance(defaultAppDescriptor, deviceWindow, simDevice, core, null);

          deviceWindow.LoadTPadApp(defatultApp);
          deviceWindow.Show();
          AppInstances.Add(defatultApp);
        }
        //*************** TO RUN ON SIMULATOR WINDOW *********************
        else
        {
          core.CoreStart(simDevice, simDevice);

          TPadApplicationDescriptor defaultAppDescriptor = Launcher.GetApplicationDescriptor();
          ITPadApp defatultApp = Launcher.GetAppInstance(defaultAppDescriptor, simDevice, simDevice, core, null);
          simDevice.LoadTPadApp(defatultApp);
          gTop.Children.Add(simDevice);
          AppInstances.Add(defatultApp);
        }
        //*************** END ********************************************
      }
      catch (Exception exception)
      {
        MessageBox.Show(exception.Message);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private void bNext_Click(object sender, RoutedEventArgs e)
    {
      ActualPage = (ActualPage + 1) % TotalPages;
      OnPropertyChanged("ActualPage");
    }

    private void bPrevious_Click(object sender, RoutedEventArgs e)
    {
      if (ActualPage == 0)
        ActualPage = TotalPages - 1;
      else
        ActualPage = ActualPage - 1;
      OnPropertyChanged("ActualPage");
    }

    private ImageCodecInfo GetEncoder(ImageFormat format)
    {
      ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
      foreach (ImageCodecInfo codec in codecs)
      {
        if (codec.FormatID == format.Guid)
          return codec;
      }
      return null;
    }

    protected override void OnClosed(EventArgs e)
    {
      base.OnClosed(e);

      foreach (ITPadApp instance in AppInstances)
      {
        if (instance.Container is Window)
        {
          instance.Core.CoreStop();
          (instance.Container as Window).Closed -= deviceWindow_Closed;
          (instance.Container as Window).Close();
        }
      }
    }

    void deviceWindow_Closed(object sender, EventArgs e)
    {
      chbUseFeatureTracking.IsEnabled = true;

      ITPadApp instanceClosed = AppInstances.FirstOrDefault(tmp => tmp.Container == sender);
      instanceClosed.Core.CoreStop();

      AppInstances.Remove(instanceClosed);
      gTop.Children.Remove(instanceClosed.Controller as UserControl);

      if (!(instanceClosed.Container is UserControl))
        (instanceClosed.Container as TPadWindow).Close();

      deviceCount--;
    }

    public void GetCoordinatesForScreenCapture(out int zeroX, out int zeroY)
    {
      zeroX = -1;
      zeroY = -1;

      int bordersize, bordertop;
      if (WindowState != System.Windows.WindowState.Maximized)
      {
        bordersize = (int)(ActualWidth - gTop.ActualWidth) / 2;
        bordertop = (int)(ActualHeight - gTop.ActualHeight - bordersize);
        zeroX = (int)(Left + bordersize);
        zeroY = (int)(Top + bordertop);
      }
      else
      {
        bordersize = (int)(ActualWidth - gTop.ActualWidth) / 2;
        bordertop = (int)(ActualHeight - gTop.ActualHeight - bordersize);
        zeroX = 0;
        zeroY = (int)(Top + bordertop);
      }
    }

    private SimulatorDevice topDevice = null;
    private SimulatorDevice bottomDevice = null;
    void simDevice_OnStackingControl(object sender, StackingControlEventArgs e)
    {
      SimulatorDevice source = (SimulatorDevice)sender;
      if (e.NewState == StackingControlState.Stacking)
      {
        if (topDevice == null)
          topDevice = source;
        else
        {
          bottomDevice = source;

          topDevice.StackingControlState = StackingControlState.StackedTop;
          bottomDevice.StackingControlState = StackingControlState.StackedBotton;

          topDevice.Location = bottomDevice.Location;
          topDevice.RotationAngle = bottomDevice.RotationAngle + 180;
          topDevice.SetValue(Grid.ZIndexProperty, 1);
          bottomDevice.SetValue(Grid.ZIndexProperty, 0);
          bottomDevice.DeviceOnTopID = topDevice.Core.Device.ID;
        }
      }
      else if (e.NewState == StackingControlState.None)
      {
        if (topDevice != null)
          topDevice.StackingControlState = StackingControlState.None;

        if (bottomDevice != null)
          bottomDevice.StackingControlState = StackingControlState.None;
        if (bottomDevice != null)
          bottomDevice.DeviceOnTopID = 0;

        topDevice = null;
        bottomDevice = null;
      }
    }

    void simDevice_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      SimulatorDevice source = (SimulatorDevice)sender;
      if (sender != topDevice || bottomDevice == null)
        return;

      if (e.PropertyName != "Location" && e.PropertyName != "RotationAngle")
        return;

      if (bottomDevice.Location != topDevice.Location)
        bottomDevice.Location = topDevice.Location;
      if (bottomDevice.RotationAngle != topDevice.RotationAngle)
        bottomDevice.RotationAngle = topDevice.RotationAngle - 180;
    }

    private void cbDocument_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ActualDocument = GetDocument((cbDocument.SelectedItem as ComboBoxItem).Tag as String, cbDocument.SelectedIndex);
    }

    private TPadDocument GetDocument(String conventionName, int documentID)
    {
      String docFolder = String.Format(@"Documents\{0}\", conventionName);
      if (!Directory.Exists(docFolder))
        throw new ArgumentException(String.Format("Folder '{0}' does not exist!", docFolder));

      TPadDocument result = new TPadDocument() { ID = documentID, Folder = docFolder, FileName = docFolder + String.Format("{0}.pdf", conventionName) };
      result.DocumentSize = new Size(21.59, 27.94); //US Letter - 215.9 mm × 279.4 mm

      String[] pages = Directory.GetFiles(docFolder, "*.png");
      Array.Sort<String>(pages);
      result.Pages = new TPadPage[pages.Length];
      for (int index = 0; index < pages.Length; index++)
        result.Pages[index] = new TPadPage() { PageIndex = index, FileName = pages[index] };

      return result;
    }

    private void btnInstanceClose_Click(object sender, RoutedEventArgs e)
    {
      deviceWindow_Closed(((sender as Button).DataContext as ITPadApp).Container, null);
    }

    private void btnWhatUpMSG_Click(object sender, RoutedEventArgs e)
    {
      Ubicomp.Utils.NET.MTF.TransportComponent.Instance.Send(
        new Ubicomp.Utils.NET.MTF.TransportMessage()
        {
          MessageId = Guid.NewGuid(),
          MessageType = 3,
          TimeStamp = DateTime.Now.ToString(),
          MessageSource = new Ubicomp.Utils.NET.MTF.EventSource()
          {
            ResourceId = Guid.NewGuid(),
            FriendlyName = "R.Sharkey",
            ResourceName = "R.Sharkey"
          },
          MessageData = new FakeWhatUpMessage() { From = "R.Sharkey", Message = "Hi! What's going on?" }
        });
    }

  }

}
