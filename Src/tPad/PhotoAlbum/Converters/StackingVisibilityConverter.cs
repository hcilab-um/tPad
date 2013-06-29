using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPad.App.PhotoAlbum.Converters
{
  public class StackingVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      StackingState state = (StackingState)value;
      StackingState target = (StackingState)Enum.Parse(typeof(StackingState), parameter as String);
      if (state == target)
        return System.Windows.Visibility.Visible;
      return System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
