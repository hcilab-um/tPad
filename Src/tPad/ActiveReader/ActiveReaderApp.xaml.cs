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
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace UofM.HCI.tPab.App.ActiveReader
{
  /// <summary>
  /// Interaction logic for ActiveReaderApp.xaml
  /// </summary>
  public partial class ActiveReaderApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public TPadProfile Profile { get; set; }
    public TPadDevice Device { get; set; }
    public ITPadAppContainer Container { get; set; }

    public String DocumentPath { get; set; }

    public double WidthScalingFactor { get; set; }
    public double HeightScalingFactor { get; set; }

    private int ActualPage { get; set; }
    private Document ActualDocument { get; set; }

    public ActiveReaderApp(String documentPDF, ITPadAppContainer container = null)
    {
      Device = TPadCore.Instance.Device;
      Profile = TPadCore.Instance.Profile;

      WidthScalingFactor = 1;
      HeightScalingFactor = 1;

      ActualPage = -1;
      ActualDocument = null;

      Container = container;
      DocumentPath = documentPDF;
      InitializeComponent();
    }

    private void arApp_Loaded(object sender, RoutedEventArgs e)
    {
      TPadCore.Instance.Device.StackingChanged += new StackingChangedEventHandler(Device_StackingChanged);
      TPadCore.Instance.Device.FlippingChanged += new FlippingChangedEventHandler(Device_FlippingChanged);
      TPadCore.Instance.Device.RegistrationChanged += new RegistrationChangedEventHandler(Device_RegistrationChanged);

      WidthScalingFactor = ActualWidth / Profile.Resolution.Width;
      HeightScalingFactor = ActualHeight / Profile.Resolution.Height;
      OnPropertyChanged("WidthScalingFactor");
      OnPropertyChanged("HeightScalingFactor");
    }

    void Device_StackingChanged(object sender, StackingEventArgs e)
    {
      throw new NotImplementedException();
    }

    void Device_FlippingChanged(object sender, FlippingEventArgs e)
    {
      throw new NotImplementedException();
    }

    void Device_RegistrationChanged(object sender, RegistrationEventArgs e)
    {
      if (e.NewLocation.Status != LocationStatus.Located)
        return;

      //if (ActualDocument == null)
      //  ActualDocument = e.NewLocation.Document;
      //if(ActualDocument.DocumentName.CompareTo(e.NewLocation.Document.DocumentName) != 0)

      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          trCanvas.Angle = e.NewLocation.RotationAngle * -1;
          trCanvas.CenterX = e.NewLocation.LocationPx.X + ActualWidth / 2;
          trCanvas.CenterY = e.NewLocation.LocationPx.Y + ActualHeight / 2;

          ttCanvas.X = e.NewLocation.LocationPx.X * -1;
          ttCanvas.Y = e.NewLocation.LocationPx.Y * -1;
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private bool isHighlighting = false;
    private Point lastPosition;
    private Line newHighlight;
    private void cHighlights_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        isHighlighting = true;
        lastPosition = Mouse.GetPosition(gAnchoredLayers);

        newHighlight = new Line() { Stroke = Brushes.YellowGreen, Opacity = 0.5, StrokeThickness = 10 };
        newHighlight.MouseMove += cHighlights_MouseMove;
        newHighlight.MouseUp += cHighlights_MouseUp;
        newHighlight.X1 = lastPosition.X;
        newHighlight.Y1 = lastPosition.Y;
        newHighlight.X2 = lastPosition.X;
        newHighlight.Y2 = lastPosition.Y;
        cHighlights.Children.Add(newHighlight);
      }
    }

    private void cHighlights_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!isHighlighting)
        return;

      Point newPosition = Mouse.GetPosition(gAnchoredLayers);
      newHighlight.X2 = newPosition.X;
      newHighlight.Y2 = newPosition.Y;
      newHighlight.MouseMove -= cHighlights_MouseMove;
      newHighlight.MouseUp -= cHighlights_MouseUp;
      isHighlighting = false;
    }

    private void cHighlights_MouseMove(object sender, MouseEventArgs e)
    {
      if (!isHighlighting)
        return;

      Point newPosition = Mouse.GetPosition(gAnchoredLayers);
      newHighlight.X2 = newPosition.X;
      newHighlight.Y2 = newPosition.Y;
    }

    private void gFixedLayers_MouseDown(object sender, MouseButtonEventArgs e)
    {

    }
  }
}
