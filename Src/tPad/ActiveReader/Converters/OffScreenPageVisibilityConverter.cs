using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace UofM.HCI.tPab.App.ActiveReader.Converters
{
  public class OffScreenPageVisibilityConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var actualPage = (int)values[0];
      var pageIndex = (int)values[1];
      if (actualPage == pageIndex)
        return Visibility.Visible;
      return Visibility.Hidden;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
