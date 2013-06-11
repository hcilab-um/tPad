using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace UofM.HCI.tPad.App.ActiveReader.Converters
{
  public class PageBackgroundConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      LinearGradientBrush gradientBrush = new LinearGradientBrush(Color.FromRgb(253, 254, 192), Color.FromRgb(252, 233, 158), new Point(0.5, 0), new Point(0.5, 1));

      var actualPage = (int)values[0];
      var pageIndex = (int)values[1];
      if (actualPage == pageIndex)
        return Brushes.GhostWhite;
      return gradientBrush;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
