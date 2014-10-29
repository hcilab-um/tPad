using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPad.App.PhotoAlbum.Converters
{
  public class DraggingToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      PhotoAlbumApp.DraggingDirection state = (PhotoAlbumApp.DraggingDirection)value;
      PhotoAlbumApp.DraggingDirection target = (PhotoAlbumApp.DraggingDirection)Enum.Parse(typeof(PhotoAlbumApp.DraggingDirection), parameter as String);
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
