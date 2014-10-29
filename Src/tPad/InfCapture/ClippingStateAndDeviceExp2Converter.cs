using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace UofM.HCI.tPad.App.InfCapture
{
  public class ClippingStateAndDeviceExp2Converter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (values[0] == DependencyProperty.UnsetValue)
        return Visibility.Collapsed;

      Device actualD = (Device)values[0];
      ClippingState actualPM = (ClippingState)values[1];

      if ("RETAKE".Equals(parameter.ToString()))
      {
        if (actualD == Device.Normal && actualPM == ClippingState.Clipping)
          return Visibility.Visible;
      }
      else if ("FEED".Equals(parameter.ToString()))
      {
        if (actualD == Device.tPad)
          return Visibility.Visible;
      }
      return Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
