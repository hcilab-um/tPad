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
using System.Windows.Shapes;
using System.ComponentModel;

namespace UofM.HCI.tPad
{
  /// <summary>
  /// Interaction logic for TPadWindow.xaml
  /// </summary>
  public partial class TPadWindow : Window, ITPadAppContainer, INotifyPropertyChanged
  {

    public TPadCore Core { get; set; }
    public TPadProfile Profile { get; set; }
    private ITPadAppLauncher Launcher { get; set; }

    private double sizeMultiplier = 1;
    public double SizeMultiplier
    {
      get { return sizeMultiplier; }
      set
      {
        sizeMultiplier = value;
        OnPropertyChanged("SizeMultiplier");
      }
    }

    public int InstanceNumber { get; set; }

    public TPadWindow(TPadProfile profile, ITPadAppLauncher launcher, TPadCore core)
    {
      SizeMultiplier = 0.75; // This makes the window smaller when using a single monitor set-up -- for development
      Launcher = launcher;
      Profile = profile;
      Core = core;

      InitializeComponent();
    }

    public void LoadTPadApp(ITPadApp tPadApp, bool foreground = true)
    {
      if (tPadApp == null)
        return;

      tPadApp.Closed += tPadApp_Closed;

      UserControl TPadApp = tPadApp as UserControl;
      TPadApp.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
      TPadApp.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
      gTPadApp.Children.Add(TPadApp);

      if (foreground)
        Show(tPadApp);
      else
        Hide(tPadApp);
    }

    public void Hide(ITPadApp tPadApp)
    {
      UserControl app = tPadApp as UserControl;
      app.Visibility = System.Windows.Visibility.Collapsed;
    }

    public void Show(ITPadApp tPadApp)
    {
      UserControl app = tPadApp as UserControl;
      int nextIndex = gTPadApp.Children.Cast<FrameworkElement>().Max(element => Canvas.GetZIndex(element)) + 1;
      Canvas.SetZIndex(app, nextIndex);
      app.Visibility = System.Windows.Visibility.Visible;
    }

    void tPadApp_Closed(object sender, EventArgs e)
    {
      ITPadApp tPadApp = (ITPadApp)sender;
      tPadApp.Closed -= tPadApp_Closed;
      gTPadApp.Children.Remove(tPadApp as UserControl);
    }

    private void tpWindow_Loaded(object sender, RoutedEventArgs e)
    {
      if (System.Windows.Forms.SystemInformation.MonitorCount == 1)
        return;

      if (InstanceNumber > 0)
        return;

      var tPadDisplay = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(
        tmp => tmp.Bounds.Width == Profile.Resolution.Width && tmp.Bounds.Height == Profile.Resolution.Height);
      if (tPadDisplay == null)
        return;

      SizeMultiplier = 1;
      OnPropertyChanged("SizeMultiplier");

      Left = tPadDisplay.WorkingArea.Left;
      Top = tPadDisplay.WorkingArea.Top;
      WindowStyle = System.Windows.WindowStyle.None;
      WindowState = System.Windows.WindowState.Maximized;
    }

    private void tpWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      OnPropertyChanged("WidthFactor");
      OnPropertyChanged("HeightFactor");
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }
  }

}
