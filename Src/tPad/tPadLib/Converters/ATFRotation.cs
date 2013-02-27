using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPab.Converters
{
  public class ATFRotation : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      bool flag = (bool)values[0];
      float angle = (float)values[1];

      if (!flag)
        return 0;

      double destAngle = (double)GetDestAngle(ref angle);
      return destAngle;
    }

    public static float GetDestAngle(ref float angle)
    {
      angle = angle % 360;
      float destAngle = 0;
      if (315 < angle || angle <= 45)
        destAngle = 0;
      else if (45 < angle && angle <= 135)
        destAngle = 270;
      else if (135 < angle && angle <= 225)
        destAngle = 180;
      else if (225 < angle && angle <= 315)
        destAngle = 90;
      return destAngle;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
