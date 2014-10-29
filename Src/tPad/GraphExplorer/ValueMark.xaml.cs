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

    public event EventHandler MarkClosed;
    private void OnMarkClosed()
    {
      if (MarkClosed != null)
        MarkClosed(this, null);
    }

    public event EventHandler MarkMoved;
    private void OnMarkMoved(double x, double y)
    {
      if (MarkMoved != null)
        MarkMoved(this, new MarkMovedEventArgs() { X = x, Y = y });
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

    private bool isDown = false, isMoving = false;
    private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
    {
      isDown = true;
      initial = Mouse.GetPosition(this);
    }

    private Point initial;
    private void Grid_MouseMove(object sender, MouseEventArgs e)
    {
      if (!isDown)
        return;
      isMoving = true;

      Point actual = Mouse.GetPosition(this);
      Vector displacement = actual - initial;
      OnMarkMoved(displacement.X, displacement.Y);
    }

    private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
    {
      isMoving = false;
      isDown = false;
    }

    private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
    {
      OnMarkClosed();
    }

    private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
    {
      isMoving = false;
      isDown = false;
    }
  }

  public class MarkMovedEventArgs : EventArgs
  {
    public double X { get; set; }
    public double Y { get; set; }
  }

}
