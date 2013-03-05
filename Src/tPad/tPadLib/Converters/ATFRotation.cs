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
      double destAngle = (double)values[1];

      if (!flag)
        return (double)0;

      return (double)destAngle;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
