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
using UofM.HCI.tPab.Util;
using UofM.HCI.tPab.Services;

namespace UofM.HCI.tPab
{
  /// <summary>
  /// Interaction logic for Simulatorç.xaml
  /// </summary>
  public partial class Simulator : Window, INotifyPropertyChanged, ITPadAppContainer
  {

    private float widthFactor, heightFactor;
    private float rotationAngle;
    private System.Drawing.Point location;

    private UserControl TPadApp { get; set; }
    public Rect TPadAppBounds { get; set; }
    private Size BorderDiff { get; set; }

    private Document actualDocument = null;
    public Document ActualDocument
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
        if (ActualDocument == null || ActualDocument.PageFileNames == null)
          return 0;
        return ActualDocument.PageFileNames.Length;
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

    public float RotationAngle
    {
      get { return rotationAngle; }
      set
      {
        rotationAngle = value;
        OnPropertyChanged("RotationAngle");
      }
    }

    public System.Drawing.Point Location 
    {
      get { return location; }
      set
      {
        location = value;
        OnPropertyChanged("Location");
      }
    }

    public Simulator(Application launcher)
    {
      Document document = TPadCore.Instance.Registration.ActualDocument;
      if (!File.Exists(document.PageFileNames[0]))
        throw new ArgumentException(String.Format("Document \"{1}\" not found!", document.PageFileNames[0]));

      InitializeComponent();
      iDocument.SizeChanged += new SizeChangedEventHandler(iDocument_SizeChanged);

      ActualDocument = document;
    }

    public void LoadTPadApp(ITPadApp tPadApp)
    {
      if (tPadApp == null)
        return;

      TPadApp = tPadApp as UserControl;
      TPadApp.VerticalAlignment = System.Windows.VerticalAlignment.Center;
      TPadApp.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
      gTPadApp.Children.Add(TPadApp);
      TPadAppBounds = Rect.Empty;
      BorderDiff = Size.Empty;
    }

    private void wSimulator_Loaded(object sender, RoutedEventArgs e)
    {
      if (TPadAppBounds == Rect.Empty)
        TPadAppBounds = VisualTreeHelper.GetDescendantBounds(TPadApp);
      Rect ttPadBounds = TPadApp.TransformToAncestor(gTPadApp).TransformBounds(TPadAppBounds);
      if (BorderDiff == Size.Empty)
        BorderDiff = new Size(ttPadBounds.Left, ttPadBounds.Top);
      Location = new System.Drawing.Point((int)BorderDiff.Width, (int)BorderDiff.Height);

      Rect docBounds = iDocument.TransformToAncestor(gTop).TransformBounds(VisualTreeHelper.GetDescendantBounds(iDocument));
      tAlign.X = docBounds.Left;
    }

    void iDocument_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      CalculateFactors();
    }

    private void CalculateFactors()
    {
      // This is the number of pixels per centimeter on the height.
      HeightFactor = (float)(iDocument.ActualHeight / TPadCore.Instance.Profile.DocumentSize.Height);
      // This is the number of pixels per centimeter on the width
      WidthFactor = (float)(iDocument.ActualWidth / TPadCore.Instance.Profile.DocumentSize.Width);

      // These two values should be nearly the same
      if (Math.Abs(HeightFactor - WidthFactor) >= 0.5)
        throw new ArgumentException("The document image does not match the specified document profile");

      //Resize the device
      gTPadApp.Width = WidthFactor * TPadCore.Instance.Profile.DeviceSize.Width;
      gTPadApp.Height = HeightFactor * TPadCore.Instance.Profile.DeviceSize.Height;
      //Adjusts the screen size to the device size
      TPadApp.Width = WidthFactor * TPadCore.Instance.Profile.ScreenSize.Width;
      TPadApp.Height = HeightFactor * TPadCore.Instance.Profile.ScreenSize.Height;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private bool isTraslating = false, isRotating = false;
    private Point lastPosition;
    private void rFrame_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        isTraslating = true;
        lastPosition = Mouse.GetPosition(this);
      }
      else if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Pressed)
      {
        isRotating = true;
        lastPosition = Mouse.GetPosition(this);
      }
    }

    private void rFrame_MouseMove(object sender, MouseEventArgs e)
    {
      if (isTraslating)
      {
        //Gets the new position and checks whether there has been any movement since last time
        Point newPosition = Mouse.GetPosition(this);
        if (newPosition == lastPosition)
          return;

        //Finds how much the mouse moved from last frame
        Vector displacement = newPosition - lastPosition;

        //Replaces the last position
        lastPosition = newPosition;

        //Adds such displacement to the current position of the app control
        Point currentLocation = new Point(gTPadApp.Margin.Left, gTPadApp.Margin.Top);
        Point newLocation = currentLocation + displacement;
        gTPadApp.Margin = new Thickness(newLocation.X, newLocation.Y, 0, 0);

        // Updates the device location
        Point point = new Point(newLocation.X + BorderDiff.Width, newLocation.Y + BorderDiff.Height);
        Point rotatedPoint = tRotate.Transform(point);
        Location = new System.Drawing.Point((int)point.X, (int)point.Y);
      }
      else if (isRotating)
      {
        //Gets the new position and checks whether there has been any movement since last time
        Point newPosition = Mouse.GetPosition(this);
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

    public System.Drawing.Bitmap GetDeviceView(out float angle)
    {
      angle = RotationAngle;
      GetDeviceViewDelegate gdvDelegate = new GetDeviceViewDelegate(SafeGetDeviceView);
      Object[] args = new Object[0];
      MemoryStream result = (MemoryStream)Dispatcher.Invoke(gdvDelegate, args);
      if (result == null)
        return null;

      System.Drawing.Bitmap frame = new System.Drawing.Bitmap(result);
      return frame;
    }


    private MemoryStream SafeGetDeviceView()
    {
      if (!IsActive)
        return null;

      int bordersize = (int)(RestoreBounds.Width - gTop.ActualWidth) / 2;
      int bordertop = (int)(RestoreBounds.Height - gTop.ActualHeight - bordersize);
      int zeroX = (int)(Left + bordersize);
      int zeroY = (int)(Top + bordertop);

      if (TPadAppBounds == Rect.Empty)
        TPadAppBounds = VisualTreeHelper.GetDescendantBounds(TPadApp);
      if (TPadAppBounds.Size.Width == 0 || TPadAppBounds.Size.Height == 0)
      {
        TPadAppBounds = Rect.Empty;
        return null;
      }

      Rect ttPadBounds = TPadApp.TransformToAncestor(this).TransformBounds(TPadAppBounds);

      System.Drawing.Bitmap capture = ImageHelper.ScreenCapture(zeroX + ttPadBounds.Left, zeroY + ttPadBounds.Top, ttPadBounds.Width, ttPadBounds.Height);
      MemoryStream result = new MemoryStream();
      try
      {
        capture.Save(result, System.Drawing.Imaging.ImageFormat.Png);
      }
      catch (Exception ex)
      { }
      return result;
    }

    private delegate MemoryStream GetDeviceViewDelegate();

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

  }

}
