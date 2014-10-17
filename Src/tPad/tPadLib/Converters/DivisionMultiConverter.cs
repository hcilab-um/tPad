using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace UofM.HCI.tPad.Converters
{
  public class DivisionMultiConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (values.Length == 0)
        return (double)1;

      foreach(var value in values)
        if(value == DependencyProperty.UnsetValue)
          return (double)1;

      var factor = (double)values[0];
      for (int i = 1; i < values.Length; i++)
        factor = factor / (double)values[i];

      return Math.Round(factor, 2);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
