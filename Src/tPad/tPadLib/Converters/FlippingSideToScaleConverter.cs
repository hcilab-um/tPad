using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using UofM.HCI.tPab.Monitors;

namespace UofM.HCI.tPab.Converters
{
  public class FlippingSideToScaleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      FlippingMode mode = (FlippingMode)value;
      if (mode == FlippingMode.FaceUp)
        return 1;
      return -1;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
