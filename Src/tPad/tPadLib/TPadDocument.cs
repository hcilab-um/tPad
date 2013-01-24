using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using System.Windows.Controls;


namespace UofM.HCI.tPab
{
  public class TPadDocument
  {
    public String DocumentName { get; set; }
    public TPadPage[] Pages { get; set; }
  }

  public class Note : ITPadMarker
  {
    public TextBox annotation { get; set; }
    public Image icon { get; set; }

    public double X
    {
      get { return icon.Margin.Left; }
    }

    public double Y
    {
      get { return icon.Margin.Top; }
    }

    public System.Drawing.PointF Position
    {
      get { return new System.Drawing.PointF((float)X, (float)Y); }
    }
  }

  public class Highlight : ITPadMarker
  {
    public Line line { get; set; }

    public double X
    {
      get { return line.X1 + (line.X2 - line.X1) * 0.5; }
    }

    public double Y
    {
      get { return line.Y1 + (line.Y2 - line.Y1) * 0.5; }
    }

    public System.Drawing.PointF Position
    {
      get { return new System.Drawing.PointF((float)X, (float)Y); }
    }
  }

  public class TPadPage
  {
    public int PageIndex { get; set; }
    public String FileName { get; set; }
    public ObservableCollection<ITPadMarker> Highlights { get; set; }
    public ObservableCollection<ITPadMarker> Annotations { get; set; }
    public ObservableCollection<ITPadMarker> SearchResults { get; set; }

    public TPadPage(String fileName = null)
    {
      FileName = fileName;
      Highlights = new ObservableCollection<ITPadMarker>();
      Annotations = new ObservableCollection<ITPadMarker>();
      SearchResults = new ObservableCollection<ITPadMarker>();
    }
  }
}
