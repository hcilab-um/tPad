using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace UofM.HCI.tPad.App.InfCapture
{
  public class DeviceExp2Converter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      Device targetD = (Device)Enum.Parse(typeof(Device), parameter.ToString());
      Device actualD = (Device)value;
      if (actualD == targetD)
        return Visibility.Visible;
      return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
