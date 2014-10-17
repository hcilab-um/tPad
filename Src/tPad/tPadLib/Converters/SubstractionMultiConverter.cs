using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPad.Converters
{
  public class SubstractionMultiConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var minuend = Double.Parse(values[0].ToString());

      for (int i = 1; i < values.Length; i++)
        minuend -= Double.Parse(values[i].ToString());

      return minuend;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
