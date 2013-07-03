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

namespace UofM.HCI.tPad.App.Dashboard
{
  /// <summary>
  /// Interaction logic for NotificationDialog.xaml
  /// </summary>
  public partial class NotificationDialog : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public event EventHandler Closed;
    public event BoolEventHandler IsTopApp;
    public event RequestUserFocus RequestFocus;

    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }
    public Dictionary<String, String> Context { get { return null; } }

    private String message = String.Empty;
    public String Message 
    {
      get { return message; }
      set 
      {
        message = value;
        OnPropertyChanged("Message");
      }
    }

    public NotificationDialog()
    {
      InitializeComponent();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    public void LoadInitContext(Dictionary<string, string> init) {}

    public void Close()
    {
      if (Closed != null)
        Closed(this, null);
    }
  }
}
