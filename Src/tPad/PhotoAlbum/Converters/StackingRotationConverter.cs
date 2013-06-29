using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPad.App.PhotoAlbum.Converters
{
  public class StackingRotationConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      StackingState state = (StackingState)value;
      if (state == StackingState.NotStacked)
        return 0;
      if (state == StackingState.StackedOnTop)
        return 90;
      return 270;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
