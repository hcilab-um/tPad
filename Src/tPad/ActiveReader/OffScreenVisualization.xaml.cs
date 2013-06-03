using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace UofM.HCI.tPab.App.ActiveReader
{
  /// <summary>
  /// Interaction logic for OffScreenVisualization.xaml
  /// </summary>
  public partial class OffScreenVisualization : UserControl, INotifyPropertyChanged
  {

    public static readonly DependencyProperty ActualMarkerProperty = DependencyProperty.Register("ActualMarker", typeof(ObservableCollection<IActiveReaderMarker>), typeof(OffScreenVisualization));
    public ObservableCollection<IActiveReaderMarker> ActualMarker
    {
      get { return (ObservableCollection<IActiveReaderMarker>)GetValue(ActualMarkerProperty); }
      set { SetValue(ActualMarkerProperty, value); }
    }

    public static readonly DependencyProperty ActualIconProperty = DependencyProperty.Register("ActualIcon", typeof(ImageSource), typeof(OffScreenVisualization));
    public ImageSource ActualIcon
    {
      get { return (ImageSource)GetValue(ActualIconProperty); }
      set { SetValue(ActualIconProperty, value); }
    }

    public static readonly DependencyProperty PageHeightProperty = DependencyProperty.Register("PageHeight", typeof(float), typeof(OffScreenVisualization));
    public float PageHeight
    {
      get { return (float)GetValue(PageHeightProperty); }
      set { SetValue(PageHeightProperty, value); }
    }

    public static readonly DependencyProperty PageWidthProperty = DependencyProperty.Register("PageWidth", typeof(float), typeof(OffScreenVisualization));
    public float PageWidth
    {
      get { return (float)GetValue(PageWidthProperty); }
      set { SetValue(PageWidthProperty, value); }
    }

    public static readonly DependencyProperty DeviceWidthProperty = DependencyProperty.Register("DeviceWidth", typeof(float), typeof(OffScreenVisualization));
    public float DeviceWidth
    {
      get { return (float)GetValue(DeviceWidthProperty); }
      set { SetValue(DeviceWidthProperty, value); }
    }

    public static readonly DependencyProperty DeviceHeightProperty = DependencyProperty.Register("DeviceHeight", typeof(float), typeof(OffScreenVisualization));
    public float DeviceHeight
    {
      get { return (float)GetValue(DeviceHeightProperty); }
      set { SetValue(DeviceHeightProperty, value); }
    }

    public static readonly DependencyProperty DeviceLocationProperty = DependencyProperty.Register("DeviceLocation", typeof(Point), typeof(OffScreenVisualization));
    public Point DeviceLocation
    {
      get { return (Point)GetValue(DeviceLocationProperty); }
      set { SetValue(DeviceLocationProperty, value); }
    }

    public static readonly DependencyProperty DeviceRotationProperty = DependencyProperty.Register("DeviceRotation", typeof(float), typeof(OffScreenVisualization));
    public float DeviceRotation
    {
      get { return (float)GetValue(DeviceRotationProperty); }
      set { SetValue(DeviceRotationProperty, value); }
    }

    public static readonly DependencyProperty UIRotationProperty = DependencyProperty.Register("UIRotation", typeof(double), typeof(OffScreenVisualization));
    public double UIRotation
    {
      get { return (double)GetValue(UIRotationProperty); }
      set { SetValue(UIRotationProperty, value); }
    }

    public static readonly DependencyProperty HeightFactorProperty = DependencyProperty.Register("HeightFactor", typeof(float), typeof(OffScreenVisualization));
    public float HeightFactor
    {
      get { return (float)GetValue(HeightFactorProperty); }
      set { SetValue(HeightFactorProperty, value); }
    }

    public static readonly DependencyProperty WidthFactorProperty = DependencyProperty.Register("WidthFactor", typeof(float), typeof(OffScreenVisualization));
    public float WidthFactor
    {
      get { return (float)GetValue(WidthFactorProperty); }
      set { SetValue(WidthFactorProperty, value); }
    }

    public OffScreenVisualization()
    {
      InitializeComponent();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }
  }
}
