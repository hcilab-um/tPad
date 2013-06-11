using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPad.Converters
{
  public class ATFTranslation : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      bool flag = (bool)values[0];
      double destAngle = (double)values[1];
      double amount = (double)values[2];
      String XY = (String)parameter;

      if (!flag)
        return (double)0;

      if (destAngle == 0)
        return (double)0;
      else if (destAngle == 90 && XY == "X")
        return amount;
      else if (destAngle == 180)
        return amount;
      else if (destAngle == 270 && XY == "Y")
        return amount;
      return (double)0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
