using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPab.Converters
{
  public class ATFSize : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      bool flag = (bool)values[0];
      float destAngle = (float)values[1];
      double shortSide = (double)values[2];
      double longSide = (double)values[3];
      String dimension = (String)parameter;

      if (!flag || destAngle == 0 || destAngle == 180)
      {
        if (dimension == "Width")
          return shortSide;
        else
          return longSide;
      }
      else if (destAngle == 90 || destAngle == 270)
      {
        if (dimension == "Width")
          return longSide;
        else
          return shortSide;
      }
      return (double)0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
