using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPad.App.PhotoAlbum.Converters
{
  public class StackingDimensionsConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      StackingState state = (StackingState)values[0];
      double baseValue = (double)values[1];
      double alternative = (double)values[2];
      double modifier = Double.Parse(parameter as String);

      if (state == StackingState.NotStacked)
        return baseValue;
      return alternative * modifier;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
