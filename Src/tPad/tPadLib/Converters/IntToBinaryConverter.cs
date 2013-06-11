using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPad.Converters
{
  class IntToBinaryConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      int original = (int)value;
      string binary = System.Convert.ToString(original, 2);
      while (binary.Length < 2)
        binary = "0" + binary;
      binary = binary + binary;
      return binary;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
