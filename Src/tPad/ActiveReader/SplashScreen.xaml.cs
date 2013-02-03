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

namespace UofM.HCI.tPab.App.ActiveReader
{
  /// <summary>
  /// Interaction logic for SplashScreen.xaml
  /// </summary>
  public partial class SplashScreen : Window, INotifyPropertyChanged
  {

    private double loadingProgress = 0;
    public double LoadingProgress
    {
      get { return loadingProgress; }
      set 
      {
        loadingProgress = value;
        OnPropertyChanged("LoadingProgress");
      }
    }

    public SplashScreen()
    {
      InitializeComponent();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

  }
}
