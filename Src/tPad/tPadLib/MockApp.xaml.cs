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

namespace UofM.HCI.tPab
{
  /// <summary>
  /// Interaction logic for MockApp.xaml
  /// </summary>
  public partial class MockApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }

    public double WidthScalingFactor { get; set; }
    public double HeightScalingFactor { get; set; }

    public MockApp(TPadCore core, ITPadAppContainer container)
    {
      Core = core;
      Container = container;

      WidthScalingFactor = 1;
      HeightScalingFactor = 1;

      InitializeComponent();
    }

    private void mockApp_Loaded(object sender, RoutedEventArgs e)
    {
      WidthScalingFactor = ActualWidth / Core.Profile.Resolution.Width;
      HeightScalingFactor = ActualHeight / Core.Profile.Resolution.Height;
      OnPropertyChanged("WidthScalingFactor");
      OnPropertyChanged("HeightScalingFactor");
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }
  }
}
