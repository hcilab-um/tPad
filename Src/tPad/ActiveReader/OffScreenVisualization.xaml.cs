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
using System.Drawing;

namespace UofM.HCI.tPab.App.ActiveReader
{
  /// <summary>
  /// Interaction logic for OffScreenVisualization.xaml
  /// </summary>
  public partial class OffScreenVisualization : UserControl, INotifyPropertyChanged
  {

    public static readonly DependencyProperty ActualDocumentProperty = DependencyProperty.Register("ActualDocument", typeof(TPadDocument), typeof(OffScreenVisualization));
    public TPadDocument ActualDocument
    {
      get { return (TPadDocument)GetValue(ActualDocumentProperty); }
      set { SetValue(ActualDocumentProperty, value); }
    }

    public static readonly DependencyProperty ActualPageProperty = DependencyProperty.Register("ActualPage", typeof(int), typeof(OffScreenVisualization));
    public int ActualPage
    {
      get { return (int)GetValue(ActualPageProperty); }
      set { SetValue(ActualPageProperty, value); }
    }

    public static readonly DependencyProperty PageHeightProperty = DependencyProperty.Register("PageHeight", typeof(float), typeof(OffScreenVisualization));
    public float PageHeight
    {
      get { return (float)GetValue(PageHeightProperty); }
      set { SetValue(PageHeightProperty, value); }
    }

    public static readonly DependencyProperty PageWidthProperty = DependencyProperty.Register("PageWidth", typeof(float), typeof(OffScreenVisualization));
    public float PageWidth
    {
      get { return (float)GetValue(PageWidthProperty); }
      set { SetValue(PageWidthProperty, value); }
    }

    public static readonly DependencyProperty DeviceLocationProperty = DependencyProperty.Register("DeviceLocation", typeof(PointF), typeof(OffScreenVisualization));
    public PointF DeviceLocation
    {
      get { return (PointF)GetValue(DeviceLocationProperty); }
      set { SetValue(DeviceLocationProperty, value); }
    }

    public static readonly DependencyProperty HeightFactorProperty = DependencyProperty.Register("HeightFactor", typeof(float), typeof(OffScreenVisualization));
    public float HeightFactor
    {
      get { return (float)GetValue(HeightFactorProperty); }
      set { SetValue(HeightFactorProperty, value); }
    }

    public static readonly DependencyProperty WidthFactorProperty = DependencyProperty.Register("WidthFactor", typeof(float), typeof(OffScreenVisualization));
    public float WidthFactor
    {
      get { return (float)GetValue(WidthFactorProperty); }
      set { SetValue(WidthFactorProperty, value); }
    }

    public TPadPage ActualPageObject
    {
      get
      {
        if (ActualDocument == null || ActualPage == -1 || ActualDocument.Pages == null || ActualDocument.Pages.Length <= ActualPage)
          return null;
        return ActualDocument.Pages[ActualPage];
      }
    }

    public OffScreenVisualization()
    {
      InitializeComponent();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      base.OnPropertyChanged(e);
      if (e.Property == OffScreenVisualization.ActualPageProperty || e.Property == OffScreenVisualization.ActualDocumentProperty)
      {
        OnPropertyChanged("ActualPageObject");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }
  }
}
