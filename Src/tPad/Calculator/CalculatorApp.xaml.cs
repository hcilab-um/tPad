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

namespace UofM.HCI.tPad.App.Calculator
{
  /// <summary>
  /// Interaction logic for Calculator.xaml
  /// </summary>
  public partial class CalculatorApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public event BoolEventHandler IsTopApp;
    public event RequestUserFocus RequestFocus;
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

    public Guid AppUUID { get; private set; }
    public TPadProfile Profile { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }
    public Dictionary<String, Object> Context { get { return null; } }

    public CalculatorApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, Guid appUUID)
    {
      var tmp = new Xceed.Wpf.Toolkit.Calculator();

      AppUUID = appUUID;
      Core = core;
      Profile = core.Profile;
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

    public void DeActivate() { }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    public void Close()
    {
      if (Closed != null)
        Closed(this, EventArgs.Empty);
    }

    public void LoadInitContext(Dictionary<string, Object> init) 
    {
      if (init == null)
        return;
      if (!init.Keys.Contains("main"))
        return;

      double inputValue = 0;
      if(!Double.TryParse(init["main"] as String, out inputValue))
        return;

      cControl.Value = (decimal)inputValue;
    }

  }
}
