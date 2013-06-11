using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace UofM.HCI.tPad.Converters
{
  public class MultiplierMultiConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (values.Length == 0)
        return 1;

      var multiplier = (double)1;
      for (int i = 0; i < values.Length; i++)
        multiplier = multiplier * (double)values[i];

      if (parameter != null)
      {
        var extraM = Double.Parse(parameter as String);
        multiplier = multiplier * extraM;
      }

      return multiplier;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
