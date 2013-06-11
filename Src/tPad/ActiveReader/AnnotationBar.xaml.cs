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

namespace UofM.HCI.tPad.App.ActiveReader
{
  /// <summary>
  /// Interaction logic for ARannotationBar.xaml
  /// </summary>
  public partial class AnnotationBar : UserControl, INotifyPropertyChanged
  {

    public static readonly DependencyProperty ActualDocumentProperty = DependencyProperty.Register("ActualDocument", typeof(TPadDocument), typeof(AnnotationBar), new PropertyMetadata(new PropertyChangedCallback(ActualDocumentChanged)));
    public TPadDocument ActualDocument
    {
      get { return (TPadDocument)GetValue(ActualDocumentProperty); }
      set { SetValue(ActualDocumentProperty, value); }
    }

    public static readonly DependencyProperty ActualPageProperty = DependencyProperty.Register("ActualPage", typeof(int), typeof(AnnotationBar));
    public int ActualPage
    {
      get { return (int)GetValue(ActualPageProperty); }
      set { SetValue(ActualPageProperty, value); }
    }

    public static readonly DependencyProperty PageHeightProperty = DependencyProperty.Register("PageHeight", typeof(double), typeof(AnnotationBar));
    public double PageHeight
    {
      get { return (double)GetValue(PageHeightProperty); }
      set { SetValue(PageHeightProperty, value); }
    }

    public static readonly DependencyProperty DeviceLocationProperty = DependencyProperty.Register("DeviceLocation", typeof(Point), typeof(AnnotationBar));
    public Point DeviceLocation
    {
      get { return (Point)GetValue(DeviceLocationProperty); }
      set { SetValue(DeviceLocationProperty, value); }
    }

    public static readonly DependencyProperty HeightFactorProperty = DependencyProperty.Register("HeightFactor", typeof(float), typeof(AnnotationBar));
    public float HeightFactor
    {
      get { return (float)GetValue(HeightFactorProperty); }
      set { SetValue(HeightFactorProperty, value); }
    }

    private double rectangleHeight = 0;
    public double RectangleHeight
    {
      get { return rectangleHeight; }
      set
      {
        rectangleHeight = value;
        OnPropertyChanged("RectangleHeight");
      }
    }

    public AnnotationBar()
    {
      InitializeComponent();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private static void ActualDocumentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      AnnotationBar obj = sender as AnnotationBar;
      if (obj != null)
        obj.ComputeRelativeHeight();
    }

    private void annoBar_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      ComputeRelativeHeight();
    }

    private void ComputeRelativeHeight()
    {
      if (ActualDocument != null)
        RectangleHeight = (ActualHeight / (double)ActualDocument.Pages.Length);
      else RectangleHeight = 0;
    }
  }
}
