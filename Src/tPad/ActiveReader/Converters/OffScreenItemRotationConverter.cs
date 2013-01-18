using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Drawing;
using System.Windows;

namespace UofM.HCI.tPab.App.ActiveReader.Converters
{
  class OffScreenItemRotationConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var deviceWidth = float.Parse(values[0].ToString());
      var deviceHeight = float.Parse(values[1].ToString());
      var highlightPositionX = float.Parse(values[2].ToString());
      var highlightPositionY = float.Parse(values[3].ToString());
      var deviceLocation = (PointF)values[4];
      var widthFactor = float.Parse(values[5].ToString());
      var heightFactor = float.Parse(values[6].ToString());
      var devicePageWidthFactor = float.Parse(values[7].ToString()) / deviceWidth;

      //border (lines) of device
      float xBorderRight = (deviceLocation.X * widthFactor) + deviceWidth - (42 / devicePageWidthFactor);
      float xBorderLeft = deviceLocation.X * widthFactor;
      float yBorderTop = deviceLocation.Y * heightFactor;
      float yBorderBottom = (deviceLocation.Y * heightFactor) + deviceHeight;

      //compute linear equation (i.e. gradient and the point n at which the line crosses the y-axis) of line between center of device and hoghlight position
      PointF deviceCenter = new PointF((deviceLocation.X * widthFactor) + (deviceWidth / 2.0f), (deviceLocation.Y * heightFactor) + (deviceHeight / 2.0f));
      float gradient = (highlightPositionY - deviceCenter.Y) / (highlightPositionX - deviceCenter.X);
      float n = deviceCenter.Y - (deviceCenter.X * gradient);
      float angle = (float)((Math.Atan(gradient) * 180) / Math.PI);

      //intersection with left device border
      float yBorderLeft = (gradient * xBorderLeft) + n;
      if (yBorderLeft < yBorderBottom && yBorderLeft > yBorderTop && highlightPositionX < xBorderLeft)
        return (double)(angle + 90);

      //intersection with right device border
      float yBorderRight = (gradient * xBorderRight) + n;
      if (yBorderRight >= yBorderTop && yBorderRight <= yBorderBottom && highlightPositionX > xBorderRight)
        return (double)(angle - 90);

      //intersection with top device border
      float xBorderTop = (yBorderTop - n) / gradient;
      if (xBorderTop >= xBorderLeft && xBorderTop <= xBorderRight && highlightPositionY < yBorderTop)
      {
        if (angle < 0)
          return (double)(angle - 90);
        return (double)(angle + 90);
      }

      //intersection with bottom device border
      float xBorderBottom = (yBorderBottom - n) / gradient;
      if (xBorderBottom >= xBorderLeft && xBorderBottom <= xBorderRight && highlightPositionY > yBorderBottom)
      {
        if (angle < 0)
          return (double)(angle + 90);
        return (double)(angle - 90);
      }

      //highlight is within the device display
      return 0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
