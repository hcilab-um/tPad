﻿using System;
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
using UofM.HCI.tPab.Util;
using System.Drawing.Imaging;

namespace UofM.HCI.tPab
{
  /// <summary>
  /// Interaction logic for SimulatorDevice.xaml
  /// </summary>
  public partial class SimulatorDevice : UserControl, INotifyPropertyChanged, ITPadAppController, ITPadAppContainer
  {

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

    private System.Drawing.Point location;
    public System.Drawing.Point Location
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

    private Window parentWindow { get; set; }
    private ITPadApp TPadApp { get; set; }
    public Rect TPadAppBounds { get; set; }
    private Size BorderDiff { get; set; }

    public SimulatorDevice()
    {
      InitializeComponent();
    }

    private void sDevice_Loaded(object sender, RoutedEventArgs e)
    {
      parentWindow = Window.GetWindow(this);

      if (TPadAppBounds == Rect.Empty)
        TPadAppBounds = VisualTreeHelper.GetDescendantBounds(TPadApp as UserControl);
      Rect ttPadBounds = (TPadApp as UserControl).TransformToAncestor(this).TransformBounds(TPadAppBounds);
      if (BorderDiff == Size.Empty)
        BorderDiff = new Size(ttPadBounds.Left, ttPadBounds.Top);
      Location = new System.Drawing.Point((int)BorderDiff.Width, (int)BorderDiff.Height);
    }

    public void LoadTPadApp(ITPadApp tPadApp)
    {
      if (tPadApp == null)
        return;

      TPadApp = tPadApp;
      (TPadApp as UserControl).VerticalAlignment = System.Windows.VerticalAlignment.Center;
      (TPadApp as UserControl).HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
      BindingOperations.SetBinding((TPadApp as UserControl), UserControl.WidthProperty, new Binding("AppWidth") { Source = this });
      BindingOperations.SetBinding((TPadApp as UserControl), UserControl.HeightProperty, new Binding("AppHeight") { Source = this });

      gTPadApp.Children.Add(TPadApp as UserControl);
      TPadAppBounds = Rect.Empty;
      BorderDiff = Size.Empty;
    }

    private delegate MemoryStream GetDeviceViewDelegate();
    public System.Drawing.Bitmap GetDeviceView(out float angle)
    {
      Thread.Sleep(100);
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
      int zeroX = 0, zeroY = 0;
      (Parent as Simulator).GetCoordinatesForScreenCapture(out zeroX, out zeroY);

      if (TPadAppBounds == Rect.Empty)
        TPadAppBounds = VisualTreeHelper.GetDescendantBounds(TPadApp as UserControl);
      if (TPadAppBounds.Size.Width == 0 || TPadAppBounds.Size.Height == 0)
      {
        TPadAppBounds = Rect.Empty;
        return null;
      }

      Rect ttPadBounds = (TPadApp as UserControl).TransformToAncestor(this).TransformBounds(TPadAppBounds);
      System.Drawing.Bitmap capture = ImageHelper.ScreenCapture(zeroX + ttPadBounds.Left, zeroY + ttPadBounds.Top, ttPadBounds);
      MemoryStream result = new MemoryStream();
      try
      {
        capture.Save(result, ImageFormat.Bmp);
      }
      catch { }

      return result;
    }

    private bool isTraslating = false, isRotating = false;
    private Point lastPosition;
    private void rFrame_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        isTraslating = true;
        lastPosition = Mouse.GetPosition(parentWindow);
      }
      else if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Pressed)
      {
        isRotating = true;
        lastPosition = Mouse.GetPosition(parentWindow);
      }
    }

    private void rFrame_MouseMove(object sender, MouseEventArgs e)
    {
      if (isTraslating)
      {
        //Gets the new position and checks whether there has been any movement since last time
        Point newPosition = Mouse.GetPosition(parentWindow);
        if (newPosition == lastPosition)
          return;

        //Finds how much the mouse moved from last frame
        Vector displacement = newPosition - lastPosition;

        //Replaces the last position
        lastPosition = newPosition;

        //Adds such displacement to the current position of the app control
        Point currentLocation = new Point(Margin.Left, Margin.Top);
        Point newLocation = currentLocation + displacement;
        Margin = new Thickness(newLocation.X, newLocation.Y, 0, 0);

        // Updates the device location
        Point point = new Point(newLocation.X + BorderDiff.Width, newLocation.Y + BorderDiff.Height);
        Point rotatedPoint = tRotate.Transform(point);

        Location = new System.Drawing.Point((int)point.X, (int)point.Y);
      }
      else if (isRotating)
      {
        //Gets the new position and checks whether there has been any movement since last time
        Point newPosition = Mouse.GetPosition(parentWindow);
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

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    #region ITPadAppController implementation

    public int ActualPage
    {
      get { throw new NotImplementedException(); }
    }

    public TPadDocument ActualDocument
    {
      get { throw new NotImplementedException(); }
    }

    public float WidthFactor
    {
      get { throw new NotImplementedException(); }
    }

    public float HeightFactor
    {
      get { throw new NotImplementedException(); }
    }

    public float SimCaptureToSourceImageRatio
    {
      get { throw new NotImplementedException(); }
    }

    #endregion

  }
}
