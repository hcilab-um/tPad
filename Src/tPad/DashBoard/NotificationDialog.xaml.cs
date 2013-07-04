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

    public event EventHandler ClickedOK;
    public event EventHandler ClickedCancel;

    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }
    public Dictionary<String, Object> Context { get { return null; } }

    public TPadApplicationDescriptor NotificationApp { get; set; }
    public TPadApplicationDescriptor ActualApp { get; set; }

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

    private String buttonOK = String.Empty;
    public String ButtonOK
    {
      get { return buttonOK; }
      set
      {
        buttonOK = value;
        OnPropertyChanged("ButtonOK");
      }
    }

    private String buttonCancel = String.Empty;
    public String ButtonCancel
    {
      get { return buttonCancel; }
      set
      {
        buttonCancel = value;
        OnPropertyChanged("ButtonCancel");
      }
    }

    public NotificationDialog(TPadCore core)
    {
      Core = core;
      Core.Device.FlippingChanged += new FlippingChangedEventHandler(Device_FlippingChanged);
      Core.Device.HomePressed += new EventHandler(Device_HomePressed);

      InitializeComponent();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    public void LoadInitContext(Dictionary<String, Object> init)
    {
      if (init == null || init.Count == 0)
        return;

      if (init.Keys.Contains("message"))
        Message = init["message"] as String;
      if (init.Keys.Contains("buttonOK"))
        ButtonOK = init["buttonOK"] as String;
      if (init.Keys.Contains("buttonCancel"))
        ButtonCancel = init["buttonCancel"] as String;
      if (init.Keys.Contains("sender"))
        NotificationApp = init["sender"] as TPadApplicationDescriptor;
      if (init.Keys.Contains("currentApp"))
        ActualApp = init["currentApp"] as TPadApplicationDescriptor;
    }

    public void Close()
    {
      if (Closed != null)
        Closed(this, null);
    }

    void Device_FlippingChanged(object sender, FlippingEventArgs e)
    {
      //The device has flipped already, therefore it has to ask whether it's the top app on the other side
      if (!IsTopApp(this, new ObjectEventArgs() { Parameter = Core.Device.OppositeFlippingSide }))
        return;

      e.Handled = true;
      btnOK_Click(sender, null);
    }

    void Device_HomePressed(object sender, EventArgs e)
    {
      if (!IsTopApp(this, null))
        return;

      Close();
    }

    private void btnOK_Click(object sender, RoutedEventArgs e)
    {
      Close();

      if (ClickedOK != null)
        ClickedOK(this, null);
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      Close();

      if (ClickedCancel != null)
        ClickedCancel(this, null);
    }
  }
}
