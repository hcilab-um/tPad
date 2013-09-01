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

    internal void Translate(int offsetX, int offsetY)
    {
      TopLeft.Translate(offsetX, offsetY);
      TopRight.Translate(offsetX, offsetY);
      BottomLeft.Translate(offsetX, offsetY);
      BottomRight.Translate(offsetX, offsetY);
    }

    internal BindableRect Clone()
    {
      BindableRect clone = new BindableRect();
      clone.TopLeft = TopLeft.Clone();
      clone.TopRight = TopRight.Clone();
      clone.BottomLeft = BottomLeft.Clone();
      clone.BottomRight = BottomRight.Clone();

      return clone;
    }
  }
}
