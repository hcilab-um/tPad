using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace UofM.HCI.tPab.Converters
{
  public class LocationToMargingConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (values == null || values.Length != 2 || values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
        return new Thickness(0);

      System.Drawing.Point location = (System.Drawing.Point)values[0];
      Size borderDiff = (Size)values[1];

      return new Thickness(location.X - borderDiff.Width, location.Y - borderDiff.Height, 0, 0);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
