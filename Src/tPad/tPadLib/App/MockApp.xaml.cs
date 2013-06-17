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

namespace UofM.HCI.tPad.App
{
  /// <summary>
  /// Interaction logic for MockApp.xaml
  /// </summary>
  public partial class MockApp : UserControl, ITPadApp, INotifyPropertyChanged
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
    public Dictionary<String, String> Context { get { return null; } }

    public MockApp(TPadProfile profile, ITPadAppContainer container, ITPadAppController controller)
    {
      Profile = profile;
      Container = container;
      Controller = controller;

      InitializeComponent();
    }

    public void Close()
    {
      if (Closed != null)
        Closed(this, EventArgs.Empty);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    public void LoadInitContext(Dictionary<string, string> init)
    {
      throw new NotImplementedException();
    }
  }
}
