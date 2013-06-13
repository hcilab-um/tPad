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

namespace UofM.HCI.tPad.App.Tracer
{
  /// <summary>
  /// Interaction logic for TracerApp.xaml
  /// </summary>
  public partial class TracerApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public event EventHandler Closed;
    public event PropertyChangedEventHandler PropertyChanged;

    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }

    private bool showMenu = false;
    public bool ShowMenu 
    {
      get { return showMenu; }
      set 
      {
        showMenu = value;
        OnPropertyChanged("ShowMenu");
      }
    }

    public TracerApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller)
    {
      Core = core;
      Container = container;
      Controller = controller;
      InitializeComponent();
    }

    public void Close()
    {
      if (Closed != null)
        Closed(this, EventArgs.Empty);
    }

    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private void btnMenu_Click(object sender, RoutedEventArgs e)
    {
      ShowMenu = !ShowMenu;
      if (ShowMenu)
        btnMenu.Content = "Back";
      else
        btnMenu.Content = "Menu";
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }
  }
}
