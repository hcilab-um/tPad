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

namespace UofM.HCI.tPad.App.Ruler
{
  /// <summary>
  /// Interaction logic for RulerApp.xaml
  /// </summary>
  public partial class RulerApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public event EventHandler Closed;
    public event PropertyChangedEventHandler PropertyChanged;

    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }

    private double distance = 0.0;
    public double Distance
    {
      get { return distance; }
      set
      {
        distance = value;
        OnPropertyChanged("Distance");
      }
    }

    public RulerApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller)
    {
      Core = core;
      Container = container;
      Controller = controller;

      InitializeComponent();
      Distance = GetDistanceBetweenPoints(new Point(measureLine.X1, measureLine.Y1), new Point(measureLine.X2, measureLine.Y2));
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

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

    private Point startPos, finalPos;
    private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (startPos.X == 0 && startPos.Y == 0)
      {
        startPos = Mouse.GetPosition(this);
        measureLine.X1 = startPos.X;
        measureLine.Y1 = startPos.Y;
        Distance = GetDistanceBetweenPoints(startPos, new Point(measureLine.X2, measureLine.Y2));
      }
      else
      {
        finalPos = Mouse.GetPosition(this);
        measureLine.X2 = finalPos.X;
        measureLine.Y2 = finalPos.Y;
        Distance = GetDistanceBetweenPoints(startPos, finalPos);
        startPos = finalPos = new Point(0, 0);
      }
    }

    private double GetDistanceBetweenPoints(Point start, Point final)
    {
      double a = start.X - final.X;
      double b = start.Y - final.Y;
      double distance = Math.Sqrt(a * a + b * b);
      return distance;
    }

  }

}
