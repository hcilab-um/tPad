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

namespace UofM.HCI.tPab
{
  /// <summary>
  /// Interaction logic for TPadWindow.xaml
  /// </summary>
  public partial class TPadWindow : Window, ITPadAppContainer, INotifyPropertyChanged
  {

    private ITPadAppLauncher Launcher { get; set; }
    public TPadProfile Profile { get; set; }

    private UserControl TPadApp { get; set; }
    public Rect TPadAppBounds { get; set; }
    private Size BorderDiff { get; set; }

    /// <summary>
    /// This is the number of pixels per cm
    /// </summary>
    public float WidthFactor
    {
      get { return (float)(ActualWidth / Profile.ScreenSize.Width); }
    }

    public float HeightFactor
    {
      get { return (float)(ActualHeight / Profile.ScreenSize.Height); }
    }

    public float SimCaptureToSourceImageRatio
    {
      get { throw new NotImplementedException(); }
    }

    public double SizeMultiplier { get; set; }

    public TPadWindow(ITPadAppLauncher launcher)
    {
      SizeMultiplier = 0.75; // This makes the window smaller when using a single monitor set-up -- for development
      Launcher = launcher;
      Profile = TPadCore.Instance.Profile;

      InitializeComponent();
    }

    public void LoadTPadApp(ITPadApp tPadApp)
    {
      if (tPadApp == null)
        return;

      TPadApp = tPadApp as UserControl;
      TPadApp.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
      TPadApp.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
      gTPadApp.Children.Add(TPadApp);
      TPadAppBounds = Rect.Empty;
      BorderDiff = Size.Empty;
    }

    protected override void OnClosed(EventArgs e)
    {
      base.OnClosed(e);
      if (Launcher != null)
        Launcher.CloseAll(this);
    }

    private void tpWindow_Loaded(object sender, RoutedEventArgs e)
    {
      if (System.Windows.Forms.SystemInformation.MonitorCount == 1)
        return;

      var tPadDisplay = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(tmp => tmp.Bounds.Width == Profile.Resolution.Width && tmp.Bounds.Height == Profile.Resolution.Height);
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
