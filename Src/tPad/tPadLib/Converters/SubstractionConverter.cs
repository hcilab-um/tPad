using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPad.Converters
{
  public class SubstractionConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var minuend = Double.Parse(value.ToString());
      var subtrahend = Double.Parse(parameter.ToString());

      return minuend - subtrahend;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
