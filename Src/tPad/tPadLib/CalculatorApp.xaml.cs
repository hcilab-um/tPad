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
  /// Interaction logic for Calculator.xaml
  /// </summary>
  public partial class CalculatorApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    private TPadCore core;
    public TPadCore Core
    {
      get { return core; }
      set
      {
        core = value;
        OnPropertyChanged("Core");
      }
    }

    public TPadProfile Profile { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }

    public double WidthScalingFactor { get; set; }
    public double HeightScalingFactor { get; set; }

    public CalculatorApp(TPadProfile profile, ITPadAppContainer container, ITPadAppController controller)
    {
      Profile = profile;
      Container = container;
      Controller = controller;

      WidthScalingFactor = 1;
      HeightScalingFactor = 1;
      InitializeComponent();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private void calculatorApp_Loaded(object sender, RoutedEventArgs e)
    {
      WidthScalingFactor = ActualWidth / Profile.Resolution.Width;
      HeightScalingFactor = ActualHeight / Profile.Resolution.Height;
      OnPropertyChanged("WidthScalingFactor");
      OnPropertyChanged("HeightScalingFactor");
    }
  }
}
