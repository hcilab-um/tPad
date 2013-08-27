using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace UofM.HCI.tPad.App.InfCapture
{
  public class DeviceExp2Converter : IMultiValueConverter
  {

    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      bool? showFeed = (bool?)values[1];
      if (showFeed.Value)
        return Visibility.Visible;

      if (values[0] == DependencyProperty.UnsetValue)
        return Visibility.Collapsed;

      Device targetD = (Device)Enum.Parse(typeof(Device), parameter.ToString());
      Device actualD = (Device)values[0];
      if (actualD == targetD)
        return Visibility.Visible;
      return Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
