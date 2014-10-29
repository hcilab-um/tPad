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
    public event BoolEventHandler IsTopApp;
    public event RequestUserFocus RequestFocus;
    public event EventHandler Closed;
    public event PropertyChangedEventHandler PropertyChanged;
    public event RequestAction RequestAction;

    public Guid AppUUID { get; private set; }
    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }
    public Dictionary<String, Object> Context { get { return null; } }

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

    public TracerApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, Guid appUUID)
    {
      AppUUID = appUUID;
      Core = core;
      Container = container;
      Controller = controller;
      InitializeComponent();
    }

    private void tracerApp_Loaded(object sender, RoutedEventArgs e)
    {
      Core.Device.FlippingChanged += new FlippingChangedEventHandler(Device_FlippingChanged);
    }

    void Device_FlippingChanged(object sender, FlippingEventArgs e)
    {
      if (Core.Device.FlippingSide == Monitors.FlippingMode.FaceUp)
      {
        ShowMenu = false;
        btnMenu.Content = "Menu";
      }
      else if (Core.Device.FlippingSide == Monitors.FlippingMode.FaceDown)
      {
        ShowMenu = true;
        btnMenu.Content = "Back";
      }
    }

    private void btnMenu_Click(object sender, RoutedEventArgs e)
    {
      ShowMenu = !ShowMenu;
      if (ShowMenu)
        btnMenu.Content = "Back";
      else
        btnMenu.Content = "Menu";
    }

    void colorCanvas_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
    {
      inkCanvas.DefaultDrawingAttributes.Color = e.NewValue;
    }

    private void RadioButton_Checked(object sender, RoutedEventArgs e)
    {
      double size = Double.Parse((sender as RadioButton).Tag as String);
      inkCanvas.DefaultDrawingAttributes.Width = size;
      inkCanvas.DefaultDrawingAttributes.Height = size;
    }

    public void DeActivate() { }

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

    public void Activate(Dictionary<string, Object> init) { }

    private void bPen_Click(object sender, RoutedEventArgs e)
    {
      if (sender == bBluePen)
        inkCanvas.DefaultDrawingAttributes.Color = Colors.Blue;
      if (sender == bRedPen)
        inkCanvas.DefaultDrawingAttributes.Color = Colors.Red;
      if (sender == bYellowPen)
        inkCanvas.DefaultDrawingAttributes.Color = Colors.Yellow;
      if (sender == bEraser)
        inkCanvas.DefaultDrawingAttributes.Color = Colors.White;
    }
  }
}
