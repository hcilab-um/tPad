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

namespace UofM.HCI.tPab.App.ActiveReader
{
  /// <summary>
  /// Interaction logic for FigureViewer.xaml
  /// </summary>
  public partial class FigureViewer : UserControl, INotifyPropertyChanged
  {
    public static readonly DependencyProperty FigureSourceProperty = DependencyProperty.Register("FigureSource", typeof(BitmapSource), typeof(FigureViewer));
    public BitmapSource FigureSource
    {
      get { return (BitmapSource)GetValue(FigureSourceProperty); }
      set { SetValue(FigureSourceProperty, value); }
    }

    public FigureViewer()
    {
      InitializeComponent();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private void cFigureViewer_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        figureViewer.Visibility = Visibility.Collapsed;
      }
    }
  }
}
