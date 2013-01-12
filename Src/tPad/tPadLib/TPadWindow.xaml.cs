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

namespace UofM.HCI.tPab
{
  /// <summary>
  /// Interaction logic for TPadWindow.xaml
  /// </summary>
  public partial class TPadWindow : Window, ITPadAppContainer
  {

    private UserControl TPadApp { get; set; }
    public Rect TPadAppBounds { get; set; }
    private Size BorderDiff { get; set; }

    public float WidthFactor
    {
      get { return 1; }
    }

    public float HeightFactor
    {
      get { return 1; }
    }

    public float RotationAngle
    {
      get { throw new NotImplementedException(); }
    }

    public System.Drawing.Point Location
    {
      get { throw new NotImplementedException(); }
    }

    public int ActualPage
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public float SimCaptureToSourceImageRatio
    {
      get { throw new NotImplementedException(); }
    }

    public TPadWindow()
    {
      InitializeComponent();
    }

    public void LoadTPadApp(ITPadApp tPadApp)
    {
      if (tPadApp == null)
        return;

      TPadApp = tPadApp as UserControl;
      TPadApp.VerticalAlignment = System.Windows.VerticalAlignment.Center;
      TPadApp.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
      gTPadApp.Children.Add(TPadApp);
      TPadAppBounds = Rect.Empty;
      BorderDiff = Size.Empty;
    }

  }

}
