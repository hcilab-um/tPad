using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace UofM.HCI.tPad.App.InfCapture
{
  public class ClippingStateExp2Converter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      ClippingState targetPM = (ClippingState)Enum.Parse(typeof(ClippingState), parameter.ToString());
      ClippingState actualPM = (ClippingState)value;
      if (actualPM == targetPM)
        return Visibility.Visible;
      return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
