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

namespace UofM.HCI.tPab.Applications
{
  /// <summary>
  /// Interaction logic for Calculator.xaml
  /// </summary>
  public partial class CalculatorApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public event EventHandler Closed;

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

    public CalculatorApp(TPadProfile profile, ITPadAppContainer container, ITPadAppController controller)
    {
      var tmp = new Xceed.Wpf.Toolkit.Calculator();

      Profile = profile;
      Container = container;
      Controller = controller;

      InitializeComponent();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
      core.GlyphDetection.Continue();
      Close();
    }

    public void Close()
    {
      if (Closed != null)
        Closed(this, EventArgs.Empty);
    }
  }
}
