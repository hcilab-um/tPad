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

namespace UofM.HCI.tPab
{
  /// <summary>
  /// Interaction logic for Simulatorç.xaml
  /// </summary>
  public partial class Simulator : Window, INotifyPropertyChanged
  {

    private float widthFactor, heightFactor;
    private float rotationAngle;
    private TPadProfile Profile { get; set; }
    private BitmapFrame DocumentBM { get; set; }
    private UserControl TPadApp { get; set; }
   
    private float WidthFactor
    {
      get { return widthFactor; }
      set
      {
        widthFactor = value;
        OnPropertyChanged("WidthFactor");
      }
    }

    private float HeightFactor
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

    public Simulator(Application launcher, TPadProfile profile, String document, UserControl app = null)
    {
      if (!File.Exists(document))
        throw new ArgumentException(String.Format("Document \"{1}\" not found!", document));

      Profile = profile;
      InitializeComponent();
      iDocument.SizeChanged += new SizeChangedEventHandler(iDocument_SizeChanged);
      DocumentBM = BitmapFrame.Create(new Uri(document, UriKind.Relative));

      if (app != null && app is ITPadApp)
      {
        TPadApp = app;
        TPadApp.VerticalAlignment = System.Windows.VerticalAlignment.Center;
        TPadApp.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
        gTPadApp.Children.Add(TPadApp);
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      iDocument.Source = DocumentBM;
    }

    void iDocument_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      CalculateFactors();
    }

    private void CalculateFactors()
    {
      // This is the number of pixels per centimeter on the height.
      HeightFactor = (float)(iDocument.ActualHeight / Profile.DocumentSize.Height);
      // This is the number of pixels per centimeter on the width
      WidthFactor = (float)(iDocument.ActualWidth / Profile.DocumentSize.Width);

      // These two values should be nearly the same
      if (Math.Abs(HeightFactor - WidthFactor) >= 0.5)
        throw new ArgumentException("The document image does not match the specified document profile");

      //Resize the device
      gTPadApp.Width = WidthFactor * Profile.DeviceSize.Width;
      gTPadApp.Height = HeightFactor * Profile.DeviceSize.Height;
      //Adjusts the screen size to the device size
      TPadApp.Width = WidthFactor * Profile.ScreenSize.Width;
      TPadApp.Height = HeightFactor * Profile.ScreenSize.Height;
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

  }

}
