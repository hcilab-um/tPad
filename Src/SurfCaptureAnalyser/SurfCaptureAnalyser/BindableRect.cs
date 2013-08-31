using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SurfCaptureAnalyser
{
  public class BindableRect
  {
    public BindablePoint TopLeft { get; set; }
    public BindablePoint TopRight { get; set; }
    public BindablePoint BottomLeft { get; set; }
    public BindablePoint BottomRight { get; set; }

    public BindableRect()
    { }

    public BindableRect(double x, double y, double width, double height)
    {
      TopLeft = new BindablePoint(x, y);
      TopRight = new BindablePoint(x + width, y);
      BottomLeft = new BindablePoint(x, y + height);
      BottomRight = new BindablePoint(x + width, y + height);
    }
  }
}
