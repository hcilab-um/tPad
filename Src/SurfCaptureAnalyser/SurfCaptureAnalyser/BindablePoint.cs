using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SurfCaptureAnalyser
{
  public class BindablePoint : INotifyPropertyChanged
  {
    private double x = 0;
    private double y = 0;

    [XmlAttribute]
    public double X
    {
      get { return x; }
      set
      {
        x = value;
        OnPropertyChanged("X");
      }
    }

    [XmlAttribute]
    public double Y
    {
      get { return y; }
      set
      {
        y = value;
        OnPropertyChanged("Y");
      }
    }

    public BindablePoint()
    { }

    public BindablePoint(double iX, double iY)
    {
      X = iX;
      Y = iY;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    internal System.Drawing.PointF ToPointF()
    {
      return new System.Drawing.PointF((float)X, (float)Y);
    }

    internal void Translate(int offsetX, int offsetY)
    {
      X += offsetX;
      Y += offsetY;
    }

    internal BindablePoint Clone()
    {
      return new BindablePoint(X, Y);
    }
  }
}
