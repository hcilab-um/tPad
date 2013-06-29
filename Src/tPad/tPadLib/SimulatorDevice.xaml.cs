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
using System.IO;
using System.Threading;
using UofM.HCI.tPad.Util;
using System.Drawing.Imaging;
using UofM.HCI.tPad.Monitors;

namespace UofM.HCI.tPad
{

  /// <summary>
  /// Interaction logic for SimulatorDevice.xaml
  /// </summary>
  public partial class SimulatorDevice : UserControl, INotifyPropertyChanged, ITPadAppController, ITPadAppContainer
  {

    public event EventHandler<StackingControlEventArgs> OnStackingControl;

    public static readonly DependencyProperty AppWidthProperty = DependencyProperty.Register("AppWidth", typeof(double), typeof(SimulatorDevice));
    public double AppWidth
    {
      get { return (double)GetValue(AppWidthProperty); }
      set { SetValue(AppWidthProperty, value); }
    }

    public static readonly DependencyProperty AppHeightProperty = DependencyProperty.Register("AppHeight", typeof(double), typeof(SimulatorDevice));
    public double AppHeight
    {
      get { return (double)GetValue(AppHeightProperty); }
      set { SetValue(AppHeightProperty, value); }
    }

    public static readonly DependencyProperty FrameWidthProperty = DependencyProperty.Register("FrameWidth", typeof(double), typeof(SimulatorDevice));
    public double FrameWidth
    {
      get { return (double)GetValue(FrameWidthProperty); }
      set { SetValue(FrameWidthProperty, value); }
    }

    public static readonly DependencyProperty FrameHeightProperty = DependencyProperty.Register("FrameHeight", typeof(double), typeof(SimulatorDevice));
    public double FrameHeight
    {
      get { return (double)GetValue(FrameHeightProperty); }
      set { SetValue(FrameHeightProperty, value); }
    }

    public static readonly DependencyProperty InitialXProperty = DependencyProperty.Register("InitialX", typeof(double), typeof(SimulatorDevice));
    public double InitialX
    {
      get { return (double)GetValue(InitialXProperty); }
      set { SetValue(InitialXProperty, value); }
    }

    private System.Windows.Point location;
    public System.Windows.Point Location
    {
      get { return location; }
      set
      {
        location = value;
        OnPropertyChanged("Location");
      }
    }

    private float rotationAngle;
    public float RotationAngle
    {
      get { return rotationAngle; }
      set
      {
        rotationAngle = value;
        OnPropertyChanged("RotationAngle");
      }
    }

    private StackingControlState stackingControlState = StackingControlState.None;
    public StackingControlState StackingControlState
    {
      get { return stackingControlState; }
      set
      {
        stackingControlState = value;
        OnPropertyChanged("StackingControlState");
      }
    }

    private Simulator sWindow { get; set; }
    private Rect TPadAppBounds { get; set; }
    public Rect ScreenCorrectedAppBounds { get; set; }

    //This is the ID of the device on top, the device beloe starts the communication
    private int deviceOnTopID = 0;
    public int DeviceOnTopID
    {
      get { return deviceOnTopID; }
      set
      {
        deviceOnTopID = value;
        OnPropertyChanged("DeviceOnTopID");
      }
    }

    private FlippingMode flippingSide = FlippingMode.FaceUp;
    public FlippingMode FlippingSide
    {
      get { return flippingSide; }
      set
      {
        flippingSide = value;
        OnPropertyChanged("FlippingSide");
      }
    }

    public TPadCore Core { get; set; }

    public TPadProfile Profile { get; set; }

    public double WidthMultiplier
    {
      get { return AppWidth / Profile.Resolution.Width; }
    }

    public double HeightMultiplier
    {
      get { return AppHeight / Profile.Resolution.Height; }
    }

    private bool justShaked = false;
    public bool JustShaked
    {
      get
      {
        var tmp = justShaked;
        justShaked = false;
        return tmp;
      }
      set
      {
        justShaked = value;
        OnPropertyChanged("JustShaked");
      }
    }

    public SimulatorDevice(Simulator simulator, TPadCore core)
    {
      sWindow = simulator;
      Profile = core.Profile;
      Core = core;
      DeviceOnTopID = 0;
      CalculatorGlyph = false;
      TPadAppBounds = Rect.Empty;

      Core.Device.StackingChanged += new StackingChangedEventHandler(Device_StackingChanged);

      InitializeComponent();
    }

    private void sDevice_Loaded(object sender, RoutedEventArgs e)
    {
      RotationAngle = 0;

      Point locationCms = new Point(sWindow.ActualDocument.DocumentSize.Width / 2, sWindow.ActualDocument.DocumentSize.Height / 2);
      Location = new Point(locationCms.X * WidthFactor, locationCms.Y * HeightFactor);

      OnPropertyChanged("WidthMultiplier");
      OnPropertyChanged("HeightMultiplier");
    }

    public void LoadTPadApp(ITPadApp tPadApp)
    {
      if (tPadApp == null)
        return;

      tPadApp.Closed += tPadApp_Closed;

      (tPadApp as UserControl).VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
      (tPadApp as UserControl).HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
      gTPadApp.Children.Add(tPadApp as UserControl);
      Show(tPadApp);
    }

    public void Hide(ITPadApp tPadApp)
    {
      UserControl app = tPadApp as UserControl;
      app.Visibility = System.Windows.Visibility.Collapsed;
    }

    public void Show(ITPadApp tPadApp)
    {
      UserControl app = tPadApp as UserControl;
      int nextIndex = gTPadApp.Children.Cast<FrameworkElement>().Max(element => Canvas.GetZIndex(element)) + 1;
      Canvas.SetZIndex(app, nextIndex);
      app.Visibility = System.Windows.Visibility.Visible;
    }

    void tPadApp_Closed(object sender, EventArgs e)
    {
      ITPadApp tPadApp = (ITPadApp)sender;
      tPadApp.Closed -= tPadApp_Closed;
      gTPadApp.Children.Remove(tPadApp as UserControl);
    }

    private delegate MemoryStream GetDeviceViewDelegate();
    public System.Drawing.Bitmap GetDeviceView(out float angle)
    {
      //Thread.Sleep(100);
      angle = RotationAngle;
      GetDeviceViewDelegate gdvDelegate = new GetDeviceViewDelegate(SafeGetDeviceView);
      Object[] args = new Object[0];
      MemoryStream result = (MemoryStream)Dispatcher.Invoke(gdvDelegate, args);

      if (result == null)
        return null;
      try
      {
        System.Drawing.Bitmap frame = new System.Drawing.Bitmap(result);
        return frame;
      }
      catch { return null; }
    }

    private MemoryStream SafeGetDeviceView()
    {
      MemoryStream result = new MemoryStream();
      try
      {
        int zeroX = 0, zeroY = 0;
        sWindow.GetCoordinatesForScreenCapture(out zeroX, out zeroY);


        if (TPadAppBounds == Rect.Empty)
          TPadAppBounds = VisualTreeHelper.GetDescendantBounds(cWrapper);
        if (TPadAppBounds.IsEmpty || TPadAppBounds.Size.Width == 0 || TPadAppBounds.Size.Height == 0)
        {
          TPadAppBounds = Rect.Empty;
          return null;
        }
        else
        {
          ScreenCorrectedAppBounds = new Rect(0, 0, TPadAppBounds.Width * WidthMultiplier, TPadAppBounds.Height * HeightMultiplier);
        }

        Rect ttPadBounds = cWrapper.TransformToAncestor(sWindow).TransformBounds(TPadAppBounds);
        if(ttPadBounds.Width == 0 || ttPadBounds.Height == 0)
          return null;

        System.Drawing.Bitmap capture = ImageHelper.ScreenCapture(zeroX + ttPadBounds.Left, zeroY + ttPadBounds.Top, ttPadBounds);
        capture.Save(result, ImageFormat.Bmp);
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message);
      }

      return result;
    }

    private bool isTraslating = false, isRotating = false;
    private Point lastPosition;
    private void rFrame_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        isTraslating = true;
        lastPosition = Mouse.GetPosition(sWindow);
      }
      else if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Pressed)
      {
        isRotating = true;
        lastPosition = Mouse.GetPosition(sWindow);
      }
      else if ((e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Pressed)
        || (Keyboard.IsKeyDown(Key.S) && e.RightButton == MouseButtonState.Pressed))
      {
        StackingCommand();
      }
    }

    private void rFrame_MouseMove(object sender, MouseEventArgs e)
    {
      if (isTraslating)
      {
        //Gets the new position and checks whether there has been any movement since last time
        Point newPosition = Mouse.GetPosition(sWindow);
        if (newPosition == lastPosition)
          return;

        //Finds how much the mouse moved from last frame
        Vector displacement = newPosition - lastPosition;

        //Replaces the last position
        lastPosition = newPosition;

        //Adds such displacement to the current position of the app control
        Point newLocation = Location + displacement;

        // Updates the device location
        Location = newLocation;
      }
      else if (isRotating)
      {
        //Gets the new position and checks whether there has been any movement since last time
        Point newPosition = Mouse.GetPosition(sWindow);
        if (newPosition == lastPosition)
          return;

        //Finds how much the mouse moved from last frame
        Vector displacement = newPosition - lastPosition;

        //Replaces the last position
        lastPosition = newPosition;

        //Adds such displacement to the current position of the app control
        RotationAngle -= (float)displacement.X;
      }
    }

    private void rFrame_MouseUp(object sender, MouseButtonEventArgs e)
    {
      isTraslating = false;
      isRotating = false;
    }

    private void StackingCommand()
    {
      //it means that the device will be paired up with another, and it depends on the simulator to mediate whether this is first (on top) or second (bottom)
      if (StackingControlState == tPad.StackingControlState.None)
      {
        StackingControlState = tPad.StackingControlState.Stacking;
        OnStackingControl(this, new StackingControlEventArgs() { Device = Core.Device, PreviousState = tPad.StackingControlState.None, NewState = StackingControlState });
      }
      //it means that the device will no longer be stacked on top of another and the operation is cancelled
      else if (StackingControlState == tPad.StackingControlState.Stacking)
      {
        StackingControlState = tPad.StackingControlState.None;
        OnStackingControl(this, new StackingControlEventArgs() { Device = Core.Device, PreviousState = tPad.StackingControlState.Stacking, NewState = StackingControlState });
      }
      //it means that the current stacking finishes
      else if (StackingControlState == tPad.StackingControlState.StackedTop)
      {
        StackingControlState = tPad.StackingControlState.None;
        OnStackingControl(this, new StackingControlEventArgs() { Device = Core.Device, PreviousState = tPad.StackingControlState.Stacking, NewState = StackingControlState });
      }
    }

    void Device_StackingChanged(object sender, StackingEventArgs e)
    {
      if (e.State == StackingState.NotStacked)
      {
        StackingControlState = tPad.StackingControlState.None;
        OnStackingControl(this, new StackingControlEventArgs() { Device = Core.Device, NewState = StackingControlState });
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    #region ITPadAppController implementation

    public int ActualPage
    {
      get { return sWindow.ActualPage; }
    }

    public TPadDocument ActualDocument
    {
      get { return sWindow.ActualDocument; }
    }

    public float WidthFactor
    {
      get { return sWindow.WidthFactor; }
    }

    public float HeightFactor
    {
      get { return sWindow.HeightFactor; }
    }

    public float SimCaptureToSourceImageRatio
    {
      get { return sWindow.SimCaptureToSourceImageRatio; }
    }

    #endregion

    private void btHorizontalFlip_Click(object sender, RoutedEventArgs e)
    {
      if (FlippingSide == FlippingMode.FaceUp)
        FlippingSide = FlippingMode.FaceDown;
      else
        FlippingSide = FlippingMode.FaceUp;
    }

    private void btVerticalFlip_Click(object sender, RoutedEventArgs e)
    {
      RotationAngle += 180;
      btHorizontalFlip_Click(sender, e);
    }

    public bool CalculatorGlyph { get; set; }
    private void btLaunchCalculator_Click(object sender, RoutedEventArgs e)
    {
      CalculatorGlyph = btLaunchCalculator.IsChecked.Value;
      OnPropertyChanged("CalculatorGlyph");
    }

    private void btShakeDevice_Click(object sender, RoutedEventArgs e)
    {
      JustShaked = true;
    }

    private void btStack_Click(object sender, RoutedEventArgs e)
    {
      StackingCommand();
    }
  }

  public enum StackingControlState { None, Stacking, StackedTop, StackedBotton };

  public class StackingControlEventArgs : EventArgs
  {
    public TPadDevice Device { get; set; }
    public StackingControlState PreviousState { get; set; }
    public StackingControlState NewState { get; set; }
  }

}