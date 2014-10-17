using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace UofM.HCI.tPad.Converters
{
  public class PointsToAngleConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      double x1 = (double)values[0];
      double y1 = (double)values[1];
      double x2 = (double)values[2];
      double y2 = (double)values[3];

      return Angle(new Point(x1, y1), new Point(x2, y2)) * -1;
    }

    const double Rad2Deg = 180.0 / Math.PI;
    const double Deg2Rad = Math.PI / 180.0;

    /// <summary>
    /// Calculates angle in radians between two points and x-axis.
    /// </summary>
    private double Angle(Point start, Point end)
    {
      return Math.Atan2(start.Y - end.Y, end.X - start.X) * Rad2Deg;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
