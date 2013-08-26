using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPad.Converters
{
  public class AdditionConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var factor1 = Double.Parse(value.ToString());
      var factor2 = Double.Parse(parameter.ToString());

      return factor1 + factor2;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
