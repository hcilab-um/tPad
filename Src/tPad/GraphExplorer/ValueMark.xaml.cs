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

namespace UofM.HCI.tPad.App.GraphExplorer
{
  /// <summary>
  /// Interaction logic for ValueMark.xaml
  /// </summary>
  public partial class ValueMark : UserControl, INotifyPropertyChanged
  {

    public event EventHandler MarkClicked;
    private void OnMarkClicked()
    {
      if (MarkClicked != null)
        MarkClicked(this, null);
    }

    private double mark = 0;
    public double Mark 
    {
      get { return mark; }
      set 
      {
        mark = value;
        OnPropertyChanged("Mark");
      }
    }

    public ValueMark()
    {
      InitializeComponent();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
    {
      OnMarkClicked();
    }
  }
}
