using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPad.App.PhotoAlbum.Converters
{
  public class StackingTranslationConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      StackingState state = (StackingState)values[0];
      double alternative = (double)values[1];
      String axis = (String)parameter;

      if (state == StackingState.StackedOnTop && "X".Equals(axis))
        return alternative;
      if (state == StackingState.StackedBelow && "Y".Equals(axis))
        return alternative / 2;
      return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
