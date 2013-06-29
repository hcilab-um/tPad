using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPad.App.PhotoAlbum.Converters
{
  public class DraggingToColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      PhotoAlbumApp.DraggingDirection direction = (PhotoAlbumApp.DraggingDirection)value;
      if (direction == PhotoAlbumApp.DraggingDirection.None)
        return System.Windows.Media.Brushes.Transparent;
      return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x33, 0xFA, 0xDA, 0xDD));
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
