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
      if (values == null || values.Length != 2)
        return (double)0;
      if(values[0] == System.Windows.DependencyProperty.UnsetValue || values[1] == System.Windows.DependencyProperty.UnsetValue)
        return (double)0;

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
